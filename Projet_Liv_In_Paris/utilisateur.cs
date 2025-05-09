using System;
using MySql.Data.MySqlClient;

public class Utilisateur
{
    /// <summary>
    /// Crée un utilisateur
    /// </summary>
    /// <param name="idUtilisateur"></param>
    /// <param name="motDePasse"></param>
    public void AjouterUtilisateur(string idUtilisateur, string motDePasse, MySqlConnection conn)
    {
       
            if (IsUserNotInTable(idUtilisateur, conn))
            {
                var cmd = new MySqlCommand("INSERT INTO Utilisateur (idUtilisateur, MotDePasse) VALUES (@Id, @MDP)", conn);
                cmd.Parameters.AddWithValue("@Id", idUtilisateur);
                cmd.Parameters.AddWithValue("@MDP", motDePasse);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Utilisateur ajouté !");
            }
            else{ Console.WriteLine("Utilisateur deja present");}
    }
    public bool IsUserNotInTable(string iduser, MySqlConnection conn)
    {
        bool notExists = false;
                string query = $"SELECT COUNT(*) FROM Utilisateur WHERE idUtilisateur = @userId;";
                using (MySqlCommand command = new MySqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@userId", iduser);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    notExists = (count == 0);
                }
            return notExists;
    }
    /// <summary>
    /// Affiche l'ensemble des utilisateurs
    /// </summary>
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