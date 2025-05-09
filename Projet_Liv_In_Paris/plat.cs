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

        Console.Write("Entrez le prix du plat : ");
        if (!float.TryParse(Console.ReadLine(), out float prix))
        {
            Console.WriteLine("Erreur : Veuillez entrer un prix valide.");
            return;
        }

        string query = "INSERT INTO Plat (IdPlat, TypePlat, Quantité, Nationalité, RegimeSpe, IngrediantPrin, DateFabrication, DatePeremption, prix, idCuisinier) VALUES (@IdPlat, @TypePlat, @Quantite, @Nationalite, @RegimeSpe, @IngrediantPrin,NOW(), Now(), @Prix, @id)";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@IdPlat", idPlat);
        cmd.Parameters.AddWithValue("@TypePlat", typePlat);
        cmd.Parameters.AddWithValue("@Quantite", quantite);
        cmd.Parameters.AddWithValue("@Nationalite", nationalite);
        cmd.Parameters.AddWithValue("@RegimeSpe", regimeSpe);
        cmd.Parameters.AddWithValue("@IngrediantPrin", ingrediantPrin);
        cmd.Parameters.AddWithValue("@Prix", prix);
        cmd.Parameters.AddWithValue("@id", idCuisinier);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Plat ajouté");
    }
    public void AfficherPlat(MySqlConnection conn)
    {
        var cmd = new MySqlCommand("SELECT TypePlat,Quantité,Nationalité,RegimeSpe,IngrediantPrin,DateFabrication,DatePeremption,Prix FROM Plat", conn);
            var reader = cmd.ExecuteReader();
            Console.WriteLine("Plat   Quantité Nationalité Regime Speciale Ingredient Principale Date de Fabrication Date Peremption       Prix");
            while (reader.Read())
            {
                Console.WriteLine($"{reader["TypePlat"]}   {reader["Quantité"]}     {reader["Nationalité"]}         {reader["RegimeSpe"]}            {reader["IngrediantPrin"]}          {reader["DateFabrication"]} {reader["DatePeremption"]}    {reader["Prix"]}");
            }
    }
    public void AfficherCuisiniersAlphabetique(MySqlConnection conn)
    {
            var cmd = new MySqlCommand("SELECT * FROM Cuisinier", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["idCuisinier"]}, Nom : {reader["nom"]}, Adresse: {reader["adresse"]}");
            }
    }
}