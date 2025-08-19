using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic.ApplicationServices;
using System.ComponentModel.DataAnnotations;

namespace Pokedex;

/// <summary>
///  PokedexDB is a class that connects to the Pokedex database.
/// </summary>
public class PokedexDB
{
    // connection string
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

    /// <summary>
    /// This method will retrieve all collected pokemon for a specific user. N
    // o two users can have the same username, so we can use that to get the user id and then get all pokemon for that user id
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public List<CollectedPokemon> GetAllCollectedPokemon(string username) // this will be replace with the above method
    {
        List<CollectedPokemon> collectedList = new List<CollectedPokemon>();
        // use try/catch to handle exceptions 
        try
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open(); //open the connection for the database to enter here
            // step 1: get the user id from the username
            string getUserId = "SELECT UserId FROM  Users WHERE Username = @Username";
            int userId;
            // make a SqlCommand object to execute the query
            using (SqlCommand getUserIdCommand = new SqlCommand(getUserId, connection))
            {
                getUserIdCommand.Parameters.AddWithValue("@Username", username);
                object result = getUserIdCommand.ExecuteScalar(); // ExecuteScalar will return the first column of the first row

                if (result == null)
                {
                    MessageBox.Show("User not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return collectedList; // return an empty list if user not found
                }
                userId = Convert.ToInt32(result); // convert the result to an int
            }

            // Step 2: Get all collected Pokémon for that user
            // preivew stats or all stats for individual collected pokemon
            string query = @"
                SELECT * FROM CollectedPokemon
                WHERE UserId = @UserId";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using SqlDataReader reader = command.ExecuteReader();


            while (reader.Read())
            {
                collectedList.Add(new CollectedPokemon
                {
                    Name = reader["Name"].ToString(),
                    Level = Convert.ToInt32(reader["Level"]),
                    HP = Convert.ToInt32(reader["HP"]),
                    Defense = Convert.ToInt32(reader["Defense"]),
                    Attack = Convert.ToInt32(reader["Attack"]),
                    SpAttack = Convert.ToInt32(reader["SpecialAttack"]),
                    SpDefense = Convert.ToInt32(reader["SpecialDefense"]),
                    Speed = Convert.ToInt32(reader["Speed"]),
                    PokemonType1 = reader["PokemonType1"].ToString(),
                    PokemonType2 = reader["PokemonType2"] == DBNull.Value ? null : reader["PokemonType2"].ToString()
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error retrieving collected Pokémon:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return collectedList;
    }

    /// <summary>
    /// Gets or sets the collection of Pokémon that have been collected.
    /// </summary>
    public List<CollectedPokemon> MyCollection { get; set; }

    public void RetrieveUserCollection()
    {
    // current placeholder username,
    // nothing actually functions in this method right now


    }
}
#region Additional Information/Dev Notes
/*
        Add pokemon user to the database as a sample user? 
        Add a method to add a user to the database?
        
        Pre-made character that user can play as? = sample user data?

        For example, "Ash Ketchum" from the Pokémon anime series:

        Character Selected/Selection: Ash Ketchum  -x-
                                         Brock 
                                         Misty


        username: GottaCatchEmAll!2004
        password: pass123*
        email: detectivepikachu@sampledex.com
        trainer level: 5
        UserId: 1


        This user will have a collection of Pokémon that the player can interact with.
        The player can view the Pokémon's stats, level them up, and evolve them.
        The player can also catch new Pokémon and add them to their collection.

*/

#endregion
