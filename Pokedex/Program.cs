using System;
using System.Windows.Forms;

namespace Pokedex
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start the application with the Main Menu
            Application.Run(new MainMenu());
        }
    }
}