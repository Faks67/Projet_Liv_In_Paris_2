using System;
using MySql.Data.MySqlClient;

public class Plat
{
    public void AjouterPlat(MySqlConnection conn)
    {
        Console.Write("Entrez l'ID du plat : ");
        string idPlat = Console.ReadLine();

        Console.Write("Entrez le type du plat : ");
        string typePlat = Console.ReadLine();

        Console.Write("Entrez la quantité : ");
        int quantite = int.Parse(Console.ReadLine());

        Console.Write("Entrez la nationalité du plat : ");
        string nationalite = Console.ReadLine();

        Console.Write("Entrez le régime spécial (ou 'Aucun') : ");
        string regimeSpe = Console.ReadLine();

        Console.Write("Entrez l'ingrédient principal : ");
        string ingrediantPrin = Console.ReadLine();

        Console.Write("Entrez la date de fabrication (yyyy-MM-dd) : ");
         DateTime dateFabrication = DateTime.Parse(Console.ReadLine());

        Console.Write("Entrez la date de péremption (yyyy-MM-dd) : ");
        DateTime datePeremption = DateTime.Parse(Console.ReadLine());

        Console.Write("Entrez l'ID de la recette : ");
        string idRecette = Console.ReadLine();

        Console.Write("Entrez le prix du plat : ");
        if (!float.TryParse(Console.ReadLine(), out float prix))
        {
            Console.WriteLine("Erreur : Veuillez entrer un prix valide.");
            return;
        }

        string query = "INSERT INTO Plat (IdPlat, TypePlat, Quantité, Nationalité, RegimeSpe, IngrediantPrin, " +
                    "DateFabrication, DatePeremption, idRecette, prix) " +
                    "VALUES (@IdPlat, @TypePlat, @Quantite, @Nationalite, @RegimeSpe, @IngrediantPrin, " +
                    "@DateFabrication, @DatePeremption, @idRecette, @Prix)";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@IdPlat", idPlat);
        cmd.Parameters.AddWithValue("@TypePlat", typePlat);
        cmd.Parameters.AddWithValue("@Quantite", quantite);
        cmd.Parameters.AddWithValue("@Nationalite", nationalite);
        cmd.Parameters.AddWithValue("@RegimeSpe", regimeSpe);
        cmd.Parameters.AddWithValue("@IngrediantPrin", ingrediantPrin);
        cmd.Parameters.AddWithValue("@DateFabrication", dateFabrication.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@DatePeremption", datePeremption.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@idRecette", idRecette);
        cmd.Parameters.AddWithValue("@Prix", prix);
    }
}