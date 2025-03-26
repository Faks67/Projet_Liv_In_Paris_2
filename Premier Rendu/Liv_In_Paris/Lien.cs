using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet66666
{
    public class Lien
    {
        public int Source { get; set; }
        public int Destination { get; set; }
        public Lien(int source, int destination)
        {
            Source = source;
            Destination = destination;
        }
        /// <summary>
        /// crée les arretes entre les sommets du graphes soit sous la forme de liste d'adjacence ou sous la forme de matrice d'adjacence
        /// </summary>
        /// <param name="listeAdjacence"></param>
        /// <param name="matriceAdjacence"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="utiliseListeAdjacence"></param>
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
}
