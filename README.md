# Liv'In Paris 2

Nos noms : Al05097823 : Houdin Alexandre  // Faks67 : Hussain Shah Fakar

Plateforme de livraison de plats faits maison entre cuisiniers et clients parisiens, livrés en métro.

Version 2 de [Projet_Liv_In_Paris](https://github.com/Faks67/Projet_Liv_In_Paris), avec :
- **un site web** (`LivInParis.Web`) : catalogue, panier, livraison à une adresse avec itinéraire en métro sur une carte de Paris, espaces client, cuisinier et administration ;
- **l'application console** d'origine (`Projet_Liv_In_Paris`), corrigée pour que tous les menus fonctionnent.

Les deux partagent la même base MySQL (`liv_in_paris`) et le même plan du métro (`Metro_GareFinal.xlsx`).

## Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL Server 9.1 installé dans `C:\Program Files\MySQL\MySQL Server 9.1` (sinon, définir la variable `MYSQL_HOME` vers son dossier)

## Lancer le site web

Double-cliquer sur `lancer_site.cmd` : il démarre MySQL si besoin, lance le serveur et ouvre http://localhost:5080.

Ou à la main :

1. `demarrer_mysql.cmd` (laisser sa fenêtre ouverte)
2. `dotnet run --project LivInParis.Web`
3. Ouvrir http://localhost:5080

### Ce que fait le site

| Espace | Fonctions |
|---|---|
| Tout le monde | Catalogue avec recherche et filtres (cuisine, régime), fiche de chaque plat, panier, plan du métro interactif : trajet le plus rapide entre deux adresses ou stations |
| Client | Inscription avec adresse (autocomplétion), commande livrée à n'importe quelle adresse avec itinéraire depuis chaque cuisinier et durée estimée, suivi et annulation des commandes |
| Cuisinier | Mise en vente et modification des plats (stock, prix, conservation), commandes à préparer (« Partir livrer », « Marquer livrée », « Refuser »), itinéraire de livraison, clients servis, plats les plus vendus |
| Admin | Chiffre d'affaires, panier moyen, graphiques (commandes par jour, livraisons par cuisinier, cuisines), clients, cuisiniers, commandes filtrables par période et client, coloration de Welsh-Powell, exports JSON |

Un même compte peut être à la fois client et cuisinier (page « Mon profil »).

### Comment les trajets sont calculés

- Le plan du métro (stations, coordonnées GPS, lignes, temps entre stations et temps de correspondance) est lu depuis `Metro_GareFinal.xlsx`.
- Une adresse est convertie en coordonnées par le service public de géocodage (Géoplateforme IGN), puis rattachée à la station la plus proche.
- Le plus court chemin en minutes est calculé avec Dijkstra (`LivInParis.Web/Services/Metro.cs`), marche à pied comprise au départ et à l'arrivée.

### Comptes de démonstration

| Compte | Identifiant | Mot de passe |
|---|---|---|
| Client + cuisinier | U1 à U5 | pass123, pass234, pass345, pass456, pass567 |
| Administrateur | admin | admin (à changer dans `LivInParis.Web/appsettings.json`) |

Les mots de passe sont hachés (PBKDF2) à la première connexion sur le site.

## Lancer l'application console

```
dotnet run --project Projet_Liv_In_Paris/Projet_Liv_In_Paris.csproj
```

La partie graphes (choix 1) fonctionne sans MySQL. En mode client / cuisinier, la console demande les ID `CL1`…`CL5` / `C1`…`C5`.

## Données

Pour repartir des données de test d'origine : arrêter MySQL, supprimer le dossier `mysql-data/`, puis relancer `demarrer_mysql.cmd`.

Au démarrage, le site ajoute à la base les colonnes dont il a besoin (portions, adresse et coordonnées de livraison, durée estimée, description des plats). La base reste compatible avec la console.

## Corrections de l'application console par rapport à la version 1

- Les menus reviennent au menu précédent (choix `0 - Retour`) au lieu de fermer le programme.
- Une erreur (ID inconnu, ID déjà pris, saisie invalide…) affiche un message au lieu de planter.
- Commander un plat : le client choisit un plat et un nombre de portions ; cuisinier, prix, stations de départ et d'arrivée sont remplis automatiquement et le stock du plat est décrémenté.
- Requêtes SQL alignées sur le schéma (`idTransaction`, `adresse`, `dateCommande`, `nomPlat` n'existaient pas).
- Les ID cuisinier (`C1`…) ne sont plus lus comme des nombres ; les numéros de téléphone sont stockés en texte.
- Prix acceptés avec virgule ou point, dates au format `aaaa-mm-jj` ou `jj/mm/aaaa`.
- Recherche des stations insensible aux accents et apostrophes (« Place d’Italie » = « place d'italie »).
- Plus de chemins absolus vers un PC particulier ; les images de trajet s'ouvrent automatiquement.
