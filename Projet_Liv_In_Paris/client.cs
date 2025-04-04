using System;
using MySql.Data.MySqlClient;

public class ClientManager
{
    public void AjouterClient(string idClient, string zoneDeLivraison, string stationProche, string idTransaction, string idUtilisateur, string nom, string prenom, string adresse, MySqlConnection conn)
    {
            var cmd = new MySqlCommand("INSERT INTO Client (idClient, ZoneDeLivraison, StationProche, idTransaction, idUtilisateur,adresse, nom, prenom) VALUES (@ID, @Zone, @Station, @Transaction, @Utilisateur, @adresse, @nom, @prenom)", conn);
            cmd.Parameters.AddWithValue("@ID", idClient);
            cmd.Parameters.AddWithValue("@Zone", zoneDeLivraison);
            cmd.Parameters.AddWithValue("@Station", stationProche);
            cmd.Parameters.AddWithValue("@Transaction", idTransaction);
            cmd.Parameters.AddWithValue("@Utilisateur", idUtilisateur);
            cmd.Parameters.AddWithValue("@nom", nom);
            cmd.Parameters.AddWithValue("@prenom", prenom);
            cmd.Parameters.AddWithValue("@adresse", adresse);
            cmd.ExecuteNonQuery();
        Console.WriteLine("Client ajouté !");
    }
    public void ModifierClient(int IdClient, string ZoneDeLivraison, string StationProche, int IdTransaction, MySqlConnection conn)
        {
                var cmd  = new MySqlCommand("UPDATE Client SET ZoneDeLivraison = @zone, StationProche = @station, idTransaction = @idTransaction WHERE idClient = @idClient", conn);
                    cmd.Parameters.AddWithValue("@idClient", IdClient);
                    cmd.Parameters.AddWithValue("@zone", ZoneDeLivraison);
                    cmd.Parameters.AddWithValue("@station", StationProche);
                    cmd.Parameters.AddWithValue("@idTransaction", IdTransaction);
                    cmd.ExecuteNonQuery();
            Console.WriteLine("Client modifié");
        }
        public void SupprimerClient(int idCLient, MySqlConnection conn)
        {

                var cmd = new MySqlCommand("DELETE FROM Client WHERE idClient = @idClient", conn);
                cmd.Parameters.AddWithValue("@idClient", idCLient);
                cmd.ExecuteNonQuery();
        }

    public void AfficherClientsAlphabetique(MySqlConnection conn)
    {

            var cmd = new MySqlCommand("SELECT * FROM Client", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["idClient"]}, Nom : {reader["nom"]}, Zone: {reader["ZoneDeLivraison"]}, Station: {reader["StationProche"]}");
            }
        
    }
    public void AfficherClientsRue(MySqlConnection conn)
    {

            var cmd = new MySqlCommand("SELECT Distinct adresse ,nom  FROM Client ORDER BY adresse ASC", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader["adresse"]} {reader["nom"]}");
            }
        
    }
    public void AfficherTotalCommandesParClient(MySqlConnection conn)
    {
            string query = "SELECT idClient, SUM(prix) AS Total FROM Commande GROUP BY idClient ORDER BY Total DESC";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                Console.WriteLine("Liste des clients avec leur total de commandes:");
                Console.WriteLine("----------------------------------------------");
                while (reader.Read())
                {
                    string idClient = reader["idClient"].ToString();
                    float total = reader["Total"] != DBNull.Value ? Convert.ToSingle(reader["Total"]) : 0;
                    Console.WriteLine($"Client ID: {idClient}, Total des commandes: {total} €");
                }
            }
    }
}