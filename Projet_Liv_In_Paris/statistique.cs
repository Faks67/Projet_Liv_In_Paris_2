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
        using var cmd = new MySqlCommand("SELECT idCuisinier, COUNT(*) AS NbLivraisons FROM Commande GROUP BY idCuisinier ORDER BY NbLivraisons DESC", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"Cuisinier {reader["idCuisinier"]} a fait {reader["NbLivraisons"]} livraisons");
        }
    }
    /// <summary>
    /// Permet d'affiche le prix moyen des commandes
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherMoyennePrixCommandes(MySqlConnection conn)
    {
        using var cmd = new MySqlCommand("SELECT AVG(prix) FROM Commande", conn);
        object moyenne = cmd.ExecuteScalar();
        string texte = moyenne == DBNull.Value ? "aucune commande" : $"{Convert.ToDouble(moyenne):F2} €";
        Console.WriteLine($"Moyenne des prix des commandes : {texte}");
    }
    /// <summary>
    /// Permet d'afficher toutes les commandes realisé durant une periode determiné
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherCommandesParPeriode(MySqlConnection conn)
    {
        DateTime dateDebut = Saisie.Date("Entrez la date de début (aaaa-mm-jj) : ");
        DateTime dateFin = Saisie.Date("Entrez la date de fin (aaaa-mm-jj) : ");
        using var cmd = new MySqlCommand("SELECT * FROM Commande WHERE dateCo >= @dateDebut AND dateCo < @dateFin ORDER BY dateCo", conn);
        cmd.Parameters.AddWithValue("@dateDebut", dateDebut);
        cmd.Parameters.AddWithValue("@dateFin", dateFin.AddDays(1));
        using var reader = cmd.ExecuteReader();
        Console.WriteLine($"Commandes entre le {dateDebut:dd/MM/yyyy} et le {dateFin:dd/MM/yyyy} :");
        if (!reader.HasRows) Console.WriteLine("Aucune commande sur cette période.");
        while (reader.Read())
        {
            Console.WriteLine($"Commande {reader["idCommande"]}: {reader["statut"]}, Prix: {reader["prix"]} €, Départ: {reader["depart"]}, Arrivée: {reader["arrive"]}, Date: {reader["dateCo"]}");
        }
    }
    /// <summary>
    /// Permet d'afficher les commandes en fonction du client et de la nationalité du plat realisé
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherCommandesClientNationalite(MySqlConnection conn)
    {
        string idClient = Saisie.Texte("Entrez l'ID du client : ");
        string query = "SELECT co.idCommande, co.statut, co.prix, co.idClient, pl.TypePlat, pl.Nationalité FROM Commande co JOIN Plat pl ON co.IdPlat = pl.IdPlat WHERE co.idClient = @idClient ORDER BY pl.Nationalité ASC;";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@idClient", idClient);
        using var reader = cmd.ExecuteReader();
        if (!reader.HasRows) Console.WriteLine("Aucune commande pour ce client.");
        while (reader.Read())
        {
            Console.WriteLine($"Commande {reader["idCommande"]} Statut : {reader["statut"]}, Plat : {reader["TypePlat"]}, Prix: {reader["prix"]} €, Nationalité : {reader["Nationalité"]}");
        }
    }
    /// <summary>
    /// Permet d'afficher les commandes par client et par période
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherCommandesClientPeriode(MySqlConnection conn)
    {
        string idClient = Saisie.Texte("Entrez l'ID du client : ");
        DateTime dateDebut = Saisie.Date("Entrez la date de début (aaaa-mm-jj) : ");
        DateTime dateFin = Saisie.Date("Entrez la date de fin (aaaa-mm-jj) : ");
        string query = "SELECT * FROM Commande WHERE idClient = @idClient AND dateCo >= @dateDebut AND dateCo < @dateFin ORDER BY dateCo";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@idClient", idClient);
        cmd.Parameters.AddWithValue("@dateDebut", dateDebut);
        cmd.Parameters.AddWithValue("@dateFin", dateFin.AddDays(1));
        using var reader = cmd.ExecuteReader();
        if (!reader.HasRows) Console.WriteLine("Aucune commande pour ce client sur cette période.");
        while (reader.Read())
        {
            Console.WriteLine($"Commande {reader["idCommande"]}: {reader["statut"]}, Prix: {reader["prix"]} €, Départ: {reader["depart"]}, Arrivée: {reader["arrive"]}, Date: {reader["dateCo"]}");
        }
    }
    public void AfficherMoyenneClients(MySqlConnection conn)
    {
        string query = "SELECT AVG(TotalDepense) AS MoyenneDep FROM (SELECT SUM(prix) AS TotalDepense FROM Commande GROUP BY idClient) AS DepensesParClient;";
        using var cmd = new MySqlCommand(query, conn);
        object moyenne = cmd.ExecuteScalar();
        string texte = moyenne == DBNull.Value ? "aucune commande" : $"{Convert.ToDouble(moyenne):F2} €";
        Console.WriteLine("Moyenne des dépenses par client : " + texte);
    }
}
