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






