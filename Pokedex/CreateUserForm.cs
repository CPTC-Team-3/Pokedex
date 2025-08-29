using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Pokedex
{
    /// <summary>
    /// Form for creating a new user when an empty slot is selected
    /// </summary>
    public partial class CreateUserForm : Form
    {
        public User? CreatedUser { get; private set; }
        
        private TextBox usernameTextBox;
        private TextBox firstNameTextBox;
        private TextBox lastNameTextBox;
        private TextBox passwordTextBox;
        private TextBox emailTextBox;
        private Button createButton;
        private Button cancelButton;
        
        // Pokemon selection controls
        private PictureBox bulbasaurPictureBox;
        private PictureBox charmanderPictureBox;
        private PictureBox squirtlePictureBox;
        private string selectedPokemon = "";

        public CreateUserForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "Create New User";
            this.Size = new Size(400, 520); // Increased height for Pokemon selection
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.DarkSlateBlue;

            int yPosition = 20;
            int spacing = 50;

            // Username
            CreateLabelAndTextBox("Username:", ref usernameTextBox, ref yPosition, spacing);
            
            // First Name
            CreateLabelAndTextBox("First Name:", ref firstNameTextBox, ref yPosition, spacing);
            
            // Last Name
            CreateLabelAndTextBox("Last Name:", ref lastNameTextBox, ref yPosition, spacing);
            
            // Password
            CreateLabelAndTextBox("Password:", ref passwordTextBox, ref yPosition, spacing);
            passwordTextBox.UseSystemPasswordChar = true;
            
            // Email
            CreateLabelAndTextBox("Email:", ref emailTextBox, ref yPosition, spacing);

            // Pokemon Selection
            CreatePokemonSelection(ref yPosition);

            // Buttons
            yPosition += 20;
            createButton = new Button();
            createButton.Text = "Create User";
            createButton.Size = new Size(100, 30);
            createButton.Location = new Point(90, yPosition);
            createButton.BackColor = Color.LightGreen;
            createButton.Click += CreateButton_Click;
            this.Controls.Add(createButton);

            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Size = new Size(100, 30);
            cancelButton.Location = new Point(210, yPosition);
            cancelButton.BackColor = Color.LightCoral;
            cancelButton.Click += CancelButton_Click;
            this.Controls.Add(cancelButton);
        }

        private void CreateLabelAndTextBox(string labelText, ref TextBox textBox, ref int yPosition, int spacing)
        {
            Label label = new Label();
            label.Text = labelText;
            label.ForeColor = Color.White;
            label.Font = new Font("Arial", 10, FontStyle.Bold);
            label.Size = new Size(100, 20);
            label.Location = new Point(20, yPosition);
            this.Controls.Add(label);

            textBox = new TextBox();
            textBox.Size = new Size(200, 20);
            textBox.Location = new Point(140, yPosition);
            this.Controls.Add(textBox);

            yPosition += spacing;
        }

        private void CreatePokemonSelection(ref int yPosition)
        {
            // Label for Pokemon selection
            Label pokemonLabel = new Label();
            pokemonLabel.Text = "Choose Your First Pokemon:";
            pokemonLabel.ForeColor = Color.White;
            pokemonLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            pokemonLabel.Size = new Size(250, 20);
            pokemonLabel.Location = new Point(20, yPosition);
            this.Controls.Add(pokemonLabel);

            yPosition += 30;

            // Pokemon picture boxes
            int pokemonSize = 80;
            int pokemonSpacing = 100;
            int startX = 40;

            // NOTE: Keep the ../ in the path to go back to the project root directory.
            // This is required to load the images correctly.

            // Bulbasaur
            bulbasaurPictureBox = CreatePokemonPictureBox("Bulbasaur", "../../../lib/textures/Pokemon/bulbasaur.png", 
                new Point(startX, yPosition), pokemonSize);
            this.Controls.Add(bulbasaurPictureBox);

            // Charmander
            charmanderPictureBox = CreatePokemonPictureBox("Charmander", "../../../lib/textures/Pokemon/charmander.png", 
                new Point(startX + pokemonSpacing, yPosition), pokemonSize);
            this.Controls.Add(charmanderPictureBox);

            // Squirtle
            squirtlePictureBox = CreatePokemonPictureBox("Squirtle", "../../../lib/textures/Pokemon/squirtle.png", 
                new Point(startX + (pokemonSpacing * 2), yPosition), pokemonSize);
            this.Controls.Add(squirtlePictureBox);

            yPosition += pokemonSize + 20;
        }

        private PictureBox CreatePokemonPictureBox(string pokemonName, string imagePath, Point location, int size)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.Size = new Size(size, size);
            pictureBox.Location = location;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            pictureBox.BackColor = Color.White;
            pictureBox.Cursor = Cursors.Hand;
            pictureBox.Tag = pokemonName;

            // Load image with error handling
            try
            {
                if (File.Exists(imagePath))
                {
                    pictureBox.Image = Image.FromFile(imagePath);
                }
                else
                {
                    // Create a placeholder if image doesn't exist
                    Bitmap placeholder = new Bitmap(size, size);
                    using (Graphics g = Graphics.FromImage(placeholder))
                    {
                        g.Clear(Color.LightGray);
                        g.DrawString(pokemonName, new Font("Arial", 8), Brushes.Black, 5, size / 2 - 10);
                    }
                    pictureBox.Image = placeholder;
                }
            }
            catch
            {
                // Create a simple placeholder on any error
                Bitmap placeholder = new Bitmap(size, size);
                using (Graphics g = Graphics.FromImage(placeholder))
                {
                    g.Clear(Color.LightGray);
                    g.DrawString(pokemonName, new Font("Arial", 8), Brushes.Black, 5, size / 2 - 10);
                }
                pictureBox.Image = placeholder;
            }

            // Add click event
            pictureBox.Click += PokemonPictureBox_Click;

            return pictureBox;
        }

        private void PokemonPictureBox_Click(object sender, EventArgs e)
        {
            if (sender is PictureBox clickedPictureBox && clickedPictureBox.Tag is string pokemonName)
            {
                // Reset all borders
                bulbasaurPictureBox.BorderStyle = BorderStyle.FixedSingle;
                charmanderPictureBox.BorderStyle = BorderStyle.FixedSingle;
                squirtlePictureBox.BorderStyle = BorderStyle.FixedSingle;

                // Reset all backgrounds
                bulbasaurPictureBox.BackColor = Color.White;
                charmanderPictureBox.BackColor = Color.White;
                squirtlePictureBox.BackColor = Color.White;

                // Highlight selected Pokemon
                clickedPictureBox.BorderStyle = BorderStyle.Fixed3D;
                clickedPictureBox.BackColor = Color.LightYellow;

                selectedPokemon = pokemonName;
            }
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text) ||
                string.IsNullOrWhiteSpace(firstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(lastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(passwordTextBox.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate Pokemon selection
            if (string.IsNullOrEmpty(selectedPokemon))
            {
                MessageBox.Show("Please select your first Pokemon.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                PokedexDB db = new PokedexDB();
                CreatedUser = db.NewUser(
                    usernameTextBox.Text.Trim(),
                    firstNameTextBox.Text.Trim(),
                    lastNameTextBox.Text.Trim(),
                    passwordTextBox.Text,
                    emailTextBox.Text.Trim()
                );

                if (CreatedUser != null)
                {
                    // Add the selected Pokemon to the user's collection
                    bool pokemonAdded = db.AddPokemonToUser(CreatedUser.UserId, selectedPokemon);
                    
                    if (pokemonAdded)
                    {
                        MessageBox.Show($"User created successfully! {selectedPokemon} has been added to your collection!", 
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("User created but failed to add Pokemon. Please contact support.", 
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Failed to create user. Please try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating user: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}