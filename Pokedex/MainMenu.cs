using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pokedex
{
    /// <summary>
    /// Represents the Main Menu screen of the game.
    /// </summary>
    public partial class MainMenu : Form
    {
        private Label loadingLabel;
        private Button playButton;
        private bool isLoading = true;

        /// <summary>
        /// Initializes a new instance of the MainMenu class.
        /// </summary>
        public MainMenu()
        {
            InitializeComponent();
            SetupLoadingScreen();
            
            // Start database connection test asynchronously
            _ = TestDatabaseConnectionAsync();
        }

        /// <summary>
        /// Sets up the loading screen UI components.
        /// </summary>
        private void SetupLoadingScreen()
        {
            // Set form properties
            this.Text = "Pokedex - Main Menu";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(800, 600);
            this.BackColor = Color.DarkSlateBlue;

            // Create loading label
            loadingLabel = new Label();
            loadingLabel.Text = "Loading game...";
            loadingLabel.Font = new System.Drawing.Font("Arial", 24, System.Drawing.FontStyle.Bold);
            loadingLabel.ForeColor = Color.White;
            loadingLabel.AutoSize = true;
            loadingLabel.TextAlign = ContentAlignment.MiddleCenter;
            
            // Center the label
            loadingLabel.Location = new System.Drawing.Point(
                (this.ClientSize.Width - loadingLabel.PreferredWidth) / 2, 
                (this.ClientSize.Height - loadingLabel.PreferredHeight) / 2
            );
            loadingLabel.Anchor = AnchorStyles.None;

            // Add the label to the form
            this.Controls.Add(loadingLabel);

            // Create the Play Game button (initially hidden)
            playButton = new Button();
            playButton.Text = "Play Game";
            playButton.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            playButton.Size = new System.Drawing.Size(200, 50);
            playButton.Location = new System.Drawing.Point(
                (this.ClientSize.Width - playButton.Width) / 2, 
                (this.ClientSize.Height - playButton.Height) / 2
            );
            playButton.Anchor = AnchorStyles.None;
            playButton.Visible = false;
            playButton.Click += PlayButton_Click;

            // Add the button to the form
            this.Controls.Add(playButton);
        }

        /// <summary>
        /// Tests the database connection asynchronously
        /// </summary>
        private async Task TestDatabaseConnectionAsync()
        {
            try
            {
                // Add a small delay to show the loading screen
                await Task.Delay(2000);

                // Test database connection on a background thread
                await Task.Run(() =>
                {
                    try
                    {
                        // Initialize the PokedexDB class to connect to the database
                        PokedexDB pokedexDB = new PokedexDB();
                        
                        // If we reach here, connection was successful
                        this.Invoke(new Action(() =>
                        {
                            ShowPlayButton();
                        }));
                    }
                    catch (Exception ex)
                    {
                        // Database connection failed
                        this.Invoke(new Action(() =>
                        {
                            HandleDatabaseError(ex);
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                // Handle any other errors
                HandleDatabaseError(ex);
            }
        }

        /// <summary>
        /// Shows the Play Game button after successful database connection
        /// </summary>
        private void ShowPlayButton()
        {
            isLoading = false;
            loadingLabel.Visible = false;
            playButton.Visible = true;
            this.BackColor = SystemColors.Control; // Reset to default background
        }

        /// <summary>
        /// Handles database connection errors
        /// </summary>
        private void HandleDatabaseError(Exception ex)
        {
            // Show error message
            MessageBox.Show(
                $"ERROR: DATABASE CONNECTION ERROR OCCURRED\n\nDetails: {ex.Message}", 
                "Database Connection Error", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error
            );

            // Close the application
            Application.Exit();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Handles the Play Game button click event.
        /// Opens the game form and closes the Main Menu.
        /// </summary>
        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (!isLoading)
            {
                // Open the game form
                Form1 gameForm = new Form1();
                gameForm.Show();

                // Close the Main Menu
                this.Hide();
            }
        }

        /// <summary>
        /// Handle form resize to keep controls centered
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            if (loadingLabel != null)
            {
                loadingLabel.Location = new System.Drawing.Point(
                    (this.ClientSize.Width - loadingLabel.Width) / 2,
                    (this.ClientSize.Height - loadingLabel.Height) / 2
                );
            }

            if (playButton != null)
            {
                playButton.Location = new System.Drawing.Point(
                    (this.ClientSize.Width - playButton.Width) / 2,
                    (this.ClientSize.Height - playButton.Height) / 2
                );
            }
        }
    }
}