using System;

[Flags]
public enum ItemAction
{
	None = 0,
	Use = 1,
	Eat = 2,
	Equip = 4,
	Unequip = 8,
	Assign = 0x10,
	Switch = 0x20,
	Swap = 0x40,
	Drop = 0x80
}
