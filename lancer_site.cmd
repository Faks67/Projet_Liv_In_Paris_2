@echo off
rem Lance le site Liv'In Paris : MySQL (si besoin), le serveur web, puis le navigateur sur http://localhost:5080
chcp 65001 >nul
cd /d "%~dp0"
if not defined MYSQL_HOME set "MYSQL_HOME=C:\Program Files\MySQL\MySQL Server 9.1"

"%MYSQL_HOME%\bin\mysqladmin.exe" -h 127.0.0.1 -P 3307 -u root ping >nul 2>&1 || call "%~dp0demarrer_mysql.cmd"

echo Demarrage du site sur http://localhost:5080 (fermez cette fenetre pour l'arreter)...
start "" /b cmd /c ""%SystemRoot%\System32\timeout.exe" /t 6 /nobreak >nul & start http://localhost:5080"
dotnet run --project "%~dp0LivInParis.Web\LivInParis.Web.csproj"
pause
