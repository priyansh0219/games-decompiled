using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SerializablePawnList : IExposable
	{
		private List<Pawn> pawns;

		public List<Pawn> Pawns => pawns;

		public SerializablePawnList()
		{
		}

		public SerializablePawnList(List<Pawn> pawns)
		{
			this.pawns = pawns;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		}
	}
}
