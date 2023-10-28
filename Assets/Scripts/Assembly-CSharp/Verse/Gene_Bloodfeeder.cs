using RimWorld;

namespace Verse
{
	public class Gene_Bloodfeeder : Gene
	{
		public override void PostAdd()
		{
			base.PostAdd();
			if (pawn.IsPrisonerOfColony && pawn.guest?.interactionMode != null && pawn.guest.interactionMode.hideIfNoBloodfeeders)
			{
				pawn.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
			}
		}
	}
}
