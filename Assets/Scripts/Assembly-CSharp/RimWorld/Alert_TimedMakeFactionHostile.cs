using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class Alert_TimedMakeFactionHostile : Alert
	{
		private List<GlobalTargetInfo> worldObjectsResult = new List<GlobalTargetInfo>();

		private List<GlobalTargetInfo> WorldObjects
		{
			get
			{
				worldObjectsResult.Clear();
				foreach (WorldObject allWorldObject in Find.WorldObjects.AllWorldObjects)
				{
					TimedMakeFactionHostile component = allWorldObject.GetComponent<TimedMakeFactionHostile>();
					if (component != null && component.TicksLeft.HasValue)
					{
						worldObjectsResult.Add(allWorldObject);
					}
				}
				return worldObjectsResult;
			}
		}

		public override string GetLabel()
		{
			return "FactionWillBecomeHostileIfNotLeavingWithin".Translate();
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (GlobalTargetInfo worldObject in WorldObjects)
			{
				stringBuilder.Append("- ");
				stringBuilder.Append(worldObject.Label);
				stringBuilder.Append(" (");
				stringBuilder.Append(worldObject.WorldObject.GetComponent<TimedMakeFactionHostile>().TicksLeft.Value.ToStringTicksToPeriod());
				stringBuilder.AppendLine(")");
			}
			return "FactionWillBecomeHostileIfNotLeavingWithinDesc".Translate(stringBuilder.ToString().TrimEndNewlines());
		}

		public override AlertReport GetReport()
		{
			List<GlobalTargetInfo> worldObjects = WorldObjects;
			Map currentMap = Find.CurrentMap;
			List<Pawn> culprits;
			if (!WorldRendererUtility.WorldRenderedNow && currentMap != null && worldObjects.Contains(currentMap.Parent) && !(culprits = currentMap.mapPawns.FreeHumanlikesSpawnedOfFaction(currentMap.ParentFaction)).NullOrEmpty())
			{
				return AlertReport.CulpritsAre(culprits);
			}
			if (worldObjects.Count > 0)
			{
				return AlertReport.CulpritsAre(worldObjects);
			}
			return false;
		}
	}
}
