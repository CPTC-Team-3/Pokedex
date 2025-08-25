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
    { // this just makes sure that the database is connected the real work is below this 
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
    /// This method will retrieve all collected pokemon for a specific user.
    //  No two users can have the same UserId.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public List<CollectedPokemon> GetAllCollectedPokemon(int userId)
    {
        List<CollectedPokemon> collectedList = new List<CollectedPokemon>();

        try
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string query = @"
            SELECT Name, Level, HP, Defense, Attack, SpecialAttack, SpecialDefense, Speed, PokemonType1, PokemonType2
            FROM CollectedPokemon
            WHERE UserId = @UserId";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using SqlDataReader reader = command.ExecuteReader();
            {
                while (reader.Read())
                {
                    collectedList.Add(new CollectedPokemon
                    {
                        Name = reader["Name"].ToString(),
                        Level = reader["Level"] != DBNull.Value ? Convert.ToInt32(reader["Level"]) : 0,
                        HP = reader["HP"] != DBNull.Value ? Convert.ToInt32(reader["HP"]) : 0,
                        Defense = reader["Defense"] != DBNull.Value ? Convert.ToInt32(reader["Defense"]) : 0,
                        Attack = reader["Attack"] != DBNull.Value ? Convert.ToInt32(reader["Attack"]) : 0,
                        SpAttack = reader["SpecialAttack"] != DBNull.Value ? Convert.ToInt32(reader["SpecialAttack"]) : 0,
                        SpDefense = reader["SpecialDefense"] != DBNull.Value ? Convert.ToInt32(reader["SpecialDefense"]) : 0,
                        Speed = reader["Speed"] != DBNull.Value ? Convert.ToInt32(reader["Speed"]) : 0,
                        PokemonType1 = reader["PokemonType1"].ToString(),
                        PokemonType2 = reader["PokemonType2"] == DBNull.Value ? null : reader["PokemonType2"].ToString()
                    });
                    // used DBNull.Value to check for null values in the database and assign default values if necessary
                }
            }
        }
        catch (Exception ex)
        {
            // formatted error message for better readability
            MessageBox.Show($"Error retrieving collected Pokémon:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return collectedList;
    }

    // insert a new user and later retrieve user data.. save data.. :) 

    /// <summary>
    /// This is to send a new user up to the database so that we can save progress like: collected pokemon,
    /// trainer level, etc. INSERT STATEMENT should be used so that the data can be stored in the database. 
    /// </summary>
    /// <returns></returns>
    public List<User> NewUser()
    {
        List<User> newUser = new List<User>();
        // insert query needed so that a new user can be made. 


        return newUser;
    }

    /// <summary>
    /// Creates a new save file list. INSERT STATEMENT should be used so that the data can be stored in the database. 
    /// </summary>
    /// <returns>A new instance of a <see cref="List{T}"/> containing <see cref="SaveFile"/> objects. The list is initially
    /// empty.</returns>
    
    public List<User> GetExistingUser(int UserId)
    {
        List<User> existingUser = new List<User>();
        // select query needed

        return existingUser;

    }

    /// <summary>
    /// Allows the user to save progress and data.
    /// </summary>
    /// <returns>saveFile</returns>
    public List<SaveFile> NewSave()
    {
        List<SaveFile> saveFile = new List<SaveFile>();
        // insert query 

        return saveFile; 
    }

    /// <summary>
    /// Retrieve a save file from the user. 
    /// </summary>
    /// <param name="saveFileId"></param>
    /// <returns>existingSave</returns>
    public List<SaveFile> LoadSave(int saveFileId)
    {
        List<SaveFile> existingSave = new List<SaveFile>();

        return existingSave;
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
