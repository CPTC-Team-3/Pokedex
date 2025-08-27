using System;
using System.Windows.Forms;

namespace Pokedex
{
    /// <summary>
    /// Represents the Main Menu screen of the game.
    /// </summary>
    public partial class MainMenu : Form
    {
        /// <summary>
        /// Initializes a new instance of the MainMenu class.
        /// </summary>
        public MainMenu()
        {
            // Directly call SetupMainMenu to initialize the form
            SetupMainMenu();
        }

        /// <summary>
        /// Sets up the Main Menu UI components.
        /// </summary>
        private void SetupMainMenu()
        {
            // Set form properties
            this.Text = "Pokedex - Main Menu";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(800, 600);

            // Create a Start button
            Button startButton = new Button();
            startButton.Text = "Start Game";
            startButton.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            startButton.Size = new System.Drawing.Size(200, 50);
            startButton.Location = new System.Drawing.Point((this.ClientSize.Width - startButton.Width) / 2, (this.ClientSize.Height - startButton.Height) / 2);
            startButton.Anchor = AnchorStyles.None;

            // Add click event handler
            startButton.Click += StartButton_Click;

            // Add the button to the form
            this.Controls.Add(startButton);
        }

        /// <summary>
        /// Handles the Start button click event.
        /// Opens the game form and closes the Main Menu.
        /// </summary>
        private void StartButton_Click(object sender, EventArgs e)
        {
            // Open the game form
            Form1 gameForm = new Form1();
            gameForm.Show();

            // Close the Main Menu
            this.Hide();
        }
    }
}