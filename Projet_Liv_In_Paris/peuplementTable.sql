use liv_in_paris;
Insert into `utilisateur`(idUtilisateur, MotDePasse) values (1,1);
Insert into `utilisateur`(idUtilisateur, MotDePasse) values (2,2);
Insert into `client`(idClient, idTransaction, idUtilisateur) values (1,0,1);
Insert into `recette`(idRecette,descriptionR,TempsPrep,cout) values ('a','a','a',1);

LOAD DATA INFILE 'C:/ProgramData/MySQL/MySQL Server 8.0/Uploads/Clients.csv' 
INTO TABLE Particulier
FIELDS TERMINATED BY ';'
LINES STARTING BY ''
TERMINATED BY '\r\n'
IGNORE 1 LINES
(idClient,nom,prenom,nomRue,nbRue,CodePostal,Ville,numTel,addMail,Station)
;

LOAD DATA INFILE 'C:/ProgramData/MySQL/MySQL Server 8.0/Uploads/Cuisiniers.csv' 
INTO TABLE Cuisinier
FIELDS TERMINATED BY ';'
LINES STARTING BY ''
TERMINATED BY '\r\n'
IGNORE 1 LINES
(idCuisinier,nom,prenom,nomRue,nbRue,CodePostal,Ville,numTel,addMail,Station)
;
LOAD DATA INFILE 'C:/ProgramData/MySQL/MySQL Server 8.0/Uploads/Commandes.csv' 
INTO TABLE Plat
FIELDS TERMINATED BY ';'
LINES STARTING BY ''
TERMINATED BY '\r\n'
IGNORE 1 LINES
(idPLat,IdCuisinier,IdClient,nom,prix,Quantité,TypePlat,DateFabrication,DatePeremption,RegimeSpe,Nationalité)
;


USE LivInParis;

INSERT INTO Client (NomEntreprise, MotDePasse) VALUES
('Boulangerie Dupont', 'mdp123'),
('Entreprise XYZ', 'xyz789');

INSERT INTO Cuisinier (MotDePasse) VALUES
('cuisine123'),
('chef789');

INSERT INTO Station (Nom, Latitude, Longitude) VALUES
('Gare de Lyon', 40.8632, 5.4764),
('Châtelet', 45.3880, 3.3489);

INSERT INTO Utilisateur (Nom, Prenom, Adresse, Telephone, Email, IdCuisinier, IdClient, IdStationProche, EstBanni) VALUES
('Dupond', 'Jean', '10 rue de Paris', '0123456789', 'jean.dupond@email.com', NULL, 1, 1, FALSE),
('Martin', 'Sofie', '15 avenue de Lyon', '0987654321', 'sofie.martin@email.com', 1, NULL, 2, FALSE);

INSERT INTO Recette (Nom) VALUES
('Ratatouille'),
('Tarte aux fraises');

INSERT INTO Plat (Nom, Prix, IdCuisinier, Type, Personnes, DateFabrication, DatePeremption, Regime, IdRecette, CheminAccesPhoto, Nationalite) VALUES
('Lasagnes', 12.50, 1, 'Plat Principal', 2, '2025-03-01', '2025-03-03', 'Végétarien', 1, 'lasagnes.jpg', 'Italienne'),
('Couscous', 15.00, 2, 'Plat Principal', 4, '2025-03-02', '2025-03-04', 'Halal', 2, 'couscous.jpg', 'Marocaine');

INSERT INTO Ingredient (Nom, Prix) VALUES
('Tomates', 2.50),
('Semoule', 1.20);

INSERT INTO ListeIngredients (IdIngredient, IdRecette, Quantite) VALUES
(1, 1, 3),
(2, 2, 2);

INSERT INTO Commande (IdClient, DateCommande, Statut) VALUES
(1, NOW(), 'En attente'),
(2, NOW(), 'Validée'),
(2, NOW(), 'Validée'),
(2, NOW(), 'Validée'),
(2, NOW(), 'Validée');

INSERT INTO LigneDeCommande (IdCommande, IdPlat, Quantite, DateLivraison, LieuLivraison) VALUES
(1, 1, 2, '2025-03-02', '10 rue de Paris'),
(2, 2, 1, '2025-03-03', '15 avenue de Lyon'),
(3, 2, 1, '2025-03-03', '15 avenue de Lyon'),
(4, 2, 1, '2025-03-03', '15 avenue de Lyon'),
(5, 2, 1, '2025-03-03', '15 avenue de Lyon');

INSERT INTO Livraison (IdLigneCommande, IdLivreur, IdStationDepart, IdStationArrivee, Statut) VALUES
(1, 1, 1, 2, 'En attente'),
(2, 1, 2, 1, 'En cours'),
(3, 1, 2, 1, 'Livrée'),
(4, 1, 2, 1, 'Livrée'),
(5, 2, 2, 1, 'Livrée');

INSERT INTO Ligne (Nom) VALUES
('Ligne 1'),
('Ligne 14');

INSERT INTO Correspondance (IdStation, IdLigne) VALUES
(1, 1),
(2, 2);

INSERT INTO Transaction (IdCommande, Montant, Reussie, DateTransaction) VALUES
(1, 25.00, TRUE, NOW()),
(2, 15.00, TRUE, NOW());





