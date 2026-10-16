# Liv'In Paris 2

Nos noms : Al05097823 : Houdin Alexandre  // Faks67 : Hussain Shah Fakar

Application console C# (.NET 8) de livraison de plats entre cuisiniers et clients à Paris :
- **Graphes** : plan du métro chargé depuis `Metro_GareFinal.xlsx`, plus court chemin (Dijkstra) dessiné en image, et coloration Welsh-Powell des liens cuisiniers / clients.
- **SQL** : espaces client, cuisinier et admin sur une base MySQL (`liv_in_paris`).

Version 2 de [Projet_Liv_In_Paris](https://github.com/Faks67/Projet_Liv_In_Paris), corrigée pour que tous les menus fonctionnent.

## Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL Server 9.1 installé dans `C:\Program Files\MySQL\MySQL Server 9.1` (sinon, définir la variable `MYSQL_HOME` vers son dossier)

## Lancer le projet

1. Double-cliquer sur `demarrer_mysql.cmd` et laisser la fenêtre MySQL ouverte.
   Au premier lancement, il crée une instance MySQL dédiée au projet (dossier `mysql-data/`, port 3307, utilisateur root sans mot de passe, accessible seulement depuis ce PC) et la remplit avec les données de test. Votre MySQL habituel n'est pas modifié.
2. Compiler et lancer le programme :

   ```
   dotnet run --project Projet_Liv_In_Paris/Projet_Liv_In_Paris.csproj
   ```

La partie graphes (choix 1) fonctionne sans MySQL.

Pour repartir des données de test d'origine : arrêter MySQL, supprimer le dossier `mysql-data/`, puis relancer `demarrer_mysql.cmd`.

## Comptes de test

| Rôle | ID à saisir |
|---|---|
| Clients | CL1 à CL5 |
| Cuisiniers | C1 à C5 |

## Corrections par rapport à la version 1

- Les menus reviennent au menu précédent (choix `0 - Retour`) au lieu de fermer le programme.
- Une erreur (ID inconnu, ID déjà pris, saisie invalide…) affiche un message au lieu de planter.
- Commander un plat : le client choisit un plat et un nombre de portions ; cuisinier, prix, stations de départ et d'arrivée sont remplis automatiquement et le stock du plat est décrémenté.
- Requêtes SQL alignées sur le schéma (`idTransaction`, `adresse`, `dateCommande`, `nomPlat` n'existaient pas).
- Les ID cuisinier (`C1`…) ne sont plus lus comme des nombres ; les numéros de téléphone sont stockés en texte.
- Prix acceptés avec virgule ou point, dates au format `aaaa-mm-jj` ou `jj/mm/aaaa`.
- Recherche des stations insensible aux accents et apostrophes (« Place d’Italie » = « place d'italie »).
- Plus de chemins absolus vers un PC particulier ; les images de trajet s'ouvrent automatiquement.
