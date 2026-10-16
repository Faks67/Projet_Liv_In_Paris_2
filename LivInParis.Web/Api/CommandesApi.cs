using System.Security.Claims;
using Dapper;
using LivInParis.Web.Services;
using MySqlConnector;

namespace LivInParis.Web.Api;

public record Article(string IdPlat, int Portions);

public record NouvelleCommande(List<Article> Articles, string Adresse, double? Lat, double? Lon);

public static class CommandesApi
{
    public const string SelectCommande = @"
        SELECT co.idCommande AS id, co.statut AS statut, co.prix AS prix, co.Portions AS portions, co.dateCo AS date,
               co.depart AS depart, co.arrive AS arrive, co.AdresseLivraison AS adresse, co.DureeEstimee AS duree,
               co.IdPlat AS idPlat, p.TypePlat AS plat, p.Nationalité AS nationalite,
               co.idClient AS idClient, CONCAT(cl.prenom, ' ', cl.nom) AS client,
               co.idCuisinier AS idCuisinier, CONCAT(cu.prenom, ' ', cu.nom) AS cuisinier
        FROM Commande co
        LEFT JOIN Plat p ON p.IdPlat = co.IdPlat
        JOIN Client cl ON cl.idClient = co.idClient
        JOIN Cuisinier cu ON cu.idCuisinier = co.idCuisinier";

    public static readonly string[] Statuts = { "En cours", "En livraison", "Livrée", "Annulée" };

