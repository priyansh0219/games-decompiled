using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public static class HealthUtility
	{
		public static readonly Color GoodConditionColor = new Color(0.6f, 0.8f, 0.65f);

		public static readonly Color RedColor = ColorLibrary.RedReadable;

		public static readonly Color ImpairedColor = new Color(0.9f, 0.35f, 0f);

		public static readonly Color SlightlyImpairedColor = new Color(0.9f, 0.7f, 0f);

		private static List<Hediff> tmpHediffs = new List<Hediff>();

		public static string GetGeneralConditionLabel(Pawn pawn, bool shortVersion = false)
		{
			if (pawn.health.Dead)
			{
				return "Dead".Translate();
			}
			if (pawn.Deathresting)
			{
				return "Deathresting".Translate().CapitalizeFirst();
			}
			if (!pawn.health.capacities.CanBeAwake)
			{
				return "Unconscious".Translate();
			}
			if (pawn.health.InPainShock)
			{
				return (shortVersion && "PainShockShort".CanTranslate()) ? "PainShockShort".Translate() : "PainShock".Translate();
			}
			if (pawn.Downed && !LifeStageUtility.AlwaysDowned(pawn))
			{
				return "Incapacitated".Translate();
			}
			bool flag = false;
			for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
			{
				if (pawn.health.hediffSet.hediffs[i] is Hediff_Injury hd && !hd.IsPermanent())
				{
					flag = true;
				}
			}
			if (flag)
			{
				return "Injured".Translate();
			}
			if (pawn.health.hediffSet.PainTotal > 0.3f)
			{
				return "InPain".Translate();
			}
			return "Healthy".Translate();
		}

		public static Pair<string, Color> GetPartConditionLabel(Pawn pawn, BodyPartRecord part)
		{
			float partHealth = pawn.health.hediffSet.GetPartHealth(part);
			float maxHealth = part.def.GetMaxHealth(pawn);
			float num = partHealth / maxHealth;
			string text = "";
			Color white = Color.white;
			if (partHealth <= 0f)
			{
				Hediff_MissingPart hediff_MissingPart = null;
				List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
				for (int i = 0; i < missingPartsCommonAncestors.Count; i++)
				{
					if (missingPartsCommonAncestors[i].Part == part)
					{
						hediff_MissingPart = missingPartsCommonAncestors[i];
						break;
					}
				}
				if (hediff_MissingPart == null)
				{
					bool fresh = false;
					if (hediff_MissingPart != null && hediff_MissingPart.IsFreshNonSolidExtremity)
					{
						fresh = true;
					}
					bool solid = part.def.IsSolid(part, pawn.health.hediffSet.hediffs);
					text = GetGeneralDestroyedPartLabel(part, fresh, solid);
					white = Color.gray;
				}
				else
				{
					text = hediff_MissingPart.LabelCap;
					white = hediff_MissingPart.LabelColor;
				}
			}
			else if (num < 0.4f)
			{
				text = "SeriouslyImpaired".Translate();
				white = RedColor;
			}
			else if (num < 0.7f)
			{
				text = "Impaired".Translate();
				white = ImpairedColor;
			}
			else if (num < 0.999f)
			{
				text = "SlightlyImpaired".Translate();
				white = SlightlyImpairedColor;
			}
			else
			{
				text = "GoodCondition".Translate();
				white = GoodConditionColor;
			}
			return new Pair<string, Color>(text, white);
		}

		public static string GetGeneralDestroyedPartLabel(BodyPartRecord part, bool fresh, bool solid)
		{
			if (part.parent == null)
			{
				return "SeriouslyImpaired".Translate();
			}
			if (part.depth == BodyPartDepth.Inside || fresh)
			{
				if (solid)
				{
					return "ShatteredBodyPart".Translate();
				}
				return "DestroyedBodyPart".Translate();
			}
			return "MissingBodyPart".Translate();
		}

		private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
		{
			return from x in bodyModel.GetNotMissingParts()
				where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs))
				select x;
		}

		public static void HealNonPermanentInjuriesAndRestoreLegs(Pawn p)
		{
			if (p.Dead)
			{
				return;
			}
			tmpHediffs.Clear();
			tmpHediffs.AddRange(p.health.hediffSet.hediffs);
			for (int i = 0; i < tmpHediffs.Count; i++)
			{
				if (tmpHediffs[i] is Hediff_Injury hediff_Injury && !hediff_Injury.IsPermanent())
				{
					p.health.RemoveHediff(hediff_Injury);
				}
				else if (tmpHediffs[i] is Hediff_MissingPart hediff_MissingPart && hediff_MissingPart.Part.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore) && (hediff_MissingPart.Part.parent == null || p.health.hediffSet.GetNotMissingParts().Contains(hediff_MissingPart.Part.parent)))
				{
					p.health.RestorePart(hediff_MissingPart.Part);
				}
			}
			tmpHediffs.Clear();
		}

		[Obsolete]
		public static void HealNonPermanentInjuriesAndFreshWounds(Pawn p)
		{
			if (p.Dead)
			{
				return;
			}
			tmpHediffs.Clear();
			tmpHediffs.AddRange(p.health.hediffSet.hediffs);
			foreach (Hediff tmpHediff in tmpHediffs)
			{
				if (tmpHediff.def.everCurableByItem)
				{
					if (tmpHediff is Hediff_Injury hediff_Injury && !hediff_Injury.IsPermanent())
					{
						p.health.RemoveHediff(hediff_Injury);
					}
					else if (tmpHediff is Hediff_MissingPart hediff_MissingPart && hediff_MissingPart.IsFresh)
					{
						hediff_MissingPart.IsFresh = false;
						p.health.Notify_HediffChanged(hediff_MissingPart);
					}
				}
			}
			tmpHediffs.Clear();
		}

		[Obsolete]
		public static Hediff_Injury HealRandomPermanentInjury(Pawn p)
		{
			if (p.Dead)
			{
				return null;
			}
			tmpHediffs.Clear();
			foreach (Hediff hediff in p.health.hediffSet.hediffs)
			{
				if (hediff.def.everCurableByItem && hediff is Hediff_Injury hd && hd.IsPermanent())
				{
					tmpHediffs.Add(hediff);
				}
			}
			Hediff_Injury hediff_Injury = tmpHediffs.RandomElementWithFallback() as Hediff_Injury;
			if (hediff_Injury != null)
			{
				p.health.RemoveHediff(hediff_Injury);
			}
			tmpHediffs.Clear();
			return hediff_Injury;
		}

		public static void GiveRandomSurgeryInjuries(Pawn p, int totalDamage, BodyPartRecord operatedPart)
		{
			IEnumerable<BodyPartRecord> source = ((operatedPart != null) ? (from x in p.health.hediffSet.GetNotMissingParts()
				where !x.def.conceptual
				select x into pa
				where pa == operatedPart || pa.parent == operatedPart || (operatedPart != null && operatedPart.parent == pa)
				select pa) : (from x in p.health.hediffSet.GetNotMissingParts()
				where !x.def.conceptual
				select x));
			source = source.Where((BodyPartRecord x) => GetMinHealthOfPartsWeWantToAvoidDestroying(x, p) >= 2f);
			BodyPartRecord brain = p.health.hediffSet.GetBrain();
			if (brain != null)
			{
				float maxBrainHealth = brain.def.GetMaxHealth(p);
				source = source.Where((BodyPartRecord x) => x != brain || p.health.hediffSet.GetPartHealth(x) >= maxBrainHealth * 0.5f + 1f);
			}
			while (totalDamage > 0 && source.Any())
			{
				BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
				float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
				int num = Mathf.Max(3, GenMath.RoundRandom(partHealth * Rand.Range(0.5f, 1f)));
				float minHealthOfPartsWeWantToAvoidDestroying = GetMinHealthOfPartsWeWantToAvoidDestroying(bodyPartRecord, p);
				if (minHealthOfPartsWeWantToAvoidDestroying - (float)num < 1f)
				{
					num = Mathf.RoundToInt(minHealthOfPartsWeWantToAvoidDestroying - 1f);
				}
				if (bodyPartRecord == brain && partHealth - (float)num < brain.def.GetMaxHealth(p) * 0.5f)
				{
					num = Mathf.Max(Mathf.RoundToInt(partHealth - brain.def.GetMaxHealth(p) * 0.5f), 1);
				}
				if (num > 0)
				{
					DamageDef def = Rand.Element(DamageDefOf.Cut, DamageDefOf.Scratch, DamageDefOf.Stab, DamageDefOf.Crush);
					DamageInfo dinfo = new DamageInfo(def, num, 0f, -1f, null, bodyPartRecord);
					dinfo.SetIgnoreArmor(ignoreArmor: true);
					dinfo.SetIgnoreInstantKillProtection(ignore: true);
					p.TakeDamage(dinfo);
					totalDamage -= num;
					continue;
				}
				break;
			}
		}

		private static float GetMinHealthOfPartsWeWantToAvoidDestroying(BodyPartRecord part, Pawn pawn)
		{
			float num = 999999f;
			while (part != null)
			{
				if (ShouldRandomSurgeryInjuriesAvoidDestroying(part, pawn))
				{
					num = Mathf.Min(num, pawn.health.hediffSet.GetPartHealth(part));
				}
				part = part.parent;
			}
			return num;
		}

		private static bool ShouldRandomSurgeryInjuriesAvoidDestroying(BodyPartRecord part, Pawn pawn)
		{
			if (part == pawn.RaceProps.body.corePart)
			{
				return true;
			}
			if (part.def.tags.Any((BodyPartTagDef x) => x.vital))
			{
				return true;
			}
			for (int i = 0; i < part.parts.Count; i++)
			{
				if (ShouldRandomSurgeryInjuriesAvoidDestroying(part.parts[i], pawn))
				{
					return true;
				}
			}
			return false;
		}

		public static void DamageUntilDowned(Pawn p, bool allowBleedingWounds = true)
		{
			if (p.Downed)
			{
				return;
			}
			HediffSet hediffSet = p.health.hediffSet;
			p.health.forceDowned = true;
			IEnumerable<BodyPartRecord> source = from x in HittablePartsViolence(hediffSet)
				where !p.health.hediffSet.hediffs.Any((Hediff y) => y.Part == x && y.CurStage != null && y.CurStage.partEfficiencyOffset < 0f)
				select x;
			int num = 0;
			while (num < 300 && !p.Downed && source.Any())
			{
				num++;
				BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
				int num2 = Mathf.RoundToInt(hediffSet.GetPartHealth(bodyPartRecord));
				float statValue = p.GetStatValue(StatDefOf.IncomingDamageFactor);
				if (statValue > 0f)
				{
					num2 = (int)((float)num2 / statValue);
				}
				num2 -= 3;
				if (num2 >= 8)
				{
					DamageDef damageDef = ((bodyPartRecord.depth != BodyPartDepth.Outside) ? DamageDefOf.Blunt : ((allowBleedingWounds || !(bodyPartRecord.def.bleedRate > 0f)) ? RandomViolenceDamageType() : DamageDefOf.Blunt));
					int num3 = Rand.RangeInclusive(Mathf.RoundToInt((float)num2 * 0.65f), num2);
					HediffDef hediffDefFromDamage = GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
					if (!p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, (float)num3 * p.GetStatValue(StatDefOf.IncomingDamageFactor)))
					{
						DamageInfo dinfo = new DamageInfo(damageDef, num3, 999f, -1f, null, bodyPartRecord);
						dinfo.SetAllowDamagePropagation(val: false);
						p.TakeDamage(dinfo);
					}
				}
			}
			if (p.Dead)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(string.Concat(p, " died during GiveInjuriesToForceDowned"));
				for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
				{
					stringBuilder.AppendLine("   -" + p.health.hediffSet.hediffs[i].ToString());
				}
				Log.Error(stringBuilder.ToString());
			}
			p.health.forceDowned = false;
		}

		public static void DamageUntilDead(Pawn p)
		{
			HediffSet hediffSet = p.health.hediffSet;
			int num = 0;
			while (!p.Dead && num < 200 && HittablePartsViolence(hediffSet).Any())
			{
				num++;
				BodyPartRecord bodyPartRecord = HittablePartsViolence(hediffSet).RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
				int num2 = Rand.RangeInclusive(8, 25);
				DamageDef def = ((bodyPartRecord.depth != BodyPartDepth.Outside) ? DamageDefOf.Blunt : RandomViolenceDamageType());
				DamageInfo dinfo = new DamageInfo(def, num2, 999f, -1f, null, bodyPartRecord);
				dinfo.SetIgnoreInstantKillProtection(ignore: true);
				p.TakeDamage(dinfo);
			}
			if (!p.Dead)
			{
				Log.Error(string.Concat(p, " not killed during GiveInjuriesToKill"));
			}
		}

		public static void DamageLegsUntilIncapableOfMoving(Pawn p, bool allowBleedingWounds = true)
		{
			int num = 0;
			p.health.forceDowned = true;
			while (p.health.capacities.CapableOf(PawnCapacityDefOf.Moving) && num < 300)
			{
				num++;
				IEnumerable<BodyPartRecord> source = from x in p.health.hediffSet.GetNotMissingParts()
					where x.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore) && p.health.hediffSet.GetPartHealth(x) >= 2f
					select x;
				if (!source.Any())
				{
					break;
				}
				BodyPartRecord bodyPartRecord = source.RandomElement();
				float maxHealth = bodyPartRecord.def.GetMaxHealth(p);
				float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
				int min = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.12f), 1, (int)partHealth - 1);
				int max = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.27f), 1, (int)partHealth - 1);
				int num2 = Rand.RangeInclusive(min, max);
				DamageDef damageDef = ((allowBleedingWounds || !(bodyPartRecord.def.bleedRate > 0f)) ? RandomViolenceDamageType() : DamageDefOf.Blunt);
				HediffDef hediffDefFromDamage = GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
				if (p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, num2))
				{
					break;
				}
				DamageInfo dinfo = new DamageInfo(damageDef, num2, 999f, -1f, null, bodyPartRecord);
				dinfo.SetAllowDamagePropagation(val: false);
				p.TakeDamage(dinfo);
			}
			p.health.forceDowned = false;
		}

		public static void DamageLimbsUntilIncapableOfManipulation(Pawn p, bool allowBleedingWounds = true)
		{
			int num = 0;
			p.health.forceDowned = true;
			while (p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && num < 300)
			{
				num++;
				IEnumerable<BodyPartRecord> source = from x in p.health.hediffSet.GetNotMissingParts()
					where x.def.tags.Contains(BodyPartTagDefOf.ManipulationLimbCore) && p.health.hediffSet.GetPartHealth(x) >= 2f
					select x;
				if (!source.Any())
				{
					break;
				}
				BodyPartRecord bodyPartRecord = source.RandomElement();
				float maxHealth = bodyPartRecord.def.GetMaxHealth(p);
				float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
				int min = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.12f), 1, (int)partHealth - 1);
				int max = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.27f), 1, (int)partHealth - 1);
				int num2 = Rand.RangeInclusive(min, max);
				DamageDef damageDef = RandomViolenceDamageType();
				HediffDef hediffDefFromDamage = GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
				if (p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, num2))
				{
					break;
				}
				DamageInfo dinfo = new DamageInfo(damageDef, num2, 999f, -1f, null, bodyPartRecord);
				dinfo.SetAllowDamagePropagation(val: false);
				p.TakeDamage(dinfo);
			}
			p.health.forceDowned = false;
		}

		public static DamageDef RandomViolenceDamageType()
		{
			switch (Rand.RangeInclusive(0, 4))
			{
			case 0:
				return DamageDefOf.Bullet;
			case 1:
				return DamageDefOf.Blunt;
			case 2:
				return DamageDefOf.Stab;
			case 3:
				return DamageDefOf.Scratch;
			case 4:
				return DamageDefOf.Cut;
			default:
				return null;
			}
		}

		public static DamageDef RandomPermanentInjuryDamageType(bool allowFrostbite)
		{
			switch (Rand.RangeInclusive(0, 3 + (allowFrostbite ? 1 : 0)))
			{
			case 0:
				return DamageDefOf.Bullet;
			case 1:
				return DamageDefOf.Scratch;
			case 2:
				return DamageDefOf.Bite;
			case 3:
				return DamageDefOf.Stab;
			case 4:
				return DamageDefOf.Frostbite;
			default:
				throw new Exception();
			}
		}

		public static HediffDef GetHediffDefFromDamage(DamageDef dam, Pawn pawn, BodyPartRecord part)
		{
			HediffDef result = dam.hediff;
			if (part.def.IsSkinCovered(part, pawn.health.hediffSet) && dam.hediffSkin != null)
			{
				result = dam.hediffSkin;
			}
			if (part.def.IsSolid(part, pawn.health.hediffSet.hediffs) && dam.hediffSolid != null)
			{
				result = dam.hediffSolid;
			}
			return result;
		}

		public static bool TryAnesthetize(Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return false;
			}
			pawn.health.forceDowned = true;
			pawn.health.AddHediff(HediffDefOf.Anesthetic);
			pawn.health.forceDowned = false;
			return true;
		}

		public static void AdjustSeverity(Pawn pawn, HediffDef hdDef, float sevOffset)
		{
			if (sevOffset != 0f)
			{
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hdDef);
				if (firstHediffOfDef != null)
				{
					firstHediffOfDef.Severity += sevOffset;
				}
				else if (sevOffset > 0f)
				{
					firstHediffOfDef = HediffMaker.MakeHediff(hdDef, pawn);
					firstHediffOfDef.Severity = sevOffset;
					pawn.health.AddHediff(firstHediffOfDef);
				}
			}
		}

		public static BodyPartRemovalIntent PartRemovalIntent(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.health.hediffSet.hediffs.Any((Hediff d) => d.Visible && d.Part == part && d.def.isBad))
			{
				return BodyPartRemovalIntent.Amputate;
			}
			return BodyPartRemovalIntent.Harvest;
		}

		public static int TicksUntilDeathDueToBloodLoss(Pawn pawn)
		{
			float bleedRateTotal = pawn.health.hediffSet.BleedRateTotal;
			if (bleedRateTotal < 0.0001f)
			{
				return int.MaxValue;
			}
			float num = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss)?.Severity ?? 0f;
			return (int)((1f - num) / bleedRateTotal * 60000f);
		}

		public static TaggedString FixWorstHealthCondition(Pawn pawn)
		{
			Hediff hediff = FindLifeThreateningHediff(pawn);
			if (hediff != null)
			{
				return Cure(hediff);
			}
			if (TicksUntilDeathDueToBloodLoss(pawn) < 2500)
			{
				Hediff hediff2 = FindMostBleedingHediff(pawn);
				if (hediff2 != null)
				{
					return Cure(hediff2);
				}
			}
			if (pawn.health.hediffSet.GetBrain() != null)
			{
				Hediff_Injury hediff_Injury = FindPermanentInjury(pawn, Gen.YieldSingle(pawn.health.hediffSet.GetBrain()));
				if (hediff_Injury != null)
				{
					return Cure(hediff_Injury);
				}
			}
			float coverageAbsWithChildren = ThingDefOf.Human.race.body.GetPartsWithDef(BodyPartDefOf.Hand).First().coverageAbsWithChildren;
			BodyPartRecord bodyPartRecord = FindBiggestMissingBodyPart(pawn, coverageAbsWithChildren);
			if (bodyPartRecord != null)
			{
				return Cure(bodyPartRecord, pawn);
			}
			Hediff_Injury hediff_Injury2 = FindPermanentInjury(pawn, from x in pawn.health.hediffSet.GetNotMissingParts()
				where x.def == BodyPartDefOf.Eye
				select x);
			if (hediff_Injury2 != null)
			{
				return Cure(hediff_Injury2);
			}
			Hediff hediff3 = FindImmunizableHediffWhichCanKill(pawn);
			if (hediff3 != null)
			{
				return Cure(hediff3);
			}
			Hediff hediff4 = FindNonInjuryMiscBadHediff(pawn, onlyIfCanKill: true);
			if (hediff4 != null)
			{
				return Cure(hediff4);
			}
			Hediff hediff5 = FindNonInjuryMiscBadHediff(pawn, onlyIfCanKill: false);
			if (hediff5 != null)
			{
				return Cure(hediff5);
			}
			if (pawn.health.hediffSet.GetBrain() != null)
			{
				Hediff_Injury hediff_Injury3 = FindInjury(pawn, Gen.YieldSingle(pawn.health.hediffSet.GetBrain()));
				if (hediff_Injury3 != null)
				{
					return Cure(hediff_Injury3);
				}
			}
			BodyPartRecord bodyPartRecord2 = FindBiggestMissingBodyPart(pawn);
			if (bodyPartRecord2 != null)
			{
				return Cure(bodyPartRecord2, pawn);
			}
			Hediff_Addiction hediff_Addiction = FindAddiction(pawn);
			if (hediff_Addiction != null)
			{
				return Cure(hediff_Addiction);
			}
			Hediff_Injury hediff_Injury4 = FindPermanentInjury(pawn);
			if (hediff_Injury4 != null)
			{
				return Cure(hediff_Injury4);
			}
			Hediff_Injury hediff_Injury5 = FindInjury(pawn);
			if (hediff_Injury5 != null)
			{
				return Cure(hediff_Injury5);
			}
			return null;
		}

		private static Hediff FindLifeThreateningHediff(Pawn pawn)
		{
			Hediff hediff = null;
			float num = -1f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (!hediffs[i].Visible || !hediffs[i].def.everCurableByItem || hediffs[i].FullyImmune())
				{
					continue;
				}
				bool num2 = hediffs[i].CurStage?.lifeThreatening ?? false;
				bool flag = hediffs[i].def.lethalSeverity >= 0f && hediffs[i].Severity / hediffs[i].def.lethalSeverity >= 0.8f;
				if (num2 || flag)
				{
					float num3 = ((hediffs[i].Part != null) ? hediffs[i].Part.coverageAbsWithChildren : 999f);
					if (hediff == null || num3 > num)
					{
						hediff = hediffs[i];
						num = num3;
					}
				}
			}
			return hediff;
		}

		private static Hediff FindMostBleedingHediff(Pawn pawn)
		{
			float num = 0f;
			Hediff hediff = null;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Visible && hediffs[i].def.everCurableByItem)
				{
					float bleedRate = hediffs[i].BleedRate;
					if (bleedRate > 0f && (bleedRate > num || hediff == null))
					{
						num = bleedRate;
						hediff = hediffs[i];
					}
				}
			}
			return hediff;
		}

		private static Hediff FindImmunizableHediffWhichCanKill(Pawn pawn)
		{
			Hediff hediff = null;
			float num = -1f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Visible && hediffs[i].def.everCurableByItem && hediffs[i].TryGetComp<HediffComp_Immunizable>() != null && !hediffs[i].FullyImmune() && CanEverKill(hediffs[i]))
				{
					float severity = hediffs[i].Severity;
					if (hediff == null || severity > num)
					{
						hediff = hediffs[i];
						num = severity;
					}
				}
			}
			return hediff;
		}

		private static Hediff FindNonInjuryMiscBadHediff(Pawn pawn, bool onlyIfCanKill)
		{
			Hediff hediff = null;
			float num = -1f;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].Visible && hediffs[i].def.isBad && hediffs[i].def.everCurableByItem && !(hediffs[i] is Hediff_Injury) && !(hediffs[i] is Hediff_MissingPart) && !(hediffs[i] is Hediff_Addiction) && !(hediffs[i] is Hediff_AddedPart) && (!onlyIfCanKill || CanEverKill(hediffs[i])))
				{
					float num2 = ((hediffs[i].Part != null) ? hediffs[i].Part.coverageAbsWithChildren : 999f);
					if (hediff == null || num2 > num)
					{
						hediff = hediffs[i];
						num = num2;
					}
				}
			}
			return hediff;
		}

		private static BodyPartRecord FindBiggestMissingBodyPart(Pawn pawn, float minCoverage = 0f)
		{
			BodyPartRecord bodyPartRecord = null;
			foreach (Hediff_MissingPart missingPartsCommonAncestor in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
			{
				if (!(missingPartsCommonAncestor.Part.coverageAbsWithChildren < minCoverage) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(missingPartsCommonAncestor.Part) && (bodyPartRecord == null || missingPartsCommonAncestor.Part.coverageAbsWithChildren > bodyPartRecord.coverageAbsWithChildren))
				{
					bodyPartRecord = missingPartsCommonAncestor.Part;
				}
			}
			return bodyPartRecord;
		}

		private static Hediff_Addiction FindAddiction(Pawn pawn)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i] is Hediff_Addiction hediff_Addiction && hediff_Addiction.Visible && hediff_Addiction.def.everCurableByItem)
				{
					return hediff_Addiction;
				}
			}
			return null;
		}

		private static Hediff_Injury FindPermanentInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
		{
			Hediff_Injury hediff_Injury = null;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i] is Hediff_Injury hediff_Injury2 && hediff_Injury2.Visible && hediff_Injury2.IsPermanent() && hediff_Injury2.def.everCurableByItem && (allowedBodyParts == null || allowedBodyParts.Contains(hediff_Injury2.Part)) && (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity))
				{
					hediff_Injury = hediff_Injury2;
				}
			}
			return hediff_Injury;
		}

		private static Hediff_Injury FindInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
		{
			Hediff_Injury hediff_Injury = null;
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i] is Hediff_Injury hediff_Injury2 && hediff_Injury2.Visible && hediff_Injury2.def.everCurableByItem && (allowedBodyParts == null || allowedBodyParts.Contains(hediff_Injury2.Part)) && (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity))
				{
					hediff_Injury = hediff_Injury2;
				}
			}
			return hediff_Injury;
		}

		public static TaggedString Cure(Hediff hediff)
		{
			Pawn pawn = hediff.pawn;
			pawn.health.RemoveHediff(hediff);
			if (hediff.def.cureAllAtOnceIfCuredByItem)
			{
				int num = 0;
				while (true)
				{
					num++;
					if (num > 10000)
					{
						Log.Error("Too many iterations.");
						break;
					}
					Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediff.def);
					if (firstHediffOfDef == null)
					{
						break;
					}
					pawn.health.RemoveHediff(firstHediffOfDef);
				}
			}
			return "HealingCureHediff".Translate(pawn, hediff.def.label);
		}

		private static TaggedString Cure(BodyPartRecord part, Pawn pawn)
		{
			pawn.health.RestorePart(part);
			return "HealingRestoreBodyPart".Translate(pawn, part.Label);
		}

		private static bool CanEverKill(Hediff hediff)
		{
			if (hediff.def.stages != null)
			{
				for (int i = 0; i < hediff.def.stages.Count; i++)
				{
					if (hediff.def.stages[i].lifeThreatening)
					{
						return true;
					}
				}
			}
			return hediff.def.lethalSeverity >= 0f;
		}

		public static bool IsMissingSightBodyPart(Pawn p)
		{
			List<Hediff> hediffs = p.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i] is Hediff_MissingPart hediff_MissingPart && hediff_MissingPart.Part.def.tags.Contains(BodyPartTagDefOf.SightSource))
				{
					return true;
				}
			}
			return false;
		}
	}
}
