public class Lien<T>
{
    public T Source { get; set; }
    public T Destination { get; set; }
    public double Poids { get; set; }  // Temps ou distance entre les stations

    public Lien(T source, T destination, double poids)
    {
        Source = source;
        Destination = destination;
        Poids = poids;
    }
}
