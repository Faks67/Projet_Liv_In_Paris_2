using System.ComponentModel.DataAnnotations.Schema;

namespace Projet_Liv_In_Paris;

class Program
{
    static void Main(string[] args)
    {
        
        Graphe graphe = new Graphe(6);
        graphe.AjouterLien(0, 1);
        graphe.AjouterLien(0, 2);
        graphe.AjouterLien(1, 3);
        graphe.AjouterLien(1, 4);
        graphe.AjouterLien(2, 5);
        graphe.AjouterLien(3, 4);
        string[] tab = new string[103];
        tab = File.ReadAllLines("/Users/Codage/C#/Projet_Liv_In_Paris/Projet_Liv_In_Paris/Association-soc-karate/soc-karate.mtx");
        graphe.BFS(0);
        graphe.DFS(0);
        Console.WriteLine("");
    }

}
