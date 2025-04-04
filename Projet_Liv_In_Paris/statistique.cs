using System;
using MySql.Data.MySqlClient;

public class statistique
{
        public void AfficherNombreLivraisonsParCuisinier(MySqlConnection conn)
    {
            var cmd = new MySqlCommand("SELECT CuisinierID, COUNT(*) AS NbLivraisons FROM Commandes GROUP BY CuisinierID", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"Cuisinier {reader["CuisinierID"]} a fait {reader["NbLivraisons"]} livraisons");
            }
    }
    public void AfficherMoyennePrixCommandes(MySqlConnection conn)
    {
            var cmd = new MySqlCommand("SELECT AVG(Prix) FROM Commandes", conn);
            object moyenne = cmd.ExecuteScalar();
            Console.WriteLine($"Moyenne des prix des commandes : {moyenne}€");
    }
    public void AfficherCommandesParPeriode(DateTime dateDebut, DateTime dateFin, MySqlConnection conn)
        {
                string query = "SELECT * FROM Commande WHERE dateCo BETWEEN @dateDebut AND @dateFin";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@dateDebut", dateDebut);
                    cmd.Parameters.AddWithValue("@dateFin", dateFin);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("Commandes entre " + dateDebut + " et " + dateFin + ":");
                        while (reader.Read())
                        {
                            Console.WriteLine($"Commande {reader["idCommande"]}: {reader["statut"]}, Prix: {reader["prix"]} €, Départ: {reader["depart"]}, Arrivée: {reader["arrive"]}, Date: {reader["dateCo"]}");
                        }
                    }
                }
        }
        public void AfficherCommandesClientNationalite(string idClient, string nationalite, MySqlConnection conn)
        {
                string query = "SELECT * FROM Commande WHERE idClient = @idClient AND nationalite = @nationalite";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idClient", idClient);
                    cmd.Parameters.AddWithValue("@nationalite", nationalite);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Commande {reader["idCommande"]}: {reader["statut"]}, Prix: {reader["prix"]} €, Départ: {reader["depart"]}, Arrivée: {reader["arrive"]}, Date: {reader["dateCo"]}");
                        }
                    }
                }
            }
        public void AfficherCommandesClientPeriode(string idClient,  DateTime dateDebut, DateTime dateFin, MySqlConnection conn)
        {
                string query = "SELECT * FROM Commande WHERE idClient = @idClient AND dateCo BETWEEN @dateDebut AND @dateFin";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idClient", idClient);
                    cmd.Parameters.AddWithValue("@dateDebut", dateDebut);
                    cmd.Parameters.AddWithValue("@dateFin", dateFin);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Commande {reader["idCommande"]}: {reader["statut"]}, Prix: {reader["prix"]} €, Départ: {reader["depart"]}, Arrivée: {reader["arrive"]}, Date: {reader["dateCo"]}");
                        }
                    }
                }
            }
            public void AfficherMoyenneClients(MySqlConnection conn)
        {
                string query = "SELECT AVG(SUM(prix)) AS MoyenneDep FROM Commande GROUP BY idClient";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine("Moyenne des comptes clients: " + reader["MoyenneDep"] + " €");
                    }
                }
            
        }
}