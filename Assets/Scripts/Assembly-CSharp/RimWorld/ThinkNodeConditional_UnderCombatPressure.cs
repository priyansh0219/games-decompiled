using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNodeConditional_UnderCombatPressure : ThinkNode_Conditional
	{
		public float maxThreatDistance = 2f;

		public int minCloseTargets = 2;

		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.Spawned && !pawn.Downed)
			{
				return PawnUtility.EnemiesAreNearby(pawn, 9, passDoors: true, maxThreatDistance, minCloseTargets);
			}
			return false;
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNodeConditional_UnderCombatPressure obj = (ThinkNodeConditional_UnderCombatPressure)base.DeepCopy(resolve);
			obj.maxThreatDistance = maxThreatDistance;
			obj.minCloseTargets = minCloseTargets;
			return obj;
		}
	}
}
