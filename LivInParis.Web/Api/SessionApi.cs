using System.Security.Claims;
using Dapper;
using LivInParis.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;

namespace LivInParis.Web.Api;

public record Connexion(string Identifiant, string MotDePasse);

public record Inscription(
    string Role, string? Identifiant, string? MotDePasse,
    string Nom, string Prenom, string Adresse, double? Lat, double? Lon, string? Station,
    string? Email, string? Telephone, int? CodePostal);

public record Profil(string Nom, string Prenom, string Adresse, double? Lat, double? Lon, string? Station,
    string? Email, string? Telephone, int? CodePostal);

/// <summary>
/// Connexion, inscription et profil. Un même utilisateur peut être à la fois client et cuisinier.
/// </summary>
public static class Session
{
    public const string Utilisateur = "uid";
    public const string Client = "client";
    public const string Cuisinier = "cuisinier";
    public const string Admin = "admin";

    public static string? IdClient(this ClaimsPrincipal u) => u.FindFirstValue(Client);
    public static string? IdCuisinier(this ClaimsPrincipal u) => u.FindFirstValue(Cuisinier);
    public static bool EstAdmin(this ClaimsPrincipal u) => u.HasClaim(c => c.Type == Admin);

    public static IResult Erreur(string message, int code = 400) => Results.Json(new { erreur = message }, statusCode: code);

    public static void MapSessionApi(this WebApplication app)
    {
        app.MapGet("/api/session", async (ClaimsPrincipal user, Db db) =>
        {
            if (user.Identity?.IsAuthenticated != true) return Results.Ok(new { connecte = false });
            await using var conn = await db.OuvrirAsync();
            return Results.Ok(await DecrireAsync(conn, user));
        });

        app.MapPost("/api/connexion", async (Connexion c, HttpContext ctx, Db db, IConfiguration config) =>
        {
            string id = c.Identifiant?.Trim() ?? "";
            string adminId = config["Admin:Identifiant"] ?? "admin";
            string? adminMdp = config["Admin:MotDePasse"];
            if (id == adminId && !string.IsNullOrEmpty(adminMdp) && c.MotDePasse == adminMdp)
            {
                await SeConnecterAsync(ctx, id, null, null, admin: true);
                return Results.Ok(new { connecte = true, idUtilisateur = id, admin = true });
            }

            await using var conn = await db.OuvrirAsync();
            string? stocke = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT MotDePasse FROM Utilisateur WHERE idUtilisateur = @id", new { id });
            if (stocke == null || !MotDePasse.Verifier(c.MotDePasse ?? "", stocke, out bool aRehacher))
                return Erreur("Identifiant ou mot de passe incorrect.", 401);
            if (aRehacher)
                await conn.ExecuteAsync("UPDATE Utilisateur SET MotDePasse = @h WHERE idUtilisateur = @id",
                    new { h = MotDePasse.Hacher(c.MotDePasse!), id });

            var (client, cuisinier) = await RolesAsync(conn, id);
            await SeConnecterAsync(ctx, id, client, cuisinier, admin: false);
            return Results.Ok(new { connecte = true });
        });

        app.MapPost("/api/deconnexion", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync();
            return Results.Ok(new { connecte = false });
        });

