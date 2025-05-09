using System;
using MySql.Data.MySqlClient;

public class Client
{
    /// <summary>
    /// Permet d'ajouter un client a la base de donnée client
    /// </summary>
    /// <param name="conn"></param>
    public void AjouterClient(MySqlConnection conn)
    {
        Console.Write("Entrez l'ID de l'utilisateur : ");
        string idUtilisateur = Console.ReadLine();
        Console.Write("Entrez le mot de passe de l'utilisateur : ");
        string mdp = Console.ReadLine();
        Utilisateur user = new Utilisateur();
        user.AjouterUtilisateur(idUtilisateur, mdp, conn);
        Console.Write("Entrez l'ID du client : ");
        string idClient = Console.ReadLine();
        Console.Write("Entrez la zone de livraison : ");
        string zoneDeLivraison = Console.ReadLine();
        Console.Write("Entrez la station proche : ");
        string stationProche = Console.ReadLine();
        Console.Write("Entrez l'ID de la transaction : ");
        string idTransaction = Console.ReadLine();
        Console.Write("Entrez le nom : ");
        string nom = Console.ReadLine();
        Console.Write("Entrez le prénom : ");
        string prenom = Console.ReadLine();
        Console.Write("Entrez l'adresse : ");
        string adresse = Console.ReadLine();
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
    /// <summary>
    /// Permet de modifier un client
    /// </summary>
    /// <param name="conn"></param>
    public void ModifierClient(MySqlConnection conn)
    {
        Console.Write("Entrez l'ID du client : ");
        string IdClient = Console.ReadLine();
        Console.Write("Entrez la zone de livraison : ");
        string ZoneDeLivraison = Console.ReadLine();
        Console.Write("Entrez la station proche : ");
        string StationProche = Console.ReadLine();
        Console.Write("Entrez l'ID de la transaction : ");
        int IdTransaction = int.Parse(Console.ReadLine());
        var cmd  = new MySqlCommand("UPDATE Client SET ZoneDeLivraison = @zone, StationProche = @station WHERE idClient = @idClient", conn);
        cmd.Parameters.AddWithValue("@idClient", IdClient);
        cmd.Parameters.AddWithValue("@zone", ZoneDeLivraison);
        cmd.Parameters.AddWithValue("@station", StationProche);
        cmd.Parameters.AddWithValue("@idTransaction", IdTransaction);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Client modifié");
    }
    public void ModifierClientParty(MySqlConnection conn, string IdClient)
    {
        Console.Write("Entrez la zone de livraison : ");
        string ZoneDeLivraison = Console.ReadLine();
        Console.Write("Entrez la station proche : ");
        string StationProche = Console.ReadLine();
        Console.Write("Entrez l'ID de la transaction : ");
        int IdTransaction = int.Parse(Console.ReadLine());
        var cmd  = new MySqlCommand("UPDATE Client SET ZoneDeLivraison = @zone, StationProche = @station WHERE idClient = @idClient", conn);
        cmd.Parameters.AddWithValue("@idClient", IdClient);
        cmd.Parameters.AddWithValue("@zone", ZoneDeLivraison);
        cmd.Parameters.AddWithValue("@station", StationProche);
        cmd.Parameters.AddWithValue("@idTransaction", IdTransaction);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Client modifié");
    }
    /// <summary>
    /// Permet de supprimer un client
    /// </summary>
    /// <param name="conn"></param>
    public void SupprimerClient(MySqlConnection conn)
    {
        Console.Write("Entrez l'ID du client : ");
        string idClient = Console.ReadLine();
        var cmd = new MySqlCommand("DELETE FROM Client WHERE idClient = @idClient", conn);
        cmd.Parameters.AddWithValue("@idClient", idClient);
        cmd.ExecuteNonQuery();
    }
    /// <summary>
    /// Permet d'afficher les clients en ordre alphabetique
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherClientsAlphabetique(MySqlConnection conn)
    {

            var cmd = new MySqlCommand("SELECT * FROM Client ORDER BY nom", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["idClient"]}, Nom : {reader["nom"]}, Zone: {reader["ZoneDeLivraison"]}, Station: {reader["StationProche"]}");
            }
    }
    /// <summary>
    /// Permet d'afficher les clients en les regroupant par nom de rue
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherClientsRue(MySqlConnection conn)
    {

            var cmd = new MySqlCommand("SELECT Distinct adresse ,nom  FROM Client ORDER BY adresse ASC", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader["adresse"]} {reader["nom"]}");
            }
        
    }
    /// <summary>
    /// Permet d'affiche le total d'argent depensé en commande en ordre descendant des clients
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherTotalCommandesParClient(MySqlConnection conn)
    {
            string query = "SELECT idClient, SUM(prix) AS Total FROM Commande GROUP BY idClient ORDER BY Total DESC";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                Console.WriteLine("Liste des clients avec leur total de commandes:");
                while (reader.Read())
                {
                    string idClient = reader["idClient"].ToString();
                    float total = reader["Total"] != DBNull.Value ? Convert.ToSingle(reader["Total"]) : 0;
                    Console.WriteLine($"Client ID: {idClient}, Total des commandes: {total} ");
                }
            }
    }
}