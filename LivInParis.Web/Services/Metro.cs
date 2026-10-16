using System.Globalization;
using System.Text;
using OfficeOpenXml;

namespace LivInParis.Web.Services;

public record Station(int Id, string Nom, string Ligne, double Lat, double Lon, string Commune);

public record EtapeTrajet(int IdStation, string Nom, string Ligne, double Lat, double Lon, double Minutes);

public record Troncon(string Ligne, string Depart, string Arrivee, int NbArrets, double Minutes);

public record Itineraire(double Minutes, List<EtapeTrajet> Etapes, List<Troncon> Troncons, int Correspondances);

/// <summary>
/// Plan du métro chargé depuis Metro_GareFinal.xlsx : stations, liaisons pondérées (minutes) et plus court chemin (Dijkstra)
/// </summary>
public class Metro
{
    private readonly Dictionary<int, Station> stations = new();
    private readonly Dictionary<int, List<(int vers, double minutes, bool correspondance)>> voisins = new();
    private readonly List<(int a, int b, string ligne)> segments = new();

    public IReadOnlyCollection<Station> Stations => stations.Values;

    public Metro(string cheminExcel)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(new FileInfo(cheminExcel));
        var feuille = package.Workbook.Worksheets[0];
        int nbLignes = feuille.Dimension.Rows;

        var precedent = new Dictionary<int, int>();
        var suivant = new Dictionary<int, int>();
        var tempsDepuisPrecedent = new Dictionary<int, double>();
        var tempsChangement = new Dictionary<int, double>();

        for (int i = 2; i <= nbLignes; i++)
        {
            if (!int.TryParse(feuille.Cells[i, 1].Text, out int id)) continue;
            string nom = feuille.Cells[i, 3].Text.Trim();
            if (nom == "") continue;
            double lon = Nombre(feuille.Cells[i, 4].Text);
            double lat = Nombre(feuille.Cells[i, 5].Text);
            stations[id] = new Station(id, nom, feuille.Cells[i, 2].Text.Trim(), lat, lon, feuille.Cells[i, 6].Text.Trim());
            precedent[id] = (int)Nombre(feuille.Cells[i, 8].Text);
            suivant[id] = (int)Nombre(feuille.Cells[i, 9].Text);
            tempsChangement[id] = Nombre(feuille.Cells[i, 10].Text);
            tempsDepuisPrecedent[id] = Nombre(feuille.Cells[i, 12].Text);
            voisins[id] = new();
        }

        // Liaisons sur une même ligne : la colonne "Temps entre 2 stations" donne le temps depuis la station précédente
        foreach (var (id, station) in stations)
        {
            int prec = precedent[id];
            if (prec != 0 && stations.ContainsKey(prec))
                AjouterLiaison(prec, id, tempsDepuisPrecedent[id], station.Ligne);

            int suiv = suivant[id];
            if (suiv != 0 && stations.ContainsKey(suiv) && precedent[suiv] != id)
            {
                // Embranchement : la station suivante ne nous désigne pas comme précédente, on estime le temps
                AjouterLiaison(id, suiv, Math.Max(1, Math.Round(DistanceKm(station, stations[suiv]) * 2)), station.Ligne);
            }
        }

