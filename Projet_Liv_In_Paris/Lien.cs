class Lien
{
<<<<<<< HEAD
    public int Source { get; set; }
    public int Destination { get; set; }

    public Lien(int source, int destination)
=======
    public int Source;
    public int Destination;
     public Lien(int source, int destination)
>>>>>>> refs/remotes/origin/main
    {
        Source = source;
        Destination = destination;
    }
<<<<<<< HEAD

=======
 
>>>>>>> refs/remotes/origin/main
    public static void AjouterLien(Dictionary<int, List<int>> listeAdjacence, int[,] matriceAdjacence, int source, int destination, bool utiliseListeAdjacence)
    {
        if (utiliseListeAdjacence)
        {
            listeAdjacence[source].Add(destination);
            listeAdjacence[destination].Add(source);
        }
        else
        {
            matriceAdjacence[source, destination] = 1;
            matriceAdjacence[destination, source] = 1;
        }
    }
}