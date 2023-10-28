using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnAllegianceDrawer : PawnOverlayDrawer
	{
		public PawnAllegianceDrawer(Pawn pawn)
			: base(pawn)
		{
		}

		protected override void WriteCache(CacheKey key, List<DrawCall> writeTarget)
		{
			throw new NotImplementedException();
		}
	}
}