        // Correspondances : stations de même nom sur des lignes différentes
        foreach (var groupe in stations.Values.GroupBy(s => Cle(s.Nom)).Where(g => g.Count() > 1))
        {
            var liste = groupe.ToList();
            for (int i = 0; i < liste.Count; i++)
                for (int j = i + 1; j < liste.Count; j++)
                {
                    var a = liste[i];
                    var b = liste[j];
                    bool memeLigne = a.Ligne == b.Ligne;
                    double minutes = memeLigne ? 0 : Math.Max(2, Math.Max(tempsChangement[a.Id], tempsChangement[b.Id]));
                    voisins[a.Id].Add((b.Id, minutes, !memeLigne));
                    voisins[b.Id].Add((a.Id, minutes, !memeLigne));
                }
        }
    }

    private void AjouterLiaison(int a, int b, double minutes, string ligne)
    {
        if (voisins[a].Any(v => v.vers == b)) return;
        minutes = Math.Max(1, minutes);
        voisins[a].Add((b, minutes, false));
        voisins[b].Add((a, minutes, false));
        segments.Add((a, b, ligne));
    }

    /// <summary>
    /// Segments à dessiner pour le plan (une entrée par liaison entre deux stations d'une même ligne)
    /// </summary>
    public IEnumerable<object> Segments() => segments.Select(s => new
    {
        ligne = s.ligne,
        de = new[] { stations[s.a].Lat, stations[s.a].Lon },
        vers = new[] { stations[s.b].Lat, stations[s.b].Lon }
    });

    /// <summary>
    /// Stations dédoublonnées par nom (pour les listes de choix)
    /// </summary>
    public IEnumerable<object> StationsUniques() => stations.Values
        .GroupBy(s => Cle(s.Nom))
        .Select(g => new
        {
            id = g.First().Id,
            nom = g.First().Nom,
            lignes = g.Select(s => s.Ligne).Distinct().OrderBy(OrdreLigne).ToList(),
            lat = g.First().Lat,
            lon = g.First().Lon
        })
        .OrderBy(s => s.nom, StringComparer.Create(new CultureInfo("fr-FR"), true));

    public Station? Get(int id) => stations.GetValueOrDefault(id);

    /// <summary>
    /// Retrouve une station par son nom (insensible aux accents, tirets, apostrophes)
    /// </summary>
    public Station? ParNom(string nom)
    {
        string cle = Cle(nom);
        return stations.Values.FirstOrDefault(s => Cle(s.Nom) == cle);
    }

    /// <summary>
    /// Station la plus proche d'un point GPS, avec la distance à pied
    /// </summary>
    public (Station station, double km, double minutesAPied) PlusProche(double lat, double lon)
    {
        var point = new Station(0, "", "", lat, lon, "");
        var proche = stations.Values.MinBy(s => DistanceKm(s, point))!;
        double km = DistanceKm(proche, point);
        // ~4,5 km/h à pied, avec un détour de 30 % par rapport à la ligne droite
        return (proche, km, Math.Round(km * 1.3 / 4.5 * 60));
    }

    /// <summary>
    /// Plus court chemin (Dijkstra) en minutes entre deux stations, toutes stations homonymes confondues
    /// </summary>
    public Itineraire? Dijkstra(int depart, int arrivee)
    {
        if (!stations.ContainsKey(depart) || !stations.ContainsKey(arrivee)) return null;
        var departs = Homonymes(depart);
        var arrivees = Homonymes(arrivee).ToHashSet();

        var distances = new Dictionary<int, double>();
        var predecesseurs = new Dictionary<int, int>();
        var file = new PriorityQueue<int, double>();
        foreach (int d in departs)
        {
            distances[d] = 0;
            file.Enqueue(d, 0);
        }

        int? trouve = null;
        while (file.TryDequeue(out int courant, out double dist))
        {
            if (dist > distances[courant]) continue;
            if (arrivees.Contains(courant)) { trouve = courant; break; }
            foreach (var (vers, minutes, _) in voisins[courant])
            {
                double nouvelle = dist + minutes;
                if (nouvelle < distances.GetValueOrDefault(vers, double.PositiveInfinity))
                {
                    distances[vers] = nouvelle;
                    predecesseurs[vers] = courant;
                    file.Enqueue(vers, nouvelle);
                }
            }
        }
        if (trouve == null) return null;

        var chemin = new List<int>();
        for (int n = trouve.Value; ; n = predecesseurs[n])
        {
            chemin.Add(n);
            if (!predecesseurs.ContainsKey(n)) break;
        }
        chemin.Reverse();

        // On retire les passages entre quais d'une même station (même nom, même ligne)
        var nettoye = new List<int> { chemin[0] };
        foreach (int id in chemin.Skip(1))
        {
            var dernier = stations[nettoye[^1]];
            if (Cle(dernier.Nom) == Cle(stations[id].Nom) && dernier.Ligne == stations[id].Ligne) continue;
            nettoye.Add(id);
        }

        var etapes = nettoye.Select(id =>
        {
            var s = stations[id];
            return new EtapeTrajet(id, s.Nom, s.Ligne, s.Lat, s.Lon, distances[id]);
        }).ToList();

        // Regroupement par ligne empruntée
        var troncons = new List<Troncon>();
        int debut = 0;
        for (int i = 1; i <= etapes.Count; i++)
        {
            bool fin = i == etapes.Count || etapes[i].Ligne != etapes[debut].Ligne;
            if (!fin) continue;
            if (i - 1 > debut)
                troncons.Add(new Troncon(etapes[debut].Ligne, etapes[debut].Nom, etapes[i - 1].Nom, i - 1 - debut, etapes[i - 1].Minutes - etapes[debut].Minutes));
            debut = i;
        }

        return new Itineraire(distances[trouve.Value], etapes, troncons, Math.Max(0, troncons.Count - 1));
    }

    private List<int> Homonymes(int id)
    {
        string cle = Cle(stations[id].Nom);
        return stations.Values.Where(s => Cle(s.Nom) == cle).Select(s => s.Id).ToList();
    }

    public static int OrdreLigne(string ligne)
    {
        string chiffres = new string(ligne.TakeWhile(char.IsDigit).ToArray());
        int n = int.TryParse(chiffres, out int v) ? v : 99;
        return n * 10 + (ligne.Contains("bis") ? 5 : 0);
    }

    public static double DistanceKm(Station a, Station b)
    {
        const double R = 6371;
        double dLat = (b.Lat - a.Lat) * Math.PI / 180;
        double dLon = (b.Lon - a.Lon) * Math.PI / 180;
        double h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(a.Lat * Math.PI / 180) * Math.Cos(b.Lat * Math.PI / 180) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * R * Math.Asin(Math.Sqrt(h));
    }

    /// <summary>
    /// Clé de comparaison : minuscules, sans accents, sans ponctuation
    /// </summary>
    public static string Cle(string nom)
    {
        var sb = new StringBuilder();
        foreach (char c in nom.ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            if (char.IsLetterOrDigit(c) && CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString();
    }

    private static double Nombre(string texte) =>
        double.TryParse(texte.Replace(',', '.').Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : 0;
}
