using System;
using MySql.Data.MySqlClient;

public static class Database
{
    private static string connectionString = "server=localhost;database=liv_in_paris;user=root;password=root";
    /// <summary>
    /// Création de la connection avec la database
    /// </summary>
    /// <returns></returns>
    public static MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionString);
    }
}