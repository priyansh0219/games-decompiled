namespace Verse.AI
{
	public class MentalBreakWorker_IdeoChange : MentalBreakWorker
	{
		public override bool BreakCanOccur(Pawn pawn)
		{
			if (Find.IdeoManager.classicMode || !base.BreakCanOccur(pawn))
			{
				return false;
			}
			return true;
		}
	}
}
