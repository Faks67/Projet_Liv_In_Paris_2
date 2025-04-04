// using Liv_In_Paris;

// class Program
// {
//     static void Main(string[] args)
//     {
        // Console.WriteLine("Choisissez la méthode de stockage du graphe:");
        // Console.WriteLine("1 - Liste d'adjacence");
        // Console.WriteLine("2 - Matrice d'adjacence");
        // int choix = int.Parse(Console.ReadLine());
        // bool utiliseListeAdjacence = choix == 1;
 
        // Graphe graphe = new Graphe(34, utiliseListeAdjacence);
        // graphe.ChargerDepuisFichier("soc-karate.mtx");
 
        // Console.WriteLine("\nParcours en Largeur :");
        // graphe.ParcoursLargeur(0);
 
        // Console.WriteLine("\nParcours en Profondeur :");
        // graphe.ParcoursProfondeur(0, new HashSet<int>());
 
        // Console.WriteLine("\nLe graphe est-il connexe ? " + graphe.EstConnexe());
        // Connexion connexion;
        // Statistiques statistiques;

//     }
// }
// static string connectionString = "server=localhost;database=VotreDB;user=root;password=VotreMotDePasse";

//     static void Main()
//     {
//         while (true)
//         {
//             Console.Clear();
//             Console.WriteLine("=== MENU PRINCIPAL ===");
//             Console.WriteLine("1. Gestion des Clients");
//             Console.WriteLine("2. Gestion des Cuisiniers");
//             Console.WriteLine("3. Gestion des Commandes");
//             Console.WriteLine("4. Statistiques");
//             Console.WriteLine("5. Quitter");
//             Console.Write("Choisissez une option : ");

//             string choix = Console.ReadLine();

//             switch (choix)
//             {
//                 case "1":
//                     ModuleClient();
//                     break;
//                 case "2":
//                     ModuleCuisinier();
//                     break;
//                 case "3":
//                     ModuleCommande();
//                     break;
//                 case "4":
//                     ModuleStatistiques();
//                     break;
//                 case "5":
//                     return;
//                 default:
//                     Console.WriteLine("Option invalide, veuillez réessayer.");
//                     break;
//             }
//         }
//     }

//     static void ModuleClient()
//     {
//         Console.Clear();
//         Console.WriteLine("=== Gestion des Clients ===");
//         Console.WriteLine("1. Ajouter un client");
//         Console.WriteLine("2. Modifier un client");
//         Console.WriteLine("3. Supprimer un client");
//         Console.WriteLine("4. Afficher les clients (ordre alphabétique)");
//         Console.WriteLine("5. Afficher les clients (par rue)");
//         Console.WriteLine("6. Afficher les meilleurs clients (montant des achats)");
//         Console.Write("Choisissez une option : ");

//         string choix = Console.ReadLine();
//         using (MySqlConnection conn = new MySqlConnection(connectionString))
//         {
//             conn.Open();
//             MySqlCommand cmd;

//             switch (choix)
//             {
//                 case "1":
//                     Console.Write("Nom du client : ");
//                     string nom = Console.ReadLine();
//                     Console.Write("Rue : ");
//                     string rue = Console.ReadLine();
//                     Console.Write("Montant des achats : ");
//                     decimal montant = Convert.ToDecimal(Console.ReadLine());

//                     cmd = new MySqlCommand("INSERT INTO Clients (Nom, Rue, MontantAchats) VALUES (@Nom, @Rue, @Montant)", conn);
//                     cmd.Parameters.AddWithValue("@Nom", nom);
//                     cmd.Parameters.AddWithValue("@Rue", rue);
//                     cmd.Parameters.AddWithValue("@Montant", montant);
//                     cmd.ExecuteNonQuery();
//                     Console.WriteLine("Client ajouté !");
//                     break;

//                 case "4":
//                     cmd = new MySqlCommand("SELECT * FROM Clients ORDER BY Nom ASC", conn);
//                     MySqlDataReader reader = cmd.ExecuteReader();
//                     while (reader.Read())
//                     {
//                         Console.WriteLine($"{reader["ID"]} | {reader["Nom"]} | {reader["Rue"]} | {reader["MontantAchats"]}€");
//                     }
//                     reader.Close();
//                     break;

//                 default:
//                     Console.WriteLine("Option non implémentée.");
//                     break;
//             }
//         }
//         Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
//         Console.ReadKey();
//     }

//     static void ModuleCuisinier()
//     {
//         Console.Clear();
//         Console.WriteLine("=== Gestion des Cuisiniers ===");
//         Console.WriteLine("1. Ajouter un cuisinier");
//         Console.WriteLine("2. Afficher les clients servis");
//         Console.WriteLine("3. Voir les plats réalisés par fréquence");
//         Console.WriteLine("4. Voir le plat du jour");
//         Console.Write("Choisissez une option : ");

//         string choix = Console.ReadLine();
//         using (MySqlConnection conn = new MySqlConnection(connectionString))
//         {
//             conn.Open();
//             MySqlCommand cmd;

//             switch (choix)
//             {
//                 case "4":
//                     cmd = new MySqlCommand("SELECT Nom, PlatDuJour FROM Cuisiniers", conn);
//                     MySqlDataReader reader = cmd.ExecuteReader();
//                     while (reader.Read())
//                     {
//                         Console.WriteLine($"Cuisinier: {reader["Nom"]} | Plat du jour: {reader["PlatDuJour"]}");
//                     }
//                     reader.Close();
//                     break;

