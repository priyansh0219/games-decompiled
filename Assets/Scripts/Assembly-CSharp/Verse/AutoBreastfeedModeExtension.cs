using System;

namespace Verse
{
	public static class AutoBreastfeedModeExtension
	{
		public static TaggedString Translate(this AutofeedMode mode)
		{
			switch (mode)
			{
			case AutofeedMode.Never:
				return "AutofeedModeNever".Translate();
			case AutofeedMode.Childcare:
				return "AutofeedModeChildcare".Translate();
			case AutofeedMode.Urgent:
				return "AutofeedModeUrgent".Translate();
			default:
				throw new NotImplementedException();
			}
		}

		public static TaggedString GetTooltip(this AutofeedMode mode, Pawn baby, Pawn feeder)
		{
			string key;
			switch (mode)
			{
			case AutofeedMode.Never:
				key = "AutofeedModeTooltipNever";
				break;
			case AutofeedMode.Childcare:
				key = "AutofeedModeTooltipChildcare";
				break;
			case AutofeedMode.Urgent:
				key = "AutofeedModeTooltipUrgent";
				break;
			default:
				throw new NotImplementedException();
			}
			return key.Translate(baby.Named("BABY"), feeder.Named("FEEDER"));
		}
	}
}
