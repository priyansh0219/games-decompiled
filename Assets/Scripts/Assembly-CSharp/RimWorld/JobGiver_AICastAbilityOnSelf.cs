using Verse;

namespace RimWorld
{
	public class JobGiver_AICastAbilityOnSelf : JobGiver_AICastAbility
	{
		protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
		{
			return new LocalTargetInfo(caster);
		}
	}
}
