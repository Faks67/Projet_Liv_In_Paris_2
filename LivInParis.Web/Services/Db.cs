using System.Security.Cryptography;
using Dapper;
using MySqlConnector;

namespace LivInParis.Web.Services;

/// <summary>
/// Accès à la base liv_in_paris et petites mises à niveau du schéma nécessaires au site
/// </summary>
public class Db(string chaineConnexion)
{
    public async Task<MySqlConnection> OuvrirAsync()
    {
        var conn = new MySqlConnection(chaineConnexion);
        await conn.OpenAsync();
        return conn;
    }

    /// <summary>
    /// Ajoute les colonnes utilisées par le site si elles n'existent pas encore (la base de la v1 reste compatible)
    /// </summary>
    public async Task MettreAJourSchemaAsync()
    {
        await using var conn = await OuvrirAsync();
        async Task<int?> TailleColonne(string table, string colonne) =>
            await conn.QueryFirstOrDefaultAsync<int?>(
                "SELECT COALESCE(CHARACTER_MAXIMUM_LENGTH, 0) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @table AND COLUMN_NAME = @colonne",
                new { table, colonne });

        async Task AjouterColonne(string table, string colonne, string definition)
        {
            if (await TailleColonne(table, colonne) == null)
                await conn.ExecuteAsync($"ALTER TABLE {table} ADD COLUMN {colonne} {definition}");
        }

        // Mots de passe hachés (PBKDF2) : il faut plus de 50 caractères
        if (await TailleColonne("Utilisateur", "MotDePasse") is int t && t < 255)
            await conn.ExecuteAsync("ALTER TABLE Utilisateur MODIFY MotDePasse VARCHAR(255) NOT NULL");
        if (await TailleColonne("Cuisinier", "numTel") is 0)
            await conn.ExecuteAsync("ALTER TABLE Cuisinier MODIFY numTel VARCHAR(20)");

        await AjouterColonne("Commande", "Portions", "INT NOT NULL DEFAULT 1");
        await AjouterColonne("Commande", "AdresseLivraison", "VARCHAR(255) NULL");
        await AjouterColonne("Commande", "LatLivraison", "DOUBLE NULL");
        await AjouterColonne("Commande", "LonLivraison", "DOUBLE NULL");
        await AjouterColonne("Commande", "DureeEstimee", "INT NULL");
        await AjouterColonne("Plat", "Description", "VARCHAR(255) NULL");
    }

    /// <summary>
    /// Prochain identifiant libre de la forme PREFIXEn (CL6, C6, P6, CMD11...)
    /// </summary>
    public static async Task<string> NouvelIdAsync(MySqlConnection conn, string table, string colonne, string prefixe, MySqlTransaction? tx = null)
    {
        var ids = await conn.QueryAsync<string>($"SELECT {colonne} FROM {table} WHERE {colonne} LIKE @p", new { p = prefixe + "%" }, tx);
        int max = 0;
        foreach (string id in ids)
            if (int.TryParse(id[prefixe.Length..], out int n) && n > max) max = n;
        return prefixe + (max + 1);
    }
}

/// <summary>
/// Hachage des mots de passe. Les anciens mots de passe en clair (données de la v1) restent acceptés
/// et sont hachés à la première connexion.
/// </summary>
public static class MotDePasse
{
    private const int Iterations = 100_000;

    public static string Hacher(string motDePasse)
    {
        byte[] sel = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(motDePasse, sel, Iterations, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2${Iterations}${Convert.ToBase64String(sel)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verifier(string motDePasse, string stocke, out bool aRehacher)
    {
        aRehacher = false;
        var parties = stocke.Split('$');
        if (parties.Length == 4 && parties[0] == "pbkdf2")
        {
            byte[] sel = Convert.FromBase64String(parties[2]);
            byte[] attendu = Convert.FromBase64String(parties[3]);
            byte[] calcule = Rfc2898DeriveBytes.Pbkdf2(motDePasse, sel, int.Parse(parties[1]), HashAlgorithmName.SHA256, attendu.Length);
            return CryptographicOperations.FixedTimeEquals(calcule, attendu);
        }
        bool ok = CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(motDePasse), System.Text.Encoding.UTF8.GetBytes(stocke));
        aRehacher = ok;
        return ok;
    }
}
