using System;

namespace Verse
{
	public static class ShowWeaponsUnderPortraitModeExtension
	{
		public static string ToStringHuman(this ShowWeaponsUnderPortraitMode mode)
		{
			switch (mode)
			{
			case ShowWeaponsUnderPortraitMode.Never:
				return "Never".Translate().CapitalizeFirst();
			case ShowWeaponsUnderPortraitMode.WhileDrafted:
				return "ShowWeapons_WhileDrafted".Translate().CapitalizeFirst();
			case ShowWeaponsUnderPortraitMode.Always:
				return "ShowWeapons_Always".Translate().CapitalizeFirst();
			default:
				throw new NotImplementedException();
			}
		}
	}
}
