using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;
using MySql.Data.MySqlClient;

class Program
{
    static void Main()
    {
        Console.WriteLine("Que souhaitez vous faire?\n1 - Acceder au graphes\n2 - Acceder au SQL\nQuittez ");
        string pressed = Console.ReadLine();

        switch (pressed)
        {
            case "1":
            Graphes();
            break;
            case "2":
            SQL();
            break;
            default :
            Environment.Exit(0);
            break;
        }
        }
        public static void Graphes()
        {
            Console.WriteLine(" Chargement du fichier Excel...");
            Graphe<int> graphe = new Graphe<int>(); 
            string cheminFichier = "Metro_GareFinal.xlsx";           
            graphe.ChargerDepuisExcel(cheminFichier);
            Console.WriteLine("\nStations chargées :");
            Console.WriteLine("\n Génération du graphe visuel...");
            graphe.AfficherGrapheVisuel();
            Console.WriteLine(" Graphe visuel généré avec succès !");
            Console.WriteLine("\n Entrez l'ID de la station de départ : ");
            int idDepart = int.Parse(Console.ReadLine());
            Console.WriteLine(" Entrez l'ID de la station d'arrivée : ");
            int idArrivee = int.Parse(Console.ReadLine());


            var (chemin, poids) = graphe.Dijkstra(idDepart, idArrivee);

            if (chemin != null)
            {
                string nomDepart = graphe.GetStationNom(idDepart);
                string nomArrivee = graphe.GetStationNom(idArrivee);
                Console.WriteLine($"\n Le chemin le plus court entre {nomDepart} et {nomArrivee} :");
                foreach (int id in chemin)
                    Console.WriteLine($"{id}: {graphe.GetStationNom(id)}");

                Console.WriteLine($" Poids total du trajet : {poids}");
                graphe.AfficherGrapheVisuel(chemin);
            }
            else
            {
                string nomDepart = graphe.GetStationNom(idDepart);
                string nomArrivee = graphe.GetStationNom(idArrivee);
                Console.WriteLine($"❌ Aucune route trouvée entre {nomDepart} et  {nomArrivee}.");
            }
            Environment.Exit(0);
        }
        public static void SQL()
        {
            MySqlConnection conn = Database.GetConnection();
        conn.Open();
        Console.WriteLine("Que souhaitez vous faire?\n1 - Se connecter en mode client\n2 - Se connecter en mode cuisinier\n3 - Se connecter en mode Admin\nQuittez ");
        string pressed = Console.ReadLine();

        switch (pressed)
        {
            case "1" : 
            VerClient(conn);
            break;
            case "2":
            VerCuisinier(conn);
            break;
            case "3" : 
            Admin(conn);
            break;
            default :
            Environment.Exit(0);
            break;
        }
        conn.Close();
        }

