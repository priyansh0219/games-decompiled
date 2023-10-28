using System;

namespace Verse
{
	public static class AnimalNameDisplayModeExtension
	{
		public static string ToStringHuman(this AnimalNameDisplayMode mode)
		{
			switch (mode)
			{
			case AnimalNameDisplayMode.None:
				return "None".Translate();
			case AnimalNameDisplayMode.TameNamed:
				return "AnimalNameDisplayMode_TameNamed".Translate();
			case AnimalNameDisplayMode.TameAll:
				return "AnimalNameDisplayMode_TameAll".Translate();
			default:
				throw new NotImplementedException();
			}
		}

		public static bool ShouldDisplayAnimalName(this AnimalNameDisplayMode mode, Pawn animal)
		{
			switch (mode)
			{
			case AnimalNameDisplayMode.None:
				return false;
			case AnimalNameDisplayMode.TameAll:
				return animal.Name != null;
			case AnimalNameDisplayMode.TameNamed:
				if (animal.Name != null)
				{
					return !animal.Name.Numerical;
				}
				return false;
			default:
				throw new NotImplementedException(Prefs.AnimalNameMode.ToStringSafe());
			}
		}
	}
}
