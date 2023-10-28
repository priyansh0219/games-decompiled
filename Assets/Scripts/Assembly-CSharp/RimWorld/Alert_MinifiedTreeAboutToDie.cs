using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class Alert_MinifiedTreeAboutToDie : Alert
	{
		public const int TicksLeftThreshold = 60000;

		private List<Pawn> colonistsBotheredByTreesDyingResult = new List<Pawn>();

		private List<Thing> treesAboutToDieResult = new List<Thing>();

		private List<Pawn> ColonistsBotheredByTreesDying
		{
			get
			{
				colonistsBotheredByTreesDyingResult.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (Pawn item in maps[i].mapPawns.FreeColonistsSpawned)
					{
						if (item.Ideo != null && item.Ideo.WarnPlayerOnDesignateChopTree)
						{
							colonistsBotheredByTreesDyingResult.Add(item);
						}
					}
				}
				return colonistsBotheredByTreesDyingResult;
			}
		}

		private List<Thing> TreesAboutToDie
		{
			get
			{
				treesAboutToDieResult.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					CheckForTrees(maps[i].listerThings.ThingsOfDef(ThingDefOf.MinifiedTree));
					foreach (Thing item in maps[i].listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder))
					{
						ThingOwner thingOwner = item.TryGetInnerInteractableThingOwner();
						if (thingOwner != null)
						{
							CheckForTrees(thingOwner);
						}
					}
				}
				foreach (Caravan caravan in Find.WorldObjects.Caravans)
				{
					CheckForTrees(CaravanInventoryUtility.AllInventoryItems(caravan));
				}
				return treesAboutToDieResult;
				void CheckForTrees(IList<Thing> things)
				{
					for (int j = 0; j < things.Count; j++)
					{
						if (things[j] is MinifiedTree minifiedTree && minifiedTree.TicksTillDeath <= 60000)
						{
							treesAboutToDieResult.Add(minifiedTree);
						}
					}
				}
			}
		}

		public override string GetLabel()
		{
			return "AlertMinifiedTreeAboutToDie".Translate(TreesAboutToDie.Count.ToStringCached());
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn item in ColonistsBotheredByTreesDying)
			{
				stringBuilder.AppendLine("  - " + item.NameShortColored.Resolve());
			}
			return "AlertMinifiedTreeAboutToDieDesc".Translate(TreesAboutToDie.Count.ToStringCached()) + ":\n\n" + stringBuilder.ToString();
		}

		public override AlertReport GetReport()
		{
			List<Thing> treesAboutToDie = TreesAboutToDie;
			if (treesAboutToDie.Count == 0 || ColonistsBotheredByTreesDying.Count == 0)
			{
				return false;
			}
			return AlertReport.CulpritsAre(treesAboutToDie);
		}
	}
}
