class Noeud 
{ 
    public int Id { get; } 
    public Noeud(int id) => Id = id; 
    public static void AjouterNoeud(Dictionary<int, List<int>> listeAdjacence, int id) 
    { 
        if (!listeAdjacence.ContainsKey(id)) { listeAdjacence[id] = new List<int>(); } 
    } 
}
 