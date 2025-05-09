using OfficeOpenXml;
using System.Drawing;
using System.Globalization;

public class Graphe<T>
{
    private Dictionary<int, Noeud<int>> noeuds;
    private List<Lien<int>> liens;
    private Dictionary<int, string> stationsParId;
    public List<Lien> liaisons { get; set; }
    private Dictionary<int, string> lignesParStation = new Dictionary<int, string>();
    public Dictionary<int, List<int>> Adjacence { get; set; } = new();



    public Graphe()
    {
        noeuds = new Dictionary<int, Noeud<int>>();
        liens = new List<Lien<int>>();
        stationsParId = new Dictionary<int, string>();
    }

    public class Lien
    {
        public int Source { get; set; }
        public int Destination { get; set; }
        public double Poids { get; set; }  
    }


    public static string NormaliserNom(string nom)
    {
        return nom.ToLower().Trim().Replace(",", ".");
    }
    /// <summary>
    /// méthode afin de lire le fichier excel, on se base sur les collones et on applique les liaisons
    /// </summary>
    /// <param name="cheminFichier"></param>
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
                var feuille = package.Workbook.Worksheets[0];
                int nbLignes = feuille.Dimension.Rows;

                Dictionary<string, List<int>> stationsParNom = new Dictionary<string, List<int>>();
                Dictionary<int, string> lignesParStation = new Dictionary<int, string>(); 

                for (int i = 2; i <= nbLignes; i++) 
                {
                    int idStation = int.Parse(feuille.Cells[i, 1].Text);
                    string ligne = feuille.Cells[i, 2].Text.Trim(); 
                    string nomStation = NormaliserNom(feuille.Cells[i, 3].Text);

                    if (string.IsNullOrWhiteSpace(nomStation))
                        continue;

                    stationsParId[idStation] = nomStation;
                    lignesParStation[idStation] = ligne; 

                    if (!stationsParNom.ContainsKey(nomStation))
                    {
                        stationsParNom[nomStation] = new List<int>();
                    }
                    stationsParNom[nomStation].Add(idStation);

                    string longitudeStr = feuille.Cells[i, 4].Text.Replace(",", ".").Trim();
                    string latitudeStr = feuille.Cells[i, 5].Text.Replace(",", ".").Trim();

                    if (!double.TryParse(latitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude) ||
                        !double.TryParse(longitudeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude))
                    {
                        Console.WriteLine($"❌ Erreur conversion (ligne {i}) : Latitude: '{latitudeStr}', Longitude: '{longitudeStr}'");
                        continue;
                    }

                    noeuds[idStation] = new Noeud<int>(idStation, latitude, longitude);
                }

                for (int i = 2; i <= nbLignes; i++)
                {
                    if (!int.TryParse(feuille.Cells[i, 8].Text, out int idPrecedent) ||
                        !int.TryParse(feuille.Cells[i, 9].Text, out int idSuivant) ||
                        !double.TryParse(feuille.Cells[i, 12].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double poids))
                    {
                        continue;
                    }

                    if (idPrecedent == 0 || idSuivant == 0)
                        continue;
                   
                    string nomStation = NormaliserNom(feuille.Cells[i, 3].Text);

                    if (stationsParNom.ContainsKey(nomStation))
                    {
                        var idsStationNom = stationsParNom[nomStation];

                        foreach (int idNom in idsStationNom)
                        {
                            if (stationsParId.ContainsKey(idPrecedent))
                            {
                                liens.Add(new Lien<int>(idPrecedent, idNom, poids));
                                liens.Add(new Lien<int>(idNom, idPrecedent, poids));
                            }

                            if (stationsParId.ContainsKey(idSuivant))
                            {
                                liens.Add(new Lien<int>(idSuivant, idNom, poids));
                                liens.Add(new Lien<int>(idNom, idSuivant, poids));
                            }
                        }
                    }

                }

