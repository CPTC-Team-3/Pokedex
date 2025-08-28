# Pokedex - <br> CPW 211 Teams Project 

## Game Description: 
You play as a new Pokémon Trainer, you are given a Pokedex to:
discover and collect new Pokémon, view Pokemon Stats, Pokémon Types,
and retrieve data from previously collected Pokemon. 

## Database Info:
Example of the database schema and values used in the project: 
<img src="team-docs/POKE-DATA.png" alt="Database Schema">


# Tech Stack:
- Language: C#
- Framework .Net 9.0 (Windows Forms)
- Database: SQL Server ('PokedexDB')

# How to run the program: 
1. Download zip file in the repository or clone the repository. 
2. Open the solution file "(`.sln`)" in Visual Studio.
3. Ensure you have the required .NET 9.0 installed.
4. **Database Setup** (Choose one of the following options):

   ## Option A: Using LocalDB (Recommended for Development)
   LocalDB is included with Visual Studio and doesn't require a separate SQL Server installation.
   
   1. **Enable LocalDB** (if not already enabled):
      - Open Visual Studio Installer
      - Modify your Visual Studio installation
      - Under "Individual components", ensure "SQL Server Express 2019 LocalDB" is checked
      - Install if needed and restart Visual Studio
   
   2. **Create the Database**:
      - Open SQL Server Object Explorer in Visual Studio (View → SQL Server Object Explorer)
      - Expand "(localdb)\MSSQLLocalDB"
      - Right-click on "Databases" → Add New Database → Name it "PokedexDB"
      - OR use SQL Server Management Studio (SSMS) and connect to `(localdb)\MSSQLLocalDB`
   
   3. **Run the Database Script**:
      - Open the SQL script file: `team-docs/PokedexDatabase.sql`
      - Execute the script against the PokedexDB database to create tables and sample data

   ## Option B: Using Full SQL Server with SSMS
   1. Ensure SQL Server 2022 (or later) is installed and running
   2. Open SQL Server Management Studio and connect to your server
   3. Run the SQL script in the `team-docs/PokedexDatabase.sql` file to create the database and tables
   4. **Update the connection string** in `Pokedex/PokedexDB.cs` to match your SQL Server instance if different from LocalDB

5. **Run the Application**:
   - Run the project by clicking the "Start" button in Visual Studio
   - The application will test the database connection on startup

## Troubleshooting Database Connection Issues:
- **"Cannot connect to (localdb)\MSSQLLocalDB"**: Ensure LocalDB is installed and running (see Option A above)
- **Connection timeout**: Make sure the database service is running and accessible
- **Database not found**: Verify the PokedexDB database was created successfully using the provided SQL script
- **Permission issues**: Ensure your Windows user has access to the LocalDB instance


# Meet Team #3 <br> (Alphabetical Order):
- Adelisse Ferris
- Alex Fischer 
- Kourtnie Moore

## Special Thanks to: <br>
- Joeseph Ortiz

*more information to be added later~~*
