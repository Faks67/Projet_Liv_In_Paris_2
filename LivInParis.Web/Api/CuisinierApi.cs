using System.Security.Claims;
using Dapper;
using LivInParis.Web.Services;

namespace LivInParis.Web.Api;

public record PlatSaisi(string Nom, string? Description, int Stock, string Nationalite, string Regime, string Ingredient, float Prix, int JoursConservation);

public record NouveauStatut(string Statut);

public static class CuisinierApi
{
    // Avancement possible d'une commande côté cuisinier
    private static readonly Dictionary<string, string[]> Transitions = new()
    {
        ["En cours"] = new[] { "En livraison", "Annulée" },
        ["En livraison"] = new[] { "Livrée" },
    };

    public static void MapCuisinierApi(this WebApplication app)
    {
        var groupe = app.MapGroup("/api/cuisinier").RequireAuthorization("cuisinier");

        groupe.MapGet("/tableau", async (ClaimsPrincipal user, Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            var id = user.IdCuisinier();
            return Results.Ok(new
            {
                chiffreAffaires = await conn.ExecuteScalarAsync<double?>("SELECT SUM(prix) FROM Commande WHERE idCuisinier = @id AND statut <> 'Annulée'", new { id }) ?? 0,
                commandes = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Commande WHERE idCuisinier = @id AND statut <> 'Annulée'", new { id }),
                aPreparer = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Commande WHERE idCuisinier = @id AND statut IN ('En cours', 'En livraison')", new { id }),
                clients = await conn.ExecuteScalarAsync<int>("SELECT COUNT(DISTINCT idClient) FROM Commande WHERE idCuisinier = @id AND statut <> 'Annulée'", new { id }),
                platsPopulaires = await conn.QueryAsync(
                    @"SELECT p.TypePlat AS nom, SUM(co.Portions) AS portions FROM Commande co JOIN Plat p ON p.IdPlat = co.IdPlat
                      WHERE co.idCuisinier = @id AND co.statut <> 'Annulée' GROUP BY p.TypePlat ORDER BY portions DESC LIMIT 5", new { id }),
                clientsServis = await conn.QueryAsync(
                    @"SELECT cl.idClient AS id, CONCAT(cl.prenom, ' ', cl.nom) AS nom, COUNT(*) AS commandes, MAX(co.dateCo) AS derniere
                      FROM Commande co JOIN Client cl ON cl.idClient = co.idClient
                      WHERE co.idCuisinier = @id AND co.statut <> 'Annulée' GROUP BY cl.idClient, cl.prenom, cl.nom ORDER BY derniere DESC", new { id })
            });
        });

        groupe.MapGet("/plats", async (ClaimsPrincipal user, Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            return Results.Ok(await conn.QueryAsync(PlatsApi.SelectPlat + " WHERE p.idCuisinier = @id ORDER BY p.DateFabrication DESC, p.IdPlat DESC", new { id = user.IdCuisinier() }));
        });

        groupe.MapPost("/plats", async (PlatSaisi p, ClaimsPrincipal user, Db db) =>
        {
            if (Valider(p) is string erreur) return Session.Erreur(erreur);
            await using var conn = await db.OuvrirAsync();
            string id = await Db.NouvelIdAsync(conn, "Plat", "IdPlat", "P");
            await conn.ExecuteAsync(
                @"INSERT INTO Plat (IdPlat, TypePlat, Description, Quantité, Nationalité, RegimeSpe, IngrediantPrin, DateFabrication, DatePeremption, prix, idCuisinier)
                  VALUES (@id, @nom, @description, @stock, @nationalite, @regime, @ingredient, CURDATE(), DATE_ADD(CURDATE(), INTERVAL @jours DAY), @prix, @cuisinier)",
                Parametres(p, id, user.IdCuisinier()!));
            return Results.Ok(new { id });
        });

        groupe.MapPut("/plats/{id}", async (string id, PlatSaisi p, ClaimsPrincipal user, Db db) =>
        {
            if (Valider(p) is string erreur) return Session.Erreur(erreur);
            await using var conn = await db.OuvrirAsync();
            int n = await conn.ExecuteAsync(
                @"UPDATE Plat SET TypePlat = @nom, Description = @description, Quantité = @stock, Nationalité = @nationalite, RegimeSpe = @regime,
                         IngrediantPrin = @ingredient, prix = @prix, DatePeremption = DATE_ADD(DateFabrication, INTERVAL @jours DAY)
                  WHERE IdPlat = @id AND idCuisinier = @cuisinier",
                Parametres(p, id, user.IdCuisinier()!));
            return n == 0 ? Session.Erreur("Plat introuvable.", 404) : Results.Ok();
        });

        groupe.MapDelete("/plats/{id}", async (string id, ClaimsPrincipal user, Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            if (await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Commande WHERE IdPlat = @id", new { id }) > 0)
                return Session.Erreur("Ce plat a déjà été commandé : mettez plutôt son stock à 0 pour le retirer du catalogue.", 409);
            int n = await conn.ExecuteAsync("DELETE FROM Plat WHERE IdPlat = @id AND idCuisinier = @cuisinier", new { id, cuisinier = user.IdCuisinier() });
            return n == 0 ? Session.Erreur("Plat introuvable.", 404) : Results.Ok();
        });

        groupe.MapGet("/commandes", async (ClaimsPrincipal user, Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            return Results.Ok(await conn.QueryAsync(CommandesApi.SelectCommande +
                " WHERE co.idCuisinier = @id ORDER BY FIELD(co.statut, 'En cours', 'En livraison', 'Livrée', 'Annulée'), co.dateCo DESC", new { id = user.IdCuisinier() }));
        });

        groupe.MapPost("/commandes/{id}/statut", async (string id, NouveauStatut s, ClaimsPrincipal user, Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            string? actuel = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT statut FROM Commande WHERE idCommande = @id AND idCuisinier = @cuisinier", new { id, cuisinier = user.IdCuisinier() });
            if (actuel == null) return Session.Erreur("Commande introuvable.", 404);
            if (!Transitions.TryGetValue(actuel, out var possibles) || !possibles.Contains(s.Statut))
                return Session.Erreur($"Impossible de passer de « {actuel} » à « {s.Statut} ».", 409);
            await CommandesApi.ChangerStatutAsync(conn, id, s.Statut);
            return Results.Ok();
        });
    }

    private static string? Valider(PlatSaisi p)
    {
        if (string.IsNullOrWhiteSpace(p.Nom)) return "Donnez un nom au plat.";
        if (p.Stock < 0 || p.Stock > 1000) return "Stock invalide.";
        if (p.Prix <= 0 || p.Prix > 500) return "Prix invalide.";
        if (p.JoursConservation < 0 || p.JoursConservation > 365) return "Durée de conservation invalide.";
        return null;
    }

    private static object Parametres(PlatSaisi p, string id, string cuisinier) => new
    {
        id, cuisinier,
        nom = Session.Tronquer(p.Nom),
        description = Session.Tronquer(p.Description ?? "", 255),
        stock = p.Stock,
        nationalite = Session.Tronquer(p.Nationalite ?? ""),
        regime = Session.Tronquer(string.IsNullOrWhiteSpace(p.Regime) ? "Aucun" : p.Regime),
        ingredient = Session.Tronquer(p.Ingredient ?? ""),
        prix = Math.Round(p.Prix, 2),
        jours = p.JoursConservation
    };
}
