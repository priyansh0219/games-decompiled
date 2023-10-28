using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_CorpsesLarge : SpecialThingFilterWorker
	{
		private const float MinBodySize = 0.75f;

		public override bool Matches(Thing t)
		{
			if (t is Corpse corpse)
			{
				return corpse.InnerPawn.BodySize > 0.75f;
			}
			return false;
		}

		public override bool CanEverMatch(ThingDef def)
		{
			return AlwaysMatches(def);
		}

		public override bool AlwaysMatches(ThingDef def)
		{
			if (def.IsCorpse && def.ingestible?.sourceDef?.race != null)
			{
				RaceProperties race = def.ingestible.sourceDef.race;
				float num = float.MinValue;
				for (int i = 0; i < race.lifeStageAges.Count; i++)
				{
					if (race.lifeStageAges[i].def.bodySizeFactor > num)
					{
						num = race.lifeStageAges[i].def.bodySizeFactor;
					}
				}
				return num * race.baseBodySize > 0.75f;
			}
			return false;
		}
	}
}
