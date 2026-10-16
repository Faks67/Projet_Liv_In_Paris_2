using System;
using MySql.Data.MySqlClient;

public class Client
{
    /// <summary>
    /// Permet d'ajouter un client dans la base de donnée
    /// </summary>
    /// <param name="conn"></param>
    /// <returns>L'ID du client créé</returns>
    public string AjouterClient(MySqlConnection conn)
    {
        string idUtilisateur = new Utilisateur().CreerUtilisateur(conn);
        string idClient = Saisie.Texte("Entrez l'ID du client (ex : CL6) : ");
        while (Existe(idClient, conn))
        {
            Console.WriteLine("Cet ID client existe déjà, choisissez-en un autre.");
            idClient = Saisie.Texte("Entrez l'ID du client (ex : CL6) : ");
        }
        string zoneDeLivraison = Saisie.Texte("Entrez la zone de livraison : ");
        string stationProche = Saisie.Texte("Entrez la station de métro la plus proche : ");
        string nom = Saisie.Texte("Entrez le nom : ");
        string prenom = Saisie.Texte("Entrez le prénom : ");
        string adresse = Saisie.Texte("Entrez l'adresse : ");
        using var cmd = new MySqlCommand("INSERT INTO Client (idClient, ZoneDeLivraison, StationProche, idUtilisateur, adresse, nom, prenom) VALUES (@ID, @Zone, @Station, @Utilisateur, @adresse, @nom, @prenom)", conn);
        cmd.Parameters.AddWithValue("@ID", idClient);
        cmd.Parameters.AddWithValue("@Zone", zoneDeLivraison);
        cmd.Parameters.AddWithValue("@Station", stationProche);
        cmd.Parameters.AddWithValue("@Utilisateur", idUtilisateur);
        cmd.Parameters.AddWithValue("@nom", nom);
        cmd.Parameters.AddWithValue("@prenom", prenom);
        cmd.Parameters.AddWithValue("@adresse", adresse);
        cmd.ExecuteNonQuery();
        Console.WriteLine($"Client ajouté ! Votre ID client est : {idClient} (gardez-le pour vous reconnecter)");
        return idClient;
    }
    /// <summary>
    /// Vérifie qu'un client existe
    /// </summary>
    public bool Existe(string idClient, MySqlConnection conn)
    {
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Client WHERE idClient = @id", conn);
        cmd.Parameters.AddWithValue("@id", idClient);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }
    /// <summary>
    /// Renvoie la station proche du client
    /// </summary>
    public string GetStation(string idClient, MySqlConnection conn)
    {
        using var cmd = new MySqlCommand("SELECT StationProche FROM Client WHERE idClient = @id", conn);
        cmd.Parameters.AddWithValue("@id", idClient);
        return cmd.ExecuteScalar()?.ToString() ?? "";
    }
    /// <summary>
    /// Permet de modifier un client
    /// </summary>
    /// <param name="conn"></param>
    public void ModifierClient(MySqlConnection conn)
    {
        string idClient = Saisie.Texte("Entrez l'ID du client : ");
        if (!Existe(idClient, conn))
        {
            Console.WriteLine("Client introuvable.");
            return;
        }
        ModifierClientParty(conn, idClient);
    }
    /// <summary>
    /// Permet de modifier un client sans changer son id
    /// </summary>
    public void ModifierClientParty(MySqlConnection conn, string IdClient)
    {
        string zoneDeLivraison = Saisie.Texte("Entrez la zone de livraison : ");
        string stationProche = Saisie.Texte("Entrez la station de métro la plus proche : ");
        string adresse = Saisie.Texte("Entrez l'adresse : ");
        using var cmd = new MySqlCommand("UPDATE Client SET ZoneDeLivraison = @zone, StationProche = @station, adresse = @adresse WHERE idClient = @idClient", conn);
        cmd.Parameters.AddWithValue("@idClient", IdClient);
        cmd.Parameters.AddWithValue("@zone", zoneDeLivraison);
        cmd.Parameters.AddWithValue("@station", stationProche);
        cmd.Parameters.AddWithValue("@adresse", adresse);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Client modifié");
    }
    /// <summary>
    /// Permet de supprimer un client (avec ses commandes et son compte utilisateur)
    /// </summary>
    /// <param name="conn"></param>
    public void SupprimerClient(MySqlConnection conn)
    {
        string idClient = Saisie.Texte("Entrez l'ID du client : ");
        if (!Existe(idClient, conn))
        {
            Console.WriteLine("Client introuvable.");
            return;
        }
        if (!Saisie.OuiNon($"Supprimer le client {idClient} ainsi que ses commandes et avis ?")) return;

        string idUtilisateur;
        using (var cmd = new MySqlCommand("SELECT idUtilisateur FROM Client WHERE idClient = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", idClient);
            idUtilisateur = cmd.ExecuteScalar()?.ToString() ?? "";
        }
        foreach (string table in new[] { "Commande", "Avis", "Particulier", "Entreprise", "Client" })
        {
            using var cmd = new MySqlCommand($"DELETE FROM {table} WHERE idClient = @id", conn);
            cmd.Parameters.AddWithValue("@id", idClient);
            cmd.ExecuteNonQuery();
        }
        new Utilisateur().SupprimerUtilisateur(idUtilisateur, conn);
        Console.WriteLine("Client supprimé");
    }
    /// <summary>
    /// Permet d'afficher les clients en ordre alphabetique
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherClientsAlphabetique(MySqlConnection conn)
    {
        using var cmd = new MySqlCommand("SELECT * FROM Client ORDER BY nom, prenom", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"ID: {reader["idClient"]}, Nom : {reader["nom"]} {reader["prenom"]}, Zone: {reader["ZoneDeLivraison"]}, Station: {reader["StationProche"]}");
        }
    }
    /// <summary>
    /// Permet d'afficher les clients en les regroupant par nom de rue
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherClientsRue(MySqlConnection conn)
    {
        using var cmd = new MySqlCommand("SELECT DISTINCT adresse, nom, prenom FROM Client ORDER BY adresse ASC", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"{reader["adresse"]} : {reader["nom"]} {reader["prenom"]}");
        }
    }
    /// <summary>
    /// Permet d'affiche le total d'argent depensé en commande en ordre descendant des clients
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherTotalCommandesParClient(MySqlConnection conn)
    {
        string query = "SELECT idClient, SUM(prix) AS Total FROM Commande GROUP BY idClient ORDER BY Total DESC";
        using var cmd = new MySqlCommand(query, conn);
        using var reader = cmd.ExecuteReader();
        Console.WriteLine("Liste des clients avec leur total de commandes:");
        while (reader.Read())
        {
            float total = reader["Total"] != DBNull.Value ? Convert.ToSingle(reader["Total"]) : 0;
            Console.WriteLine($"Client ID: {reader["idClient"]}, Total des commandes: {total} €");
        }
    }
}
