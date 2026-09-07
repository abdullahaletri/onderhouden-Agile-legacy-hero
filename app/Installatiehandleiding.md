# Installatiehandleiding Excellent Taste

Deze handleiding helpt je om de C#-versie van Excellent Taste lokaal te starten.

## Benodigdheden

- Visual Studio 2022 of nieuwer
- .NET 8 SDK
- Git
- MySQL Server of MariaDB

## Project openen

Open de solution:

```text
ExcellentTaste.sln
```

Laat Visual Studio daarna de NuGet-pakketten herstellen. De packages worden opgehaald via `nuget.org`; je hoeft geen lokale package-map te selecteren.

De solution bevat vijf projecten:

```text
ExcellentTaste.Core      gedeelde logica en repositories
ExcellentTaste.Api       API die de applicatie beschikbaar maakt
ExcellentTaste.WinForms  desktopclient voor de API
ExcellentTaste.Web       kleine ASP.NET-webclient voor de API
ExcellentTaste.StudentTests kleine tests bij tickets
```

## Database

De API maakt verbinding met MySQL. De structuur en testgegevens komen uit dezelfde dump als het PHP-project.

Gebruik je XAMPP en zie je in phpMyAdmin al de database `excellenttaste_db` met tabellen zoals `klant`, `reservering` en `menuitem`? Dan is de database al klaar.

De dump staat in:

```text
database/excellenttaste_db.sql
```

Moet je de database nog aanmaken? Start MySQL of MariaDB en importeer de dump:

```text
mysql -u root -p < database/excellenttaste_db.sql
```

De standaard verbinding staat in:

```text
src/ExcellentTaste.Api/appsettings.json
```

Standaard gebruikt de API de normale XAMPP-instellingen:

```text
Server=127.0.0.1
Port=3306
Database=excellenttaste_db
User ID=root
Password=
```

Pas de connection string aan als jouw MySQL-gebruiker of wachtwoord anders is.

De WinForms-client geeft dezelfde XAMPP-connection string mee aan de gedeelde databasecode.

## API starten

Start eerst de API:

```text
dotnet run --project src/ExcellentTaste.Api/ExcellentTaste.Api.csproj --urls http://localhost:5000
```

Controleer daarna in de browser:

```text
http://localhost:5000/api/status
```

Als de API werkt, zie je dat de status actief is.

Handige testroutes:

```text
http://localhost:5000/api/reserveringen
http://localhost:5000/api/menuitems
http://localhost:5000/api/bestellingen/1/bon
```

## WinForms-client starten

Laat de API draaien.

Start daarna:

```text
ExcellentTaste.WinForms
```

Het standaard API-adres is:

```text
http://localhost:5000
```

Klik op:

- `Reserveringen laden`
- `Menu laden`

Als er gegevens verschijnen, werkt de koppeling met de API.

## Webclient starten

Laat de API draaien.

Start daarna:

```text
ExcellentTaste.Web
```

De webclient probeert automatisch verbinding te maken met:

```text
http://localhost:5000
```

Op de startpagina worden reserveringen uit de API getoond.

## Studententests draaien

Voor sommige tickets zijn xUnit-tests toegevoegd. Deze tests hebben geen database of draaiende API nodig.

Draai vanuit de map `ExcellentTaste-CSharp`:

```text
dotnet test tests/ExcellentTaste.StudentTests/ExcellentTaste.StudentTests.csproj
```

Je kunt de tests ook draaien via `Test Explorer` in Visual Studio.

Bij de start van de opdracht is het normaal dat niet alle tests slagen. Een falende test wijst naar gedrag dat nog onderzocht of opgelost moet worden.

## Veelvoorkomende problemen

**De WinForms-app toont geen gegevens**

Controleer of `ExcellentTaste.Api` draait en of het API-adres klopt.

**De webclient zegt dat de API niet bereikbaar is**

Start eerst de API en vernieuw daarna de webpagina.

**De database bevat oude testdata**

Importeer `database/excellenttaste_db.sql` opnieuw. Let op: als je tabellen al bestaan, verwijder eerst de database of maak een lege database aan voordat je importeert.

**Het project herstelt NuGet-pakketten niet via internet**

De MySQL-driver en testpackages worden via NuGet opgehaald. Controleer of Visual Studio of `dotnet restore` toegang heeft tot `nuget.org`.
