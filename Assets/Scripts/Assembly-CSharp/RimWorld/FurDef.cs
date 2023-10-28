using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class FurDef : StyleItemDef
	{
		public List<FurCoveredGraphicData> bodyTypeGraphicPaths;

		public string GetFurBodyGraphicPath(Pawn pawn)
		{
			for (int i = 0; i < bodyTypeGraphicPaths.Count; i++)
			{
				if (bodyTypeGraphicPaths[i].bodyType == pawn.story.bodyType)
				{
					return bodyTypeGraphicPaths[i].texturePath;
				}
			}
			return null;
		}
	}
}
