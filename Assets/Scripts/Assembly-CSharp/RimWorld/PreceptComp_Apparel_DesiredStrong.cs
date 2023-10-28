using Verse;

namespace RimWorld
{
	public class PreceptComp_Apparel_DesiredStrong : PreceptComp_Apparel
	{
		public override void Notify_MemberGenerated(Pawn pawn, Precept precept, bool newborn)
		{
			if (!newborn && AppliesToPawn(pawn, precept))
			{
				GiveApparelToPawn(pawn, (Precept_Apparel)precept);
			}
		}
	}
}
