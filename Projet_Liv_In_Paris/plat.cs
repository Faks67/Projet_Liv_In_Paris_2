using System;
using MySql.Data.MySqlClient;

public class Plat
{
    /// <summary>
    /// Permet d'ajouter un plat dans la base de donnée
    /// </summary>
    /// <param name="conn"></param>
    public void AjouterPlat(string idCuisinier, MySqlConnection conn)
    {
        string idPlat = Saisie.Texte("Entrez l'ID du plat (ex : P6) : ");
        while (Existe(idPlat, conn))
        {
            Console.WriteLine("Cet ID de plat existe déjà, choisissez-en un autre.");
            idPlat = Saisie.Texte("Entrez l'ID du plat (ex : P6) : ");
        }
        string typePlat = Saisie.Texte("Entrez le nom du plat : ");
        int quantite = Saisie.Entier("Entrez la quantité (nombre de portions) : ");
        string nationalite = Saisie.Texte("Entrez la nationalité du plat : ");
        string regimeSpe = Saisie.Texte("Entrez le régime spécial (ou 'Aucun') : ");
        string ingrediantPrin = Saisie.Texte("Entrez l'ingrédient principal : ");
        float prix = Saisie.Decimal("Entrez le prix d'une portion : ");
        int joursConservation = Saisie.Entier("Le plat se conserve combien de jours ? ");

        string query = "INSERT INTO Plat (IdPlat, TypePlat, Quantité, Nationalité, RegimeSpe, IngrediantPrin, DateFabrication, DatePeremption, prix, idCuisinier) VALUES (@IdPlat, @TypePlat, @Quantite, @Nationalite, @RegimeSpe, @IngrediantPrin, NOW(), DATE_ADD(NOW(), INTERVAL @jours DAY), @Prix, @id)";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@IdPlat", idPlat);
        cmd.Parameters.AddWithValue("@TypePlat", typePlat);
        cmd.Parameters.AddWithValue("@Quantite", quantite);
        cmd.Parameters.AddWithValue("@Nationalite", nationalite);
        cmd.Parameters.AddWithValue("@RegimeSpe", regimeSpe);
        cmd.Parameters.AddWithValue("@IngrediantPrin", ingrediantPrin);
        cmd.Parameters.AddWithValue("@jours", joursConservation);
        cmd.Parameters.AddWithValue("@Prix", prix);
        cmd.Parameters.AddWithValue("@id", idCuisinier);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Plat ajouté");
    }
    /// <summary>
    /// Vérifie qu'un plat existe
    /// </summary>
    public bool Existe(string idPlat, MySqlConnection conn)
    {
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Plat WHERE IdPlat = @id", conn);
        cmd.Parameters.AddWithValue("@id", idPlat);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }
    /// <summary>
    /// Affiche les plats disponibles (avec leur ID pour pouvoir les commander)
    /// </summary>
    public void AfficherPlat(MySqlConnection conn)
    {
        string query = "SELECT p.IdPlat, p.TypePlat, p.Quantité, p.Nationalité, p.RegimeSpe, p.IngrediantPrin, p.DatePeremption, p.prix, c.prenom, c.nom " +
                       "FROM Plat p JOIN Cuisinier c ON p.idCuisinier = c.idCuisinier ORDER BY p.IdPlat";
        using var cmd = new MySqlCommand(query, conn);
        using var reader = cmd.ExecuteReader();
        if (!reader.HasRows)
        {
            Console.WriteLine("Aucun plat disponible pour le moment.");
            return;
        }
        Console.WriteLine($"{"ID",-6}{"Plat",-14}{"Portions",-10}{"Nationalité",-13}{"Régime",-10}{"Ingrédient",-12}{"Périme le",-12}{"Prix",-8}Cuisinier");
        while (reader.Read())
        {
            string peremption = Convert.ToDateTime(reader["DatePeremption"]).ToString("dd/MM/yyyy");
            Console.WriteLine($"{reader["IdPlat"],-6}{reader["TypePlat"],-14}{reader["Quantité"],-10}{reader["Nationalité"],-13}{reader["RegimeSpe"],-10}{reader["IngrediantPrin"],-12}{peremption,-12}{reader["prix"] + " €",-8}{reader["prenom"]} {reader["nom"]}");
        }
    }
    public void AfficherCuisiniersAlphabetique(MySqlConnection conn)
    {
        new Cuisinier().AfficherCuisiniersAlphabetique(conn);
    }
}
