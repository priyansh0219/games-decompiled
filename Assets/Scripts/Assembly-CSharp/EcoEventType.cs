using System;

public enum EcoEventType
{
	None = 0,
	[Obsolete("Use EcoTarget system instead", true)]
	Here = 100,
	Power = 200,
	LiveMeat = 300,
	Vegetable = 400,
	Sound = 500,
	Light = 600,
	Motion = 700,
	Blood = 800,
	Vibration = 900,
	Shiny = 1000,
	Visible = 1100,
	Cold = 1200,
	Touch = 1300,
	Smoke = 1400,
	Acid = 1500,
	Electricity = 1600,
	Heat = 1700,
	DeadMeat = 1800
}
