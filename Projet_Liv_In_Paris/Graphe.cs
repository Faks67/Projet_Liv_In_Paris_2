using OfficeOpenXml;
using System.Drawing;
using System.Globalization;

public class Graphe<T>
{
    private Dictionary<int, Noeud<int>> noeuds;
    private List<Lien<int>> liens;
    private Dictionary<int, string> stationsParId;
    public List<Liaison> liaisons { get; set; }


    public Graphe()
    {
        noeuds = new Dictionary<int, Noeud<int>>();
        liens = new List<Lien<int>>();
        stationsParId = new Dictionary<int, string>();
    }

    public class Liaison
    {
        public int Source { get; set; }
        public int Destination { get; set; }
        public double Poids { get; set; }  // Par exemple, le temps ou la distance
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
                // Charger la première feuille du fichier Excel
                var feuille = package.Workbook.Worksheets[0];

                int nbLignes = feuille.Dimension.Rows;

                // Initialiser le dictionnaire pour gérer les stations par nom
                Dictionary<string, List<int>> stationsParNom = new Dictionary<string, List<int>>();

                // Charger les stations et les liaisons depuis le fichier Excel
                for (int i = 2; i <= nbLignes; i++) // Assumer que la première ligne est un en-tête
                {
                    int idStation = int.Parse(feuille.Cells[i, 1].Text);
                    string nomStation = NormaliserNom(feuille.Cells[i, 3].Text);

                    if (string.IsNullOrWhiteSpace(nomStation))
                        continue;

                    // Ajouter les stations à la collection avec leurs noms
                    stationsParId[idStation] = nomStation;

                    // Ajouter les stations au dictionnaire par nom
                    if (!stationsParNom.ContainsKey(nomStation))
                    {
                        stationsParNom[nomStation] = new List<int>();
                    }
                    stationsParNom[nomStation].Add(idStation); // Associer chaque nom à plusieurs IDs

                    // Extraire et convertir les valeurs latitude et longitude
                    string longitudeStr = feuille.Cells[i, 4].Text.Replace(",", ".").Trim();
                    string latitudeStr = feuille.Cells[i, 5].Text.Replace(",", ".").Trim();

                    if (!double.TryParse(latitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude) ||
                        !double.TryParse(longitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude))
                    {
                        Console.WriteLine($"❌ Erreur conversion (ligne {i}) : Latitude: '{latitudeStr}', Longitude: '{longitudeStr}'");
                        continue; // Passe à la ligne suivante si la conversion échoue
                    }

                    // Ajouter la station au dictionnaire de noeuds
                    noeuds[idStation] = new Noeud<int>(idStation, latitude, longitude);
                }

                // Charger les liaisons (arcs) depuis les colonnes 8 (ID précédent), 9 (ID suivant) et 12 (Poids)
                for (int i = 2; i <= nbLignes; i++)
                {
                    int idPrecedent = int.Parse(feuille.Cells[i, 8].Text);  // ID de la station précédente (colonne 8)
                    int idSuivant = int.Parse(feuille.Cells[i, 9].Text);    // ID de la station suivante (colonne 9)
                    double poids = double.Parse(feuille.Cells[i, 12].Text, CultureInfo.InvariantCulture);  // Poids de la liaison (colonne 12)

                    // Si l'un des IDs est 0, il n'y a pas de liaison
                    if (idPrecedent == 0 || idSuivant == 0)
                    {
                        continue; // Ignore cette ligne si il n'y a pas de liaison
                    }

                    // Vérifier si les deux stations existent dans le dictionnaire des stations
                    if (stationsParId.ContainsKey(idPrecedent) && stationsParId.ContainsKey(idSuivant))
                    {
                        // Ajouter la liaison entre la station précédente et la suivante
                        liens.Add(new Lien<int>(idPrecedent, idSuivant, poids));
                        // Ajouter aussi la liaison bidirectionnelle : de la station suivante vers la station précédente
                        liens.Add(new Lien<int>(idSuivant, idPrecedent, poids));
                    }
                }

                // Ajouter des liaisons supplémentaires entre les stations avec des noms similaires (ou identiques)
                foreach (var nom in stationsParNom.Keys)
                {
                    var ids = stationsParNom[nom];
                    if (ids.Count > 1)
                    {
                        // Relier chaque station ayant le même nom
                        for (int j = 0; j < ids.Count; j++)
                        {
                            for (int k = j + 1; k < ids.Count; k++)
                            {
                                int id1 = ids[j];
                                int id2 = ids[k];

                                // Ici vous pouvez ajuster la logique de poids, par exemple en calculant la distance
                                double poids = CalculerPoids(id1, id2);

                                // Ajouter la liaison entre ces deux stations
                                liens.Add(new Lien<int>(id1, id2, poids));
                                liens.Add(new Lien<int>(id2, id1, poids));  // Bidirectionnel
                            }
                        }
                    }
                }
            }

