using System.Collections.Generic;

namespace Verse
{
	public class RitualStagePositions : IExposable
	{
		public Dictionary<Pawn, PawnStagePosition> positions = new Dictionary<Pawn, PawnStagePosition>();

		private List<Pawn> pawnListTmp;

		private List<PawnStagePosition> positionListTmp;

		public void ExposeData()
		{
			Scribe_Collections.Look(ref positions, "positions", LookMode.Reference, LookMode.Deep, ref pawnListTmp, ref positionListTmp);
		}
	}
}
