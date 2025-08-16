using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokedex
{
    /// <summary>
    /// This class represents a user in the Pokedex application. It keeps track of user information such as username, 
    /// password, email, and trainer level. So that we can specify each user and which pokemon they have collected.
    /// </summary>
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int TrainerLevel { get; set; } = 1;

    /* USER TABLE CONTENTS:
    UserId INT PRIMARY KEY IDENTITY(1,1),
	Username VARCHAR(50) NOT NULL,
	Password VARCHAR(255) NOT NULL,
	Email VARCHAR(100) NOT NULL,
	TrainerLevel INT NOT NULL DEFAULT 1
         */
    }
}
