namespace Pokedex;
public class Pokemon
{
    // <summary>    
    // Represents a Pokemon's number in the pokedex.
    // </summary>
    public int PokemonID { get; set; }

    /// <summary>
    /// Refers to the name of the Pokemon.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The total amount of health points (HP) a Pokemon has.
    /// </summary>
    public int HP { get; set; }

    /// <summary>   
    /// Determines the power of a Pokemon's attack.
    /// </summary>
    public int Attack { get; set; }

    /// <summary>
    /// Determines the amount of damage a Pokemon will resist.
    /// </summary>
    public int Defense { get; set; }

    /// <summary>
    /// Determines the power of a Pokemon's special attack.
    /// </summary>
    public int SpAttack { get; set; }

    /// <summary>
    /// Determines how much damage a Pokemon will resist from special attacks.  
    /// </summary>
    public int SpDefense { get; set; }

    /// <summary>
    /// Determines the speed of a Pokemon. (This is important for determining which pokemon attacks first in battle.)
    /// </summary>
    public int Speed { get; set; }

    /// <summary>
    /// The level of the Pokemon, which can affect its stats and abilities.
    /// To evolve a basic Pokemon, you must reach level 16. To evolve a stage 1 Pokemon, you must reach level 36.
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// The Pokmon's primary type.
    /// </summary>
    public required string PokemonType1 { get; set; }

    /// <summary>
    /// The Pokemon's secondary type, if applicable.
    /// </summary>
    public string? PokemonType2 { get; set; }

}