    public static void MapCommandesApi(this WebApplication app)
    {
        app.MapPost("/api/commandes", async (NouvelleCommande n, ClaimsPrincipal user, Db db, Metro metro) =>
        {
            if (n.Articles == null || n.Articles.Count == 0) return Session.Erreur("Votre panier est vide.");
            if (n.Lat is not double lat || n.Lon is not double lon || string.IsNullOrWhiteSpace(n.Adresse))
                return Session.Erreur("Choisissez une adresse de livraison dans la liste proposée.");
            if (n.Articles.Any(a => a.Portions < 1 || a.Portions > 50)) return Session.Erreur("Nombre de portions invalide.");

            string idClient = user.IdClient()!;
            var (stationLivraison, _, _) = metro.PlusProche(lat, lon);
            var destination = new Point(lat, lon, n.Adresse);

            await using var conn = await db.OuvrirAsync();
            await using var tx = await conn.BeginTransactionAsync();
            var creees = new List<string>();
            foreach (var article in n.Articles.GroupBy(a => a.IdPlat).Select(g => new Article(g.Key, g.Sum(a => a.Portions))))
            {
                var plat = await conn.QueryFirstOrDefaultAsync(
                    @"SELECT p.TypePlat AS nom, p.Quantité AS stock, p.prix AS prix, p.DatePeremption AS peremption, p.idCuisinier AS idCuisinier, c.Station AS station
                      FROM Plat p JOIN Cuisinier c ON c.idCuisinier = p.idCuisinier WHERE p.IdPlat = @id FOR UPDATE",
                    new { id = article.IdPlat }, tx);
                if (plat == null) return Session.Erreur($"Le plat {article.IdPlat} n'existe plus.");
                if ((int)plat.stock < article.Portions)
                    return Session.Erreur($"Plus que {plat.stock} portion(s) de « {plat.nom} » disponible(s).", 409);
                if (plat.peremption is DateTime peremption && peremption.Date < DateTime.Today)
                    return Session.Erreur($"« {plat.nom} » n'est plus disponible (date de péremption dépassée).", 409);

                // Durée estimée : trajet porte à porte depuis la station du cuisinier
                int? duree = null;
                if (metro.ParNom((string)plat.station) is Station sc)
                    duree = (int?)MetroApi.Calculer(metro, new Point(sc.Lat, sc.Lon, sc.Nom), destination)?.TotalMinutes;

                string idCommande = await Db.NouvelIdAsync(conn, "Commande", "idCommande", "CMD", tx);
                await conn.ExecuteAsync(
                    @"INSERT INTO Commande (idCommande, statut, prix, idCuisinier, depart, arrive, dateCo, idClient, IdPlat, Portions, AdresseLivraison, LatLivraison, LonLivraison, DureeEstimee)
                      VALUES (@idCommande, 'En cours', @prix, @idCuisinier, @depart, @arrive, NOW(), @idClient, @idPlat, @portions, @adresse, @lat, @lon, @duree)",
                    new
                    {
                        idCommande, prix = (float)plat.prix * article.Portions, idCuisinier = (string)plat.idCuisinier,
                        depart = (string)plat.station, arrive = stationLivraison.Nom, idClient, idPlat = article.IdPlat,
                        portions = article.Portions, adresse = Session.Tronquer(n.Adresse, 255), lat, lon, duree
                    }, tx);
                await conn.ExecuteAsync("UPDATE Plat SET Quantité = Quantité - @n WHERE IdPlat = @id", new { n = article.Portions, id = article.IdPlat }, tx);
                creees.Add(idCommande);
            }
            await tx.CommitAsync();
            return Results.Ok(new { commandes = creees });
        }).RequireAuthorization("client");

        app.MapGet("/api/commandes/mes", async (ClaimsPrincipal user, Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            return Results.Ok(await conn.QueryAsync(SelectCommande + " WHERE co.idClient = @id ORDER BY co.dateCo DESC, co.idCommande DESC", new { id = user.IdClient() }));
        }).RequireAuthorization("client");

        app.MapPost("/api/commandes/{id}/annuler", async (string id, ClaimsPrincipal user, Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            var commande = await conn.QueryFirstOrDefaultAsync("SELECT idClient, statut FROM Commande WHERE idCommande = @id", new { id });
            if (commande == null || (string)commande.idClient != user.IdClient()) return Session.Erreur("Commande introuvable.", 404);
            if ((string)commande.statut != "En cours") return Session.Erreur("Cette commande ne peut plus être annulée.", 409);
            await ChangerStatutAsync(conn, id, "Annulée");
            return Results.Ok();
        }).RequireAuthorization("client");

        // Itinéraire de livraison d'une commande (client, cuisinier concerné ou admin)
        app.MapGet("/api/commandes/{id}/trajet", async (string id, ClaimsPrincipal user, Db db, Metro metro) =>
        {
            await using var conn = await db.OuvrirAsync();
            var c = await conn.QueryFirstOrDefaultAsync(
                "SELECT idClient, idCuisinier, depart, arrive, AdresseLivraison AS adresse, LatLivraison AS lat, LonLivraison AS lon FROM Commande WHERE idCommande = @id", new { id });
            if (c == null) return Session.Erreur("Commande introuvable.", 404);
            if (!user.EstAdmin() && (string)c.idClient != user.IdClient() && (string)c.idCuisinier != user.IdCuisinier())
                return Session.Erreur("Commande introuvable.", 404);

            if (metro.ParNom((string)(c.depart ?? "")) is not Station sd)
                return Session.Erreur($"La station de départ « {c.depart} » est inconnue du plan du métro.", 422);
            Point arrivee;
            if (c.lat is double lat && c.lon is double lon)
                arrivee = new Point(lat, lon, (string?)c.adresse ?? "Adresse de livraison");
            else if (metro.ParNom((string)(c.arrive ?? "")) is Station sa)
                arrivee = new Point(sa.Lat, sa.Lon, sa.Nom);
            else
                return Session.Erreur($"La station d'arrivée « {c.arrive} » est inconnue du plan du métro.", 422);

            var trajet = MetroApi.Calculer(metro, new Point(sd.Lat, sd.Lon, $"Cuisine – {sd.Nom}"), arrivee);
            return trajet == null ? Session.Erreur("Aucun itinéraire trouvé.", 404) : Results.Ok(trajet);
        }).RequireAuthorization();
    }

    /// <summary>
    /// Change le statut ; une commande annulée remet les portions en stock
    /// </summary>
    public static async Task ChangerStatutAsync(MySqlConnection conn, string id, string statut)
    {
        await using var tx = await conn.BeginTransactionAsync();
        if (statut == "Annulée")
            await conn.ExecuteAsync(
                "UPDATE Plat p JOIN Commande co ON co.IdPlat = p.IdPlat SET p.Quantité = p.Quantité + co.Portions WHERE co.idCommande = @id AND co.statut <> 'Annulée'",
                new { id }, tx);
        await conn.ExecuteAsync("UPDATE Commande SET statut = @statut WHERE idCommande = @id", new { id, statut }, tx);
        await tx.CommitAsync();
    }
}
