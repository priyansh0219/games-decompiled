using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class RoomRoleWorker_WorshipRoom : RoomRoleWorker
	{
		private const int MinScore = 2000;

		private Ideo firstAltarIdeo;

		public override string PostProcessedLabel(string baseLabel)
		{
			if (firstAltarIdeo == null || firstAltarIdeo.WorshipRoomLabel.NullOrEmpty())
			{
				return base.PostProcessedLabel(baseLabel);
			}
			return firstAltarIdeo.WorshipRoomLabel;
		}

		public override float GetScore(Room room)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return -1f;
			}
			List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
			firstAltarIdeo = null;
			int num = 0;
			for (int i = 0; i < containedAndAdjacentThings.Count; i++)
			{
				if (!containedAndAdjacentThings[i].def.isAltar)
				{
					continue;
				}
				CompStyleable compStyleable = containedAndAdjacentThings[i].TryGetComp<CompStyleable>();
				if (compStyleable != null && compStyleable.SourcePrecept?.ideo?.StructureMeme != null)
				{
					if (firstAltarIdeo == null)
					{
						firstAltarIdeo = compStyleable.SourcePrecept.ideo;
					}
					num++;
				}
			}
			return (num != 0) ? Mathf.Max(2000, num * 75) : 0;
		}
	}
}
