using Verse;

namespace RimWorld
{
	public class CompUseEffect_FixWorstHealthCondition : CompUseEffect
	{
		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
			TaggedString taggedString = HealthUtility.FixWorstHealthCondition(usedBy);
			if (PawnUtility.ShouldSendNotificationAbout(usedBy))
			{
				Messages.Message(taggedString, usedBy, MessageTypeDefOf.PositiveEvent);
			}
		}
	}
}
