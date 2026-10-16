@echo off
rem Demarre l'instance MySQL dediee a Liv in Paris (port 3307, accessible seulement depuis ce PC).
rem Au premier lancement, cree la base liv_in_paris avec les donnees de test.
chcp 65001 >nul
cd /d "%~dp0"
if not defined MYSQL_HOME set "MYSQL_HOME=C:\Program Files\MySQL\MySQL Server 9.1"
set "DATA=%~dp0mysql-data"

if exist "%MYSQL_HOME%\bin\mysqld.exe" goto mysql_ok
echo MySQL introuvable dans "%MYSQL_HOME%".
echo Installez MySQL Server ou definissez MYSQL_HOME vers son dossier d'installation.
pause
exit /b 1

:mysql_ok
set PREMIER_LANCEMENT=0
if exist "%DATA%\mysql.ibd" goto demarrer
echo Premier lancement : creation de l'instance MySQL du projet...
"%MYSQL_HOME%\bin\mysqld.exe" --no-defaults --initialize-insecure --basedir="%MYSQL_HOME%" --datadir="%DATA%"
set PREMIER_LANCEMENT=1

:demarrer
start "MySQL Liv in Paris (laisser ouvert)" "%MYSQL_HOME%\bin\mysqld.exe" --no-defaults --basedir="%MYSQL_HOME%" --datadir="%DATA%" --port=3307 --bind-address=127.0.0.1 --mysqlx=OFF --console
if "%PREMIER_LANCEMENT%"=="0" goto fin

echo Attente du demarrage de MySQL...
set ESSAIS=0
:attente
"%MYSQL_HOME%\bin\mysqladmin.exe" -h 127.0.0.1 -P 3307 -u root ping >nul 2>&1 && goto pret
set /a ESSAIS+=1
if %ESSAIS% geq 30 goto echec
"%SystemRoot%\System32\timeout.exe" /t 1 /nobreak >nul
goto attente

:pret
echo Creation de la base liv_in_paris...
"%MYSQL_HOME%\bin\mysql.exe" -h 127.0.0.1 -P 3307 -u root --default-character-set=utf8mb4 --table < "Projet_Liv_In_Paris\installer_base.sql"
goto fin

:echec
echo MySQL ne repond pas.
pause
exit /b 1

:fin
echo.
echo MySQL est lance sur le port 3307. Laissez sa fenetre ouverte puis lancez le programme.
"%SystemRoot%\System32\timeout.exe" /t 5 >nul
