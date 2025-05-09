using System;
using MySql.Data.MySqlClient;

public class Commande
{
    /// <summary>
    /// Permet de créer une commande
    /// </summary>
    /// <param name="conn"></param>
    public void CreerCommande(MySqlConnection conn)
        {
            Console.Write("Entrez l'ID de la commande : ");
            string idCommande = Console.ReadLine();
            Console.Write("Entrez le statut de la commande : ");
            string statut = Console.ReadLine();
            Console.Write("Entrez l'ID du client : ");
            string idClient = Console.ReadLine();
            Console.Write("Entrez l'ID du cuisinier : ");
            string idCuisinier = Console.ReadLine();
            Console.Write("Entrez le prix : ");
            float prix = float.Parse(Console.ReadLine());
            Console.Write("Entrez le point de départ : ");
            string depart = Console.ReadLine();
            Console.Write("Entrez le point d'arrivée : ");
            string arrive = Console.ReadLine();
            string query = "INSERT INTO Commande (idCommande, statut, idClient,idCuisinier, prix, depart, arrive, dateCo) VALUES (@idCommande, @statut, @idClient, @idCuisinier, @prix, @depart, @arrive, NOW())";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@idCommande", idCommande);
            cmd.Parameters.AddWithValue("@statut", statut);
            cmd.Parameters.AddWithValue("@idClient", idClient);
            cmd.Parameters.AddWithValue("@idCuisinier", idCuisinier);
            cmd.Parameters.AddWithValue("@prix", prix);
            cmd.Parameters.AddWithValue("@depart", depart);
            cmd.Parameters.AddWithValue("@arrive", arrive);
            cmd.ExecuteNonQuery();               
        }   
        /// <summary>
        /// Permet d'afficher les commandes
        /// </summary>
        /// <param name="conn"></param>
    public void AfficheCommande(MySqlConnection conn)
    {
        var cmd = new MySqlCommand("SELECT *  FROM Commande ORDER BY adresse ASC", conn);
        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"{reader["idCommande"]}, {reader["prix"]}, {reader["dateCo"]}");
        }  
    } 
    /// <summary>
    /// Permet d'afficher une certain commande grace à son numéro de commande
    /// </summary>
    /// <param name="conn"></param>
    public void AfficheCommandeParty(MySqlConnection conn, string id)
    {
        var cmd = new MySqlCommand("SELECT *  FROM Commande WHERE idCommande = @idCo ", conn);
        cmd.Parameters.AddWithValue("@idCo", id);
        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"{reader["idCommande"]}, {reader["prix"]}, {reader["dateCo"]}");
        }  
    }
    public void AfficheCommandePrix(MySqlConnection conn)
    {
        Console.WriteLine("De quelle commande souhiatez vous avoir le prix");
        string id = Console.ReadLine();
        var cmd = new MySqlCommand("SELECT *  FROM Commande WHERE idCommande = @idCo ORDER BY adresse ASC", conn);
        cmd.Parameters.AddWithValue("@idCo", id);
        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"{reader["idCommande"]}, {reader["prix"]}, {reader["dateCo"]}");
        }  
    }  
}