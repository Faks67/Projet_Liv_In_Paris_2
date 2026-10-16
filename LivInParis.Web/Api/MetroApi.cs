using LivInParis.Web.Services;

namespace LivInParis.Web.Api;

public record Point(double Lat, double Lon, string Libelle);

/// <summary>
/// Trajet porte à porte : marche jusqu'au métro, métro (Dijkstra), marche jusqu'à l'adresse
/// </summary>
public record Trajet(
    Point Depart, Point Arrivee,
    object StationDepart, object StationArrivee,
    double MarcheDepart, double MarcheArrivee,
    Itineraire? Metro, double TotalMinutes);

public static class MetroApi
{
    public static void MapMetroApi(this WebApplication app)
    {
        app.MapGet("/api/metro/stations", (Metro metro) => metro.StationsUniques());

        app.MapGet("/api/metro/plan", (Metro metro) => new
        {
            segments = metro.Segments(),
            stations = metro.Stations.Select(s => new { s.Id, s.Nom, s.Ligne, s.Lat, s.Lon })
        });

        app.MapGet("/api/metro/proche", (double lat, double lon, Metro metro) =>
        {
            var (station, km, minutes) = metro.PlusProche(lat, lon);
            return new { station = Decrire(station), km = Math.Round(km, 2), minutesAPied = minutes };
        });

        // Trajet entre deux points : chacun est soit une station (id), soit des coordonnées GPS (adresse)
        app.MapGet("/api/metro/trajet", (int? de, double? deLat, double? deLon, string? deNom,
                                          int? vers, double? versLat, double? versLon, string? versNom, Metro metro) =>
        {
            Point? a = PointDepuis(metro, de, deLat, deLon, deNom);
            Point? b = PointDepuis(metro, vers, versLat, versLon, versNom);
            if (a == null || b == null) return Session.Erreur("Départ ou arrivée invalide.");
            var trajet = Calculer(metro, a, b);
            return trajet == null ? Session.Erreur("Aucun itinéraire trouvé.", 404) : Results.Ok(trajet);
        });
    }

    private static Point? PointDepuis(Metro metro, int? idStation, double? lat, double? lon, string? nom)
    {
        if (lat is double la && lon is double lo) return new Point(la, lo, nom ?? "Adresse");
        if (idStation is int id && metro.Get(id) is Station s) return new Point(s.Lat, s.Lon, s.Nom);
        return null;
    }

    public static object Decrire(Station s) => new { s.Id, s.Nom, s.Ligne, s.Lat, s.Lon, s.Commune };

    public static Trajet? Calculer(Metro metro, Point depart, Point arrivee)
    {
        var (sa, _, marcheA) = metro.PlusProche(depart.Lat, depart.Lon);
        var (sb, _, marcheB) = metro.PlusProche(arrivee.Lat, arrivee.Lon);
        var itineraire = metro.Dijkstra(sa.Id, sb.Id);
        if (itineraire == null) return null;

        // Si marcher directement est plus rapide que le métro, on ne prend pas le métro
        var directe = new Station(0, "", "", depart.Lat, depart.Lon, "");
        double marcheDirecte = Math.Round(Metro.DistanceKm(directe, new Station(0, "", "", arrivee.Lat, arrivee.Lon, "")) * 1.3 / 4.5 * 60);
        double total = marcheA + itineraire.Minutes + marcheB;
        if (marcheDirecte <= total)
            return new Trajet(depart, arrivee, Decrire(sa), Decrire(sb), marcheDirecte, 0, null, marcheDirecte);

        return new Trajet(depart, arrivee, Decrire(sa), Decrire(sb), marcheA, marcheB, itineraire, total);
    }
}
