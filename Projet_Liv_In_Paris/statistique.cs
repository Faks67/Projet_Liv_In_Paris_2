// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;

// namespace Liv_In_Paris
// {
//     class Statistiques
//     {
//         // Afficher par cuisinier le nombre de livraisons effectuées 
//         // Afficher les commandes selon une période de temps 
//         // Afficher la moyenne des prix des commandes
//         // Afficher la moyenne des comptes clients
//         // Afficher la liste des commandes pour un client selon la nationalité des plats, la période 

//         Connexion connexion;


//         public Statistiques(Connexion connexion)
//         {
//             this.connexion = connexion;
//             Menu();
//         }

//         public void Menu()
//         {
//             Console.Clear();
//             Console.ForegroundColor = ConsoleColor.Blue;
//             Console.WriteLine("====================================================");
//             Console.WriteLine("                       STATISTIQUES");
//             Console.WriteLine("====================================================\n\n");
//             Console.ResetColor();
//             Console.WriteLine(" Bienvenue dans le module Statistiques ");
//             Console.WriteLine("----------------------------------------------------");
//             Console.WriteLine("  1.  Afficher par cuisinier le nombre de livraisons effectuées ");
//             Console.WriteLine("  2.  Afficher les commandes selon une période de temps ");
//             Console.WriteLine("  3.  Afficher la moyenne des prix des commandes");
//             Console.WriteLine("  4.  Afficher la moyenne des comptes clients");
//             Console.WriteLine("  5.  Afficher la liste des commandes pour un client selon la nationalité des plats, la période ");
//             Console.WriteLine("  6.  Quitter");
//             Console.WriteLine("----------------------------------------------------");
//             Console.Write(" Sélectionnez une option : ");
//             string choix = Console.ReadLine();
//             string commande;
//             switch (choix)
//             {
//                 case "1":
//                     Console.Clear();
//                     commande = "SELECT U.Prenom, U.Nom, COUNT(L.IdLivraison) AS NombreDeLivraisons FROM Utilisateur U JOIN Livraison L ON U.Id = L.IdLivreur JOIN LigneDeCommande LC ON L.IdLigneCommande = LC.IdLigneCommande JOIN Plat P ON LC.IdPlat = P.IdPlat WHERE L.Statut = 'Livrée' GROUP BY U.Id;";
//                     connexion.executerRequete(commande);
//                     Console.WriteLine();
//                     connexion.afficherResultatRequete();
//                     Console.WriteLine("Appuyez sur une touche pour continuer...");
//                     Console.ReadKey();
//                     break;
//                 case "2":
//                     Console.Clear();
//                     (string dateDebut, string dateFin) = SaisirDate();

//                     commande = $"SELECT * FROM Commande WHERE DateCommande BETWEEN '{dateDebut}' AND '{dateFin}';";
//                     connexion.executerRequete(commande);
//                     Console.WriteLine();
// connexion.afficherResultatRequete();
//                     Console.WriteLine("Appuyez sur une touche pour continuer...");
//                     Console.ReadKey();
//                     break;
//                 case "3":
//                     Console.Clear();
//                     commande = "SELECT ROUND(AVG(TotalPrix), 2) AS MoyennePrixCommandes FROM (SELECT C.IdCommande, SUM(P.Prix * LC.Quantite) AS TotalPrix FROM Commande C JOIN LigneDeCommande LC ON C.IdCommande = LC.IdCommande JOIN Plat P ON LC.IdPlat = P.IdPlat GROUP BY C.IdCommande) AS SousRequete;";
//                     connexion.executerRequete(commande);
//                     Console.WriteLine();
//                     connexion.afficherResultatRequete();
//                     Console.WriteLine("Appuyez sur une touche pour continuer...");
//                     Console.ReadKey();
//                     break;
//                 case "4":
//                     Console.Clear();
//                     commande = "SELECT U.Nom, U.Prenom, ROUND(AVG(TotalPrix), 2) AS MoyennePrixCommandes FROM Utilisateur U JOIN Commande C ON U.IdClient = C.IdClient JOIN LigneDeCommande LC ON C.IdCommande = LC.IdCommande JOIN Plat P ON LC.IdPlat = P.IdPlat GROUP BY U.Id;\r\n";
//                     connexion.executerRequete(commande);
//                     Console.WriteLine();
//                     connexion.afficherResultatRequete();
//                     Console.WriteLine("Appuyez sur une touche pour continuer...");
//                     Console.ReadKey();
//                     break;
//                 case "5":
//                     Console.Clear();
//                     string nationalite = SaisirNationalite();
//                     (string dateDebut2, string dateFin2) = SaisirDate();
//                     string idClient = SaisirIdClient();
//                     commande = $@"SELECT C.IdCommande, C.DateCommande, P.Nom AS NomPlat, P.Nationalite FROM Commande C JOIN LigneDeCommande LC ON C.IdCommande = LC.IdCommande JOIN Plat P ON LC.IdPlat = P.IdPlat WHERE C.IdClient = {idClient} AND C.DateCommande BETWEEN '{dateDebut2}' AND '{dateFin2}' AND P.Nationalite = '{nationalite}';";
//                     connexion.executerRequete(commande);
//                     Console.WriteLine();
//                     connexion.afficherResultatRequete();
//                     Console.WriteLine("Appuyez sur une touche pour continuer...");
//                     Console.ReadKey();
//                     break;
//                 case "6":
//                     Environment.Exit(0);
//                     break;
//                 default:
//                     Console.ForegroundColor = ConsoleColor.Red;
//                     Console.WriteLine("Option invalide. Appuyez sur une touche pour continuer...");
//                     Console.ResetColor();
//                     Console.ReadKey();
//                     Menu();
//                     break;
//             }
//         }
//         public (string, string) SaisirDate()
//         {
//             Console.WriteLine("Entrez la date de début (YYYY-MM-DD) : ");
//             string anneeD = "";
//             string moisD = "";
//             string jourD = "";

