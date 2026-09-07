# Excellent Taste C#

C#-versie van Excellent Taste met een ASP.NET Core API, WinForms-client en webclient.

## Documentatie

De belangrijkste projectdocumentatie staat in dezelfde map als deze README:

- [Installatiehandleiding](Installatiehandleiding.md)
- [API-overzicht](API-overzicht.md)

## Database

De API gebruikt MySQL. Als je XAMPP gebruikt en `excellenttaste_db` al in phpMyAdmin staat, hoef je niets meer te importeren.

Moet je de database nog aanmaken? Importeer dan de originele database-dump uit het PHP-project:

```text
database/excellenttaste_db.sql
```

Voorbeeld met de MySQL command line:

```text
mysql -u root -p < database/excellenttaste_db.sql
```

De standaard XAMPP-verbinding gebruikt `root` zonder wachtwoord:

```text
Server=127.0.0.1;Port=3306;Database=excellenttaste_db;User ID=root;Password=;Allow Zero Datetime=True;Convert Zero Datetime=True;
```

De API-verbinding staat in:

```text
src/ExcellentTaste.Api/appsettings.json
```

Pas deze connection string alleen aan als jouw MySQL-gebruiker of wachtwoord anders is.


## Starten

Start de API. Open een terminal in dezelfde map als deze README en run het command:

```text
dotnet run --project src/ExcellentTaste.Api/ExcellentTaste.Api.csproj --urls http://localhost:5000
```

Controleer:

```text
http://localhost:5000/api/status
```
