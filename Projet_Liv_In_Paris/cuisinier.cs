using System;
using System.Data;
using MySql.Data.MySqlClient;

public class Cuisinier
{
    /// <summary>
    /// Permet d'ajouter un cuisiner dans la base de donnée
    /// </summary>
    /// <param name="conn"></param>
    public void AjouterCuisinier(MySqlConnection conn)
    {
        Console.Write("Entrez l'ID du cuisinier : ");
        int idCuisinier = int.Parse(Console.ReadLine());
        Console.Write("Entrez l'ID de l'utilisateur : ");
        string idUtilisateur = Console.ReadLine();
        Console.Write("Entrez le mot de passe de l'utilisateur : ");
        string mdp = Console.ReadLine();
        Utilisateur user = new Utilisateur();
        user.AjouterUtilisateur(idUtilisateur, mdp, conn);
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
        var cmd = new MySqlCommand("INSERT INTO Cuisinier (idCuisinier,idUtilisateur, nom, prenom, addMail, adresse, CodePostal , numTel, station) VALUES (@ID, @IDuti, @nom, @prenom, @addMail, @adresse, @CodePostal, @numtel, @station)", conn);
        cmd.Parameters.AddWithValue("@ID", idCuisinier);
        cmd.Parameters.AddWithValue("@IDuti", idUtilisateur);
        cmd.Parameters.AddWithValue("@nom", nom);
        cmd.Parameters.AddWithValue("@prenom", prenom);
        cmd.Parameters.AddWithValue("@addMail", addMail);
        cmd.Parameters.AddWithValue("@adresse", adresse);
        cmd.Parameters.AddWithValue("@CodePostal", CodePostal);
        cmd.Parameters.AddWithValue("@numTel", numTel);
        cmd.Parameters.AddWithValue("@station", station);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Cuisinier ajouté");
    }
    /// <summary>
    /// Permet de modifier un cuisinier
    /// </summary>
    /// <param name="conn"></param>
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
        var cmd = new MySqlCommand("Update Cuisinier SET nom = @nom, prenom = @prenom, addMail = @addMail, adresse = @adresse, CodePostal = @CodePostal, numTel = @numTel, station = @station WHERE idCuisinier = @ID", conn);
        cmd.Parameters.AddWithValue("@ID", idCuisinier);
        cmd.Parameters.AddWithValue("@nom", nom);
        cmd.Parameters.AddWithValue("@prenom", prenom);
        cmd.Parameters.AddWithValue("@addMail", addMail);
        cmd.Parameters.AddWithValue("@adresse", adresse);
        cmd.Parameters.AddWithValue("@CodePostal", CodePostal);
        cmd.Parameters.AddWithValue("@numTel", numTel);
        cmd.Parameters.AddWithValue("@station", station);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Cuisinier modifié");
    }
    public void ModifierCuisinierParty(MySqlConnection conn, string idCuisinier)
    {
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
        var cmd = new MySqlCommand("Update Cuisinier SET nom = @nom, prenom = @prenom, addMail = @addMail, adresse = @adresse, CodePostal = @CodePostal, numTel = @numTel, station = @station WHERE idCuisinier = @ID", conn);
        cmd.Parameters.AddWithValue("@ID", idCuisinier);
        cmd.Parameters.AddWithValue("@nom", nom);
        cmd.Parameters.AddWithValue("@prenom", prenom);
        cmd.Parameters.AddWithValue("@addMail", addMail);
        cmd.Parameters.AddWithValue("@adresse", adresse);
        cmd.Parameters.AddWithValue("@CodePostal", CodePostal);
        cmd.Parameters.AddWithValue("@numTel", numTel);
        cmd.Parameters.AddWithValue("@station", station);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Cuisinier modifié");
    }
    /// <summary>
    /// Permet de supprimer un cuisinier
    /// </summary>
    /// <param name="conn"></param>
    public void SupprimerCuisinier(MySqlConnection conn)
    {
        Console.Write("Entrez l'ID du cuisinier : ");
        int idCuisinier = int.Parse(Console.ReadLine());
        var cmd = new MySqlCommand("DELETE FROM Cuisinier WHERE idCuisinier = @idCuisinier");
        cmd.Parameters.AddWithValue("@idCuisinier", idCuisinier);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Cuisinier Supprimé");
    }
    /// <summary>
    /// Affiche l'ensemble des clients servis par un cuisinier
    /// </summary>
    /// <param name="conn"></param>
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
    /// <summary>
    /// Donne tous les plats realisé par un cuisinier, durant une periode de temps ou depuis la creation du compte
    /// </summary>
    /// <param name="conn"></param>
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
    /// <summary>
    /// Trouve le plat le plus recent du cuisinier s'il existe
    /// </summary>
    /// <param name="conn"></param>
    public void platRecent(MySqlConnection conn)
    {
        Console.Write("Entrez l'ID du cuisinier : ");
        string idCuisinier = Console.ReadLine();

        string query = "SELECT p.nomPlat, p.DateFabrication " +
                    "FROM Commande c " +
                    "JOIN Plat p ON c.idPlat = p.idPlat " +
                    "WHERE c.idCuisinier = @idCuisinier " +
                    "ORDER BY p.DateFabrication DESC " +
                    "LIMIT 1";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@idCuisinier", idCuisinier);

        using var reader = cmd.ExecuteReader();
        if (reader.Read()) // S'il y a un résultat
        {
            Console.WriteLine($"Le plat le plus récent préparé par le cuisinier {idCuisinier} est :");
            Console.WriteLine($"Nom : {reader["nomPlat"]}");
            Console.WriteLine($"Date de fabrication : {reader["DateFabrication"]}");
        }
        else
        {
            Console.WriteLine("Aucun plat trouvé pour ce cuisinier.");
        }
    }
    public void AfficherCuisiniersAlphabetique(MySqlConnection conn)
    {
            var cmd = new MySqlCommand("SELECT * FROM Cuisinier", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["idCuisinier"]}, Nom : {reader["nom"]}, Adresse: {reader["adresse"]}");
            }
    }
}
