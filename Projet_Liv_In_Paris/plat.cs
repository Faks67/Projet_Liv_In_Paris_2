using System;
using MySql.Data.MySqlClient;

public class Plat
{
    public void AjouterPlat(MySqlConnection conn)
    {
        Console.WriteLine("Donner l'Id du cuisinier");
        string idCuisinier = Console.ReadLine();
    }
}