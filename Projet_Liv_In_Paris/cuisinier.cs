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
}