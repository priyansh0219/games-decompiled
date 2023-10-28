namespace Verse.AI
{
	public class Pawn_Thinker : IExposable
	{
		public Pawn pawn;

		public ThinkTreeDef MainThinkTree => pawn.ageTracker.CurLifeStage?.thinkTreeMainOverride ?? pawn.RaceProps.thinkTreeMain;

		public ThinkNode MainThinkNodeRoot => MainThinkTree.thinkRoot;

		public ThinkTreeDef ConstantThinkTree => pawn.ageTracker.CurLifeStage?.thinkTreeConstantOverride ?? pawn.RaceProps.thinkTreeConstant;

		public ThinkNode ConstantThinkNodeRoot => ConstantThinkTree.thinkRoot;

		public Pawn_Thinker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public T TryGetMainTreeThinkNode<T>() where T : ThinkNode
		{
			return MainThinkNodeRoot.FirstNodeOfType<T>();
		}

		public T GetMainTreeThinkNode<T>() where T : ThinkNode
		{
			T val = TryGetMainTreeThinkNode<T>();
			if (val == null)
			{
				Log.Warning(string.Concat(pawn, " looked for ThinkNode of type ", typeof(T), " and didn't find it."));
			}
			return val;
		}

		public void ExposeData()
		{
		}
	}
}
