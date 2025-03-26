using Projet66666;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Choisissez la méthode de stockage du graphe:");
        Console.WriteLine("1 - Liste d'adjacence");
        Console.WriteLine("2 - Matrice d'adjacence");
        int choix = int.Parse(Console.ReadLine());
        bool utiliseListeAdjacence = choix == 1;

        Graphe graphe = new Graphe(34, utiliseListeAdjacence);
        graphe.ChargerDepuisFichier("soc-karate.mtx");

        if (!utiliseListeAdjacence)
        {
            graphe.AfficherMatriceAdjacence();
        }

        Console.WriteLine("\nParcours en Largeur (BFS):");
        graphe.ParcoursLargeur(1);

        Console.WriteLine("\nParcours en Profondeur (DFS):");
        graphe.ParcoursProfondeur(1, new HashSet<int>());
        Console.WriteLine();

        Console.WriteLine("Génération de la visualisation du graphe...");


        Visualisation vis = new Visualisation(graphe);  
        vis.DessinerGraph("graph.png");  

        Console.WriteLine("Visualisation enregistrée sous 'graph.png'.");
    }
}


