using System;
using System.Drawing;
using System.Windows.Forms;

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

        public CreateUserForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "Create New User";
            this.Size = new Size(400, 420);
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
                    this.DialogResult = DialogResult.OK;
                    this.Close();
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