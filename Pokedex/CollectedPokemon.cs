using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokedex
{
    /// <summary>
    /// This class represents a Pokemon that a user has collected.
    /// </summary>
    public class CollectedPokemon
    {
        // Should the PokemonID be included here?
        public required string Name { get; set; }
		public int Level { get; set; } = 1;
		public int HP { get; set; }
		public int Defense { get; set; }
		public int Attack { get; set; }
		public int SpAttack { get; set; }	
		public int SpDefense { get; set; }
		public int Speed { get; set; }
		public required string PokemonType1 { get; set; }
		public string? PokemonType2 { get; set; }


        /* 
         Should there be an option for Experence Points (XP) or Experience Level?
        if the user obtains a x amount of experience points, 
        they can level up their TrainerLevel and All Pokemon.

         * COLLECTEDPOKEMON TABLE CONTENTS:

        UserId INT,
        PokemonId INT,
        Name VARCHAR(35) NOT NULL,
        Level INT NOT NULL DEFAULT 1,
        HP INT NOT NULL,
        Defense INT NOT NULL,
        Attack INT NOT NULL,
        SpecialAttack INT NOT NULL,
        SpecialDefense INT NOT NULL,
        Speed INT NOT NULL,
        PokemonType1 VARCHAR(35) NOT NULL,
        PokemonType2 VARCHAR(35),
             */
    }
}
