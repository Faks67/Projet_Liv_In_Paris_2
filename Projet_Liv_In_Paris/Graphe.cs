using OfficeOpenXml;
using System.Drawing;

using System.Globalization;

public class Graphe<T>

{

    private Dictionary<int, Noeud<int>> noeuds;

    private List<Lien<int>> liens;

    private Dictionary<int, string> stationsParId;

    private Dictionary<int, int> previous;

    public Graphe()

    {

        noeuds = new Dictionary<int, Noeud<int>>();

        liens = new List<Lien<int>>();

        stationsParId = new Dictionary<int, string>();

        previous = new Dictionary<int, int>();

    }

    public static string NormaliserNom(string nom)

    {

        return nom.ToLower().Trim().Replace(",", ".");

    }

    public void ChargerDepuisExcel(string cheminFichier)

    {

        try

        {

            if (!File.Exists(cheminFichier))

            {

                Console.WriteLine($"❌ Fichier non trouvé : {cheminFichier}");

                return;

            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(cheminFichier)))

            {

                var feuilleNoeuds = package.Workbook.Worksheets[0];

                var feuilleArcs = package.Workbook.Worksheets[1];

                int nbLignesNoeuds = feuilleNoeuds.Dimension.Rows;

                for (int i = 2; i <= nbLignesNoeuds; i++)

                {

                    int idStation = int.Parse(feuilleNoeuds.Cells[i, 1].Text);

                    string nomStation = NormaliserNom(feuilleNoeuds.Cells[i, 3].Text);

                    if (string.IsNullOrWhiteSpace(nomStation))

                        continue;

                    stationsParId[idStation] = nomStation;

                    // Extraire et convertir les valeurs latitude et longitude

                    string longitudeStr = feuilleNoeuds.Cells[i, 4].Text.Replace(",", ".").Trim();

                    string latitudeStr = feuilleNoeuds.Cells[i, 5].Text.Replace(",", ".").Trim();

                    if (!double.TryParse(latitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude) ||

                        !double.TryParse(longitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude))

                    {

                        Console.WriteLine($"❌ Erreur conversion (ligne {i}) : Latitude: '{latitudeStr}', Longitude: '{longitudeStr}'");

                        continue; // Passe à la ligne suivante si la conversion échoue

                    }

                    // Maintenant, on peut utiliser latitude et longitude

                    noeuds[idStation] = new Noeud<int>(idStation, latitude, longitude);


                }

                int nbLignesArcs = feuilleArcs.Dimension.Rows;

                for (int i = 2; i <= nbLignesArcs; i++)

                {

                    int idPrecedent = int.Parse(feuilleArcs.Cells[i, 3].Text);

                    int idSuivant = int.Parse(feuilleArcs.Cells[i, 4].Text);

                    double poids = double.Parse(feuilleArcs.Cells[i, 5].Text, CultureInfo.InvariantCulture);

                    if (stationsParId.ContainsKey(idPrecedent) && stationsParId.ContainsKey(idSuivant))

                    {

                        liens.Add(new Lien<int>(idPrecedent, idSuivant, poids));

                    }

                }

            }

            Console.WriteLine("✅ Chargement du graphe terminé !");

        }

        catch (Exception ex)