//             while (string.IsNullOrEmpty(anneeD) || anneeD.Length != 4 || !int.TryParse(anneeD, out int yearD) || yearD < 0)
//             {
//                 Console.WriteLine("Année : ");
//                 anneeD = Console.ReadLine();
//             }

//             while (string.IsNullOrEmpty(moisD) || !int.TryParse(moisD, out int monthD) || monthD < 1 || monthD > 12)
//             {
                
//                 Console.WriteLine("Mois : ");
//                 moisD = Console.ReadLine();
//             }

//             while (string.IsNullOrEmpty(jourD) || !int.TryParse(jourD, out int dayD) || dayD < 1 || dayD > 31 || !EstJourValide(anneeD, moisD, dayD))
//             {
//                 Console.WriteLine("Jour : ");
//                 jourD = Console.ReadLine();
//             }

//             string dateDebut = $"{anneeD}-{moisD.PadLeft(2, '0')}-{jourD.PadLeft(2, '0')}";
//             Console.WriteLine($"Date de début valide : {dateDebut}");

//             Console.WriteLine("Entrez la date de fin (YYYY-MM-DD) : ");
//             string anneeF = "";
//             string moisF = "";
//             string jourF = "";

//             while (string.IsNullOrEmpty(anneeF) || anneeF.Length != 4 || !int.TryParse(anneeF, out int yearF) || yearF < 0)
//             {
//                 Console.WriteLine("Année : ");
//                 anneeF = Console.ReadLine();
//             }

//             while (string.IsNullOrEmpty(moisF) || !int.TryParse(moisF, out int monthF) || monthF < 1 || monthF > 12)
//             {
//                 Console.WriteLine("Mois : ");
//                 moisF = Console.ReadLine();
//             }

//             while (string.IsNullOrEmpty(jourF) || !int.TryParse(jourF, out int dayF) || dayF < 1 || dayF > 31 || !EstJourValide(anneeF, moisF, dayF))
//             {
//                 Console.WriteLine("Jour : ");
//                 jourF = Console.ReadLine();
//             }

//             string dateFin = $"{anneeF}-{moisF.PadLeft(2, '0')}-{jourF.PadLeft(2, '0')}";
//             Console.WriteLine($"Date de fin valide : {dateFin}");

//             return (dateDebut, dateFin);
//         }

//         static bool EstJourValide(string annee, string mois, int jour)
//         {
//             int month = int.Parse(mois);
//             int year = int.Parse(annee);

//             if (month == 2)
//             {
//                 if ((year % 4 == 0 && year % 100 != 0) || (year % 400 == 0))
//                 {
//                     return jour <= 29;
//                 }
//                 return jour <= 28;
//             }
//             if (month == 4 || month == 6 || month == 9 || month == 11)
//             {
//                 return jour <= 30;
//             }
//             return jour <= 31;
//         }

