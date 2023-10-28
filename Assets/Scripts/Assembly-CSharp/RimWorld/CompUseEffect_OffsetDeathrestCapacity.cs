using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompUseEffect_OffsetDeathrestCapacity : CompUseEffect
	{
		private CompProperties_UseEffectOffsetDeathrestCapacity Props => (CompProperties_UseEffectOffsetDeathrestCapacity)props;

		public override void DoEffect(Pawn user)
		{
			if (ModsConfig.BiotechActive)
			{
				base.DoEffect(user);
				user.genes?.GetFirstGeneOfType<Gene_Deathrest>()?.OffsetCapacity(Props.offset);
			}
		}

		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
			if (p.genes?.GetFirstGeneOfType<Gene_Deathrest>() == null)
			{
				failReason = "CannotDeathrest".Translate();
				return false;
			}
			return base.CanBeUsedBy(p, out failReason);
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (ModsConfig.BiotechActive)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawnImportant, "DeathrestCapacity".Translate().CapitalizeFirst(), Props.offset.ToStringWithSign(), "DeathrestCapacityDesc".Translate(), 1010);
			}
		}
	}
}
