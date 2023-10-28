using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_MechMissingBodyPart : Alert
	{
		private const int MinAgeTicks = 240000;

		private List<Pawn> targets = new List<Pawn>();

		private string labelSingular;

		private string labelPlural;

		public string LabelSingular
		{
			get
			{
				if (labelSingular.NullOrEmpty())
				{
					labelSingular = "AlertMechMissingBodyPart".Translate();
				}
				return labelSingular;
			}
		}

		public string LabelPlural
		{
			get
			{
				if (labelPlural.NullOrEmpty())
				{
					labelPlural = "AlertMechsMissingBodyPart".Translate();
				}
				return labelPlural;
			}
		}

		public Alert_MechMissingBodyPart()
		{
			defaultLabel = "MechMissingBodyPartSingular".Translate();
		}

		private void GetTargets()
		{
			targets.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				List<Pawn> list = maps[i].mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].IsColonyMech && HasMissingBodyPart(list[j]))
					{
						targets.Add(list[j]);
					}
				}
			}
		}

		private bool HasMissingBodyPart(Pawn pawn)
		{
			List<BodyPartRecord> allParts = pawn.def.race.body.AllParts;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i] is Hediff_MissingPart hediff_MissingPart && hediffs[i].ageTicks > 240000 && allParts.Contains(hediff_MissingPart.Part))
				{
					return true;
				}
			}
			return false;
		}

		public override AlertReport GetReport()
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			GetTargets();
			return AlertReport.CulpritsAre(targets);
		}

		public override string GetLabel()
		{
			if (targets.Count != 1)
			{
				return LabelPlural;
			}
			return LabelSingular;
		}

		public override TaggedString GetExplanation()
		{
			return "AlertMechMissingBodyPartDesc".Translate() + ":\n\n" + targets.Select((Pawn p) => p.LabelCap).ToLineList(" - ");
		}
	}
}
