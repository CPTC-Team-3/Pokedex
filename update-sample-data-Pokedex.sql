USE master; 
GO

USE PokedexDB;
GO

-- sample user data for testing purposes
--INSERT INTO Users (FirstName, LastName, Username, Password, Email, TrainerLevel)
--VALUES	('Ash', 'Ketchum', 'K-Ash25', 'KetchumAll123', 'TrainerAK@pokedex.com', 5),
	--	('Misty', 'Kasumi', 'MissStar', 'Starmie333', 'TrainerMK@pokedex.com', 5),
		--('Brock', 'Takeshi', '1SleepyBrock1', 'pewterGYM321', 'TrainerBT@pokedex.com', 5);

--ALTER TABLE SaveFile
--ADD CONSTRAINT FK_SaveFile_User
--FOREIGN KEY (UserId) REFERENCES Users(UserId);

-- sample pokemon data for testing purposes
--INSERT INTO CollectedPokemon (UserId, PokemonId, Name, Level, HP, Defense, Attack, SpecialAttack,
	--						SpecialDefense, Speed, PokemonType1, PokemonType2)
--VALUES
-- sample pokemon for the user to start off with. Ash is the only one with a collected pokemon currently
--(1, 1, 'Bulbasaur', 1, 45, 49, 49, 65, 65, 45, 'Grass', 'Poison');

-- formatting for the pokemon id to always be 4 digits with leading zeros
SELECT FORMAT(PokemonId, '0000') AS FormattedID, Name
FROM Pokemon;
SELECT FORMAT(PokemonId, '0000') AS FormattedID, Name
FROM CollectedPokemon;



