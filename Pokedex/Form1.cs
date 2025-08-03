namespace Pokedex
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // Initialize the PokedexDB class to connect to the database
            // test if the connection is sucessful
            try
            {
                PokedexDB pokedexDB = new PokedexDB();
            }
            catch (Exception ex)
            {
                // If there is an error initializing the database connection, show an error message
                MessageBox.Show($"ERROR DATABASE CONNECTION ERROR OCCURRED", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
