class Graphe
{
     private Dictionary<int, List<int>> _adjList = new();
    private int[,] _adjMatrix;
    private int _size;

    public Graphe(int size)
    {
        _size = size;
        _adjMatrix = new int[size, size];
        for (int i = 0; i < size; i++)
            _adjList[i] = new List<int>();
    }

    public void AjouterLien(int source, int destination)
    {
        // Ajout à la liste d'adjacence
        _adjList[source].Add(destination);
        _adjList[destination].Add(source);

        // Ajout à la matrice d'adjacence
        _adjMatrix[source, destination] = 1;
        _adjMatrix[destination, source] = 1;
    }

    // Parcours en largeur d'abord (BFS)
    public void BFS(int start)
    {
        HashSet<int> visite = new();
        Queue<int> queue = new();
        queue.Enqueue(start);
        visite.Add(start);

        Console.Write("BFS: ");
        while (queue.Count > 0)
        {
            int noeud = queue.Dequeue();
            Console.Write(noeud + " ");

            foreach (int voisin in _adjList[noeud])
            {
                if (!visite.Contains(voisin))
                {
                    visite.Add(voisin);
                    queue.Enqueue(voisin);
                }
            }
        }
        Console.WriteLine();
    }

    // Parcours en profondeur d'abord (DFS)
    public void DFS(int start)
    {
        HashSet<int> visite = new();
        Stack<int> stack = new();
        stack.Push(start);

        Console.Write("DFS: ");
        while (stack.Count > 0)
        {
            int noeud = stack.Pop();
            if (!visite.Contains(noeud))
            {
                visite.Add(noeud);
                Console.Write(noeud + " ");
                foreach (int voisin in _adjList[noeud])
                    stack.Push(voisin);
            }
        }
        Console.WriteLine();
    }
    
}