        public static void Admin(MySqlConnection conn)
        {
        Console.WriteLine("Que souhaitez vous faire?\n1 - Gestion des clients\n2 - Gestion des cuisiniers\n3 - Gestion des commandes\n4 - Statistique\nQuittez ");
        string pressed = Console.ReadLine();

        switch (pressed)
        {
            case "1" : 
            GestionClient(conn);
            break;
            case "2":
            GestionCuisinier(conn);
            break;
            case "3" : 
            GestionCommande(conn);
            break;
            case "4":
            Statistique(conn);
            break;
            default:
            Environment.Exit(0);
            conn.Close();
            break;
        }
    }
    public static void GestionClient(MySqlConnection conn)
    {
        Client client = new Client();
        Console.WriteLine("Que souhaitez vous faire ?\n1 - Ajouter un client\n2 - Mofifier un client\n3 - Supprimer un client\n4 - Afficher les clients par ordre alphabetique\n5 - Affichier les clients par rue\n6 - Afficher les clients par le montant d'achat\nQuitter");
        string choix = Console.ReadLine();
        switch (choix)
        {
            case "1" : 
            client.AjouterClient(conn);
            break;
            case "2":
            client.ModifierClient(conn);
            break;
            case "3" : 
            client.SupprimerClient(conn);
            break;
            case "4":
            client.AfficherClientsAlphabetique(conn);
            break;
            case "5" : 
            client.AfficherClientsRue(conn);
            break;
            case "6" :
            client.AfficherTotalCommandesParClient(conn);
            break;
            default:
            Environment.Exit(0);
            conn.Close();
            break;
        }
    }
    public static void GestionCuisinier(MySqlConnection conn)
    {
        Cuisinier cuisinier = new Cuisinier();
        Console.WriteLine("Que souhaitez vous faire ?\n1 - Ajouter un cuisinier\n2 - Mofifier un cuisinier\n3 - Supprimer un cuisinier\n4 - Afficher les clients servis par le cuisinier\n5 - Affichier les plats realisés\n6 - Afficher le plats le plus recent\n7 - Afficher les cuisiniers\nQuitter");
        string choix = Console.ReadLine();
        Plat plat = new Plat();
        switch (choix)
        {
            case "1":
            cuisinier.AjouterCuisinier(conn);
            break;
            case "2":
            cuisinier.ModifierCuisinier(conn);
            break;
            case "3":
            cuisinier.SupprimerCuisinier(conn);
            break;
            case "4":
            cuisinier.AfficherClient(conn);
            break;
            case "5":
            cuisinier.platRealiser(conn);
            break;
            case"6":
            cuisinier.platRecent(conn);
            break;
            case"7":
            cuisinier.AfficherCuisiniersAlphabetique(conn);
            break;
            default : 
            Environment.Exit(0);
            conn.Close();
            break;
        }
    }
    public static void GestionCommande(MySqlConnection conn)
    {
        Commande commande = new Commande();
        Console.WriteLine("Que souhaitez vois faire ?\n1 - Créer une commande\n2 - Afficher les commandes en cours\n3 - Afficher le prix d'une commande\nQuitter");
        string choix = Console.ReadLine();
        switch (choix)
        {
            case "2":
            commande.AfficheCommande(conn);
            break;
            case "3":
            commande.AfficheCommandePrix(conn);
            break;
            case "1":
            commande.CreerCommande(conn);
            break;
            default :
            Environment.Exit(0);
            conn.Close();
            break;
        }
    }
    public static void Statistique(MySqlConnection conn)
    {
        statistique stat = new statistique();
        Console.WriteLine("Que souhiatez vous faire?\n1 - Afficher les cuisiniers par nombres de livraison\n2 - Afficher les commandes durant une periode de temps\n3 - Afficher la moyenne des commandes\n4 - Affiche la moyenne des depenses client\n5 - Afficher la liste des plats par client selon la nationalité\n6 - Afficher la liste des plats par clients selon une periode\nQuitter");
        string choix = Console.ReadLine();
        switch (choix)
        {
            case "1" : 
            stat.AfficherNombreLivraisonsParCuisinier(conn);
            break;
            case "2":
            stat.AfficherCommandesClientPeriode(conn);
            break;
            case "3" : 
            stat.AfficherMoyennePrixCommandes(conn);
            break;
            case "4":
            stat.AfficherMoyenneClients(conn);
            break;
            case "5" : 
            stat.AfficherCommandesClientNationalite(conn);
            break;
            case "6":
            stat.AfficherCommandesClientPeriode(conn);
            break;
            default :
            Environment.Exit(0); 
            conn.Close();
            break;
        }
    }
    public static void VerClient(MySqlConnection conn)
    {
        string idClient ="";
        Plat plat = new Plat();
        Commande com = new Commande();
        Client client = new Client();
        Console.WriteLine("Avez vous deja un compte ? (Y/N)");
        string reponse = Console.ReadLine();
        while (reponse != "Y" && reponse != "N"){
            Console.WriteLine("Avez vous deja un compte ? (Y/N)");
            reponse = Console.ReadLine();
        }
        if (reponse == "Y")
        {
            Console.WriteLine("Quel est votre identifiant");
            idClient = Console.ReadLine();
        }
        else
        {
            client.AjouterClient(conn);
        }
        Console.WriteLine("Que souhiatez vous faire?\n1 - Afficher les plats que vous pouvez commander\n2 - Commander un plat\n3 - Modifier vos informations\n4 - Afficher vos commandes en cours\nQuitter");
        string choix = Console.ReadLine();
        switch (choix)
        {
            case "1" : 
            plat.AfficherPlat(conn);
            break;
            case "2":
            com.CreerCommande(conn);
            break;
            case "3" :
            client.ModifierClientParty(conn, idClient);
            break;
            case "4" :
            com.AfficheCommandeClient(conn, idClient);
            break;
            default :
            Environment.Exit(0); 
            conn.Close();
            break;
        }
    }
    public static void VerCuisinier(MySqlConnection conn)
    {
        string idCuisinier = "";
        Plat plat = new Plat();
        Commande com = new Commande();
        Cuisinier cuisinier = new Cuisinier();
        Console.WriteLine("Avez vous deja un compte ? (Y/N)");
        string reponse = Console.ReadLine();
        while (reponse != "Y" && reponse != "N"){
            Console.WriteLine("Avez vous deja un compte ? (Y/N)");
            reponse = Console.ReadLine();
        }
        if (reponse == "Y")
        {
            Console.WriteLine("Quel est votre identifiant");
            idCuisinier = Console.ReadLine();
        }
        else
        {
            cuisinier.AjouterCuisinier(conn);
        }
        Console.WriteLine("Que souhiatez vous faire?\n1 - Creer un plat\n2 - Afficher vos commande en cours\n3 - Modifier vos informations\n4 - Simuler un trajet d'une commande\nQuitter");
        string choix = Console.ReadLine();
        switch (choix)
        {
            case "1" : 
            plat.AjouterPlat(idCuisinier, conn);
            break;
            case "2":
            com.AfficheCommandeCuisinier(conn, idCuisinier);
            break;
            case "3" :
            cuisinier.ModifierCuisinierParty(conn, idCuisinier);
            break;
            default :
            Environment.Exit(0); 
            conn.Close();
            break;
        }
    }
}