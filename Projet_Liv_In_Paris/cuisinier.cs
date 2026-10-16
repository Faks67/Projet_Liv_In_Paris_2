using System;
using MySql.Data.MySqlClient;

public class Cuisinier
{
    /// <summary>
    /// Permet d'ajouter un cuisiner dans la base de donnée
    /// </summary>
    /// <param name="conn"></param>
    /// <returns>L'ID du cuisinier créé</returns>
    public string AjouterCuisinier(MySqlConnection conn)
    {
        string idUtilisateur = new Utilisateur().CreerUtilisateur(conn);
        string idCuisinier = Saisie.Texte("Entrez l'ID du cuisinier (ex : C6) : ");
        while (Existe(idCuisinier, conn))
        {
            Console.WriteLine("Cet ID cuisinier existe déjà, choisissez-en un autre.");
            idCuisinier = Saisie.Texte("Entrez l'ID du cuisinier (ex : C6) : ");
        }
        string nom = Saisie.Texte("Entrez le nom : ");
        string prenom = Saisie.Texte("Entrez le prénom : ");
        string addMail = Saisie.Texte("Entrez l'adresse e-mail : ");
        string adresse = Saisie.Texte("Entrez l'adresse : ");
        int codePostal = Saisie.Entier("Entrez le code postal : ");
        string numTel = Saisie.Texte("Entrez le numéro de téléphone : ");
        string station = Saisie.Texte("Entrez la station de métro la plus proche : ");
        using var cmd = new MySqlCommand("INSERT INTO Cuisinier (idCuisinier, idUtilisateur, nom, prenom, addMail, adresse, CodePostal, numTel, Station) VALUES (@ID, @IDuti, @nom, @prenom, @addMail, @adresse, @CodePostal, @numTel, @station)", conn);
        cmd.Parameters.AddWithValue("@ID", idCuisinier);
        cmd.Parameters.AddWithValue("@IDuti", idUtilisateur);
        cmd.Parameters.AddWithValue("@nom", nom);
        cmd.Parameters.AddWithValue("@prenom", prenom);
        cmd.Parameters.AddWithValue("@addMail", addMail);
        cmd.Parameters.AddWithValue("@adresse", adresse);
        cmd.Parameters.AddWithValue("@CodePostal", codePostal);
        cmd.Parameters.AddWithValue("@numTel", numTel);
        cmd.Parameters.AddWithValue("@station", station);
        cmd.ExecuteNonQuery();
        Console.WriteLine($"Cuisinier ajouté ! Votre ID cuisinier est : {idCuisinier} (gardez-le pour vous reconnecter)");
        return idCuisinier;
    }
    /// <summary>
    /// Vérifie qu'un cuisinier existe
    /// </summary>
    public bool Existe(string idCuisinier, MySqlConnection conn)
    {
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Cuisinier WHERE idCuisinier = @id", conn);
        cmd.Parameters.AddWithValue("@id", idCuisinier);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }
    /// <summary>
    /// Renvoie la station du cuisinier
    /// </summary>
    public string GetStation(string idCuisinier, MySqlConnection conn)
    {
        using var cmd = new MySqlCommand("SELECT Station FROM Cuisinier WHERE idCuisinier = @id", conn);
        cmd.Parameters.AddWithValue("@id", idCuisinier);
        return cmd.ExecuteScalar()?.ToString() ?? "";
    }
    /// <summary>
    /// Permet de modifier un cuisinier
    /// </summary>
    /// <param name="conn"></param>
    public void ModifierCuisinier(MySqlConnection conn)
    {
        string idCuisinier = Saisie.Texte("Entrez l'ID du cuisinier : ");
        if (!Existe(idCuisinier, conn))
        {
            Console.WriteLine("Cuisinier introuvable.");
            return;
        }
        ModifierCuisinierParty(conn, idCuisinier);
    }
    /// <summary>
    /// Permet de modifier un cuisinier sans changer l'id de connection
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="idCuisinier"></param>
    public void ModifierCuisinierParty(MySqlConnection conn, string idCuisinier)
    {
        string nom = Saisie.Texte("Entrez le nom : ");
        string prenom = Saisie.Texte("Entrez le prénom : ");
        string addMail = Saisie.Texte("Entrez l'adresse e-mail : ");
        string adresse = Saisie.Texte("Entrez l'adresse : ");
        int codePostal = Saisie.Entier("Entrez le code postal : ");
        string numTel = Saisie.Texte("Entrez le numéro de téléphone : ");
        string station = Saisie.Texte("Entrez la station de métro la plus proche : ");
        using var cmd = new MySqlCommand("UPDATE Cuisinier SET nom = @nom, prenom = @prenom, addMail = @addMail, adresse = @adresse, CodePostal = @CodePostal, numTel = @numTel, Station = @station WHERE idCuisinier = @ID", conn);
        cmd.Parameters.AddWithValue("@ID", idCuisinier);
        cmd.Parameters.AddWithValue("@nom", nom);
        cmd.Parameters.AddWithValue("@prenom", prenom);
        cmd.Parameters.AddWithValue("@addMail", addMail);
        cmd.Parameters.AddWithValue("@adresse", adresse);
        cmd.Parameters.AddWithValue("@CodePostal", codePostal);
        cmd.Parameters.AddWithValue("@numTel", numTel);
        cmd.Parameters.AddWithValue("@station", station);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Cuisinier modifié");
    }
    /// <summary>
    /// Permet de supprimer un cuisinier (avec ses plats, les commandes associées et son compte utilisateur)
    /// </summary>
    /// <param name="conn"></param>
    public void SupprimerCuisinier(MySqlConnection conn)
    {
        string idCuisinier = Saisie.Texte("Entrez l'ID du cuisinier : ");
        if (!Existe(idCuisinier, conn))
        {
            Console.WriteLine("Cuisinier introuvable.");
            return;
        }
        if (!Saisie.OuiNon($"Supprimer le cuisinier {idCuisinier} ainsi que ses plats et commandes ?")) return;

        string idUtilisateur;
        using (var cmd = new MySqlCommand("SELECT idUtilisateur FROM Cuisinier WHERE idCuisinier = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", idCuisinier);
            idUtilisateur = cmd.ExecuteScalar()?.ToString() ?? "";
        }
        string[] requetes =
        {
            "DELETE FROM Commande WHERE idCuisinier = @id OR IdPlat IN (SELECT IdPlat FROM Plat WHERE idCuisinier = @id)",
            "DELETE FROM Plat WHERE idCuisinier = @id",
            "DELETE FROM Cuisinier WHERE idCuisinier = @id"
        };
        foreach (string requete in requetes)
        {
            using var cmd = new MySqlCommand(requete, conn);
            cmd.Parameters.AddWithValue("@id", idCuisinier);
            cmd.ExecuteNonQuery();
        }
        new Utilisateur().SupprimerUtilisateur(idUtilisateur, conn);
        Console.WriteLine("Cuisinier supprimé");
    }
    /// <summary>
    /// Affiche l'ensemble des clients servis par un cuisinier
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherClient(MySqlConnection conn)
    {
        string idCuisinier = Saisie.Texte("Entrez l'ID du cuisinier : ");
        Console.Write("Voulez-vous afficher les clients depuis l'inscription (1) ou pendant une période spécifique (2) ? ");
        string choix = Saisie.Ligne();
        string query = "SELECT DISTINCT c.idClient, c.nom, c.prenom, co.dateCo " +
                   "FROM Client c " +
                   "JOIN Commande co ON c.idClient = co.idClient " +
                   "WHERE co.idCuisinier = @idCuisinier";
        DateTime? debut = null, fin = null;
        if (choix == "2")
        {
            debut = Saisie.Date("Entrez la date de début (aaaa-mm-jj) : ");
            fin = Saisie.Date("Entrez la date de fin (aaaa-mm-jj) : ").AddDays(1);
            query += " AND co.dateCo >= @debut AND co.dateCo < @fin";
        }
        using var cmd = new MySqlCommand(query + " ORDER BY co.dateCo", conn);
        cmd.Parameters.AddWithValue("@idCuisinier", idCuisinier);
        if (debut.HasValue) cmd.Parameters.AddWithValue("@debut", debut.Value);
        if (fin.HasValue) cmd.Parameters.AddWithValue("@fin", fin.Value);
        using var reader = cmd.ExecuteReader();
        if (!reader.HasRows) Console.WriteLine("Aucun client trouvé.");
        while (reader.Read())
        {
            Console.WriteLine($"Client ID: {reader["idClient"]}, Nom: {reader["nom"]} {reader["prenom"]}, Date: {reader["dateCo"]}");
        }
    }
    /// <summary>
    /// Donne tous les plats realisé par un cuisinier, classés par fréquence
    /// </summary>
    /// <param name="conn"></param>
    public void platRealiser(MySqlConnection conn)
    {
        string idCuisinier = Saisie.Texte("Entrez l'ID du cuisinier : ");
        string query = "SELECT p.TypePlat AS nomPlat, COUNT(*) as frequence " +
                    "FROM Commande c " +
                    "JOIN Plat p ON c.IdPlat = p.IdPlat " +
                    "WHERE c.idCuisinier = @idCuisinier " +
                    "GROUP BY p.TypePlat " +
                    "ORDER BY frequence DESC";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@idCuisinier", idCuisinier);
        using var reader = cmd.ExecuteReader();
        if (!reader.HasRows)
        {
            Console.WriteLine("Aucun plat trouvé pour ce cuisinier.");
            return;
        }
        Console.WriteLine("Plats réalisés par fréquence :");
        while (reader.Read())
        {
            Console.WriteLine($"Plat: {reader["nomPlat"]}, Nombre de fois préparé: {reader["frequence"]}");
        }
    }
    /// <summary>
    /// Trouve le plat le plus recent du cuisinier s'il existe
    /// </summary>
    /// <param name="conn"></param>
    public void platRecent(MySqlConnection conn)
    {
        string idCuisinier = Saisie.Texte("Entrez l'ID du cuisinier : ");
        string query = "SELECT TypePlat AS nomPlat, DateFabrication FROM Plat " +
                    "WHERE idCuisinier = @idCuisinier " +
                    "ORDER BY DateFabrication DESC LIMIT 1";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@idCuisinier", idCuisinier);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            Console.WriteLine($"Le plat le plus récent préparé par le cuisinier {idCuisinier} est :");
            Console.WriteLine($"Nom : {reader["nomPlat"]}");
            Console.WriteLine($"Date de fabrication : {reader["DateFabrication"]}");
        }
        else
        {
            Console.WriteLine("Aucun plat trouvé pour ce cuisinier.");
        }
    }
    /// <summary>
    /// Fonction affichant les cuisiniers par ordre alphabetique
    /// </summary>
    /// <param name="conn"></param>
    public void AfficherCuisiniersAlphabetique(MySqlConnection conn)
    {
        using var cmd = new MySqlCommand("SELECT * FROM Cuisinier ORDER BY nom, prenom", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"ID: {reader["idCuisinier"]}, Nom : {reader["nom"]} {reader["prenom"]}, Adresse: {reader["adresse"]}, Station: {reader["Station"]}");
        }
    }
}
