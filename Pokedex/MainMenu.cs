using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pokedex
{
    /// <summary>
    /// Represents the Main Menu screen of the game with user selection slots.
    /// </summary>
    public partial class MainMenu : Form
    {
        private Label loadingLabel;
        private Button playButton;
        private Panel[] userSlots;
        private List<User> allUsers;
        private int selectedSlotIndex = -1;
        private bool isLoading = true;
        private PokedexDB pokedexDB;

        /// <summary>
        /// Initializes a new instance of the MainMenu class.
        /// </summary>
        public MainMenu()
        {
            InitializeComponent();
            SetupLoadingScreen();

            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

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
            this.Size = new System.Drawing.Size(1000, 700);
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
        }

        /// <summary>
        /// Sets up the main menu with user slots after successful database connection
        /// </summary>
        private void SetupMainMenu()
        {
            this.Controls.Clear(); // Remove loading screen

            // Title label
            Label titleLabel = new Label();
            titleLabel.Text = "Select a User Slot";
            titleLabel.Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.AutoSize = true;
            titleLabel.Location = new System.Drawing.Point(
                (this.ClientSize.Width - titleLabel.PreferredWidth) / 2, 
                30
            );
            titleLabel.Anchor = AnchorStyles.Top;
            this.Controls.Add(titleLabel);

            // Create user slots in a 3x2 pattern
            userSlots = new Panel[6];
            int slotWidth = 280;
            int slotHeight = 150;
            int spacing = 30;
            int startX = (this.ClientSize.Width - (3 * slotWidth + 2 * spacing)) / 2;
            int startY = 100;

            for (int i = 0; i < 6; i++)
            {
                int row = i / 3;
                int col = i % 3;
                
                Panel slot = new Panel();
                slot.Size = new Size(slotWidth, slotHeight);
                slot.Location = new Point(
                    startX + col * (slotWidth + spacing),
                    startY + row * (slotHeight + spacing)
                );
                slot.BackColor = Color.White;
                slot.BorderStyle = BorderStyle.FixedSingle;
                slot.Cursor = Cursors.Hand;
                slot.Tag = i; // Store slot index
                
                // Add click event
                slot.Click += UserSlot_Click;
                
                userSlots[i] = slot;
                this.Controls.Add(slot);
            }

            // Create Play Game button (initially disabled)
            playButton = new Button();
            playButton.Text = "Play Game";
            playButton.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            playButton.Size = new System.Drawing.Size(200, 50);
            playButton.Location = new System.Drawing.Point(
                (this.ClientSize.Width - playButton.Width) / 2, 
                startY + 2 * (slotHeight + spacing) + 20
            );
            playButton.Anchor = AnchorStyles.Bottom;
            playButton.BackColor = Color.Gray;
            playButton.Enabled = false;
            playButton.Click += PlayButton_Click;
            this.Controls.Add(playButton);

            // Load users and populate slots
            PopulateUserSlots();
        }

        /// <summary>
        /// Populates the user slots with user information or [EMPTY] labels
        /// </summary>
        private void PopulateUserSlots()
        {
            try
            {
                allUsers = pokedexDB.GetAllUsers();
                
                for (int i = 0; i < 6; i++)
                {
                    Panel slot = userSlots[i];
                    slot.Controls.Clear();

                    if (i < allUsers.Count)
                    {
                        // Populate with user data
                        User user = allUsers[i];
                        
                        Label usernameLabel = new Label();
                        usernameLabel.Text = user.Username;
                        usernameLabel.Font = new Font("Arial", 14, FontStyle.Bold);
                        usernameLabel.ForeColor = Color.DarkBlue;
                        usernameLabel.AutoSize = true;
                        usernameLabel.Location = new Point(10, 10);
                        slot.Controls.Add(usernameLabel);

                        Label nameLabel = new Label();
                        nameLabel.Text = $"{user.FirstName} {user.LastName}";
                        nameLabel.Font = new Font("Arial", 10, FontStyle.Regular);
                        nameLabel.ForeColor = Color.Black;
                        nameLabel.AutoSize = true;
                        nameLabel.Location = new Point(10, 40);
                        slot.Controls.Add(nameLabel);

                        Label levelLabel = new Label();
                        levelLabel.Text = $"Trainer Level: {user.TrainerLevel}";
                        levelLabel.Font = new Font("Arial", 10, FontStyle.Regular);
                        levelLabel.ForeColor = Color.Black;
                        levelLabel.AutoSize = true;
                        levelLabel.Location = new Point(10, 65);
                        slot.Controls.Add(levelLabel);

                        Label idLabel = new Label();
                        idLabel.Text = $"ID: {user.UserId}";
                        idLabel.Font = new Font("Arial", 8, FontStyle.Regular);
                        idLabel.ForeColor = Color.Gray;
                        idLabel.AutoSize = true;
                        idLabel.Location = new Point(10, 90);
                        slot.Controls.Add(idLabel);

                        // Add delete button for existing users
                        Button deleteButton = new Button();
                        deleteButton.Text = "×";
                        deleteButton.Size = new Size(25, 25);
                        deleteButton.Location = new Point(slot.Width - 35, 10);
                        deleteButton.BackColor = Color.Red;
                        deleteButton.ForeColor = Color.White;
                        deleteButton.Font = new Font("Arial", 12, FontStyle.Bold);
                        deleteButton.FlatStyle = FlatStyle.Flat;
                        deleteButton.FlatAppearance.BorderSize = 0;
                        deleteButton.Cursor = Cursors.Hand;
                        deleteButton.Tag = user.UserId; // Store the user ID for deletion
                        deleteButton.Click += DeleteButton_Click;
                        
                        // Prevent the delete button click from triggering slot selection
                        deleteButton.Click += (s, e) => e = null; // This stops event bubbling
                        
                        slot.Controls.Add(deleteButton);
                    }
                    else
                    {
                        // Show [EMPTY] slot
                        Label emptyLabel = new Label();
                        emptyLabel.Text = "[EMPTY]";
                        emptyLabel.Font = new Font("Arial", 16, FontStyle.Bold);
                        emptyLabel.ForeColor = Color.Gray;
                        emptyLabel.AutoSize = true;
                        emptyLabel.Location = new Point(
                            (slot.Width - emptyLabel.PreferredWidth) / 2,
                            (slot.Height - emptyLabel.PreferredHeight) / 2
                        );
                        slot.Controls.Add(emptyLabel);
                        
                        // Center the label properly
                        emptyLabel.Location = new Point(
                            (slot.Width - emptyLabel.Width) / 2,
                            (slot.Height - emptyLabel.Height) / 2
                        );
                    }

                    // Add click events to all child controls except delete buttons
                    foreach (Control control in slot.Controls)
                    {
                        if (!(control is Button)) // Don't add click event to delete buttons
                        {
                            control.Click += (s, e) => UserSlot_Click(slot, e);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the delete button click event for user slots
        /// </summary>
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (sender is Button deleteButton && deleteButton.Tag is int userId)
            {
                try
                {
                    // Find the user to get their name for the confirmation dialog
                    User userToDelete = allUsers.FirstOrDefault(u => u.UserId == userId);
                    if (userToDelete == null)
                    {
                        MessageBox.Show("User not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Confirm deletion
                    string confirmMessage = $"Are you sure you want to delete user '{userToDelete.Username}' ({userToDelete.FirstName} {userToDelete.LastName})?\n\n" +
                                          "This will permanently delete:\n" +
                                          "• The user account\n" +
                                          "• All collected Pokémon data\n" +
                                          "• All save files\n\n" +
                                          "This action cannot be undone.";

                    DialogResult result = MessageBox.Show(confirmMessage, "Confirm User Deletion", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        // Delete the user from the database
                        bool deleteSuccess = pokedexDB.DeleteUser(userId);
                        
                        if (deleteSuccess)
                        {
                            MessageBox.Show($"User '{userToDelete.Username}' has been successfully deleted.", 
                                "User Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            // Clear current selection if the deleted user was selected
                            if (selectedSlotIndex >= 0 && selectedSlotIndex < allUsers.Count && 
                                allUsers[selectedSlotIndex].UserId == userId)
                            {
                                selectedSlotIndex = -1;
                                playButton.BackColor = Color.Gray;
                                playButton.Enabled = false;
                            }
                            
                            // Refresh the user slots to reflect the changes
                            PopulateUserSlots();
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete user. Please try again.", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Handles user slot selection
        /// </summary>
        private void UserSlot_Click(object sender, EventArgs e)
        {
            Panel clickedSlot = sender as Panel;
            if (clickedSlot == null) return;

            int slotIndex = (int)clickedSlot.Tag;
            
            // Clear previous selection
            if (selectedSlotIndex >= 0)
            {
                userSlots[selectedSlotIndex].BackColor = Color.White;
            }

            // Set new selection
            selectedSlotIndex = slotIndex;
            clickedSlot.BackColor = Color.Yellow;

            // Enable play button
            playButton.BackColor = Color.LightGreen;
            playButton.Enabled = true;
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
                        pokedexDB = new PokedexDB();
                        
                        // If we reach here, connection was successful
                        this.Invoke(new Action(() =>
                        {
                            ShowMainMenu();
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
        /// Shows the main menu after successful database connection
        /// </summary>
        private void ShowMainMenu()
        {
            isLoading = false;
            SetupMainMenu();
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
        /// Opens the game form with the selected user.
        /// </summary>
        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (isLoading || selectedSlotIndex < 0) return;

            try
            {
                User selectedUser = null;

                // Check if selected slot has a user or is empty
                if (selectedSlotIndex < allUsers.Count)
                {
                    // Existing user selected
                    selectedUser = allUsers[selectedSlotIndex];
                }
                else
                {
                    // Empty slot selected - create new user
                    CreateUserForm createUserForm = new CreateUserForm();
                    if (createUserForm.ShowDialog(this) == DialogResult.OK && createUserForm.CreatedUser != null)
                    {
                        selectedUser = createUserForm.CreatedUser;
                        // Refresh the user slots to show the new user
                        PopulateUserSlots();
                    }
                    else
                    {
                        return; // User cancelled creation
                    }
                }

                if (selectedUser != null)
                {
                    // TODO: Pass selectedUser to Form1 when we implement user-specific gameplay
                    // For now, just start the game
                    Form1 gameForm = new Form1();
                    gameForm.Show();

                    // Hide the Main Menu
                    // NOTE: We hide instead of close to allow the game to run independently!
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting game: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handle form resize to keep controls centered
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            if (loadingLabel != null && loadingLabel.Visible)
            {
                loadingLabel.Location = new System.Drawing.Point(
                    (this.ClientSize.Width - loadingLabel.Width) / 2,
                    (this.ClientSize.Height - loadingLabel.Height) / 2
                );
            }
        }
    }
}