-- Create DATABASE PokedexDB;

USE PokedexDB; 
SELECT * FROM dbo.Pokemon;

--Create TABLE Pokemon ( 
	--PokemonId INT PRIMARY KEY, 
	--Name VARCHAR(30), 
	--HP INT, 
	--Defense INT, 
	--Attack INT,
	--SpecialAttack INT, 
	--SpecialDefense INT,
	--Speed INT, 
	--PokemonType1 VARCHAR(35), 
	--PokemonType2 VARCHAR(35) NULL,  -- optionally NULL ! not all Pokemon have two types. 
--);

--INSERT INTO Pokemon (PokemonId, Name, HP, Defense, 
--Attack, SpecialAttack, SpecialDefense, Speed, PokemonType1, PokemonType2)

--VALUES 
-- GRASS/POISON TYPES 
--(1, 'Bulbasaur', 45, 49, 49, 65, 65, 45, 'Grass', 'Poison'),
--(2, 'Ivysaur', 60, 62, 63, 80, 80, 60, 'Grass', 'Poison'),
--(3, 'Venusaur', 80, 82, 83, 100, 100, 80, 'Grass', 'Poison'),

-- FIRE/FLYING TYPES  
--(4, 'Charmander', 39, 52, 43, 60, 50, 65, 'Fire', NULL),
--(5, 'Charmeleon', 58, 64, 58, 80, 65, 80, 'Fire', NULL),
--(6, 'Charizard', 78, 84, 78, 109, 85, 100, 'Fire', 'Flying'),

-- WATER TYPES
--(7, 'Squirtle', 44, 48, 65, 50, 64, 43, 'Water', NULL),
--(8, 'Wartortle', 59, 63, 80, 65, 80, 58, 'Water', NULL),
--(9, 'Blastoise', 79, 83, 100, 85, 105, 78, 'Water', NULL),

-- BUG/POISON/FLYING TYPES
--(10, 'Caterpie', 45, 30, 35, 20, 20, 45, 'Bug', NULL),
--(11, 'Metapod', 50, 20, 55, 25, 25, 30, 'Bug', NULL),
--(12, 'Butterfree', 60, 45, 50, 90, 80, 70, 'Bug', 'Flying'),
--(13, 'Weedle', 40, 35, 30, 20, 20, 50, 'Bug', 'Poison'),
--(14, 'Kakuna', 45, 25, 50, 25, 25, 35, 'Bug', 'Poison'),
--(15, 'Beedrill', 65, 90, 40, 45, 80, 75, 'Bug', 'Poison');

SELECT FORMAT(PokemonID, '0000') AS FormattedID, Name
FROM Pokemon;
-----------------------------------------------------
-- Below ALTERS columns so that the following are required and CANNOT be NULL --
-----------------------------------------------------
--ALTER TABLE Pokemon
--ALTER COLUMN Name VARCHAR(35) NOT NULL;

--ALTER TABLE Pokemon
--ALTER COLUMN HP INT NOT NULL;

--ALTER TABLE Pokemon
--ALTER COLUMN Defense INT NOT NULL;

--ALTER TABLE Pokemon
--ALTER COLUMN Attack INT NOT NULL;

--ALTER TABLE Pokemon
--ALTER COLUMN SpecialAttack INT NOT NULL;

--ALTER TABLE Pokemon
--ALTER COLUMN SpecialDefense INT NOT NULL;

--ALTER TABLE Pokemon
--ALTER COLUMN PokemonType1 VARCHAR(35) NOT NULL;
