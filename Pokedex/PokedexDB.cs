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
    /// 
    /// </summary>
    public class PokedexDB
    {
        string connectionString = "Data Source=TRAVELING_KP\\SQLEXPRESS;Initial Catalog=PokedexDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

        // this is a class that will connect the Pokedex to the database
        public PokedexDB()
        {

            // Initialize the connection string or any other necessary setup
            using SqlConnection connection = new SqlConnection(connectionString);
            {
                // Open the connection to the database
                connection.Open();
                // Execute a query to retrieve data from the database
            }
        }
    }
}
#region
/* 
- UNDER THE OPEN CONNNECTION, EXECUTE QUERY. SELECT ALL FROM TABLE
- USE READER TO READ DATA. 

string query = "SELECT * FROM YourTable";
SqlCommand command = new SqlCommand(query, connection);

using (SqlDataReader reader = command.ExecuteReader())
{

}
 */
#endregion
