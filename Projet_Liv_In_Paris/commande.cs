using System;
using MySql.Data.MySqlClient;

public class CommandeManager
{
    public void CreerCommande(string clientID, string cuisinierID, float prix, string depart, string arrivee)
    {
        using (var conn = Database.GetConnection())
        {
            conn.Open();
            var cmd = new MySqlCommand("INSERT INTO Commande (IdClient, IdCuisinier, Prix, AdresseDepart, AdresseArrivee, Date) VALUES (@IdClient, @IdCuisinier, @Prix, @Depart, @Arrivee, NOW())", conn);
            cmd.Parameters.AddWithValue("@ClientID", clientID);
            cmd.Parameters.AddWithValue("@CuisinierID", cuisinierID);
            cmd.Parameters.AddWithValue("@Prix", prix);
            cmd.Parameters.AddWithValue("@Depart", depart);
            cmd.Parameters.AddWithValue("@Arrivee", arrivee);
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine("Commande créée !");
    }        
}