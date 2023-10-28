using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_ReleaseAnimalToWild : PawnColumnWorker_Designator
	{
		protected override DesignationDef DesignationType => DesignationDefOf.ReleaseAnimalToWild;

		protected override string GetTip(Pawn pawn)
		{
			return "DesignatorReleaseAnimalToWildDesc".Translate();
		}

		protected override bool HasCheckbox(Pawn pawn)
		{
			if (pawn.RaceProps.Animal && pawn.RaceProps.IsFlesh && pawn.Faction == Faction.OfPlayer)
			{
				return pawn.SpawnedOrAnyParentSpawned;
			}
			return false;
		}

		protected override void Notify_DesignationAdded(Pawn pawn)
		{
			ReleaseAnimalToWildUtility.CheckWarnAboutBondedAnimal(pawn);
		}
	}
}
