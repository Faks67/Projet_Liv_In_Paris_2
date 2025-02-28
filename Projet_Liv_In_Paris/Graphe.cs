class Graphe
{
    private Dictionary<int, List<int>> listeAdjacence;
    private int[,] matriceAdjacence;
    private int nombreNoeuds;
    private bool utiliseListeAdjacence;
 
    public Graphe(int taille, bool utiliseListe)
    {
        nombreNoeuds = taille;
        utiliseListeAdjacence = utiliseListe;
        listeAdjacence = new Dictionary<int, List<int>>();
        matriceAdjacence = new int[taille, taille];
 
        for (int i = 0; i < taille; i++)
        {
            listeAdjacence[i] = new List<int>();
        }
    }
 
    public void ChargerDepuisFichier(string chemin)
{
    string[] lignes = File.ReadAllLines(chemin);
    bool premiereLigneIgnoree = false;
 
    foreach (var ligne in lignes)
    {
        if (ligne.StartsWith("%")) continue; // Ignore les commentaires
 
        if (!premiereLigneIgnoree)
        {
            premiereLigneIgnoree = true; // Ignore la première ligne contenant "34 34 78"
            continue;
        }
 
        string[] parties = ligne.Split(' ');
        int source = int.Parse(parties[0]) - 1;  // Convertir en index 0-based
        int destination = int.Parse(parties[1]) - 1;
 
        Noeud.AjouterNoeud(listeAdjacence, source);
        Noeud.AjouterNoeud(listeAdjacence, destination);
        Lien.AjouterLien(listeAdjacence, matriceAdjacence, source, destination, utiliseListeAdjacence);
 
        Console.WriteLine($"Ajout du lien entre {source} et {destination}");
    }
}
 
 
    public void ParcoursLargeur(int depart)
    {
        Queue<int> file = new Queue<int>();
        HashSet<int> visite = new HashSet<int>();
        file.Enqueue(depart);
        visite.Add(depart);
 
        while (file.Count > 0)
        {
            int noeud = file.Dequeue();
            Console.Write(noeud + " ");
 
            foreach (var voisin in listeAdjacence[noeud])
            {
                if (!visite.Contains(voisin))
                {
                    file.Enqueue(voisin);
                    visite.Add(voisin);
                }
            }
        }
        Console.WriteLine();
    }
 
    public void ParcoursProfondeur(int depart, HashSet<int> visite)
    {
        Console.Write(depart + " ");
        visite.Add(depart);
        foreach (var voisin in listeAdjacence[depart])
        {
            if (!visite.Contains(voisin))
            {
                ParcoursProfondeur(voisin, visite);
            }
        }
    }
 
    public bool EstConnexe()
    {
        HashSet<int> visite = new HashSet<int>();
        ParcoursProfondeur(0, visite);
        return visite.Count == nombreNoeuds;
    }
}
