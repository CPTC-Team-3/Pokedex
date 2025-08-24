USE master;
GO

DROP DATABASE IF EXISTS PokedexDB;
GO

CREATE DATABASE PokedexDB;
GO

USE PokedexDB;
GO

-- Create the Pokemon table to store Pokémon data
CREATE TABLE Pokemon (
  PokemonId INT PRIMARY KEY IDENTITY(1, 1),
  Name VARCHAR(35) NOT NULL,
  HP INT NOT NULL,
  Defense INT NOT NULL,
  Attack INT NOT NULL,
  SpecialAttack INT NOT NULL,
  SpecialDefense INT NOT NULL,
  Speed INT NOT NULL,
  PokemonType1 VARCHAR(35) NOT NULL,
  PokemonType2 VARCHAR(35)
);

-- Users table to manage user accounts, 
-- includes the trainer level which can increase with experience
CREATE TABLE Users ( -- SAVEFILE??
	UserId INT PRIMARY KEY IDENTITY(1,1),
	FirstName VARCHAR(50) NOT NULL, -- make hidden value
	LastName VARCHAR(50) NOT NULL,-- make hidden value
	Username VARCHAR(50) NOT NULL,
	Password VARCHAR(255) NOT NULL,
	Email VARCHAR(100) NOT NULL,
	TrainerLevel INT NOT NULL DEFAULT 1
);

CREATE TABLE SaveFile (
	SaveFileId INT PRIMARY KEY IDENTITY(1,1), 
	UserId INT,
	SaveDate DATETIME NOT NULL DEFAULT GETDATE(), -- Records the current date and time of the save.
	SaveName AS ('Save-' + FORMAT(SaveDate, 'yyyy-MM-dd_HH-mm')),
);

-- CollectedPokemon table to track which Pokémon each user has collected
CREATE TABLE CollectedPokemon (
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

	-- Primary key to ensure each user can only have one entry per Pokémon
	PRIMARY KEY (UserId, PokemonId),

	-- References to other tables
	FOREIGN KEY (UserId) REFERENCES Users(UserId),
	FOREIGN KEY (PokemonId) REFERENCES Pokemon(PokemonId)
	-- No foreign key needed to reference the rest of the stats, 
	--as they will be duplicated from the Pokemon Table for flexibility
);


INSERT INTO Pokemon (Name, HP, Defense, Attack, SpecialAttack, SpecialDefense, Speed, PokemonType1, PokemonType2)
VALUES
-- GRASS/POISON TYPES
('Bulbasaur', 45, 49, 49, 65, 65, 45, 'Grass', 'Poison'),
('Ivysaur', 60, 62, 63, 80, 80, 60, 'Grass', 'Poison'),
('Venusaur', 80, 82, 83, 100, 100, 80, 'Grass', 'Poison'),

-- FIRE/FLYING TYPES
('Charmander', 39, 52, 43, 60, 50, 65, 'Fire', NULL),
('Charmeleon', 58, 64, 58, 80, 65, 80, 'Fire', NULL),
('Charizard', 78, 84, 78, 109, 85, 100, 'Fire', 'Flying'),

-- WATER TYPES
('Squirtle', 44, 48, 65, 50, 64, 43, 'Water', NULL),
('Wartortle', 59, 63, 80, 65, 80, 58, 'Water', NULL),
('Blastoise', 79, 83, 100, 85, 105, 78, 'Water', NULL),

-- BUG/POISON/FLYING TYPES
('Caterpie', 45, 30, 35, 20, 20, 45, 'Bug', NULL),
('Metapod', 50, 20, 55, 25, 25, 30, 'Bug', NULL),
('Butterfree', 60, 45, 50, 90, 80, 70, 'Bug', 'Flying'),
('Weedle', 40, 35, 30, 20, 20, 50, 'Bug', 'Poison'),
('Kakuna', 45, 25, 50, 25, 25, 35, 'Bug', 'Poison'),
('Beedrill', 65, 90, 40, 45, 80, 75, 'Bug', 'Poison');


-- commented out formatting for pokemonid 
--SELECT FORMAT(PokemonId, '0000') AS FormattedID, Name
--FROM Pokemon;


