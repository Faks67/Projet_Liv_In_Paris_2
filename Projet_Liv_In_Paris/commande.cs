using System;
using MySql.Data.MySqlClient;

public class Commande
{
    public void CreerCommande(int idCommande, string statut, string idClient, float prix, string depart, string arrive, MySqlConnection conn)
        {
                string query = "INSERT INTO Commande (idCommande, statut, idClient, prix, depart, arrive, dateCo) VALUES (@idCommande, @statut, @idClient, @prix, @depart, @arrive, NOW())";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@idCommande", idCommande);
                cmd.Parameters.AddWithValue("@statut", statut);
                cmd.Parameters.AddWithValue("@idClient", idClient);
                cmd.Parameters.AddWithValue("@prix", prix);
                cmd.Parameters.AddWithValue("@depart", depart);
                cmd.Parameters.AddWithValue("@arrive", arrive);
                cmd.ExecuteNonQuery();
                
        }   
    public void AfficheCommande(MySqlConnection conn)
    {
        var cmd = new MySqlCommand("SELECT *  FROM Commande ORDER BY adresse ASC", conn);
        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"{reader["idCommande"]}, {reader["prix"]}, {reader["dateCo"]}");
        }  
    } 
     
}