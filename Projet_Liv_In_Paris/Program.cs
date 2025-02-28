namespace Projet_Liv_In_Paris;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Choisissez la méthode de stockage du graphe:");
        Console.WriteLine("1 - Liste d'adjacence");
        Console.WriteLine("2 - Matrice d'adjacence");
        int choix = int.Parse(Console.ReadLine());
        bool utiliseListeAdjacence = choix == 1;

        Graphe graphe = new Graphe(34, utiliseListeAdjacence);
        graphe.ChargerDepuisFichier("soc-karate.mtx");

        Console.WriteLine("\nParcours en Largeur :");
        graphe.ParcoursLargeur(0);

        Console.WriteLine("\nParcours en Profondeur :");
        graphe.ParcoursProfondeur(0, new HashSet<int>());

        Console.WriteLine("\nLe graphe est-il connexe ? " + graphe.EstConnexe());
    }
}
