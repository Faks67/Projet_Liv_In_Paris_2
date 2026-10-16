using System;
using MySql.Data.MySqlClient;

public class Commande
{
    /// <summary>
    /// Permet de créer une commande : le client choisit un plat, le reste (cuisinier, prix, trajet) est déduit
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="idClient">Client connecté, ou null pour le demander (mode admin)</param>
    public void CreerCommande(MySqlConnection conn, string? idClient = null)
    {
        Client client = new Client();
        if (idClient == null)
        {
            idClient = Saisie.Texte("Entrez l'ID du client : ");
            if (!client.Existe(idClient, conn))
            {
                Console.WriteLine("Client introuvable.");
                return;
            }
        }

        Plat plat = new Plat();
        plat.AfficherPlat(conn);
        string idPlat = Saisie.Texte("\nEntrez l'ID du plat à commander (ex : P1) : ");

        string idCuisinier, nomPlat;
        int portionsDispo;
        float prixPortion;
        using (var cmd = new MySqlCommand("SELECT idCuisinier, TypePlat, Quantité, prix FROM Plat WHERE IdPlat = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", idPlat);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                Console.WriteLine("Ce plat n'existe pas.");
                return;
            }
            idCuisinier = reader["idCuisinier"].ToString()!;
            nomPlat = reader["TypePlat"].ToString()!;
            portionsDispo = Convert.ToInt32(reader["Quantité"]);
            prixPortion = Convert.ToSingle(reader["prix"]);
        }
        if (portionsDispo <= 0)
        {
            Console.WriteLine("Désolé, ce plat n'est plus disponible.");
            return;
        }

        int portions = Saisie.Entier($"Combien de portions ? (1 à {portionsDispo}) : ");
        while (portions < 1 || portions > portionsDispo)
        {
            portions = Saisie.Entier($"Veuillez entrer un nombre entre 1 et {portionsDispo} : ");
        }
        float prix = prixPortion * portions;
        string depart = new Cuisinier().GetStation(idCuisinier, conn);
        string arrive = client.GetStation(idClient, conn);
        string idCommande = NouvelIdCommande(conn);

