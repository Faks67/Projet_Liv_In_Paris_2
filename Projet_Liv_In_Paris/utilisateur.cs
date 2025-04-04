using System;
using MySql.Data.MySqlClient;

public class UtilisateurManager
{
    public void AjouterUtilisateur(string idUtilisateur, string motDePasse)
    {
        using (var conn = Database.GetConnection())
        {
            conn.Open();
            var cmd = new MySqlCommand("INSERT INTO Utilisateur (idUtilisateur, MotDePasse) VALUES (@ID, @MDP)", conn);
            cmd.Parameters.AddWithValue("@ID", idUtilisateur);
            cmd.Parameters.AddWithValue("@MDP", motDePasse);
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("Utilisateur ajouté !");
    }

    public void AfficherUtilisateurs()
    {
        using (var conn = Database.GetConnection())
        {
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM Utilisateur", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["idUtilisateur"]}, Mot de passe: {reader["MotDePasse"]}");
            }
        }
    }
}