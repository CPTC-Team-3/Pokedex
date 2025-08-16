using Microsoft.Data.SqlClient;

namespace Pokedex;

/// <summary>
///  PokedexDB is a class that connects to the Pokedex database.
/// </summary>
public class PokedexDB
{
    // (localdb) connection string
    string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PokedexDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

    /// <summary>
    /// Initializes a new instance of the <see cref="PokedexDB"/> class and attempts to establish a connection to the
    /// database.
    /// </summary>
    /// <remarks>This constructor tests the database connection by opening a connection to the database and
    /// executing a simple query. If the connection is successful, a confirmation message is displayed. If the
    /// connection fails, an error message is shown.</remarks>
    /// 
    public PokedexDB()
    {
        try
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            // If connection opens successfully, show success message
                                        // Showed Message, MessageBox Title, MessageBox Buttons, and MessageBox Icon
            MessageBox.Show("Database connection, complete!", "Connection Sucessful", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // execute a test query
            string query = "SELECT * FROM Pokemon";
            SqlCommand command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            // If connection fails, show error message
            MessageBox.Show($"Failed to connect to database:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }
   
    // ~~DEV NOTE: 8/18/2025
    // public List<CollectedPokemon> GetAllCollectedPokemon(string username)
    // ^^ This method will retrieve all collected pokemon for a specific user. N
    // o two users can have the same username, so we can use that to get the user id and then get all pokemon for that user id.
    public void GetAllCollectedPokemon() // this will be replace with the above method
    {
        // use try/catch to handle exceptions 

        // create a connection to the database again

        // create a query to get all collected pokemon from collectedpokemon table
    }



}
#region Additional Information/Dev Notes
/*
 * CREATE TABLE CollectedPokemon (
    PokemonID INT NOT NULL,
    UserId INT NOT NULL,
    PRIMARY KEY (PokemonID, UserId),
    FOREIGN KEY (PokemonId) REFERENCES Pokemon(Id), : references the pokemon table and gets the information from it
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
*/

#endregion
