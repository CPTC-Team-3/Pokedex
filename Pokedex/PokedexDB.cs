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

    /// <summary>
    /// This is to insert a new user to the database so that we can save progress like: collected pokemon,
    /// trainer level, etc. INSERT STATEMENT should be used so that the data can be stored in the database. 
    /// </summary>
    /// <returns></returns>
    public User? NewUser(string username, string firstname, string lastname, string password, string email)
    {
        try
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = @"INSERT INTO Users (Username, FirstName, LastName, Password, Email)
                             VALUES (@Username, @FirstName, @LastName, @Password, @Email)";

            using SqlCommand CreateUserCommand = new SqlCommand(query, connection);
            // Add parameters for user details
            CreateUserCommand.Parameters.AddWithValue(@"Username", username);
            CreateUserCommand.Parameters.AddWithValue(@"FirstName", firstname);
            CreateUserCommand.Parameters.AddWithValue(@"LastName", lastname);
            CreateUserCommand.Parameters.AddWithValue(@"Password", password);
            CreateUserCommand.Parameters.AddWithValue(@"Email", email);

            object? result = CreateUserCommand.ExecuteScalar(); // execute scalar to get the inserted UserId
            // just one user can be created at a time so no need for a list here, one object at a time. 
            {
                if (result != null && int.TryParse(result.ToString(), out int UserId))
                {
                    return new User
                    {
                        UserId = UserId,
                        Username = username,
                        FirstName = firstname,
                        LastName = lastname,
                        Password = password,
                        Email = email,
                    };
                }
            }
        }

        catch (Exception ex)
        {
            MessageBox.Show($"Error creating user:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return null; // Return null if user creation fails
    }

    /// <summary>
    /// Creates a new save file list. INSERT STATEMENT should be used so that the data can be stored in the database. 
    /// </summary>
    /// <returns>A new instance of a <see cref="List{T}"/> containing <see cref="SaveFile"/> objects. The list is initially
    /// empty.</returns>
    
    public List<User> GetExistingUser(int userId)
    {
        // select query needed
        List<User> existingUser = new List<User>();
        try
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            // password is not included for security reasons
            string query = @"SELECT UserId, Username, FirstName, Lastname, TrainerLevel
                             FROM Users
                             WHERE UserId = @UserId";

            using SqlCommand LoadSaveCommand = new SqlCommand(query, connection);
            LoadSaveCommand.Parameters.AddWithValue("UserId", userId);

            using SqlDataReader reader = LoadSaveCommand.ExecuteReader();
            {
                while (reader.Read())
                {
                    existingUser.Add(new User
                    {
                        UserId = reader["UserId"] != DBNull.Value ? Convert.ToInt32(reader["UserId"]) : default(int),
                        Username = reader["Username"].ToString(),
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        TrainerLevel = reader["TrainerLevel"] != DBNull.Value ? Convert.ToInt32(reader["TrainerLevel"]) : default(int),
                    });
                }
            }
        }

        catch (Exception ex)
        {
            MessageBox.Show($"Error retrieving user:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return existingUser;
    }

/// <summary>
/// Creates a new save file entry in the database for the specified user.
/// </summary>
/// <remarks>This method attempts to insert a new save file record into the database. If the operation fails, an
/// error message is displayed, and the method returns <see langword="null"/>. Ensure that the database connection
/// string is properly configured before calling this method.</remarks>
/// <param name="userId">The unique identifier of the user associated with the save file.</param>
/// <param name="saveDate">The date and time when the save file is created.</param>
/// <param name="saveName">The name of the save file. This value cannot be null or empty.</param>
/// <returns>A <see cref="SaveFile"/> object representing the newly created save file if the operation succeeds; otherwise, <see
/// langword="null"/> if the save file creation fails.</returns>
    public SaveFile NewSave(int userId, DateTime saveDate, string saveName)
    {
        try
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            // insert query here 
            string query = @"INSERT INTO SaveFile (UserId, SaveDate, SaveName)
                             VALUES (@UserId, @SaveDate, @SaveName)";

            using SqlCommand CreateSaveCommand = new SqlCommand(query, connection);
            CreateSaveCommand.Parameters.AddWithValue("UserId", userId); // temp userId
            CreateSaveCommand.Parameters.AddWithValue("SaveDate", saveDate);
            CreateSaveCommand.Parameters.AddWithValue("SaveName", saveName);


            object? result = CreateSaveCommand.ExecuteScalar();
            if (result != null && int.TryParse(result.ToString(), out int saveFileId))
            {
                SaveFile newSave = new()
                {
                    SaveFileId = saveFileId,
                    UserId = userId,
                    SaveDate = saveDate,
                    SaveName = saveName
                };
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error creating save file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return null; // Return null if save creation fails
    }
 
/// <summary>
/// Retrieves a list of save files that match the specified save file ID.
/// </summary>
/// <remarks>This method connects to a database to retrieve save file information based on the provided save file
/// ID. Ensure that the database connection string is properly configured and that the database is accessible.</remarks>
/// <param name="saveFileId">The unique identifier of the save file to retrieve. Must be a valid save file ID.</param>
/// <returns>A list of <see cref="SaveFile"/> objects that match the specified save file ID.  The list will be empty if no
/// matching save files are found.</returns>
    public List<SaveFile> LoadSave(int saveFileId)
    {
        List<SaveFile> existingSave = new List<SaveFile>();
        try
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open(); 
            string query = @"SELECT SaveFileId, UserId, SaveDate, SaveName
                             FROM SaveFile
                             WHERE SaveFileId = @SaveFileId";

            using SqlCommand LoadSaveCommand = new SqlCommand(query, connection);
            LoadSaveCommand.Parameters.AddWithValue("SaveFileId", saveFileId);

            using SqlDataReader reader = LoadSaveCommand.ExecuteReader();
            {
                while (reader.Read())
                {
                    existingSave.Add(new SaveFile
                    {
                        SaveName = reader["Name"].ToString(),
                        SaveDate = reader["SaveDate"] != DBNull.Value ? 
                                                    Convert.ToDateTime(reader["SaveDate"]) : default(DateTime),
                        UserId = reader["UserId"] != DBNull.Value ? Convert.ToInt32(reader["UserId"]) : default(int),
                        SaveFileId = reader["SaveFileId"] != DBNull.Value ?
                                                            Convert.ToInt32(reader["SaveFileId"]) : default(int)
                    }); 
                }
            }
        }

        catch (Exception ex)
        {
                      MessageBox.Show($"Error retrieving saved file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return existingSave;
    }
        /// <summary>
        /// Looks up a Pokemon by its name in the database and returns the Pokemon's details.
        /// </summary>
        public Pokemon? getPokemonByName(string name)
        {
            using SqlConnection connection = new(connectionString);
            string query = "SELECT * FROM Pokemon WHERE Name = @Name";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", name);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Pokemon
                {
                    PokemonID = reader.GetInt32(reader.GetOrdinal("PokemonID")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    HP = reader.GetInt32(reader.GetOrdinal("HP")),
                    Attack = (byte)reader.GetInt32(reader.GetOrdinal("Attack")),
                    Defense = (byte)reader.GetInt32(reader.GetOrdinal("Defense")),
                    SpAttack = (byte)reader.GetInt32(reader.GetOrdinal("SpAttack")),
                    SpDefense = (byte)reader.GetInt32(reader.GetOrdinal("SpDefense")),
                    Speed = (byte)reader.GetInt32(reader.GetOrdinal("Speed")),
                    PokemonType1 = reader.GetString(reader.GetOrdinal("PokemonType1")),
                    PokemonType2 = reader.IsDBNull(reader.GetOrdinal("PokemonType2")) ? null : reader.GetString(reader.GetOrdinal("PokemonType2"))
                };
            }
            return null;
        }

        /// <summary>
        /// Looks up a Pokemon by its ID in the database and returns the Pokemon's details.
        /// </summary>
        public Pokemon? getPokemonById(int id)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            string query = "SELECT * FROM Pokemon WHERE PokemonID = @Id";
            SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Pokemon
                {
                    PokemonID = reader.GetInt32(reader.GetOrdinal("PokemonID")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    HP = reader.GetInt32(reader.GetOrdinal("HP")),
                    Attack = (byte)reader.GetInt32(reader.GetOrdinal("Attack")),
                    Defense = (byte)reader.GetInt32(reader.GetOrdinal("Defense")),
                    SpAttack = (byte)reader.GetInt32(reader.GetOrdinal("SpAttack")),
                    SpDefense = (byte)reader.GetInt32(reader.GetOrdinal("SpDefense")),
                    Speed = (byte)reader.GetInt32(reader.GetOrdinal("Speed")),
                    PokemonType1 = reader.GetString(reader.GetOrdinal("PokemonType1")),
                    PokemonType2 = reader.IsDBNull(reader.GetOrdinal("PokemonType2")) ? null : reader.GetString(reader.GetOrdinal("PokemonType2"))
                };
            }
            return null;
        }

