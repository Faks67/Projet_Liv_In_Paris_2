using System;
using System.Data;
using MySql.Data.MySqlClient;

public class CuisinierManager
{
    public void AjouterCuisinier(int idCuisinier, int idUtilisateur, string nom, string prenom, string addMail, string adresse, int CodePostal, int numTel, string station, MySqlConnection conn)
    {
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

        Console.WriteLine("Cuisinier ajouté !");
    }
    public void ModifierCuisinier(int idCuisinier, string nom, string prenom, string addMail, string adresse, int CodePostal, int numTel, string station, MySqlConnection conn)
    {
            var cmd = new MySqlCommand("Update Cuisinier SET nom = @nom, prenom = @prenom, addMail = @addMail, adresse = @adresse, CodePostal = @CodePostal, numTel = @numTel, station = @station WHERE idCuisinier = @idCuisinier", conn);
            cmd.Parameters.AddWithValue("@ID", idCuisinier);
            cmd.Parameters.AddWithValue("@nom", nom);
            cmd.Parameters.AddWithValue("@prenom", prenom);
            cmd.Parameters.AddWithValue("@addMail", addMail);
            cmd.Parameters.AddWithValue("@adresse", adresse);
            cmd.Parameters.AddWithValue("@CodePostal", CodePostal);
            cmd.Parameters.AddWithValue("@numTel", numTel);
            cmd.Parameters.AddWithValue("@station", station);
    }
    public void SupprimerCuisinier(int idCuisinier, MySqlConnection conn)
    {
            var cmd = new MySqlCommand("DELETE FROM Cuisinier WHERE idCuisinier = @idCuisinier");
            cmd.Parameters.AddWithValue("@idCuisinier", idCuisinier);
            cmd.ExecuteNonQuery();
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