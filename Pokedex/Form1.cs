using System.Drawing;
using System.Collections.Generic;

namespace Pokedex
{
    public partial class Form1 : Form
    {
        private List<Tile> tiles;
        private Player player;

        public Form1()
        {
            InitializeComponent();
            InitializeGame();
            SetupForm();
        }


        private void SetupForm()
        {
            // Enable the form to receive key events
            this.KeyPreview = true;
            this.Focus();

            // Optional: Set form properties for better visibility
            this.Text = "Pokedex Game";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeGame()
        {
            tiles = new List<Tile>();
            int mapX = 20, mapY = 20;

            // Create a x by y grid of tiles - alternating walkable tiles
            for (int x = 0; x < mapX; x++)
            {
                for (int y = 0; y < mapY; y++)
                {
                    bool isWalkable = (x + y) % 10 != 0; // Example logic for walkability
                    Color color = isWalkable ? Color.Green : Color.Gray;
                    tiles.Add(new Tile(x * 60, y * 60, color, isWalkable));
                }
            }

            player = new Player(0, 0); // Start player at the top-left corner
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            // Draw tiles
            foreach (var tile in tiles)
            {
                using (Brush brush = new SolidBrush(tile.TileColor))
                {
                    g.FillRectangle(brush, tile.X, tile.Y, 60, 60);
                }
            }

            // Draw player
            using (Brush playerBrush = new SolidBrush(Color.Blue))
            {
                g.FillRectangle(playerBrush, player.X, player.Y, Player.Size, Player.Size);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            int deltaX = 0;
            int deltaY = 0;

            switch (e.KeyCode)
            {
                case Keys.W:
                case Keys.Up:
                    deltaY = -1;
                    break;
                case Keys.S:
                case Keys.Down:
                    deltaY = 1;
                    break;
                case Keys.A:
                case Keys.Left:
                    deltaX = -1;
                    break;
                case Keys.D:
                case Keys.Right:
                    deltaX = 1;
                    break;
            }

            if (deltaX != 0 || deltaY != 0)
            {
                player.Move(deltaX, deltaY, tiles);
                Invalidate(); // Redraw the form
            }
        }
    }
}
