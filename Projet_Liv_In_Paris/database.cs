using System;
using MySql.Data.MySqlClient;

public static class Database
{
    // Instance MySQL dédiée au projet (lancée par demarrer_mysql.cmd) : port 3307, root sans mot de passe
    private static string connectionString = "server=127.0.0.1;port=3307;database=liv_in_paris;user=root;password=";
    /// <summary>
    /// Création de la connection avec la database
    /// </summary>
    /// <returns></returns>
    public static MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionString);
    }
}
