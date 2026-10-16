using System;
using MySql.Data.MySqlClient;

public class Utilisateur
{
    /// <summary>
    /// Demande un identifiant utilisateur libre et son mot de passe, puis crée l'utilisateur
    /// </summary>
    /// <returns>L'identifiant créé</returns>
    public string CreerUtilisateur(MySqlConnection conn)
    {
        string idUtilisateur = Saisie.Texte("Entrez l'ID de l'utilisateur : ");
        while (!IsUserNotInTable(idUtilisateur, conn))
        {
            Console.WriteLine("Cet identifiant est déjà pris, choisissez-en un autre.");
            idUtilisateur = Saisie.Texte("Entrez l'ID de l'utilisateur : ");
        }
        string mdp = Saisie.Texte("Entrez le mot de passe de l'utilisateur : ");
        AjouterUtilisateur(idUtilisateur, mdp, conn);
        return idUtilisateur;
    }
    /// <summary>
    /// Crée un utilisateur
    /// </summary>
    /// <param name="idUtilisateur"></param>
    /// <param name="motDePasse"></param>
    public void AjouterUtilisateur(string idUtilisateur, string motDePasse, MySqlConnection conn)
    {
        if (IsUserNotInTable(idUtilisateur, conn))
        {
            using var cmd = new MySqlCommand("INSERT INTO Utilisateur (idUtilisateur, MotDePasse) VALUES (@Id, @MDP)", conn);
            cmd.Parameters.AddWithValue("@Id", idUtilisateur);
            cmd.Parameters.AddWithValue("@MDP", motDePasse);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Utilisateur ajouté !");
        }
        else { Console.WriteLine("Utilisateur deja present"); }
    }
    public bool IsUserNotInTable(string iduser, MySqlConnection conn)
    {
        using var command = new MySqlCommand("SELECT COUNT(*) FROM Utilisateur WHERE idUtilisateur = @userId;", conn);
        command.Parameters.AddWithValue("@userId", iduser);
        return Convert.ToInt32(command.ExecuteScalar()) == 0;
    }
    /// <summary>
    /// Supprime un utilisateur (utilisé quand on supprime un client ou un cuisinier)
    /// </summary>
    public void SupprimerUtilisateur(string idUtilisateur, MySqlConnection conn)
    {
        using var cmd = new MySqlCommand("DELETE FROM Utilisateur WHERE idUtilisateur = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", idUtilisateur);
        cmd.ExecuteNonQuery();
    }
    /// <summary>
    /// Affiche l'ensemble des utilisateurs
    /// </summary>
    public void AfficherUtilisateurs(MySqlConnection conn)
    {
        using var cmd = new MySqlCommand("SELECT * FROM Utilisateur", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"ID: {reader["idUtilisateur"]}, Mot de passe: {reader["MotDePasse"]}");
        }
    }
}
