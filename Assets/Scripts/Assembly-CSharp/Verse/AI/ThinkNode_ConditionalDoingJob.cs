namespace Verse.AI
{
	public class ThinkNode_ConditionalDoingJob : ThinkNode_Conditional
	{
		public JobDef jobDef;

		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.CurJobDef == jobDef;
		}
	}
}
