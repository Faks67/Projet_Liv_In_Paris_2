DROP database if exists liv_in_paris;
create database liv_in_paris;
use liv_in_paris;

CREATE TABLE Utilisateur(
   idUtilisateur VARCHAR(50),
   MotDePasse VARCHAR(50) NOT NULL,
   PRIMARY KEY(idUtilisateur)
);

CREATE TABLE Livraison(
   idLivraison VARCHAR(50),
   NbLivraison VARCHAR(50),
   AdressseLivraison VARCHAR(50),
   PRIMARY KEY(idLivraison)
);

CREATE TABLE Recette(
   idRecette VARCHAR(50),
   descriptionR VARCHAR(50),
   TempsPrep VARCHAR(50),
   Cout DECIMAL(15,2),
   PRIMARY KEY(idRecette)
);

CREATE TABLE transaction(
   idTransaction VARCHAR(50),
   Prix INT,
   PRIMARY KEY(idTransaction)
);

CREATE TABLE Client(
   idClient VARCHAR(50),
   ZoneDeLivraison VARCHAR(50),
   StationProche VARCHAR(50),
   idUtilisateur VARCHAR(50) NOT NULL,
   adresse VARCHAR(50),
   nom VARCHAR(50),
   prenom VARCHAR(50),
   PRIMARY KEY(idClient),
   UNIQUE(idUtilisateur),
   FOREIGN KEY(idUtilisateur) REFERENCES Utilisateur(idUtilisateur)
);

CREATE TABLE Particulier(
   idClient VARCHAR(50),
   addMail VARCHAR(50),
   CodePostal INT,
   Ville VARCHAR(50),
   numTel VARCHAR(20),
   Station VarChar(50),
   PRIMARY KEY(idClient),
   FOREIGN KEY(idClient) REFERENCES Client(idClient)
);


CREATE TABLE Entreprise(
   idClient VARCHAR(50),
   NomEntreprise VARCHAR(50),
   Referent VARCHAR(50),
   PRIMARY KEY(idClient),
   FOREIGN KEY(idClient) REFERENCES Client(idClient)
);
CREATE TABLE Cuisinier(
   idCuisinier VARCHAR(50),
   idUtilisateur VARCHAR(50) NOT NULL,
   nom VARCHAR(50),
   prenom VARCHAR(50),
   addMail VARCHAR(50),
   adresse VARCHAR(50),
   CodePostal INT,
   numTel VARCHAR(20),
   Station VarChar(50),
   PRIMARY KEY(idCuisinier),
   UNIQUE(idUtilisateur),
   FOREIGN KEY(idUtilisateur) REFERENCES Utilisateur(idUtilisateur)
);

CREATE TABLE Plat(
   IdPlat VarChar(50),
   TypePlat VARCHAR(50),
   Quantité INT,
   Nationalité VARCHAR(50),
   RegimeSpe VARCHAR(50),
   IngrediantPrin VARCHAR(50),
   DateFabrication DATE,
   DatePeremption DaTE,
   prix FLOAT,
   idCuisinier VARCHAR(50) NOT NULL,
   PRIMARY KEY(IdPlat),
   FOREIGN KEY(idCuisinier) REFERENCES Cuisinier(idCuisinier)
);


CREATE TABLE Avis(
   idAvis VARCHAR(50),
   Note INT,
   DateAvis DATE,
   Description VARCHAR(50),
   idClient VARCHAR(50) NOT NULL,
   PRIMARY KEY(idAvis),
   FOREIGN KEY(idClient) REFERENCES Client(idClient)
);

CREATE TABLE Commande(
   idCommande VarChar(50),
   statut VARCHAR(50),
   prix FLOAT,
   idCuisinier VARCHAR(50) NOT NULL,
   depart VarChar(50),
   arrive VarChar(50),
   dateCo datetime,
	idClient VARCHAR(50) NOT NULL,
   IdPlat VarChar(50),
   PRIMARY KEY(idCommande),
   FOREIGN KEY(idClient) REFERENCES Client(idClient),
   FOREIGN KEY(idCuisinier) REFERENCES Cuisinier(idCuisinier),
   FOREIGN KEY (IdPlat) REFERENCES Plat(IdPlat)
);



