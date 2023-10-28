using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNodeConditional_HashIntervalTick : ThinkNode_Conditional
	{
		public int interval;

		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.IsHashIntervalTick(interval);
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNodeConditional_HashIntervalTick obj = (ThinkNodeConditional_HashIntervalTick)base.DeepCopy(resolve);
			obj.interval = interval;
			return obj;
		}
	}
}
