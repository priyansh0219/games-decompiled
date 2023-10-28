using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatPart_ApparelStatOffset : StatPart
	{
		private StatDef apparelStat;

		private bool subtract;

		private bool includeWeapon;

		private float hideAtValue = float.MinValue;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (!req.HasThing || req.Thing == null || !(req.Thing is Pawn pawn))
			{
				return;
			}
			if (pawn.apparel != null)
			{
				for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
				{
					float statValue = pawn.apparel.WornApparel[i].GetStatValue(apparelStat);
					statValue += StatWorker.StatOffsetFromGear(pawn.apparel.WornApparel[i], apparelStat);
					if (subtract)
					{
						val -= statValue;
					}
					else
					{
						val += statValue;
					}
				}
			}
			if (includeWeapon && pawn.equipment != null && pawn.equipment.Primary != null)
			{
				float statValue2 = pawn.equipment.Primary.GetStatValue(apparelStat);
				statValue2 += StatWorker.StatOffsetFromGear(pawn.equipment.Primary, apparelStat);
				if (subtract)
				{
					val -= statValue2;
				}
				else
				{
					val += statValue2;
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing && req.Thing != null && req.Thing is Pawn pawn && PawnWearingRelevantGear(pawn))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("StatsReport_RelevantGear".Translate());
				if (pawn.apparel != null)
				{
					for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
					{
						Apparel gear = pawn.apparel.WornApparel[i];
						if (!Mathf.Approximately(hideAtValue, GetStatValueForGear(gear)))
						{
							stringBuilder.AppendLine(InfoTextLineFrom(gear));
						}
					}
				}
				if (includeWeapon && pawn.equipment != null && pawn.equipment.Primary != null && !Mathf.Approximately(hideAtValue, GetStatValueForGear(pawn.equipment.Primary)))
				{
					stringBuilder.AppendLine(InfoTextLineFrom(pawn.equipment.Primary));
				}
				return stringBuilder.ToString();
			}
			return null;
		}

		private string InfoTextLineFrom(Thing gear)
		{
			float num = GetStatValueForGear(gear);
			if (subtract)
			{
				num = 0f - num;
			}
			return "    " + gear.LabelCap + ": " + num.ToStringByStyle(parentStat.toStringStyle, ToStringNumberSense.Offset);
		}

		private float GetStatValueForGear(Thing gear)
		{
			return gear.GetStatValue(apparelStat) + StatWorker.StatOffsetFromGear(gear, apparelStat);
		}

		private bool PawnWearingRelevantGear(Pawn pawn)
		{
			if (pawn.apparel != null)
			{
				for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
				{
					Apparel apparel = pawn.apparel.WornApparel[i];
					if (apparel.GetStatValue(apparelStat) != 0f)
					{
						return true;
					}
					if (StatWorker.StatOffsetFromGear(apparel, apparelStat) != 0f)
					{
						return true;
					}
				}
			}
			if (includeWeapon && pawn.equipment != null && pawn.equipment.Primary != null && StatWorker.StatOffsetFromGear(pawn.equipment.Primary, apparelStat) != 0f)
			{
				return true;
			}
			return false;
		}

		public override IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(StatRequest req)
		{
			if (!(req.Thing is Pawn pawn) || pawn.apparel == null)
			{
				yield break;
			}
			for (int i = 0; i < pawn.apparel.WornApparel.Count; i++)
			{
				Apparel thing = pawn.apparel.WornApparel[i];
				if (Mathf.Abs(thing.GetStatValue(apparelStat)) > 0f)
				{
					yield return new Dialog_InfoCard.Hyperlink(thing);
				}
			}
		}
	}
}