        string query = "INSERT INTO Commande (idCommande, statut, idClient, idCuisinier, prix, depart, arrive, dateCo, IdPlat) VALUES (@idCommande, 'En cours', @idClient, @idCuisinier, @prix, @depart, @arrive, NOW(), @idPlat)";
        using (var cmd = new MySqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@idCommande", idCommande);
            cmd.Parameters.AddWithValue("@idClient", idClient);
            cmd.Parameters.AddWithValue("@idCuisinier", idCuisinier);
            cmd.Parameters.AddWithValue("@prix", prix);
            cmd.Parameters.AddWithValue("@depart", depart);
            cmd.Parameters.AddWithValue("@arrive", arrive);
            cmd.Parameters.AddWithValue("@idPlat", idPlat);
            cmd.ExecuteNonQuery();
        }
        using (var cmd = new MySqlCommand("UPDATE Plat SET Quantité = Quantité - @n WHERE IdPlat = @id", conn))
        {
            cmd.Parameters.AddWithValue("@n", portions);
            cmd.Parameters.AddWithValue("@id", idPlat);
            cmd.ExecuteNonQuery();
        }
        Console.WriteLine($"\nCommande {idCommande} enregistrée : {portions} x {nomPlat} = {prix} €");
        Console.WriteLine($"Livraison de {depart} (cuisinier {idCuisinier}) vers {arrive}.");
    }
    /// <summary>
    /// Calcule le prochain identifiant libre de la forme CMDn
    /// </summary>
    private string NouvelIdCommande(MySqlConnection conn)
    {
        int max = 0;
        using var cmd = new MySqlCommand("SELECT idCommande FROM Commande WHERE idCommande LIKE 'CMD%'", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            if (int.TryParse(reader.GetString(0).Substring(3), out int n) && n > max) max = n;
        }
        return "CMD" + (max + 1);
    }
    /// <summary>
    /// Vérifie qu'une commande existe (et appartient au cuisinier si on le précise)
    /// </summary>
    public bool Existe(string idCommande, MySqlConnection conn, string? idCuisinier = null)
    {
        string query = "SELECT COUNT(*) FROM Commande WHERE idCommande = @id" + (idCuisinier != null ? " AND idCuisinier = @cuisinier" : "");
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", idCommande);
        if (idCuisinier != null) cmd.Parameters.AddWithValue("@cuisinier", idCuisinier);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }
    /// <summary>
    /// Permet d'afficher les commandes
    /// </summary>
    /// <param name="conn"></param>
    public void AfficheCommande(MySqlConnection conn)
    {
        AfficherRequete(conn, "SELECT * FROM Commande ORDER BY dateCo ASC", null, null);
    }
    /// <summary>
    /// Affiche les commandes d'un client
    /// </summary>
    public void AfficheCommandeClient(MySqlConnection conn, string id)
    {
        AfficherRequete(conn, "SELECT * FROM Commande WHERE idClient = @idCo ORDER BY dateCo ASC", "@idCo", id);
    }
    /// <summary>
    /// affiche les commandes d'un cuisinier particulier
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="id"></param>
    public void AfficheCommandeCuisinier(MySqlConnection conn, string id)
    {
        AfficherRequete(conn, "SELECT * FROM Commande WHERE idCuisinier = @idCo ORDER BY dateCo ASC", "@idCo", id);
    }
    /// <summary>
    /// affiche le prix d'une commande
    /// </summary>
    /// <param name="conn"></param>
    public void AfficheCommandePrix(MySqlConnection conn)
    {
        string id = Saisie.Texte("De quelle commande souhaitez-vous avoir le prix ? (ex : CMD1) : ");
        AfficherRequete(conn, "SELECT * FROM Commande WHERE idCommande = @idCo", "@idCo", id);
    }
    private void AfficherRequete(MySqlConnection conn, string query, string? parametre, string? valeur)
    {
        using var cmd = new MySqlCommand(query, conn);
        if (parametre != null) cmd.Parameters.AddWithValue(parametre, valeur);
        using var reader = cmd.ExecuteReader();
        if (!reader.HasRows)
        {
            Console.WriteLine("Aucune commande trouvée.");
            return;
        }
        while (reader.Read())
        {
            Console.WriteLine($"{reader["idCommande"]} | {reader["statut"]} | {reader["prix"]} € | plat {reader["IdPlat"]} | client {reader["idClient"]} | cuisinier {reader["idCuisinier"]} | {reader["depart"]} -> {reader["arrive"]} | {reader["dateCo"]}");
        }
    }
    /// <summary>
    /// Fonction permettant d'obtenir la station de depart pour le trajet
    /// </summary>
    /// <param name="idCommande"></param>
    /// <param name="conn"></param>
    /// <returns></returns>
    public string GetDepart(string idCommande, MySqlConnection conn)
    {
        using var command = new MySqlCommand("SELECT depart FROM Commande WHERE idCommande = @idCommande", conn);
        command.Parameters.AddWithValue("@idCommande", idCommande);
        return command.ExecuteScalar()?.ToString() ?? "";
    }
    /// <summary>
    /// Fonction permettant d'obtenir la station d'arrivé pour le trajet
    /// </summary>
    /// <param name="idCommande"></param>
    /// <param name="conn"></param>
    /// <returns></returns>
    public string GetArrive(string idCommande, MySqlConnection conn)
    {
        using var command = new MySqlCommand("SELECT arrive FROM Commande WHERE idCommande = @idCommande", conn);
        command.Parameters.AddWithValue("@idCommande", idCommande);
        return command.ExecuteScalar()?.ToString() ?? "";
    }
}
