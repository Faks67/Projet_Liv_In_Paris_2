using System;
using MySql.Data.MySqlClient;

public class statistique
{
    /// <summary>
    /// Permet d'afficher le nombre de livraison effectué par un cuisinier
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherNombreLivraisonsParCuisinier(MySqlConnection conn)
    {
            var cmd = new MySqlCommand("SELECT CuisinierID, COUNT(*) AS NbLivraisons FROM Commandes GROUP BY CuisinierID", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"Cuisinier {reader["CuisinierID"]} a fait {reader["NbLivraisons"]} livraisons");
            }
    }
    /// <summary>
    /// Permet d'affiche le prix moyen des commandes en cours
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherMoyennePrixCommandes(MySqlConnection conn)
    {
            var cmd = new MySqlCommand("SELECT AVG(Prix) FROM Commandes", conn);
            object moyenne = cmd.ExecuteScalar();
            Console.WriteLine($"Moyenne des prix des commandes : {moyenne}€");
    }
    /// <summary>
    /// Permet d'afficher toutes les commandes realisé durant une periode determiné
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherCommandesParPeriode(MySqlConnection conn)
        {
            Console.Write("Entrez la date de début (yyyy-MM-dd) : ");
            DateTime dateDebut = DateTime.Parse(Console.ReadLine());
            Console.Write("Entrez la date de fin (yyyy-MM-dd) : ");
            DateTime dateFin = DateTime.Parse(Console.ReadLine());
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
        /// <summary>
        /// Permet d'afficher les commandes en fonction du client et de la nationalité du plat realisé
        /// </summary>
        /// <param name="conn"></param>
        public void AfficherCommandesClientNationalite(MySqlConnection conn)
        {
            Console.Write("Entrez l'ID du client : ");
            string idClient = Console.ReadLine();
            Console.Write("Entrez la nationalité : ");
            string nationalite = Console.ReadLine();
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
        /// <summary>
        /// Permet d'afficher les commandes par client et par période
        /// </summary>
        /// <param name="conn"></param>
        public void AfficherCommandesClientPeriode(MySqlConnection conn)
        {
            Console.Write("Entrez l'ID du client : ");
            string idClient = Console.ReadLine();
            Console.Write("Entrez la date de début (yyyy-MM-dd) : ");
            DateTime dateDebut = DateTime.Parse(Console.ReadLine());
            Console.Write("Entrez la date de fin (yyyy-MM-dd) : ");
            DateTime dateFin = DateTime.Parse(Console.ReadLine());
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