
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Projet66666
{
    public class Visualisation
    {
        private List<PointF> noeuds;
        private List<Tuple<int, int>> liens;
        private int imageSize = 800;
        private int nodeSize = 30; 

        public Visualisation(List<PointF> noeuds, List<Tuple<int, int>> liens)
        {
            this.noeuds = noeuds;
            this.liens = liens;
        }

        public Visualisation(Graphe graphe)
        {
            noeuds = new List<PointF>();
            liens = new List<Tuple<int, int>>();
            GenererPositionsNoeuds(graphe);
        }
        /// <summary>
        /// place les noeuds en cercle autour du centre del'image et trace ensuite les arretes entre les noeuds
        /// </summary>
        /// <param name="graphe"></param>
        private void GenererPositionsNoeuds(Graphe graphe)
        {
            int n = graphe.ListeAdjacence.Count;
            float centerX = imageSize / 2;
            float centerY = imageSize / 2;
            float radius = imageSize / 3;

            for (int i = 0; i < n; i++)
            {
                double angle = 2 * Math.PI * i / n;
                float x = (float)(centerX + radius * Math.Cos(angle));
                float y = (float)(centerY + radius * Math.Sin(angle));
                noeuds.Add(new PointF(x, y));
            }

            foreach (var noeud in graphe.ListeAdjacence)
            {
                foreach (var voisin in noeud.Value)
                {
                    if (noeud.Key < voisin) 
                    {
                        liens.Add(new Tuple<int, int>(noeud.Key, voisin));
                    }
                }
            }
        }
        /// <summary>
        /// dessine le graphe en bleu pour les noeuds et noir pour les liens
        /// </summary>
        /// <param name="cheminFichier"></param>
        public void DessinerGraph(string cheminFichier)
        {
            using (Bitmap bitmap = new Bitmap(imageSize, imageSize))
            using (Graphics g = Graphics.FromImage(bitmap))
            using (Pen pen = new Pen(Color.Black, 2))
            using (Brush nodeBrush = new SolidBrush(Color.Blue))
            using (Brush textBrush = new SolidBrush(Color.White))
            using (Font font = new Font("Arial", 12, FontStyle.Bold))
            {
                g.Clear(Color.White); 

                foreach (var lien in liens)
                {
                    if (lien.Item1 >= 0 && lien.Item1 < noeuds.Count && lien.Item2 >= 0 && lien.Item2 < noeuds.Count)
                    {
                        PointF p1 = noeuds[lien.Item1];
                        PointF p2 = noeuds[lien.Item2];
                        g.DrawLine(pen, p1, p2); 
                    }
                }

                for (int i = 0; i < noeuds.Count; i++)
                {
                    PointF p = noeuds[i];
                    g.FillEllipse(nodeBrush, p.X - nodeSize / 2, p.Y - nodeSize / 2, nodeSize, nodeSize); 
                    g.DrawString((i + 1).ToString(), font, textBrush, p.X - 6, p.Y - 6); 
                }

                bitmap.Save(cheminFichier, ImageFormat.Png);
            }
        }
    }
}