        {

            Console.WriteLine($"❌ Erreur lors du chargement de l'Excel : {ex.Message}");

        }

    }

    public void AfficherGraphe()

    {

        Console.WriteLine("\n📌 Liste des stations :");

        foreach (var noeud in noeuds)

        {

            Console.WriteLine($"{noeud.Key}: {stationsParId[noeud.Key]}");

        }

        Console.WriteLine("\n📌 Liste des liaisons :");

        foreach (var lien in liens)

        {

            Console.WriteLine($"{lien.Source} ({stationsParId[lien.Source]}) ↔ {lien.Destination} ({stationsParId[lien.Destination]}) (Poids : {lien.Poids})");

        }

    }

    public Dictionary<int, int> Dijkstra(int start)

    {

        var distances = new Dictionary<int, double>();

        var previous = new Dictionary<int, int>();

        var priorityQueue = new List<int>();

        foreach (var node in noeuds.Keys)

        {

            distances[node] = double.MaxValue;

            previous[node] = -1;

            priorityQueue.Add(node);

        }

        distances[start] = 0;

        while (priorityQueue.Count > 0)

        {

            var currentNode = priorityQueue.OrderBy(x => distances[x]).First();

            priorityQueue.Remove(currentNode);

            foreach (var lien in liens.Where(l => l.Source == currentNode))

            {

                var alt = distances[currentNode] + lien.Poids;

                if (alt < distances[lien.Destination])

                {

                    distances[lien.Destination] = alt;

                    previous[lien.Destination] = currentNode;

                }

            }

        }

        return previous;

    }

    public List<int> ReconstituerChemin(int source, int destination, Dictionary<int, int> previous)

    {

        var chemin = new List<int>();

        if (!previous.ContainsKey(destination)) return chemin;

        int currentStation = destination;

        while (currentStation != source)

        {

            chemin.Add(currentStation);

            if (previous[currentStation] == -1) return new List<int>();

            currentStation = previous[currentStation];

        }

        chemin.Add(source);

        chemin.Reverse();

        return chemin;

    }

    public void ExplorerChemin(int start, int destination)

    {

        var previous = Dijkstra(start);

        var chemin = ReconstituerChemin(start, destination, previous);

        if (chemin.Count == 0)

        {

            Console.WriteLine("❌ Aucun chemin trouvé.");

            return;

        }

        Console.WriteLine($"Chemin de {start} ({stationsParId[start]}) à {destination} ({stationsParId[destination]}):");

        double totalTime = 0;

        for (int i = 0; i < chemin.Count - 1; i++)

        {

            int currentStation = chemin[i];

            int nextStation = chemin[i + 1];

            var lien = liens.FirstOrDefault(l => l.Source == currentStation && l.Destination == nextStation);

            if (lien != null)

            {

                totalTime += lien.Poids;

                Console.WriteLine($"{currentStation} ({stationsParId[currentStation]}) ↔ {nextStation} ({stationsParId[nextStation]}) (Temps : {lien.Poids} minutes)");

            }

        }

        Console.WriteLine($"Temps total du trajet : {totalTime} minutes");



    }

    public bool StationsExist(int source, int destination)

    {

        return noeuds.ContainsKey(source) && noeuds.ContainsKey(destination);

    }

    public void GenererGrapheChemin(int start, int destination)
    {
        // Calculer le chemin le plus court entre start et destination
        var previous = Dijkstra(start);
        var chemin = ReconstituerChemin(start, destination, previous);

        if (chemin.Count == 0)
        {
            Console.WriteLine("❌ Aucun chemin trouvé.");
            return;
        }

        // Créer une image pour dessiner le graphe
        int largeur = 800;
        int hauteur = 600;
        Bitmap image = new Bitmap(largeur, hauteur);
        Graphics g = Graphics.FromImage(image);
        g.Clear(Color.White); // Fond blanc

        // Définir des paramètres pour dessiner les noeuds et les arcs
        int rayonNoeud = 40; // Augmenter encore la taille du cercle
        int marge = 50;
        int espacement = (largeur - 2 * marge) / chemin.Count; // Espacement dynamique basé sur le nombre de stations

        // Dessiner les stations
        Dictionary<int, PointF> positions = new Dictionary<int, PointF>();
        for (int i = 0; i < chemin.Count; i++)
        {
            int stationId = chemin[i];
            float x = marge + i * espacement;
            float y = hauteur / 2;

            // Dessiner le cercle pour la station
            g.FillEllipse(Brushes.LightBlue, x - rayonNoeud, y - rayonNoeud, rayonNoeud * 2, rayonNoeud * 2);
            g.DrawEllipse(Pens.Black, x - rayonNoeud, y - rayonNoeud, rayonNoeud * 2, rayonNoeud * 2);

            // Ajouter le texte de la station avec saut de ligne pour les noms longs
            string stationNom = stationsParId[stationId];
            string[] mots = stationNom.Split(' ');
            int ligneHeight = 12; // Hauteur d'une ligne
            float yPosition = y - (mots.Length * ligneHeight) / 2;

            for (int j = 0; j < mots.Length; j++)
            {
                g.DrawString(mots[j], new Font("Arial", 8), Brushes.Black, new PointF(x - g.MeasureString(mots[j], new Font("Arial", 8)).Width / 2, yPosition + j * ligneHeight));
            }

            positions[stationId] = new PointF(x, y);
        }

        // Dessiner les liaisons entre les stations dans le chemin
        for (int i = 0; i < chemin.Count - 1; i++)
        {
            int currentStation = chemin[i];
            int nextStation = chemin[i + 1];

            PointF pointCurrent = positions[currentStation];
            PointF pointNext = positions[nextStation];

            // Calculer la direction de la ligne pour éviter que le trait passe dans le cercle
            float angle = (float)Math.Atan2(pointNext.Y - pointCurrent.Y, pointNext.X - pointCurrent.X);
            float xStart = pointCurrent.X + rayonNoeud * (float)Math.Cos(angle); // Position à l'extrémité du cercle
            float yStart = pointCurrent.Y + rayonNoeud * (float)Math.Sin(angle); // Position à l'extrémité du cercle
            float xEnd = pointNext.X - rayonNoeud * (float)Math.Cos(angle); // Position à l'extrémité du cercle
            float yEnd = pointNext.Y - rayonNoeud * (float)Math.Sin(angle); // Position à l'extrémité du cercle

            // Dessiner une ligne entre les stations
            g.DrawLine(Pens.Black, xStart, yStart, xEnd, yEnd);

            // Ajouter le poids au-dessus de la ligne
            double poids = liens.FirstOrDefault(l => l.Source == currentStation && l.Destination == nextStation)?.Poids ?? 0;
            float xPoids = (xStart + xEnd) / 2;
            float yPoids = (yStart + yEnd) / 2 - 10; // Placer le poids légèrement au-dessus de la ligne
            g.DrawString(poids.ToString("F2") + " min", new Font("Arial", 8), Brushes.Black, new PointF(xPoids, yPoids));
        }

        // Sauvegarder l'image dans un fichier
        string cheminImage = Path.Combine(Directory.GetCurrentDirectory(), "graphe_chemin.png");
        image.Save(cheminImage, System.Drawing.Imaging.ImageFormat.Png);
        Console.WriteLine($"✅ Image générée et sauvegardée à l'emplacement : {cheminImage}");

        // Nettoyage des ressources
        g.Dispose();
        image.Dispose();
    }



}
