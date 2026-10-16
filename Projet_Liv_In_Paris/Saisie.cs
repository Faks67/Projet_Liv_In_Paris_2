using System;
using System.Globalization;

/// <summary>
/// Lectures clavier sécurisées : on redemande tant que la saisie n'est pas valide au lieu de planter
/// </summary>
public static class Saisie
{
    /// <summary>
    /// Lit une ligne. Si la console est fermée (fin d'entrée), on quitte proprement.
    /// </summary>
    public static string Ligne()
    {
        string? ligne = Console.ReadLine();
        if (ligne == null)
        {
            Console.WriteLine("\nFin de la saisie, au revoir !");
            Environment.Exit(0);
        }
        return ligne.Trim();
    }

    public static string Texte(string question, bool obligatoire = true)
    {
        while (true)
        {
            Console.Write(question);
            string valeur = Ligne();
            if (valeur != "" || !obligatoire) return valeur;
            Console.WriteLine("Ce champ est obligatoire.");
        }
    }

    public static int Entier(string question)
    {
        while (true)
        {
            Console.Write(question);
            if (int.TryParse(Ligne(), out int valeur)) return valeur;
            Console.WriteLine("Veuillez entrer un nombre entier.");
        }
    }

    /// <summary>
    /// Accepte 12,5 comme 12.5
    /// </summary>
    public static float Decimal(string question)
    {
        while (true)
        {
            Console.Write(question);
            string valeur = Ligne().Replace(',', '.');
            if (float.TryParse(valeur, NumberStyles.Float, CultureInfo.InvariantCulture, out float resultat) && resultat >= 0)
                return resultat;
            Console.WriteLine("Veuillez entrer un nombre (ex : 12,5).");
        }
    }

    /// <summary>
    /// Accepte 2025-05-09 ou 09/05/2025
    /// </summary>
    public static DateTime Date(string question)
    {
        string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy" };
        while (true)
        {
            Console.Write(question);
            if (DateTime.TryParseExact(Ligne(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                return date;
            Console.WriteLine("Date invalide. Format attendu : aaaa-mm-jj (ex : 2025-05-09) ou jj/mm/aaaa.");
        }
    }

    /// <summary>
    /// Question Oui/Non : accepte Y, O, N en majuscule ou minuscule
    /// </summary>
    public static bool OuiNon(string question)
    {
        while (true)
        {
            Console.Write(question + " (O/N) : ");
            string reponse = Ligne().ToUpper();
            if (reponse == "O" || reponse == "Y" || reponse == "OUI") return true;
            if (reponse == "N" || reponse == "NON") return false;
        }
    }
}
