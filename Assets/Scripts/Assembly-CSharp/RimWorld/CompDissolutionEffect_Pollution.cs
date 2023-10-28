using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompDissolutionEffect_Pollution : CompDissolutionEffect
	{
		private struct WorldPollutionEvent
		{
			public int tile;

			public float amount;
		}

		private static List<WorldPollutionEvent> pendingWorldEvents = new List<WorldPollutionEvent>();

		public CompProperties_DissolutionEffectPollution Props => (CompProperties_DissolutionEffectPollution)props;

		public override void DoDissolutionEffectMap(int amount)
		{
			PollutionUtility.GrowPollutionAt(parent.PositionHeld, parent.MapHeld, amount * Props.cellsToPollutePerDissolution);
			FleckMaker.Static(parent.PositionHeld, parent.MapHeld, FleckDefOf.Fleck_WastePackDissolutionSource);
			SoundDefOf.WastepackDissolution.PlayOneShot(parent);
		}

		public override void DoDissolutionEffectWorld(int amount, int tileId)
		{
			float num = Props.tilePollutionPerDissolution * (float)amount;
			if (Find.World.grid[tileId].WaterCovered)
			{
				num *= Props.waterTilePollutionFactor;
			}
			WorldPollutionEvent worldPollutionEvent = default(WorldPollutionEvent);
			worldPollutionEvent.tile = tileId;
			worldPollutionEvent.amount = num;
			WorldPollutionEvent item = worldPollutionEvent;
			pendingWorldEvents.Add(item);
		}

		public static void WorldUpdate()
		{
			if (pendingWorldEvents.Count <= 0)
			{
				return;
			}
			foreach (IGrouping<int, WorldPollutionEvent> item in from e in pendingWorldEvents
				group e by e.tile)
			{
				WorldPollutionUtility.PolluteWorldAtTile(item.Key, item.Sum((WorldPollutionEvent e) => e.amount));
			}
			pendingWorldEvents.Clear();
		}
	}
}
