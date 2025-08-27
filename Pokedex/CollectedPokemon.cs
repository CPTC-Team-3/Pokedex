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

    }
}
