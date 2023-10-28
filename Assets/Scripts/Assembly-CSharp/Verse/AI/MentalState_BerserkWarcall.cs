using RimWorld;

namespace Verse.AI
{
	public class MentalState_BerserkWarcall : MentalState
	{
		public override bool ForceHostileTo(Thing t)
		{
			if (sourceFaction == null)
			{
				return t.Faction != null;
			}
			if (t.Faction != null)
			{
				return ForceHostileTo(t.Faction);
			}
			return false;
		}

		public override bool ForceHostileTo(Faction f)
		{
			return f.HostileTo(sourceFaction);
		}

		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
	}
}
