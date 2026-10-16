using Dapper;
using LivInParis.Web.Services;

namespace LivInParis.Web.Api;

public static class PlatsApi
{
    public const string SelectPlat = @"
        SELECT p.IdPlat AS id, p.TypePlat AS nom, p.Description AS description, p.Quantité AS stock,
               p.Nationalité AS nationalite, p.RegimeSpe AS regime, p.IngrediantPrin AS ingredient,
               p.DateFabrication AS dateFabrication, p.DatePeremption AS datePeremption, p.prix AS prix,
               c.idCuisinier AS idCuisinier, CONCAT(c.prenom, ' ', c.nom) AS cuisinier, c.Station AS station,
               (SELECT COALESCE(SUM(co.Portions), 0) FROM Commande co WHERE co.IdPlat = p.IdPlat AND co.statut <> 'Annulée') AS vendus,
               (p.Quantité > 0 AND (p.DatePeremption IS NULL OR p.DatePeremption >= CURDATE())) AS disponible
        FROM Plat p JOIN Cuisinier c ON c.idCuisinier = p.idCuisinier";

    public static void MapPlatsApi(this WebApplication app)
    {
        app.MapGet("/api/plats", async (string? q, string? nationalite, string? regime, Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            var plats = await conn.QueryAsync(SelectPlat + @"
                WHERE (@q IS NULL OR p.TypePlat LIKE @like OR p.IngrediantPrin LIKE @like OR p.Nationalité LIKE @like OR p.Description LIKE @like)
                  AND (@nationalite IS NULL OR p.Nationalité = @nationalite)
                  AND (@regime IS NULL OR p.RegimeSpe = @regime)
                ORDER BY disponible DESC, p.TypePlat",
                new { q = Vide(q), like = $"%{q}%", nationalite = Vide(nationalite), regime = Vide(regime) });
            return Results.Ok(plats);
        });

        app.MapGet("/api/plats/filtres", async (Db db) =>
        {
            await using var conn = await db.OuvrirAsync();
            return Results.Ok(new
            {
                nationalites = await conn.QueryAsync<string>("SELECT DISTINCT Nationalité FROM Plat WHERE Nationalité <> '' ORDER BY 1"),
                regimes = await conn.QueryAsync<string>("SELECT DISTINCT RegimeSpe FROM Plat WHERE RegimeSpe <> '' ORDER BY 1")
            });
        });
    }

    private static string? Vide(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
