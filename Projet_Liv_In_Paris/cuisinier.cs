using System;
using System.Data;
using MySql.Data.MySqlClient;

public class Cuisinier
{
    public void AjouterCuisinier(MySqlConnection conn)
    {
        Console.Write("Entrez l'ID du cuisinier : ");
        int idCuisinier = int.Parse(Console.ReadLine());
        Console.Write("Entrez l'ID de l'utilisateur : ");
        int idUtilisateur = int.Parse(Console.ReadLine());
        Console.Write("Entrez le nom : ");
        string nom = Console.ReadLine();
        Console.Write("Entrez le prénom : ");
        string prenom = Console.ReadLine();
        Console.Write("Entrez l'adresse e-mail : ");
        string addMail = Console.ReadLine();
        Console.Write("Entrez l'adresse : ");
        string adresse = Console.ReadLine();
        Console.Write("Entrez le code postal : ");
        int CodePostal = int.Parse(Console.ReadLine());
        Console.Write("Entrez le numéro de téléphone : ");
        int numTel = int.Parse(Console.ReadLine());
        Console.Write("Entrez la station : ");
        string station = Console.ReadLine();
        var cmd = new MySqlCommand("INSERT INTO Client (idClient,idUtilisateur, nom, prenom, addMail, adresse, CodePostal , numTel, station) VALUES (@ID, @IDuti, @nom, @prenom, @addMail, @adresse, @CodePostal, @numtel, @station)", conn);
        cmd.Parameters.AddWithValue("@ID", idCuisinier);
        cmd.Parameters.AddWithValue("@IDuti", idUtilisateur);
        cmd.Parameters.AddWithValue("@nom", nom);
        cmd.Parameters.AddWithValue("@prenom", prenom);
        cmd.Parameters.AddWithValue("@addMail", addMail);
        cmd.Parameters.AddWithValue("@adresse", adresse);
        cmd.Parameters.AddWithValue("@CodePostal", CodePostal);
        cmd.Parameters.AddWithValue("@numTel", numTel);
        cmd.Parameters.AddWithValue("@station", station);
    }
    public void ModifierCuisinier(MySqlConnection conn)
    {
        Console.Write("Entrez l'ID du cuisinier : ");
        int idCuisinier = int.Parse(Console.ReadLine());
        Console.Write("Entrez l'ID de l'utilisateur : ");
        int idUtilisateur = int.Parse(Console.ReadLine());
        Console.Write("Entrez le nom : ");
        string nom = Console.ReadLine();
        Console.Write("Entrez le prénom : ");
        string prenom = Console.ReadLine();
        Console.Write("Entrez l'adresse e-mail : ");
        string addMail = Console.ReadLine();
        Console.Write("Entrez l'adresse : ");
        string adresse = Console.ReadLine();
        Console.Write("Entrez le code postal : ");
        int CodePostal = int.Parse(Console.ReadLine());
        Console.Write("Entrez le numéro de téléphone : ");
        int numTel = int.Parse(Console.ReadLine());
        Console.Write("Entrez la station : ");
        string station = Console.ReadLine();
        var cmd = new MySqlCommand("Update Cuisinier SET nom = @nom, prenom = @prenom, addMail = @addMail, adresse = @adresse, CodePostal = @CodePostal, numTel = @numTel, station = @station WHERE idCuisinier = @idCuisinier", conn);
        cmd.Parameters.AddWithValue("@ID", idCuisinier);
        cmd.Parameters.AddWithValue("@nom", nom);
        cmd.Parameters.AddWithValue("@prenom", prenom);
        cmd.Parameters.AddWithValue("@addMail", addMail);
        cmd.Parameters.AddWithValue("@adresse", adresse);
        cmd.Parameters.AddWithValue("@CodePostal", CodePostal);
        cmd.Parameters.AddWithValue("@numTel", numTel);
        cmd.Parameters.AddWithValue("@station", station);
        Console.WriteLine("Cuisinier modifié");
    }
    public void SupprimerCuisinier(MySqlConnection conn)
    {
        Console.Write("Entrez l'ID du cuisinier : ");
        int idCuisinier = int.Parse(Console.ReadLine());
        var cmd = new MySqlCommand("DELETE FROM Cuisinier WHERE idCuisinier = @idCuisinier");
        cmd.Parameters.AddWithValue("@idCuisinier", idCuisinier);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Cuisinier Supprimé");
    }
    public void AfficherCuisinier(MySqlConnection conn)
    {
            var cmd = new MySqlCommand("SELECT * FROM Client ORDER BY nom ASC", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["idClient"]}, Zone: {reader["ZoneDeLivraison"]}, Station: {reader["StationProche"]}");
            }
    }
    public void AfficherClient(MySqlConnection conn)
    {
        Console.Write("Entrez l'ID du cuisinier : ");
        string idCuisinier = Console.ReadLine();
        Console.Write("Voulez-vous afficher les clients depuis l'inscription (1) ou pendant une période spécifique (2) ? ");
        string choix = Console.ReadLine();
        DateTime? startDate = null;
        DateTime? endDate = null;
        string query = "SELECT DISTINCT c.idClient, c.nom, c.prenom, co.dateCommande " +
                   "FROM Client c " +
                   "JOIN Commande co ON c.idClient = co.idClient " +
                   "JOIN Cuisinier cu ON co.idCuisinier = cu.idCuisinier " +
                   "WHERE cu.idCuisinier = @idCuisinier";
        if (choix == "2")
        {
        Console.Write("Entrez la date de début (yyyy-MM-dd) : ");
        startDate = DateTime.Parse(Console.ReadLine());
        Console.Write("Entrez la date de fin (yyyy-MM-dd) : ");
        endDate = DateTime.Parse(Console.ReadLine());
        query += " AND co.dateCommande BETWEEN @startDate AND @endDate";
        }
        using var cmd = new MySqlCommand(query, conn);
        if (startDate.HasValue) cmd.Parameters.AddWithValue("@startDate", startDate.Value);
        if (endDate.HasValue) cmd.Parameters.AddWithValue("@endDate", endDate.Value);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"Client ID: {reader["idClient"]}, Nom: {reader["nom"]} {reader["prenom"]}, Date: {reader["dateCommande"]}");
        }
    }
    public void platRealiser(MySqlConnection conn)
    {
        Console.Write("Entrez l'ID du cuisinier : ");
        string idCuisinier = Console.ReadLine();
        string query = "SELECT p.nomPlat, COUNT(*) as frequence " +
                    "FROM Commande c " +
                    "JOIN Plat p ON c.idPlat = p.idPlat " +
                    "WHERE c.idCuisinier = @idCuisinier " +
                    "GROUP BY p.nomPlat " +
                    "ORDER BY frequence DESC";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@idCuisinier", idCuisinier);

        using var reader = cmd.ExecuteReader();
        if (!reader.HasRows)
        {
            Console.WriteLine("Aucun plat trouvé pour ce cuisinier.");
            return;
        }

        Console.WriteLine("Plats réalisés par fréquence :");
        while (reader.Read())
        {
            Console.WriteLine($"Plat: {reader["nomPlat"]}, Nombre de fois préparé: {reader["frequence"]}");
        }
    }

}