//                 default:
//                     Console.WriteLine("Option non implémentée.");
//                     break;
//             }
//         }
//         Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
//         Console.ReadKey();
//     }

//     static void ModuleCommande()
//     {
//         Console.Clear();
//         Console.WriteLine("=== Gestion des Commandes ===");
//         Console.WriteLine("1. Créer une commande");
//         Console.WriteLine("2. Calculer le prix d’une commande");
//         Console.WriteLine("3. Trouver le chemin de livraison");
//         Console.Write("Choisissez une option : ");

//         string choix = Console.ReadLine();
//         using (MySqlConnection conn = new MySqlConnection(connectionString))
//         {
//             conn.Open();
//             MySqlCommand cmd;

//             switch (choix)
//             {
//                 case "2":
//                     Console.Write("Numéro de la commande : ");
//                     string commandeID = Console.ReadLine();
//                     cmd = new MySqlCommand("SELECT Prix FROM Commandes WHERE ID = @ID", conn);
//                     cmd.Parameters.AddWithValue("@ID", commandeID);
//                     object prix = cmd.ExecuteScalar();
//                     Console.WriteLine($"Prix de la commande : {prix}€");
//                     break;

//                 default:
//                     Console.WriteLine("Option non implémentée.");
//                     break;
//             }
//         }
//         Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
//         Console.ReadKey();
//     }

//     static void ModuleStatistiques()
//     {
//         Console.Clear();
//         Console.WriteLine("=== Statistiques ===");
//         Console.WriteLine("1. Nombre de livraisons par cuisinier");
//         Console.WriteLine("2. Moyenne des prix des commandes");
//         Console.Write("Choisissez une option : ");

//         string choix = Console.ReadLine();
//         using (MySqlConnection conn = new MySqlConnection(connectionString))
//         {
//             conn.Open();
//             MySqlCommand cmd;

//             switch (choix)
//             {
//                 case "2":
//                     cmd = new MySqlCommand("SELECT AVG(Prix) FROM Commandes", conn);
//                     object moyenne = cmd.ExecuteScalar();
//                     Console.WriteLine($"Moyenne des prix des commandes : {moyenne}€");
//                     break;

//                 default:
//                     Console.WriteLine("Option non implémentée.");
//                     break;
//             }
//         }
//         Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
//         Console.ReadKey();
//     }
// }
using System;
using MySql.Data.MySqlClient;

class Program
{
    static void Main()
    {
        UtilisateurManager utilisateurManager = new UtilisateurManager();
        ClientManager clientManager = new ClientManager();
        CommandeManager commande = new CommandeManager();
        MySqlConnection conn = Database.GetConnection();
        conn.Open();
        var cmd1 = new MySqlCommand("DELETE FROM Utilisateur", conn);
        var cmd = new MySqlCommand("DELETE FROM Client", conn);
        cmd.ExecuteNonQuery();
        cmd1.ExecuteNonQuery();
        // Ajout d'un utilisateur
        for (int i = 0; i<5; i++){
            utilisateurManager.AjouterUtilisateur("user" + Convert.ToString(i), "password123");

        // Affichage des utilisateurs
        // //utilisateurManager.AfficherUtilisateurs();

        // // Ajout d'un client
        // clientManager.AjouterClient("client" + Convert.ToString(i), "Zone A", "Station 1", "trans001", "user" + Convert.ToString(i),"test" + Convert.ToString(5-i), "b", "c", conn);
        // float prix = 10.5f;
        // commande.CreerCommande("client" + Convert.ToString(i), "a",prix, "a", "a");
        // // Affichage des clients
        string insertClient = "INSERT INTO Client (idClient, ZoneDeLivraison, StationProche, idTransaction, idUtilisateur) VALUES (@idClient, @zone, @station, @idTransaction, @idUtilisateur)";
                using (MySqlCommand cmdClient = new MySqlCommand(insertClient, conn))
                {
                    cmdClient.Parameters.AddWithValue("@idClient", "C123");
                    cmdClient.Parameters.AddWithValue("@zone", "Centre-ville");
                    cmdClient.Parameters.AddWithValue("@station", "Gare");
                    cmdClient.Parameters.AddWithValue("@idTransaction", "T789");
                    cmdClient.Parameters.AddWithValue("@idUtilisateur", "user0");
                    cmdClient.ExecuteNonQuery();
                }
                
                string insertCommande = "INSERT INTO Commande (idCommande, statut, idClient, prix) VALUES (@idCommande, @statut, @idClient, @prix)";
                using (MySqlCommand cmdCommande = new MySqlCommand(insertCommande, conn))
                {
                    cmdCommande.Parameters.AddWithValue("@idCommande", 1);
                    cmdCommande.Parameters.AddWithValue("@statut", "En cours");
                    cmdCommande.Parameters.AddWithValue("@idClient", "C123");
                    cmdCommande.Parameters.AddWithValue("@prix", 25.5f);
                    cmdCommande.ExecuteNonQuery();
                }
                
                Console.WriteLine("Client et commande créés avec succès.");
        }
        //clientManager.AfficherClientsAlphabetique(conn);
        clientManager.AfficherClientsRue(conn);
        conn.Close();
    }
}