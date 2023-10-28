using System;
using Verse;

namespace RimWorld
{
	public static class HungerLevelUtility
	{
		public const float FallPerTickFactor_Hungry = 0.5f;

		public const float FallPerTickFactor_UrgentlyHungry = 0.25f;

		public static string GetLabel(this HungerCategory hunger)
		{
			switch (hunger)
			{
			case HungerCategory.Starving:
				return "HungerLevel_Starving".Translate();
			case HungerCategory.UrgentlyHungry:
				return "HungerLevel_UrgentlyHungry".Translate();
			case HungerCategory.Hungry:
				return "HungerLevel_Hungry".Translate();
			case HungerCategory.Fed:
				return "HungerLevel_Fed".Translate();
			default:
				throw new InvalidOperationException();
			}
		}

		public static float HungerMultiplier(this HungerCategory cat)
		{
			switch (cat)
			{
			case HungerCategory.Fed:
				return 1f;
			case HungerCategory.Hungry:
				return 0.5f;
			case HungerCategory.UrgentlyHungry:
				return 0.25f;
			case HungerCategory.Starving:
				return 0f;
			default:
				throw new NotImplementedException();
			}
		}
	}
}
