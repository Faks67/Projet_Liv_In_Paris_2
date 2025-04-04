using System;
using System.Collections;
using MySql.Data.MySqlClient;

class Program
{
    static void Main()
    {
        Utilisateur user = new Utilisateur();
        MySqlConnection conn = Database.GetConnection();
        conn.Open();
        // var cmd1 = new MySqlCommand("DELETE FROM Utilisateur", conn);
        // var cmd = new MySqlCommand("DELETE FROM Client", conn);
        // cmd.ExecuteNonQuery();
        // cmd1.ExecuteNonQuery();
        // // Ajout d'un utilisateur
        // // for (int i = 0; i<5; i++){
        //  utilisateurManager.AjouterUtilisateur("user", "password123");

        // // Affichage des utilisateurs
        // // //utilisateurManager.AfficherUtilisateurs();

        // // // Ajout d'un client
        // // clientManager.AjouterClient("client" + Convert.ToString(i), "Zone A", "Station 1", "trans001", "user" + Convert.ToString(i),"test" + Convert.ToString(5-i), "b", "c", conn);
        // // float prix = 10.5f;
        // // commande.CreerCommande("client" + Convert.ToString(i), "a",prix, "a", "a");
        // // // Affichage des clients
                
        // //clientManager.AfficherClientsAlphabetique(conn);
        // clientManager.AfficherClientsRue(conn);
        // Console.WriteLine("test");
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
            break;

        }
        conn.Close();
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
            break;
        }
    }
    public static void GestionCuisinier(MySqlConnection conn)
    {
        Cuisinier cuisinier = new Cuisinier();
        Console.WriteLine("Que souhaitez vous faire ?\n1 - Ajouter un cuisinier\n2 - Mofifier un cuisinier\n3 - Supprimer un cuisinier\n4 - Afficher les clients servis par le cuisinier\n5 - Affichier les plats realisés\n6 - Afficher le plats le plus recent\nQuitter");
        string choix = Console.ReadLine();
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
            default : 
            Environment.Exit(0);
            break;
        }
    }
    public static void GestionCommande(MySqlConnection conn)
    {
        Commande commande = new Commande();
        Console.WriteLine("Que souhaitez vois faire ?\n1 - Afficher le prix d'une commande\n2 - Determiner le chemin a parcourir en fonction des station\n3 - Créer une commande\nQuitter");
        string choix = Console.ReadLine();
        switch (choix)
        {
            case "1":
            commande.AfficheCommandeParty(conn);
            break;
            case "2":
            break;
            case "3":
            commande.CreerCommande(conn);
            break;
            default :
            Environment.Exit(0);
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
            break;
        }
    }


}