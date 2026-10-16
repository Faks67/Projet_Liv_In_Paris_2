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




use liv_in_paris;
INSERT INTO Utilisateur (idUtilisateur, MotDePasse) VALUES
('U1', 'pass123'),
('U2', 'pass234'),
('U3', 'pass345'),
('U4', 'pass456'),
('U5', 'pass567');

-- Peuple la table Cuisinier
INSERT INTO Cuisinier (idCuisinier, idUtilisateur, nom, prenom, addMail, adresse, CodePostal, numTel, Station) VALUES
('C1', 'U1', 'Durand', 'Paul', 'paul.durand@mail.com', '123 Rue A', 75001, 123, 'Châtelet'),
('C2', 'U2', 'Martin', 'Lucas', 'lucas.martin@mail.com', '456 Rue B', 75002, 901, 'Gare du Nord'),
('C3', 'U3', 'Petit', 'Marie', 'marie.petit@mail.com', '789 Rue C', 75003, 312, 'Bastille'),
('C4', 'U4', 'Dubois', 'Clara', 'clara.dubois@mail.com', '321 Rue D', 75004, 123, 'Opéra'),
('C5', 'U5', 'Moreau', 'Jean', 'jean.moreau@mail.com', '654 Rue E', 75005, 534, 'Saint-Lazare');

-- Peuple la table Client
INSERT INTO Client (idClient, ZoneDeLivraison, StationProche, idUtilisateur, adresse, nom, prenom) VALUES
('CL1', 'Zone A', 'Barbès - Rochechouart', 'U1', '12 Rue Alpha', 'Leroy', 'Sophie'),
('CL2', 'Zone B', 'Exelmans', 'U2', '34 Rue Beta', 'Bernard', 'Thomas'),
('CL3', 'Zone C', 'Châtelet', 'U3', '56 Rue Gamma', 'Robert', 'Emma'),
('CL4', 'Zone D', 'Gare de Lyon', 'U4', '78 Rue Delta', 'Richard', 'Lucas'),
('CL5', 'Zone E', 'Place d’Italie', 'U5', '90 Rue Epsilon', 'Durant', 'Chloe');

-- Peuple la table Plat
INSERT INTO Plat (IdPlat, TypePlat, Quantité, Nationalité, RegimeSpe, IngrediantPrin, DateFabrication, DatePeremption, prix, idCuisinier) VALUES
('P1', 'Pizza', 10, 'Italienne', 'Aucun', 'Tomate', NOW(), DATE_ADD(NOW(), INTERVAL 7 DAY), 12.5, 'C1'),
('P2', 'Sushi', 15, 'Japonaise', 'Poisson', 'Saumon', NOW(), DATE_ADD(NOW(), INTERVAL 3 DAY), 18.0, 'C2'),
('P3', 'Burger', 20, 'Américaine', 'Aucun', 'Boeuf', NOW(), DATE_ADD(NOW(), INTERVAL 5 DAY), 15.0, 'C3'),
('P4', 'Couscous', 25, 'Maghrébine', 'Aucun', 'Semoule', NOW(), DATE_ADD(NOW(), INTERVAL 10 DAY), 20.0, 'C4'),
('P5', 'Paella', 8, 'Espagnole', 'Aucun', 'Riz', NOW(), DATE_ADD(NOW(), INTERVAL 6 DAY), 22.5, 'C5');

-- Peuple la table Commande
INSERT INTO Commande (idCommande, statut, prix, idCuisinier, depart, arrive, dateCo, idClient, IdPlat) VALUES
('CMD1', 'En cours', 25.0, 'C1', 'George V', 'Porte Maillot', NOW(), 'CL1', 'P1'),
('CMD2', 'Livrée', 40.0, 'C1', 'Châtelet', 'République', NOW(), 'CL2', 'P1'),
('CMD3', 'En cours', 30.0, 'C2', 'Gare du Nord', 'Belleville', NOW(), 'CL3', 'P2'),
('CMD4', 'Annulée', 20.0, 'C2', 'Gare du Nord', 'Gare de Lyon', NOW(), 'CL4', 'P2'),
('CMD5', 'Livrée', 50.0, 'C3', 'Bastille', 'Place d’Italie', NOW(), 'CL5', 'P3'),
('CMD6', 'En cours', 60.0, 'C4', 'Opéra', 'Barbès - Rochechouart', NOW(), 'CL1', 'P4'),
('CMD7', 'Livrée', 55.0, 'C5', 'Saint-Lazare', 'République', NOW(), 'CL2', 'P5'),
('CMD8', 'En cours', 45.0, 'C5', 'Saint-Lazare', 'Belleville', NOW(), 'CL3', 'P5'),
('CMD9', 'Annulée', 35.0, 'C3', 'Bastille', 'Gare de Lyon', NOW(), 'CL4', 'P3'),
('CMD10', 'Livrée', 70.0, 'C4', 'Opéra', 'Place d’Italie', NOW(), 'CL5', 'P4');

SELECT 'Clients' AS table_, COUNT(*) AS lignes FROM Client
UNION ALL SELECT 'Cuisiniers', COUNT(*) FROM Cuisinier
UNION ALL SELECT 'Plats', COUNT(*) FROM Plat
UNION ALL SELECT 'Commandes', COUNT(*) FROM Commande;
