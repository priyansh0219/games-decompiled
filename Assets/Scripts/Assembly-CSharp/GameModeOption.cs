using System;

[Flags]
public enum GameModeOption
{
	None = 0,
	Permadeath = 1,
	NoSurvival = 2,
	NoCost = 4,
	NoBlueprints = 8,
	NoEnergy = 0x10,
	NoPressure = 0x20,
	NoOxygen = 0x40,
	NoAggression = 0x80,
	NoHints = 0x100,
	NoRadiation = 0x200,
	InitialItems = 0x400,
	Cheats = 0x6FC,
	Survival = 0,
	Hardcore = 0x101,
	Freedom = 2,
	Creative = 0x6FE
}