                foreach (var nom in stationsParNom.Keys)
                {
                    var ids = stationsParNom[nom];
                    if (ids.Count > 1)
                    {
                        for (int j = 0; j < ids.Count; j++)
                        {
                            for (int k = j + 1; k < ids.Count; k++)
                            {
                                int id1 = ids[j];
                                int id2 = ids[k];
                                double poids = CalculerPoids(id1, id2);

                                liens.Add(new Lien<int>(id1, id2, poids));
                                liens.Add(new Lien<int>(id2, id1, poids));
                            }
                        }
                    }
                }

                Console.WriteLine("✅ Chargement du graphe terminé !");
                AfficherGraphe();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur lors du chargement de l'Excel : {ex.Message}");
        }
    }

    /// <summary>
    /// cette méthode c'est juste pour ajouter des poids de 1 entre les stations de meme nom
    /// </summary>
    /// <param name="id1"></param>
    /// <param name="id2"></param>
    /// <returns></returns>
    private double CalculerPoids(int id1, int id2)
    {
        return 1.0; 
    }


    /// <summary>
    /// avec cette méthode j'affiche les liaisons dans la console ainsi que les stations pour faciliter la  recherche et voir si il ne manque pas des liaisons
    /// </summary>
    public void AfficherGraphe()
    {
      

        Console.WriteLine("\n📌 Liste des liaisons :");
        foreach (var lien in liens)
        {
            Console.WriteLine($"{lien.Source} ({stationsParId[lien.Source]}) ↔ {lien.Destination} ({stationsParId[lien.Destination]}) (Poids : {lien.Poids})");
        }

        Console.WriteLine("\n📌 Liste des stations :");
        foreach (var noeud in noeuds)
        {
            Console.WriteLine($"{noeud.Key}: {stationsParId[noeud.Key]}");
        }
    }


    public void AfficherGrapheVisuel(List<int> chemin = null)
    {
        int largeur = 1600;
        int hauteur = 1200;
        Bitmap image = new Bitmap(largeur, hauteur);
        Graphics g = Graphics.FromImage(image);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.White);

        double minLongitude = noeuds.Values.Min(n => n.Longitude);
        double maxLongitude = noeuds.Values.Max(n => n.Longitude);
        double minLatitude = noeuds.Values.Min(n => n.Latitude);
        double maxLatitude = noeuds.Values.Max(n => n.Latitude);

        Dictionary<int, PointF> positions = new Dictionary<int, PointF>();

        foreach (var noeud in noeuds)
        {
            float x = (float)((noeud.Value.Longitude - minLongitude) / (maxLongitude - minLongitude) * (largeur - 100)) + 50;
            float yOriginal = (float)((noeud.Value.Latitude - minLatitude) / (maxLatitude - minLatitude) * (hauteur - 100)) + 50;
            float y = hauteur - yOriginal;
            positions[noeud.Key] = new PointF(x, y);
        }

        Font font = new Font("Arial", 6, FontStyle.Bold);
        Brush brushTexte = Brushes.Blue;
        Brush brushStation = Brushes.DarkRed;

        // 🎨 Couleurs officielles des lignes du métro parisien
        Dictionary<string, Color> couleursParLigne = new Dictionary<string, Color>()
    {
        { "1", Color.FromArgb(255, 210, 0) },
        { "2", Color.FromArgb(0, 164, 228) },
        { "3", Color.FromArgb(184, 136, 52) },
        { "3bis", Color.FromArgb(161, 202, 72) },
        { "4", Color.FromArgb(155, 48, 108) },
        { "5", Color.FromArgb(255, 121, 0) },
        { "6", Color.FromArgb(118, 195, 208) },
        { "7", Color.FromArgb(236, 100, 140) },
        { "7bis", Color.FromArgb(132, 204, 202) },
        { "8", Color.FromArgb(173, 79, 132) },
        { "9", Color.FromArgb(201, 179, 0) },
        { "10", Color.FromArgb(221, 155, 124) },
        { "11", Color.FromArgb(167, 103, 38) },
        { "12", Color.FromArgb(0, 125, 58) },
        { "13", Color.FromArgb(142, 186, 229) },
        { "14", Color.FromArgb(158, 77, 168) }
    };

        foreach (var lien in liens)
        {
            if (positions.ContainsKey(lien.Source) && positions.ContainsKey(lien.Destination))
            {
                string nomSource = stationsParId[lien.Source];
                string nomDestination = stationsParId[lien.Destination];

                if (nomSource == nomDestination)
                    continue;

                PointF pointSource = positions[lien.Source];
                PointF pointDest = positions[lien.Destination];

                
                bool estDansChemin = chemin != null &&
                                     chemin.Contains(lien.Source) &&
                                     chemin.Contains(lien.Destination) &&
                                     Math.Abs(chemin.IndexOf(lien.Source) - chemin.IndexOf(lien.Destination)) == 1;

                if (estDansChemin)
                {
                    using (Pen pen = new Pen(Color.Red, 3))
                    {
                        g.DrawLine(pen, pointSource, pointDest);
                    }
                    continue;
                }

                string ligneSource = lignesParStation.ContainsKey(lien.Source) ? lignesParStation[lien.Source] : null;
                string ligneDest = lignesParStation.ContainsKey(lien.Destination) ? lignesParStation[lien.Destination] : null;

                if (!string.IsNullOrEmpty(ligneSource) && ligneSource == ligneDest && couleursParLigne.ContainsKey(ligneSource))
                {
                    using (Pen pen = new Pen(couleursParLigne[ligneSource], 2))
                    {
                        g.DrawLine(pen, pointSource, pointDest);
                    }
                }
                else
                {
                    using (Pen pen = new Pen(Color.LightGray, 1))
                    {
                        g.DrawLine(pen, pointSource, pointDest);
                    }
                }
            }
        }

        foreach (var noeud in noeuds)
        {
            if (positions.TryGetValue(noeud.Key, out PointF position))
            {
                float rayon = 5f;
                g.FillEllipse(brushStation, position.X - rayon, position.Y - rayon, rayon * 2, rayon * 2);

                string nom = stationsParId[noeud.Key];
                g.DrawString(nom, font, brushTexte, position.X + 8, position.Y - 8);
            }
        }

        string cheminImage = Path.Combine(Directory.GetCurrentDirectory(), "graphe_visuel.png");
        image.Save(cheminImage, System.Drawing.Imaging.ImageFormat.Png);
        Console.WriteLine($"✅ Image générée et sauvegardée : {cheminImage}");

        g.Dispose();
        image.Dispose();
    }











    /// <summary>
    /// Dijkstra nous pertmet de calculer le chemin le plus court entre 2 stations. On applique la méthode de l'infini.
    /// </summary>
    /// <param name="depart"></param>
    /// <param name="arrivee"></param>
    /// <returns></returns>
    public (List<int> chemin, double poidsTotal) Dijkstra(int depart, int arrivee)
    {
        var distances = new Dictionary<int, double>();
        var predecesseurs = new Dictionary<int, int>();
        var queue = new PriorityQueue<int, double>();
        var visites = new HashSet<string>(); /// On evite de revisiter une station de même nom

        foreach (var noeud in noeuds.Keys)
        {
            distances[noeud] = double.PositiveInfinity;
        }

        distances[depart] = 0;
        queue.Enqueue(depart, 0);

        while (queue.Count > 0)
        {
            int courant = queue.Dequeue();
            string nomCourant = stationsParId[courant];

            /// Cela empeche de revenir sur une station de même nom déjà quittée
            if (visites.Contains(nomCourant))
                continue;

            visites.Add(nomCourant);

            if (courant == arrivee)
                break;

            foreach (var voisin in liens.Where(l => l.Source == courant))
            {
                int destination = voisin.Destination;
                double nouveauPoids = distances[courant] + voisin.Poids;

                if (nouveauPoids < distances[destination])
                {
                    distances[destination] = nouveauPoids;
                    predecesseurs[destination] = courant;
                    queue.Enqueue(destination, nouveauPoids);
                }
            }
        }

        if (!predecesseurs.ContainsKey(arrivee))
            return (null, double.PositiveInfinity); /// Aucun chemin trouvé

        /// Reconstruction du chemin
        List<int> chemin = new List<int>();
        int actuel = arrivee;
        while (actuel != depart)
        {
            chemin.Add(actuel);
            actuel = predecesseurs[actuel];
        }
        chemin.Add(depart);
        chemin.Reverse();

        return (chemin, distances[arrivee]);
    }





    public (List<int>, double) BellmanFord(int source, int destination)
    {
        
        Dictionary<int, double> distances = new Dictionary<int, double>();
        Dictionary<int, int> pred = new Dictionary<int, int>();
        List<int> chemin = new List<int>();

      
        foreach (var station in noeuds.Keys)
        {
            distances[station] = double.PositiveInfinity;
            pred[station] = -1;
        }

      
        distances[source] = 0;
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

        foreach (var lien in liaisons)
        {
            if (distances[lien.Source] + lien.Poids < distances[lien.Destination])
            {
                Console.WriteLine("🚨 Le graphe contient un cycle négatif !");
                return (null, double.PositiveInfinity);
            }
        }
        int current = destination;
        while (current != -1)
        {
            chemin.Insert(0, current);
            current = pred[current];
        }

  
        return (chemin, distances[destination]);
    }



    public double[,] FloydWarshall()
    {
        int n = noeuds.Count;
        int[] ids = noeuds.Keys.ToArray();
        Dictionary<int, int> indexMap = ids.Select((id, index) => new { id, index }).ToDictionary(x => x.id, x => x.index);

       
        double[,] distances = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j) distances[i, j] = 0;
                else distances[i, j] = double.PositiveInfinity;
            }
        }

        foreach (var lien in liens)
        {
            int u = indexMap[lien.Source];
            int v = indexMap[lien.Destination];
            distances[u, v] = lien.Poids;
        }

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
        
        return stationsParId.ContainsKey(idStation) ? stationsParId[idStation] : "Station inconnue";
    }

    public (List<int> chemin, double poidsTotal) TrouverUnChemin(int depart, int arrivee)
    {
        HashSet<int> visites = new HashSet<int>();
        HashSet<string> nomsVisites = new HashSet<string>();
        List<int> chemin = new List<int>();
        double poidsTotal = 0;

        bool trouve = DFS(depart, arrivee, visites, nomsVisites, chemin, ref poidsTotal, null);
        return trouve ? (chemin, poidsTotal) : (null, 0);
    }

    private bool DFS(int courant, int cible, HashSet<int> visites, HashSet<string> nomsVisites, List<int> chemin, ref double poidsTotal, string dernierNom)
    {
        visites.Add(courant);
        chemin.Add(courant);

        string nomCourant = stationsParId.ContainsKey(courant) ? stationsParId[courant] : "";
        string nomNormalise = NormaliserNom(nomCourant);

       
        bool ajoutNom = false;
        if (nomNormalise != dernierNom && nomsVisites.Contains(nomNormalise))
        {
          
            chemin.RemoveAt(chemin.Count - 1);
            visites.Remove(courant);
            return false;
        }
        if (nomNormalise != dernierNom && !nomsVisites.Contains(nomNormalise))
        {
            nomsVisites.Add(nomNormalise);
            ajoutNom = true;
        }

        if (courant == cible)
            return true;

        foreach (var lien in liens.Where(l => l.Source == courant))
        {
            if (!visites.Contains(lien.Destination))
            {
                double poidsLien = lien.Poids;
                poidsTotal += poidsLien;

                if (DFS(lien.Destination, cible, visites, nomsVisites, chemin, ref poidsTotal, nomNormalise))
                    return true;

                poidsTotal -= poidsLien; 
            }
        }

        if (ajoutNom) nomsVisites.Remove(nomNormalise);
        chemin.RemoveAt(chemin.Count - 1);
        visites.Remove(courant);
        return false;
    }



}