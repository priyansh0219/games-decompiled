using System.Linq;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PsychicBondProximity : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!ModsConfig.BiotechActive)
			{
				return ThoughtState.Inactive;
			}
			Hediff_PsychicBond hediff_PsychicBond = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) as Hediff_PsychicBond;
			if (hediff_PsychicBond?.target == null)
			{
				return ThoughtState.Inactive;
			}
			if (NearPsychicBondedPerson(p, hediff_PsychicBond))
			{
				return ThoughtState.ActiveAtStage(0);
			}
			return ThoughtState.ActiveAtStage(1);
		}

		public static bool NearPsychicBondedPerson(Pawn pawn, Hediff_PsychicBond bondHediff)
		{
			Pawn bondedPawn;
			if ((bondedPawn = bondHediff?.target as Pawn) == null)
			{
				return false;
			}
			bool flag = pawn.CarriedBy != null;
			bool flag2 = bondedPawn.CarriedBy != null;
			if (flag && flag2)
			{
				return pawn.MapHeld == bondedPawn.MapHeld;
			}
			bool flag3 = pawn.BrieflyDespawned();
			bool flag4 = bondedPawn.BrieflyDespawned();
			if (flag3 && flag4)
			{
				return pawn.MapHeld == bondedPawn.MapHeld;
			}
			if ((pawn.Spawned || bondedPawn.Spawned) && (flag3 || flag4 || flag || flag2))
			{
				return pawn.MapHeld == bondedPawn.MapHeld;
			}
			IThingHolder parentHolder = pawn.ParentHolder;
			IThingHolder parentHolder2 = bondedPawn.ParentHolder;
			if (parentHolder != null && parentHolder == parentHolder2)
			{
				return true;
			}
			if ((parentHolder != null && ThingOwnerUtility.ContentsSuspended(parentHolder)) || (parentHolder2 != null && ThingOwnerUtility.ContentsSuspended(parentHolder2)))
			{
				return false;
			}
			if (QuestUtility.GetAllQuestPartsOfType<QuestPart_LendColonistsToFaction>().FirstOrDefault((QuestPart_LendColonistsToFaction p) => p.LentColonistsListForReading.Contains(pawn) && p.LentColonistsListForReading.Contains(bondedPawn)) != null)
			{
				return true;
			}
			return false;
		}
	}
}