            Console.WriteLine("✅ Chargement du graphe terminé !");
            AfficherGraphe();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur lors du chargement de l'Excel : {ex.Message}");
        }
    }
     
    private double CalculerPoids(int id1, int id2)
{
    return 1.0; // Poids constant pour toutes les connexions entre stations du même nom
}


    // Méthode pour afficher les stations et les liaisons dans la console
    public void AfficherGraphe()
    {
        // Affichage des stations
        Console.WriteLine("\n📌 Liste des stations :");
        foreach (var noeud in noeuds)
        {
            Console.WriteLine($"{noeud.Key}: {stationsParId[noeud.Key]}");
        }

        // Affichage des liaisons
        Console.WriteLine("\n📌 Liste des liaisons :");
        foreach (var lien in liens)
        {
            Console.WriteLine($"{lien.Source} ({stationsParId[lien.Source]}) ↔ {lien.Destination} ({stationsParId[lien.Destination]}) (Poids : {lien.Poids})");
        }
    }

    public void AfficherGrapheVisuel()
    {
        // Définir les dimensions de l'image
        int largeur = 1600;
        int hauteur = 1200;
        Bitmap image = new Bitmap(largeur, hauteur);
        Graphics g = Graphics.FromImage(image);
        g.Clear(Color.White); // Fond blanc

        // Déterminer les limites des coordonnées
        double minLongitude = noeuds.Values.Min(n => n.Longitude);
        double maxLongitude = noeuds.Values.Max(n => n.Longitude);
        double minLatitude = noeuds.Values.Min(n => n.Latitude);
        double maxLatitude = noeuds.Values.Max(n => n.Latitude);

        // Dictionnaire pour stocker les positions des stations
        Dictionary<int, PointF> positions = new Dictionary<int, PointF>();

        foreach (var noeud in noeuds)
        {
            // Normalisation des coordonnées pour le placement sur l'image
            float x = (float)((noeud.Value.Longitude - minLongitude) / (maxLongitude - minLongitude) * (largeur - 100)) + 50;
            float y = (float)((noeud.Value.Latitude - minLatitude) / (maxLatitude - minLatitude) * (hauteur - 100)) + 50;

            positions[noeud.Key] = new PointF(x, y);
        }

        // Police pour les noms des stations
        Font font = new Font("Arial", 8, FontStyle.Bold);
        Brush brush = Brushes.Blue;

        // Dessiner les stations avec leurs noms bien centrés
        Dictionary<int, SizeF> taillesTextes = new Dictionary<int, SizeF>();

        foreach (var noeud in noeuds)
        {
            if (positions.ContainsKey(noeud.Key))
            {
                PointF position = positions[noeud.Key];
                string stationNom = stationsParId[noeud.Key];

                // Mesurer la taille du texte pour centrer correctement
                SizeF textSize = g.MeasureString(stationNom, font);
                PointF textPosition = new PointF(position.X - textSize.Width / 2, position.Y - textSize.Height / 2);

                // Enregistrer la taille pour l'ajustement des lignes
                taillesTextes[noeud.Key] = textSize;

                // Dessiner le nom de la station
                g.DrawString(stationNom, font, brush, textPosition);
            }
        }

        // Dessiner les liaisons entre les stations avec des traits plus fins
        Pen penLiaison = new Pen(Color.Black, 1);  // Épaisseur plus fine

        foreach (var lien in liens)
        {
            if (positions.ContainsKey(lien.Source) && positions.ContainsKey(lien.Destination))
            {
                PointF pointSource = positions[lien.Source];
                PointF pointDest = positions[lien.Destination];

                // Dessiner une ligne fine entre les stations
                g.DrawLine(penLiaison, pointSource, pointDest);
            }
        }

        // Sauvegarde de l'image
        string cheminImage = Path.Combine(Directory.GetCurrentDirectory(), "graphe_visuel.png");
        image.Save(cheminImage, System.Drawing.Imaging.ImageFormat.Png);
        Console.WriteLine($"✅ Image générée et sauvegardée : {cheminImage}");

        // Libérer les ressources
        g.Dispose();
        image.Dispose();
    }

    public bool EstConnexe()
    {
        var visite = new HashSet<int>();
        var stack = new Stack<int>();

        // Commencer depuis un noeud arbitraire, ici le premier noeud
        int noeudDepart = noeuds.Keys.First();
        stack.Push(noeudDepart);

        while (stack.Count > 0)
        {
            int current = stack.Pop();
            if (!visite.Contains(current))
            {
                visite.Add(current);
                var voisins = liens.Where(l => l.Source == current).Select(l => l.Destination);
                foreach (var voisin in voisins)
                {
                    if (!visite.Contains(voisin))
                    {
                        stack.Push(voisin);
                    }
                }
            }
        }

        return visite.Count == noeuds.Count;
    }
    public void TrouverCheminPlusCourt(int depart, int arrivee)
    {
        if (!stationsParId.ContainsKey(depart) || !stationsParId.ContainsKey(arrivee))
        {
            Console.WriteLine("❌ Station de départ ou d'arrivée invalide.");
            return;
        }

        if (!EstConnexe())
        {
            Console.WriteLine("❌ Le graphe n'est pas connexe. Impossible de garantir un chemin.");
            return;
        }

        var (chemin, poidsTotal) = Dijkstra(depart, arrivee);
        if (chemin.Count == 0)
        {
            Console.WriteLine("⚠️ Aucun chemin trouvé entre ces stations.");
            return;
        }

        Console.WriteLine($"\n🚇 Chemin le plus court de {stationsParId[depart]} à {stationsParId[arrivee]} :");
        Console.WriteLine($"Poids total du trajet : {poidsTotal} min");

        for (int i = 0; i < chemin.Count; i++)
        {
            int station = chemin[i];
            Console.Write(stationsParId[station]);
            if (i < chemin.Count - 1)
            {
                Console.Write(" → ");
            }
        }
        Console.WriteLine();
    }

    // Méthode Dijkstra à conserver
    public (List<int>, double) Dijkstra(int depart, int arrivee)
    {
        var distances = new Dictionary<int, double>();
        var predecesseurs = new Dictionary<int, int>();
        var nonVisites = new HashSet<int>(noeuds.Keys);

        foreach (var noeud in noeuds.Keys)
        {
            distances[noeud] = double.MaxValue;
        }
        distances[depart] = 0;

        // Utiliser une SortedSet avec un custom comparer pour gérer les distances
        var priorityQueue = new SortedSet<Tuple<double, int>>(Comparer<Tuple<double, int>>.Create((x, y) => x.Item1.CompareTo(y.Item1)));
        priorityQueue.Add(Tuple.Create(0.0, depart));

        while (priorityQueue.Count > 0)
        {
            var current = priorityQueue.Min;
            priorityQueue.Remove(current);
            double currentDist = current.Item1;  // Accès à la distance avec Item1
            int u = current.Item2;  // Accès à l'ID du noeud avec Item2

            if (u == arrivee) break;

            foreach (var lien in liens.Where(l => l.Source == u))
            {
                int v = lien.Destination;
                double alt = currentDist + lien.Poids;

                if (alt < distances[v])
                {
                    distances[v] = alt;
                    predecesseurs[v] = u;
                    priorityQueue.Add(Tuple.Create(distances[v], v));  // Ajouter avec la distance correcte
                }
            }
        }

        var chemin = new List<int>();
        int? actuel = arrivee;
        while (actuel.HasValue && predecesseurs.ContainsKey(actuel.Value))
        {
            chemin.Insert(0, actuel.Value);
            actuel = predecesseurs[actuel.Value];
        }
        if (chemin.Count > 0) chemin.Insert(0, depart);

        return (chemin, distances[arrivee]);
    }




    public (List<int>, double) BellmanFord(int source, int destination)
    {
        // Initialisation des distances et des prédécesseurs
        Dictionary<int, double> distances = new Dictionary<int, double>();
        Dictionary<int, int> pred = new Dictionary<int, int>(); // Pour retrouver le chemin
        List<int> chemin = new List<int>();

        // Initialisation des distances
        foreach (var station in noeuds.Keys)
        {
            distances[station] = double.PositiveInfinity;
            pred[station] = -1;
        }

        // La distance à la station source est 0
        distances[source] = 0;

        // Relaxation des arêtes (n-1) fois
        for (int i = 1; i < noeuds.Count; i++)
        {
            foreach (var lien in liaisons)
            {
                if (distances[lien.Source] + lien.Poids < distances[lien.Destination])
                {
                    distances[lien.Destination] = distances[lien.Source] + lien.Poids;
                    pred[lien.Destination] = lien.Source;
                }
            }
        }

        // Vérifier les cycles négatifs (facultatif ici)
        foreach (var lien in liaisons)
        {
            if (distances[lien.Source] + lien.Poids < distances[lien.Destination])
            {
                Console.WriteLine("🚨 Le graphe contient un cycle négatif !");
                return (null, double.PositiveInfinity);
            }
        }

        // Reconstruire le chemin depuis la destination jusqu'à la source
        int current = destination;
        while (current != -1)
        {
            chemin.Insert(0, current);
            current = pred[current];
        }

        // Retourner le chemin et la distance
        return (chemin, distances[destination]);
    }



    public double[,] FloydWarshall()
    {
        int n = noeuds.Count;
        int[] ids = noeuds.Keys.ToArray();
        Dictionary<int, int> indexMap = ids.Select((id, index) => new { id, index }).ToDictionary(x => x.id, x => x.index);

        // Initialisation de la matrice des distances
        double[,] distances = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j) distances[i, j] = 0;
                else distances[i, j] = double.PositiveInfinity;
            }
        }

        // Remplissage avec les valeurs initiales des liens
        foreach (var lien in liens)
        {
            int u = indexMap[lien.Source];
            int v = indexMap[lien.Destination];
            distances[u, v] = lien.Poids;
        }

        // Algorithme de Floyd-Warshall
        for (int k = 0; k < n; k++)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (distances[i, k] + distances[k, j] < distances[i, j])
                    {
                        distances[i, j] = distances[i, k] + distances[k, j];
                    }
                }
            }
        }

        return distances;
    }

    public string GetStationNom(int idStation)
    {
        // Supposons que tu as un dictionnaire des stations
        return stationsParId.ContainsKey(idStation) ? stationsParId[idStation] : "Station inconnue";
    }
}