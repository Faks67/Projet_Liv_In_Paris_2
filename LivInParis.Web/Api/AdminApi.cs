using System.Text.Json;
using Dapper;
using LivInParis.Web.Services;

namespace LivInParis.Web.Api;

public static class AdminApi
{
    private static readonly string[] TablesExportables = { "Client", "Cuisinier", "Plat", "Commande" };

    public static void MapAdminApi(this WebApplication app)
    {
        var groupe = app.MapGroup("/api/admin").RequireAuthorization("admin");

        groupe.MapGet("/stats", async (Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            const string valides = "statut <> 'Annulée'";
            return Results.Ok(new
            {
                clients = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Client"),
                cuisiniers = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Cuisinier"),
                plats = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Plat"),
                commandes = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Commande WHERE {valides}"),
                chiffreAffaires = await conn.ExecuteScalarAsync<double?>($"SELECT SUM(prix) FROM Commande WHERE {valides}") ?? 0,
                panierMoyen = await conn.ExecuteScalarAsync<double?>($"SELECT AVG(prix) FROM Commande WHERE {valides}") ?? 0,
                depenseMoyenneClient = await conn.ExecuteScalarAsync<double?>(
                    $"SELECT AVG(total) FROM (SELECT SUM(prix) AS total FROM Commande WHERE {valides} GROUP BY idClient) t") ?? 0,
                parStatut = await conn.QueryAsync("SELECT statut, COUNT(*) AS nombre FROM Commande GROUP BY statut"),
                livraisonsParCuisinier = await conn.QueryAsync(
                    $@"SELECT CONCAT(c.prenom, ' ', c.nom) AS cuisinier, COUNT(co.idCommande) AS livraisons
                       FROM Cuisinier c LEFT JOIN Commande co ON co.idCuisinier = c.idCuisinier AND co.{valides}
                       GROUP BY c.idCuisinier, c.prenom, c.nom ORDER BY livraisons DESC"),
                parNationalite = await conn.QueryAsync(
                    $@"SELECT p.Nationalité AS nationalite, COUNT(*) AS commandes, SUM(co.prix) AS montant
                       FROM Commande co JOIN Plat p ON p.IdPlat = co.IdPlat WHERE co.{valides}
                       GROUP BY p.Nationalité ORDER BY commandes DESC"),
                parJour = await conn.QueryAsync(
                    $@"SELECT DATE(dateCo) AS jour, COUNT(*) AS commandes, SUM(prix) AS montant
                       FROM Commande WHERE {valides} AND dateCo >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
                       GROUP BY DATE(dateCo) ORDER BY jour"),
                topClients = await conn.QueryAsync(
                    $@"SELECT cl.idClient AS id, CONCAT(cl.prenom, ' ', cl.nom) AS client, COUNT(*) AS commandes, SUM(co.prix) AS total
                       FROM Commande co JOIN Client cl ON cl.idClient = co.idClient WHERE co.{valides}
                       GROUP BY cl.idClient, cl.prenom, cl.nom ORDER BY total DESC LIMIT 10")
            });
        });

        groupe.MapGet("/clients", async (Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            return Results.Ok(await conn.QueryAsync(
                @"SELECT cl.idClient AS id, cl.nom, cl.prenom, cl.adresse, cl.StationProche AS station, cl.ZoneDeLivraison AS zone, cl.idUtilisateur AS utilisateur,
                         COUNT(co.idCommande) AS commandes, COALESCE(SUM(CASE WHEN co.statut <> 'Annulée' THEN co.prix END), 0) AS total
                  FROM Client cl LEFT JOIN Commande co ON co.idClient = cl.idClient
                  GROUP BY cl.idClient, cl.nom, cl.prenom, cl.adresse, cl.StationProche, cl.ZoneDeLivraison, cl.idUtilisateur
                  ORDER BY cl.nom, cl.prenom"));
        });

        groupe.MapGet("/cuisiniers", async (Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            return Results.Ok(await conn.QueryAsync(
                @"SELECT c.idCuisinier AS id, c.nom, c.prenom, c.adresse, c.Station AS station, c.addMail AS email, c.numTel AS telephone, c.idUtilisateur AS utilisateur,
                         (SELECT COUNT(*) FROM Plat p WHERE p.idCuisinier = c.idCuisinier) AS plats,
                         (SELECT COUNT(*) FROM Commande co WHERE co.idCuisinier = c.idCuisinier AND co.statut <> 'Annulée') AS livraisons
                  FROM Cuisinier c ORDER BY c.nom, c.prenom"));
        });

        groupe.MapGet("/commandes", async (DateTime? debut, DateTime? fin, string? client, Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            return Results.Ok(await conn.QueryAsync(CommandesApi.SelectCommande + @"
                WHERE (@debut IS NULL OR co.dateCo >= @debut) AND (@fin IS NULL OR co.dateCo < @fin)
                  AND (@client IS NULL OR co.idClient = @client)
                ORDER BY co.dateCo DESC",
                new { debut, fin = fin?.AddDays(1), client = string.IsNullOrWhiteSpace(client) ? null : client }));
        });

        groupe.MapDelete("/clients/{id}", async (string id, Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            await using var tx = await conn.BeginTransactionAsync();
            string? utilisateur = await conn.QueryFirstOrDefaultAsync<string>("SELECT idUtilisateur FROM Client WHERE idClient = @id", new { id }, tx);
            if (utilisateur == null) return Session.Erreur("Client introuvable.", 404);
            foreach (string table in new[] { "Commande", "Avis", "Particulier", "Entreprise", "Client" })
                await conn.ExecuteAsync($"DELETE FROM {table} WHERE idClient = @id", new { id }, tx);
            await SupprimerUtilisateurOrphelinAsync(conn, utilisateur, tx);
            await tx.CommitAsync();
            return Results.Ok();
        });

        groupe.MapDelete("/cuisiniers/{id}", async (string id, Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            await using var tx = await conn.BeginTransactionAsync();
            string? utilisateur = await conn.QueryFirstOrDefaultAsync<string>("SELECT idUtilisateur FROM Cuisinier WHERE idCuisinier = @id", new { id }, tx);
            if (utilisateur == null) return Session.Erreur("Cuisinier introuvable.", 404);
            await conn.ExecuteAsync("DELETE FROM Commande WHERE idCuisinier = @id OR IdPlat IN (SELECT IdPlat FROM Plat WHERE idCuisinier = @id)", new { id }, tx);
            await conn.ExecuteAsync("DELETE FROM Plat WHERE idCuisinier = @id", new { id }, tx);
            await conn.ExecuteAsync("DELETE FROM Cuisinier WHERE idCuisinier = @id", new { id }, tx);
            await SupprimerUtilisateurOrphelinAsync(conn, utilisateur, tx);
            await tx.CommitAsync();
            return Results.Ok();
        });

        groupe.MapGet("/export/{table}", async (string table, Db db) =>
        {
            string? nom = TablesExportables.FirstOrDefault(t => t.Equals(table, StringComparison.OrdinalIgnoreCase));
            if (nom == null) return Session.Erreur("Table inconnue.", 404);
            await using var conn = await db.OuvrirAsync();
            var lignes = (await conn.QueryAsync($"SELECT * FROM {nom}")).Select(l => (IDictionary<string, object>)l);
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(lignes, new JsonSerializerOptions { WriteIndented = true });
            return Results.File(json, "application/json", $"{nom.ToLowerInvariant()}.json");
        });

        // Coloration de Welsh-Powell : deux cuisiniers sont reliés s'ils ont servi un même client
        groupe.MapGet("/coloration", async (Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            var liens = (await conn.QueryAsync<(string cuisinier, string nom, string client)>(
                @"SELECT DISTINCT c.idCuisinier, CONCAT(c.prenom, ' ', c.nom), co.idClient
                  FROM Commande co JOIN Cuisinier c ON c.idCuisinier = co.idCuisinier WHERE co.statut <> 'Annulée'")).ToList();
            var noms = liens.GroupBy(l => l.cuisinier).ToDictionary(g => g.Key, g => g.First().nom);
            var clientsPar = liens.GroupBy(l => l.cuisinier).ToDictionary(g => g.Key, g => g.Select(l => l.client).ToHashSet());
            var voisins = clientsPar.Keys.ToDictionary(k => k, _ => new HashSet<string>());
            foreach (var a in clientsPar.Keys)
                foreach (var b in clientsPar.Keys)
                    if (a != b && clientsPar[a].Overlaps(clientsPar[b])) voisins[a].Add(b);

            var couleurs = new Dictionary<string, int>();
            foreach (var sommet in voisins.Keys.OrderByDescending(k => voisins[k].Count).ThenBy(k => k))
            {
                var prises = voisins[sommet].Where(couleurs.ContainsKey).Select(v => couleurs[v]).ToHashSet();
                int c = 0;
                while (prises.Contains(c)) c++;
                couleurs[sommet] = c;
            }
            return Results.Ok(new
            {
                sommets = voisins.Keys.Select(k => new { id = k, nom = noms[k], couleur = couleurs[k], clients = clientsPar[k].Count }),
                aretes = voisins.SelectMany(kv => kv.Value.Where(v => string.CompareOrdinal(kv.Key, v) < 0).Select(v => new { de = kv.Key, vers = v })),
                nombreCouleurs = couleurs.Count == 0 ? 0 : couleurs.Values.Max() + 1
            });
        });
    }

    /// <summary>
    /// Supprime l'utilisateur s'il n'est plus ni client ni cuisinier
    /// </summary>
    private static async Task SupprimerUtilisateurOrphelinAsync(MySqlConnector.MySqlConnection conn, string utilisateur, MySqlConnector.MySqlTransaction tx)
    {
        await conn.ExecuteAsync(
            @"DELETE FROM Utilisateur WHERE idUtilisateur = @u
              AND NOT EXISTS (SELECT 1 FROM Client WHERE idUtilisateur = @u)
              AND NOT EXISTS (SELECT 1 FROM Cuisinier WHERE idUtilisateur = @u)", new { u = utilisateur }, tx);
    }
}
