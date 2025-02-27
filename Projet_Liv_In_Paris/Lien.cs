class Lien
{
     public Noeud Source { get; }
    public Noeud Destination { get; }
    public Lien(Noeud source, Noeud destination)
    {
        Source = source;
        Destination = destination;
    }
    
    
}