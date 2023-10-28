using System;

namespace Verse
{
	public static class MechNameDisplayModeExtension
	{
		public static string ToStringHuman(this MechNameDisplayMode mode)
		{
			switch (mode)
			{
			case MechNameDisplayMode.None:
				return "Never".Translate().CapitalizeFirst();
			case MechNameDisplayMode.WhileDrafted:
				return "MechNameDisplayMode_WhileDrafted".Translate().CapitalizeFirst();
			case MechNameDisplayMode.Always:
				return "MechNameDisplayMode_Always".Translate().CapitalizeFirst();
			default:
				throw new NotImplementedException();
			}
		}

		public static bool ShouldDisplayMechName(this MechNameDisplayMode mode, Pawn mech)
		{
			switch (mode)
			{
			case MechNameDisplayMode.None:
				return false;
			case MechNameDisplayMode.WhileDrafted:
				if (mech.Name != null)
				{
					return mech.Drafted;
				}
				return false;
			case MechNameDisplayMode.Always:
				return mech.Name != null;
			default:
				throw new NotImplementedException(Prefs.MechNameMode.ToStringSafe());
			}
		}
	}
}