        // Inscription d'un nouvel utilisateur, ou ajout d'un rôle (client / cuisinier) à l'utilisateur connecté
        app.MapPost("/api/inscription", async (Inscription i, HttpContext ctx, ClaimsPrincipal user, Db db, Metro metro) =>
        {
            if (i.Role is not ("client" or "cuisinier")) return Erreur("Rôle inconnu.");
            if (string.IsNullOrWhiteSpace(i.Nom) || string.IsNullOrWhiteSpace(i.Prenom) || string.IsNullOrWhiteSpace(i.Adresse))
                return Erreur("Nom, prénom et adresse sont obligatoires.");
            var station = TrouverStation(metro, i.Lat, i.Lon, i.Station);
            if (station == null) return Erreur("Choisissez une adresse dans la liste proposée.");

            await using var conn = await db.OuvrirAsync();
            await using var tx = await conn.BeginTransactionAsync();
            string? idUtilisateur = user.Identity?.IsAuthenticated == true ? user.FindFirstValue(Utilisateur) : null;
            if (user.EstAdmin()) return Erreur("Le compte admin ne peut pas commander ni cuisiner.");

            if (idUtilisateur == null)
            {
                idUtilisateur = i.Identifiant?.Trim();
                if (string.IsNullOrEmpty(idUtilisateur) || idUtilisateur.Length > 50) return Erreur("Choisissez un identifiant (50 caractères max).");
                if ((i.MotDePasse ?? "").Length < 6) return Erreur("Le mot de passe doit faire au moins 6 caractères.");
                if (await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Utilisateur WHERE idUtilisateur = @idUtilisateur", new { idUtilisateur }, tx) > 0)
                    return Erreur("Cet identifiant est déjà pris.");
                await conn.ExecuteAsync("INSERT INTO Utilisateur (idUtilisateur, MotDePasse) VALUES (@idUtilisateur, @h)",
                    new { idUtilisateur, h = MotDePasse.Hacher(i.MotDePasse!) }, tx);
            }

            if (i.Role == "client")
            {
                if (await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Client WHERE idUtilisateur = @idUtilisateur", new { idUtilisateur }, tx) > 0)
                    return Erreur("Vous avez déjà un compte client.");
                string idClient = await Db.NouvelIdAsync(conn, "Client", "idClient", "CL", tx);
                await conn.ExecuteAsync(
                    "INSERT INTO Client (idClient, ZoneDeLivraison, StationProche, idUtilisateur, adresse, nom, prenom) VALUES (@idClient, @zone, @station, @idUtilisateur, @adresse, @nom, @prenom)",
                    new { idClient, zone = station.Commune, station = station.Nom, idUtilisateur, adresse = Tronquer(i.Adresse), nom = Tronquer(i.Nom), prenom = Tronquer(i.Prenom) }, tx);
            }
            else
            {
                if (await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Cuisinier WHERE idUtilisateur = @idUtilisateur", new { idUtilisateur }, tx) > 0)
                    return Erreur("Vous avez déjà un compte cuisinier.");
                string idCuisinier = await Db.NouvelIdAsync(conn, "Cuisinier", "idCuisinier", "C", tx);
                await conn.ExecuteAsync(
                    "INSERT INTO Cuisinier (idCuisinier, idUtilisateur, nom, prenom, addMail, adresse, CodePostal, numTel, Station) VALUES (@idCuisinier, @idUtilisateur, @nom, @prenom, @email, @adresse, @codePostal, @tel, @station)",
                    new { idCuisinier, idUtilisateur, nom = Tronquer(i.Nom), prenom = Tronquer(i.Prenom), email = Tronquer(i.Email ?? ""), adresse = Tronquer(i.Adresse), codePostal = i.CodePostal, tel = Tronquer(i.Telephone ?? "", 20), station = station.Nom }, tx);
            }
            await tx.CommitAsync();

            var (client, cuisinier) = await RolesAsync(conn, idUtilisateur);
            await SeConnecterAsync(ctx, idUtilisateur, client, cuisinier, admin: false);
            return Results.Ok(new { connecte = true });
        });

        app.MapPut("/api/profil/client", async (Profil p, ClaimsPrincipal user, Db db, Metro metro) =>
        {
            var station = TrouverStation(metro, p.Lat, p.Lon, p.Station);
            if (station == null) return Erreur("Choisissez une adresse dans la liste proposée.");
            await using var conn = await db.OuvrirAsync();
            await conn.ExecuteAsync(
                "UPDATE Client SET nom = @nom, prenom = @prenom, adresse = @adresse, StationProche = @station, ZoneDeLivraison = @zone WHERE idClient = @id",
                new { nom = Tronquer(p.Nom), prenom = Tronquer(p.Prenom), adresse = Tronquer(p.Adresse), station = station.Nom, zone = station.Commune, id = user.IdClient() });
            return Results.Ok(await DecrireAsync(conn, user));
        }).RequireAuthorization("client");

        app.MapPut("/api/profil/cuisinier", async (Profil p, ClaimsPrincipal user, Db db, Metro metro) =>
        {
            var station = TrouverStation(metro, p.Lat, p.Lon, p.Station);
            if (station == null) return Erreur("Choisissez une adresse dans la liste proposée.");
            await using var conn = await db.OuvrirAsync();
            await conn.ExecuteAsync(
                "UPDATE Cuisinier SET nom = @nom, prenom = @prenom, adresse = @adresse, Station = @station, addMail = @email, numTel = @tel, CodePostal = @codePostal WHERE idCuisinier = @id",
                new { nom = Tronquer(p.Nom), prenom = Tronquer(p.Prenom), adresse = Tronquer(p.Adresse), station = station.Nom, email = Tronquer(p.Email ?? ""), tel = Tronquer(p.Telephone ?? "", 20), codePostal = p.CodePostal, id = user.IdCuisinier() });
            return Results.Ok(await DecrireAsync(conn, user));
        }).RequireAuthorization("cuisinier");
    }

    public static string Tronquer(string texte, int max = 50) => texte.Trim().Length > max ? texte.Trim()[..max] : texte.Trim();

    /// <summary>
    /// Station à partir de coordonnées GPS (adresse choisie) ou, à défaut, d'un nom de station
    /// </summary>
    public static Station? TrouverStation(Metro metro, double? lat, double? lon, string? nomStation)
    {
        if (lat is double la && lon is double lo) return metro.PlusProche(la, lo).station;
        return string.IsNullOrWhiteSpace(nomStation) ? null : metro.ParNom(nomStation);
    }

    private static async Task<(string? client, string? cuisinier)> RolesAsync(MySqlConnection conn, string idUtilisateur)
    {
        string? client = await conn.QueryFirstOrDefaultAsync<string>("SELECT idClient FROM Client WHERE idUtilisateur = @idUtilisateur", new { idUtilisateur });
        string? cuisinier = await conn.QueryFirstOrDefaultAsync<string>("SELECT idCuisinier FROM Cuisinier WHERE idUtilisateur = @idUtilisateur", new { idUtilisateur });
        return (client, cuisinier);
    }

    private static async Task SeConnecterAsync(HttpContext ctx, string idUtilisateur, string? client, string? cuisinier, bool admin)
    {
        var claims = new List<Claim> { new(Utilisateur, idUtilisateur), new(ClaimTypes.Name, idUtilisateur) };
        if (client != null) claims.Add(new(Client, client));
        if (cuisinier != null) claims.Add(new(Cuisinier, cuisinier));
        if (admin) claims.Add(new(Admin, "1"));
        var identite = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(new ClaimsPrincipal(identite), new AuthenticationProperties { IsPersistent = true });
    }

    private static async Task<object> DecrireAsync(MySqlConnection conn, ClaimsPrincipal user)
    {
        var client = user.IdClient() is string idc
            ? await conn.QueryFirstOrDefaultAsync(
                "SELECT idClient AS id, nom, prenom, adresse, StationProche AS station, ZoneDeLivraison AS zone FROM Client WHERE idClient = @idc", new { idc })
            : null;
        var cuisinier = user.IdCuisinier() is string idk
            ? await conn.QueryFirstOrDefaultAsync(
                "SELECT idCuisinier AS id, nom, prenom, adresse, Station AS station, addMail AS email, numTel AS telephone, CodePostal AS codePostal FROM Cuisinier WHERE idCuisinier = @idk", new { idk })
            : null;
        return new { connecte = true, idUtilisateur = user.FindFirstValue(Utilisateur), client, cuisinier, admin = user.EstAdmin() };
    }
}
