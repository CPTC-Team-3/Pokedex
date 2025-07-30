CREATE TABLE [dbo].[Pokemon] (
    [PokemonID]      INT          NOT NULL,
    [Name]           VARCHAR (35) NULL,
    [HP]             INT          NULL,
    [Defense]        INT          NULL,
    [Attack]         INT          NULL,
    [SpecialAttack]  INT          NULL,
    [SpecialDefense] INT          NULL,
    [Speed]          INT          NULL,
    [PokemonType1]   VARCHAR (40) NULL,
    [PokemonType2]   VARCHAR (40) NULL,
    PRIMARY KEY CLUSTERED ([PokemonID] ASC)
);

