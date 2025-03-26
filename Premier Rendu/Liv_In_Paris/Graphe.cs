using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet66666
{
    public class Graphe
    {
        private Dictionary<int, List<int>> listeAdjacence;
        private int[,] matriceAdjacence;
        private int nombreNoeuds;
        private bool utiliseListeAdjacence;
        /// <summary>
        /// initialise un graphe avec 'taille' noeuds, et remplis la liste d'adjacence avec des liste vides pour chaque noeuds. 
        /// </summary>
        /// <param name="taille"></param>
        /// <param name="utiliseListe"></param>
        public Graphe(int taille, bool utiliseListe)
        {
            nombreNoeuds = taille;
            utiliseListeAdjacence = utiliseListe;
            listeAdjacence = new Dictionary<int, List<int>>();
            matriceAdjacence = new int[taille + 1, taille + 1];

            for (int i = 1; i <= taille; i++)
            {
                listeAdjacence[i] = new List<int>();
            }
        }

        public Dictionary<int, List<int>> ListeAdjacence
        {
            get { return listeAdjacence; }
        }

        /// <summary>
        /// charge le graphe depuis le fichier texte
        /// </summary>
        /// <param name="chemin"></param>
        public void ChargerDepuisFichier(string chemin)
        {
            string[] lignes = File.ReadAllLines(chemin);
            bool premiereLigneIgnoree = false;

            foreach (var ligne in lignes)
            {
                if (ligne.StartsWith("%")) continue; 

                if (!premiereLigneIgnoree)
                {
                    premiereLigneIgnoree = true; 
                    continue;
                }

                string[] parties = ligne.Split(' ');
                int source = int.Parse(parties[0]);
                int destination = int.Parse(parties[1]);

                Noeud.AjouterNoeud(listeAdjacence, source);
                Noeud.AjouterNoeud(listeAdjacence, destination);
                Lien.AjouterLien(listeAdjacence, matriceAdjacence, source, destination, utiliseListeAdjacence);
            }
        }
        /// <summary>
        /// affiche la matrice d'adjacence
        /// </summary>
        public void AfficherMatriceAdjacence()
        {
            Console.WriteLine("\nMatrice d'Adjacence :");
            for (int i = 1; i <= nombreNoeuds; i++)
            {
                for (int j = 1; j <= nombreNoeuds; j++)
                {
                    Console.Write(matriceAdjacence[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
        /// <summary>
        /// Effectue un parcours en largeur BFS
        /// </summary>
        /// <param name="depart"></param>
        public void ParcoursLargeur(int depart)
        {
            Queue<int> file = new Queue<int>();
            HashSet<int> visite = new HashSet<int>();
            file.Enqueue(depart);
            visite.Add(depart);

            Console.Write("Parcours en Largeur (BFS): ");

            while (file.Count > 0)
            {
                int noeud = file.Dequeue();
                Console.Write(noeud + " ");

                if (utiliseListeAdjacence)
                {
                    foreach (var voisin in listeAdjacence[noeud])
                    {
                        if (!visite.Contains(voisin))
                        {
                            file.Enqueue(voisin);
                            visite.Add(voisin);
                        }
                    }
                }
                else
                {
                    for (int i = 1; i <= nombreNoeuds; i++)
                    {
                        if (matriceAdjacence[noeud, i] == 1 && !visite.Contains(i))
                        {
                            file.Enqueue(i);
                            visite.Add(i);
                        }
                    }
                }
            }
            Console.WriteLine();
        }
        /// <summary>
        /// effectue un parcours en profondeur DFS
        /// </summary>
        /// <param name="depart"></param>
        /// <param name="visite"></param>
        public void ParcoursProfondeur(int depart, HashSet<int> visite)
        {
            Console.Write(depart + " ");
            visite.Add(depart);

            if (utiliseListeAdjacence)
            {
                foreach (var voisin in listeAdjacence[depart])
                {
                    if (!visite.Contains(voisin))
                    {
                        ParcoursProfondeur(voisin, visite);
                    }
                }
            }
            else
            {
                for (int i = 1; i <= nombreNoeuds; i++)
                {
                    if (matriceAdjacence[depart, i] == 1 && !visite.Contains(i))
                    {
                        ParcoursProfondeur(i, visite);
                    }
                }
            }
        }
        /// <summary>
        /// Convertit une liste d'adjacence en matrice d'adjacence
        /// </summary>
        /// <returns></returns>
        public int[,] ObtenirMatriceAdjacence()
        {
            int n = ListeAdjacence.Count;  
            int[,] matrice = new int[n, n];  

            foreach (var noeud in ListeAdjacence)
            {
                int i = noeud.Key;
                if (i >= n) continue;  
                foreach (var voisin in noeud.Value)
                {
                    if (voisin >= n) continue;  
                    matrice[i, voisin] = 1;  
                    matrice[voisin, i] = 1;  
                }
            }

            return matrice; 
        }

    }

    }