//         public string SaisirNationalite()
//         {
//             string[] nationalitesValid = { "Italienne", "Marocaine", "Française", "Chinoise", "Espagnole" }; // A rendre customisable
//             string nationalite;

//             while (true)
//             {
//                 Console.WriteLine("Entrez la nationalité des plats : ");
//                 nationalite = Console.ReadLine();

//                 if (nationalitesValid.Contains(nationalite))
//                 {
//                     return nationalite;
//                 }

//                 Console.WriteLine("Cette nationalité n'existe pas dans la liste. Veuillez en saisir une autre.");
//             }
//         }

//         string SaisirIdClient()
//         {
//             string idClient;

//             while (true)
//             {
//                 Console.WriteLine("Entrez l'ID du client : ");
//                 idClient = Console.ReadLine();

//                 if (int.TryParse(idClient, out int idEntier) && idEntier > 0)
//                 {
//                     return idEntier.ToString(); // Retourne l'ID valide
//                 }
//                 Console.WriteLine("Veuillez entrer un ID valide (un nombre positif).");
//             }
//         }

//     }


// }
using System;
using MySql.Data.MySqlClient;

public class StatistiquesManager
{
        public void NombreLivraisonsParCuisinier(MySqlConnection conn)
    {
            var cmd = new MySqlCommand("SELECT CuisinierID, COUNT(*) AS NbLivraisons FROM Commandes GROUP BY CuisinierID", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"Cuisinier {reader["CuisinierID"]} a fait {reader["NbLivraisons"]} livraisons");
            }
    }
    public void MoyennePrixCommandes(MySqlConnection conn)
    {
            var cmd = new MySqlCommand("SELECT AVG(Prix) FROM Commandes", conn);
            object moyenne = cmd.ExecuteScalar();
            Console.WriteLine($"Moyenne des prix des commandes : {moyenne}€");
    }
    public void AfficherCommandesParPeriode(DateTime dateDebut, DateTime dateFin, MySqlConnection conn)
        {
                string query = "SELECT * FROM Commande WHERE dateCo BETWEEN @dateDebut AND @dateFin";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@dateDebut", dateDebut);
                    cmd.Parameters.AddWithValue("@dateFin", dateFin);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("Commandes entre " + dateDebut + " et " + dateFin + ":");
                        while (reader.Read())
                        {
                            Console.WriteLine($"Commande {reader["idCommande"]}: {reader["statut"]}, Prix: {reader["prix"]} €, Départ: {reader["depart"]}, Arrivée: {reader["arrive"]}, Date: {reader["dateCo"]}");
                        }
                    }
                }
        }
        public void AfficherCommandesClientNationalite(string idClient, string nationalite, MySqlConnection conn)
        {
                string query = "SELECT * FROM Commande WHERE idClient = @idClient AND nationalite = @nationalite";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idClient", idClient);
                    cmd.Parameters.AddWithValue("@nationalite", nationalite);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Commande {reader["idCommande"]}: {reader["statut"]}, Prix: {reader["prix"]} €, Départ: {reader["depart"]}, Arrivée: {reader["arrive"]}, Date: {reader["dateCo"]}");
                        }
                    }
                }
            }
        public void AfficherCommandesClientPeriode(string idClient,  DateTime dateDebut, DateTime dateFin, MySqlConnection conn)
        {
                string query = "SELECT * FROM Commande WHERE idClient = @idClient AND dateCo BETWEEN @dateDebut AND @dateFin";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idClient", idClient);
                    cmd.Parameters.AddWithValue("@dateDebut", dateDebut);
                    cmd.Parameters.AddWithValue("@dateFin", dateFin);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Commande {reader["idCommande"]}: {reader["statut"]}, Prix: {reader["prix"]} €, Départ: {reader["depart"]}, Arrivée: {reader["arrive"]}, Date: {reader["dateCo"]}");
                        }
                    }
                }
            }
            public void AfficherMoyenneClients(MySqlConnection conn)
        {
                string query = "SELECT AVG(SUM(prix)) AS MoyenneCompte FROM Commande GROUP BY idClient";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine("Moyenne des comptes clients: " + reader["MoyenneCompte"] + " €");
                    }
                }
            
        }
}