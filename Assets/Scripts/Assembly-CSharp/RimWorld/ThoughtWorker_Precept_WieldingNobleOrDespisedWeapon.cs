using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_WieldingNobleOrDespisedWeapon : ThoughtWorker_Precept
	{
		public override string PostProcessLabel(Pawn p, string label)
		{
			ThingWithComps thingWithComps = p.equipment?.Primary;
			if (thingWithComps == null)
			{
				return label;
			}
			return label.Formatted(thingWithComps.Named("WEAPON"));
		}

		public override string PostProcessDescription(Pawn p, string description)
		{
			ThingWithComps thingWithComps = p.equipment?.Primary;
			if (thingWithComps == null)
			{
				return description;
			}
			return description.Formatted(thingWithComps.Named("WEAPON"));
		}

		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (p.Ideo == null || p.equipment?.Primary == null)
			{
				return false;
			}
			switch (p.Ideo.GetDispositionForWeapon(p.equipment.Primary.def))
			{
			case IdeoWeaponDisposition.Noble:
				return ThoughtState.ActiveAtStage(0);
			case IdeoWeaponDisposition.Despised:
				return ThoughtState.ActiveAtStage(1);
			default:
				return ThoughtState.Inactive;
			}
		}
	}
}
