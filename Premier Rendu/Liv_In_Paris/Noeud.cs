using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet66666
{
    public class Noeud
    {
        public int Id { get; set; }

        public Noeud(int id)
        {
            Id = id;
        }
        /// <summary>
        /// cree une liste de la longueur de taille, donc du degre de notre graphe
        /// </summary>
        /// <param name="listeAdjacence"></param>
        /// <param name="id"></param>
        public static void AjouterNoeud(Dictionary<int, List<int>> listeAdjacence, int id)
        {
            if (!listeAdjacence.ContainsKey(id))
            {
                listeAdjacence[id] = new List<int>();
            }
        }
    }
}
