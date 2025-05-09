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
('CL2', 'Zone B', 'République', 'U2', '34 Rue Beta', 'Bernard', 'Thomas'),
('CL3', 'Zone C', 'Belleville', 'U3', '56 Rue Gamma', 'Robert', 'Emma'),
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
('CMD1', 'En cours', 25.0, 'C1', 'Station A', 'Station B', NOW(), 'CL1', 'P1'),
('CMD2', 'Livrée', 40.0, 'C1', 'Station A', 'Station C', NOW(), 'CL2', 'P1'),
('CMD3', 'En cours', 30.0, 'C2', 'Station B', 'Station D', NOW(), 'CL3', 'P2'),
('CMD4', 'Annulée', 20.0, 'C2', 'Station B', 'Station E', NOW(), 'CL4', 'P2'),
('CMD5', 'Livrée', 50.0, 'C3', 'Station C', 'Station A', NOW(), 'CL5', 'P3'),
('CMD6', 'En cours', 60.0, 'C4', 'Station D', 'Station B', NOW(), 'CL1', 'P4'),
('CMD7', 'Livrée', 55.0, 'C5', 'Station E', 'Station C', NOW(), 'CL2', 'P5'),
('CMD8', 'En cours', 45.0, 'C5', 'Station E', 'Station D', NOW(), 'CL3', 'P5'),
('CMD9', 'Annulée', 35.0, 'C3', 'Station C', 'Station E', NOW(), 'CL4', 'P3'),
('CMD10', 'Livrée', 70.0, 'C4', 'Station D', 'Station A', NOW(), 'CL5', 'P4');