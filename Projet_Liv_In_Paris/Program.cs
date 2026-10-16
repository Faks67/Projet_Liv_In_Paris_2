using System;
using System.Diagnostics;
using MySql.Data.MySqlClient;

class Program
{
    private static Graphe<int>? grapheMetro;

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\n===== Liv'In Paris =====");
            Console.WriteLine("1 - Accéder aux graphes (métro)\n2 - Accéder au SQL\n0 - Quitter");
            switch (Saisie.Texte("Votre choix : "))
            {
                case "1":
                    Executer(Graphes);
                    break;
                case "2":
                    SQL();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Choix invalide.");
                    break;
            }
        }
    }

    /// <summary>
    /// Lance une action et affiche un message clair en cas d'erreur au lieu de fermer le programme
    /// </summary>
    static void Executer(Action action)
    {
        try
        {
            action();
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            Console.WriteLine("Erreur : cet identifiant existe déjà.");
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            Console.WriteLine("Erreur : un des identifiants saisis (client, cuisinier ou plat) n'existe pas.");
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            Console.WriteLine("Erreur : cet élément est encore utilisé ailleurs (par exemple dans une commande).");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur : " + ex.Message);
        }
    }

    /// <summary>
    /// Charge le graphe du métro une seule fois
    /// </summary>
    static Graphe<int> GetGrapheMetro()
    {
        if (grapheMetro == null)
        {
            Console.WriteLine(" Chargement du fichier Excel...");
            grapheMetro = new Graphe<int>();
            grapheMetro.ChargerDepuisExcel(Path.Combine(AppContext.BaseDirectory, "Metro_GareFinal.xlsx"));
        }
        return grapheMetro;
    }

    /// <summary>
    /// Ouvre une image avec la visionneuse de Windows
    /// </summary>
    static void OuvrirImage(string nomFichier)
    {
        string chemin = Path.Combine(Directory.GetCurrentDirectory(), nomFichier);
        if (!File.Exists(chemin)) return;
        if (Console.IsInputRedirected)
        {
            // Exécution automatisée (tests) : on n'ouvre pas de fenêtre
            Console.WriteLine($"Image enregistrée : {chemin}");
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo(chemin) { UseShellExecute = true });
        }
        catch
        {
            Console.WriteLine($"Image disponible ici : {chemin}");
        }
    }

    /// <summary>
    /// Calcule et dessine le plus court chemin entre deux stations
    /// </summary>
    static void AfficherTrajet(Graphe<int> graphe, int idDepart, int idArrivee)
    {
        string nomDepart = graphe.GetStationNom(idDepart);
        string nomArrivee = graphe.GetStationNom(idArrivee);
        var (chemin, poids) = graphe.Dijkstra(idDepart, idArrivee);
        if (chemin != null)
        {
            Console.WriteLine($"\n Le chemin le plus court entre {nomDepart} et {nomArrivee} :");
            foreach (int id in chemin)
                Console.WriteLine($"{id}: {graphe.GetStationNom(id)}");
            Console.WriteLine($" Poids total du trajet : {poids}");
            graphe.AfficherGrapheVisuel(chemin);
            OuvrirImage("graphe_visuel.png");
        }
        else
        {
            Console.WriteLine($" Aucune route trouvée entre {nomDepart} et {nomArrivee}.");
        }
    }

    static int LireIdStation(Graphe<int> graphe, string question)
    {
        while (true)
        {
            int id = Saisie.Entier(question);
            if (graphe.GetStationNom(id) != "Station inconnue") return id;
            Console.WriteLine("Cette station n'existe pas (les ID vont de 1 à 332).");
        }
    }

    public static void Graphes()
    {
        Graphe<int> graphe = GetGrapheMetro();
        Console.WriteLine("\n Génération du graphe visuel...");
        graphe.AfficherGrapheVisuel();
        Console.WriteLine(" Graphe visuel généré avec succès !");
        OuvrirImage("graphe_visuel.png");
        while (true)
        {
            Console.WriteLine("\n--- Graphes ---\n1 - Calculer un plus court chemin entre deux stations\n2 - Graphe des liens entre clients et cuisiniers (coloration)\n0 - Retour");
            switch (Saisie.Texte("Votre choix : "))
            {
                case "1":
                    int idDepart = LireIdStation(graphe, "\n Entrez l'ID de la station de départ : ");
                    int idArrivee = LireIdStation(graphe, " Entrez l'ID de la station d'arrivée : ");
                    AfficherTrajet(graphe, idDepart, idArrivee);
                    break;
                case "2":
                    Executer(() =>
                    {
                        graphe.test();
                        OuvrirImage("graphe_colore.png");
                    });
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Choix invalide.");
                    break;
            }
        }
    }

    public static void SQL()
    {
        using MySqlConnection conn = Database.GetConnection();
        try
        {
            conn.Open();
        }
        catch (MySqlException)
        {
            Console.WriteLine("Impossible de se connecter à la base de données.");
            Console.WriteLine("Lancez d'abord demarrer_mysql.cmd (et laissez sa fenêtre ouverte), puis réessayez.");
            return;
        }
        while (true)
        {
            Console.WriteLine("\n--- Connexion ---\n1 - Se connecter en mode client\n2 - Se connecter en mode cuisinier\n3 - Se connecter en mode Admin\n0 - Retour");
            switch (Saisie.Texte("Votre choix : "))
            {
                case "1":
                    Executer(() => VerClient(conn));
                    break;
                case "2":
                    Executer(() => VerCuisinier(conn));
                    break;
                case "3":
                    Admin(conn);
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Choix invalide.");
                    break;
            }
        }
    }

    /// <summary>
    /// Affiche un menu en boucle jusqu'à ce que l'utilisateur choisisse 0
    /// </summary>
    static void Menu(string titre, (string libelle, Action action)[] options)
    {
        while (true)
        {
            Console.WriteLine($"\n--- {titre} ---");
            for (int i = 0; i < options.Length; i++)
                Console.WriteLine($"{i + 1} - {options[i].libelle}");
            Console.WriteLine("0 - Retour");
            string choix = Saisie.Texte("Votre choix : ");
            if (choix == "0") return;
            if (int.TryParse(choix, out int n) && n >= 1 && n <= options.Length)
                Executer(options[n - 1].action);
            else
                Console.WriteLine("Choix invalide.");
        }
    }

    public static void Admin(MySqlConnection conn)
    {
        Menu("Admin", new (string, Action)[]
        {
            ("Gestion des clients", () => GestionClient(conn)),
            ("Gestion des cuisiniers", () => GestionCuisinier(conn)),
            ("Gestion des commandes", () => GestionCommande(conn)),
            ("Statistiques", () => Statistique(conn)),
            ("Exporter les bases de données", () => Export(conn)),
        });
    }

    public static void GestionClient(MySqlConnection conn)
    {
        Client client = new Client();
        Menu("Gestion des clients", new (string, Action)[]
        {
            ("Ajouter un client", () => client.AjouterClient(conn)),
            ("Modifier un client", () => client.ModifierClient(conn)),
            ("Supprimer un client", () => client.SupprimerClient(conn)),
            ("Afficher les clients par ordre alphabétique", () => client.AfficherClientsAlphabetique(conn)),
            ("Afficher les clients par rue", () => client.AfficherClientsRue(conn)),
            ("Afficher les clients par montant d'achat", () => client.AfficherTotalCommandesParClient(conn)),
        });
    }

    public static void GestionCuisinier(MySqlConnection conn)
    {
        Cuisinier cuisinier = new Cuisinier();
        Menu("Gestion des cuisiniers", new (string, Action)[]
        {
            ("Ajouter un cuisinier", () => cuisinier.AjouterCuisinier(conn)),
            ("Modifier un cuisinier", () => cuisinier.ModifierCuisinier(conn)),
            ("Supprimer un cuisinier", () => cuisinier.SupprimerCuisinier(conn)),
            ("Afficher les clients servis par un cuisinier", () => cuisinier.AfficherClient(conn)),
            ("Afficher les plats réalisés", () => cuisinier.platRealiser(conn)),
            ("Afficher le plat le plus récent", () => cuisinier.platRecent(conn)),
            ("Afficher les cuisiniers", () => cuisinier.AfficherCuisiniersAlphabetique(conn)),
        });
    }

    public static void GestionCommande(MySqlConnection conn)
    {
        Commande commande = new Commande();
        Menu("Gestion des commandes", new (string, Action)[]
        {
            ("Créer une commande", () => commande.CreerCommande(conn)),
            ("Afficher toutes les commandes", () => commande.AfficheCommande(conn)),
            ("Afficher le prix d'une commande", () => commande.AfficheCommandePrix(conn)),
        });
    }

    public static void Statistique(MySqlConnection conn)
    {
        statistique stat = new statistique();
        Menu("Statistiques", new (string, Action)[]
        {
            ("Afficher les cuisiniers par nombre de livraisons", () => stat.AfficherNombreLivraisonsParCuisinier(conn)),
            ("Afficher les commandes durant une période", () => stat.AfficherCommandesParPeriode(conn)),
            ("Afficher la moyenne des commandes", () => stat.AfficherMoyennePrixCommandes(conn)),
            ("Afficher la moyenne des dépenses client", () => stat.AfficherMoyenneClients(conn)),
            ("Afficher les plats d'un client selon la nationalité", () => stat.AfficherCommandesClientNationalite(conn)),
            ("Afficher les commandes d'un client sur une période", () => stat.AfficherCommandesClientPeriode(conn)),
        });
    }

    public static void VerClient(MySqlConnection conn)
    {
        Plat plat = new Plat();
        Commande com = new Commande();
        Client client = new Client();
        string idClient;
        if (Saisie.OuiNon("Avez-vous déjà un compte ?"))
        {
            idClient = Saisie.Texte("Quel est votre ID client ? (ex : CL1) : ");
            if (!client.Existe(idClient, conn))
            {
                Console.WriteLine("Aucun client avec cet ID.");
                return;
            }
        }
        else
        {
            idClient = client.AjouterClient(conn);
        }
        Console.WriteLine($"Bienvenue, client {idClient} !");
        Menu("Espace client", new (string, Action)[]
        {
            ("Afficher les plats que vous pouvez commander", () => plat.AfficherPlat(conn)),
            ("Commander un plat", () => com.CreerCommande(conn, idClient)),
            ("Modifier vos informations", () => client.ModifierClientParty(conn, idClient)),
            ("Afficher vos commandes", () => com.AfficheCommandeClient(conn, idClient)),
        });
    }

    public static void VerCuisinier(MySqlConnection conn)
    {
        Plat plat = new Plat();
        Commande com = new Commande();
        Cuisinier cuisinier = new Cuisinier();
        string idCuisinier;
        if (Saisie.OuiNon("Avez-vous déjà un compte ?"))
        {
            idCuisinier = Saisie.Texte("Quel est votre ID cuisinier ? (ex : C1) : ");
            if (!cuisinier.Existe(idCuisinier, conn))
            {
                Console.WriteLine("Aucun cuisinier avec cet ID.");
                return;
            }
        }
        else
        {
            idCuisinier = cuisinier.AjouterCuisinier(conn);
        }
        Console.WriteLine($"Bienvenue, cuisinier {idCuisinier} !");
        Menu("Espace cuisinier", new (string, Action)[]
        {
            ("Créer un plat", () => plat.AjouterPlat(idCuisinier, conn)),
            ("Afficher vos commandes", () => com.AfficheCommandeCuisinier(conn, idCuisinier)),
            ("Modifier vos informations", () => cuisinier.ModifierCuisinierParty(conn, idCuisinier)),
            ("Simuler le trajet d'une commande", () => SimulerTrajet(conn, idCuisinier)),
        });
    }

    /// <summary>
    /// Calcule le trajet en métro entre la station du cuisinier et celle du client pour une commande
    /// </summary>
    static void SimulerTrajet(MySqlConnection conn, string idCuisinier)
    {
        Commande com = new Commande();
        com.AfficheCommandeCuisinier(conn, idCuisinier);
        string idCommande = Saisie.Texte("De quelle commande souhaitez-vous le trajet ? (ID de commande) : ");
        if (!com.Existe(idCommande, conn, idCuisinier))
        {
            Console.WriteLine("Cette commande n'existe pas ou ne vous appartient pas.");
            return;
        }
        string depart = com.GetDepart(idCommande, conn);
        string arrive = com.GetArrive(idCommande, conn);
        Graphe<int> graphe = GetGrapheMetro();
        int stationCuisinier = graphe.GetStation(graphe.NormaliserNom(depart));
        int stationClient = graphe.GetStation(graphe.NormaliserNom(arrive));
        if (stationCuisinier == -1 || stationClient == -1)
        {
            string inconnue = stationCuisinier == -1 ? depart : arrive;
            Console.WriteLine($"La station « {inconnue} » n'a pas été trouvée sur le plan du métro.");
            return;
        }
        AfficherTrajet(graphe, stationCuisinier, stationClient);
    }

    public static void Export(MySqlConnection conn)
    {
        Menu("Export JSON", new (string, Action)[]
        {
            ("Exporter les clients", () => DatabaseExporter.ExportToJson("Client", Path.Combine(AppContext.BaseDirectory, "client.json"), conn)),
            ("Exporter les cuisiniers", () => DatabaseExporter.ExportToJson("Cuisinier", Path.Combine(AppContext.BaseDirectory, "cuisiniers.json"), conn)),
            ("Exporter les plats", () => DatabaseExporter.ExportToJson("Plat", Path.Combine(AppContext.BaseDirectory, "plats.json"), conn)),
            ("Exporter les commandes", () => DatabaseExporter.ExportToJson("Commande", Path.Combine(AppContext.BaseDirectory, "commandes.json"), conn)),
        });
    }
}
