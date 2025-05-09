using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using System.Linq;
public class Export 
{

  public void ExporterJson<T>(this List<T> objets, string chemin)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(objets, options);
            File.WriteAllText(chemin, json);
            Console.WriteLine($"Export JSON terminé avec succès : {chemin}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'export JSON : {ex.Message}");
        }
    }
}