class Lien{
    private int Source;
    private int Destination;
    public Lien(int source, int destination)
    {
        Source = source;
        Destination = destination;
    }
 
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