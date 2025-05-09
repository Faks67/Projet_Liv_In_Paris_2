using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using MySql.Data.MySqlClient;
using System.Text.Json;

public static class DatabaseExporter
{
    public static List<Dictionary<string, object>> GetTableData(string tableName, MySqlConnection conn)
    {
        var rows = new List<Dictionary<string, object>>();

        try
        {
            string query = $"SELECT * FROM {tableName}";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);
                    object value = reader.GetValue(i);
                    row[columnName] = value;
                }

                rows.Add(row);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la récupération des données : {ex.Message}");
        }

        return rows;
    }

    public static void ExportToJson(string tableName, string outputFilePath, MySqlConnection conn)
    {
        try
        {
            var data = GetTableData(tableName, conn);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(outputFilePath, json);
            Console.WriteLine($"Export JSON de '{tableName}' terminé avec succès : {outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'export JSON : {ex.Message}");
        }
    }
}