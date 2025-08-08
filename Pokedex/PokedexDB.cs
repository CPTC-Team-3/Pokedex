using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pokedex
{
    /// <summary>
    ///  PokedexDB is a class that connects to the Pokedex database.
    /// </summary>
    public class PokedexDB
    {
        // make the connection string a constant and change the data source to localhost 
        // localhost made 
        string connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=PokedexDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

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
        }
    }
}
#region Additional Information/Dev Notes

#endregion
