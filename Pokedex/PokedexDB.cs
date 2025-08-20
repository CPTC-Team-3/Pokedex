using Microsoft.Data.SqlClient;

namespace Pokedex;

/// <summary>
///  PokedexDB is a class that connects to the Pokedex database.
/// </summary>
public class PokedexDB
{

    // (localdb) connection made 
    string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PokedexDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";


    // this is a class that will connect the Pokedex to the database
    public PokedexDB()
    {
        // test query to see if the connection is successful
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
    }
    // In the database class we now need to store users collected pokemon 

    public void CollectedPokemon()
    {
        // This method will be used to store the user's collected pokemon in the database
        // I will need to create a table in the database:
        // to store the user's collected pokemon 
        // The table will have the following columns:
        // 1. PokemonID (int, primary key)
        // 2. UserID (int, foreign key)
        // 3. PokemonName (string)
        // 4. PokemonType1 (string)
        // 5. PokemonType2 (string, nullable)
        // ^^ can I collectively get the stats from the previous table in DB?
        // without doing it all one at a time?

    }

}
#region Additional Information/Dev Notes

#endregion
