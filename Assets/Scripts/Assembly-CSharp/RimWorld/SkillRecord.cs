using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class SkillRecord : IExposable, IComparable<SkillRecord>
	{
		private Pawn pawn;

		public SkillDef def;

		public int levelInt;

		public Passion passion;

		public float xpSinceLastLevel;

		public float xpSinceMidnight;

		private BoolUnknown cachedTotallyDisabled = BoolUnknown.Unknown;

		private BoolUnknown cachedPermanentlyDisabled = BoolUnknown.Unknown;

		private int? aptitudeCached;

		public const int IntervalTicks = 200;

		public const int MinLevel = 0;

		public const int MaxLevel = 20;

		public const int MaxFullRateXpPerDay = 4000;

		public const int MasterSkillThreshold = 14;

		public const float SaturatedLearningFactor = 0.2f;

		public const float LearnFactorPassionNone = 0.35f;

		public const float LearnFactorPassionMinor = 1f;

		public const float LearnFactorPassionMajor = 1.5f;

		public const float MinXPAmount = -1000f;

		private static readonly SimpleCurve XpForLevelUpCurve = new SimpleCurve
		{
			new CurvePoint(0f, 1000f),
			new CurvePoint(9f, 10000f),
			new CurvePoint(19f, 30000f)
		};

		public Pawn Pawn => pawn;

		public int Level
		{
			get
			{
				return GetLevel();
			}
			set
			{
				levelInt = Mathf.Clamp(value, 0, 20);
			}
		}

		public float XpRequiredForLevelUp => XpRequiredToLevelUpFrom(levelInt);

		public float XpProgressPercent => xpSinceLastLevel / XpRequiredForLevelUp;

		public float XpTotalEarned
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < levelInt; i++)
				{
					num += XpRequiredToLevelUpFrom(i);
				}
				return num;
			}
		}

		public bool TotallyDisabled
		{
			get
			{
				if (cachedTotallyDisabled == BoolUnknown.Unknown)
				{
					cachedTotallyDisabled = ((!CalculateTotallyDisabled()) ? BoolUnknown.False : BoolUnknown.True);
				}
				if (PermanentlyDisabled)
				{
					return true;
				}
				return cachedTotallyDisabled == BoolUnknown.True;
			}
		}

		public bool PermanentlyDisabled
		{
			get
			{
				if (cachedPermanentlyDisabled == BoolUnknown.Unknown)
				{
					cachedPermanentlyDisabled = ((!CalculatePermanentlyDisabled()) ? BoolUnknown.False : BoolUnknown.True);
				}
				return cachedPermanentlyDisabled == BoolUnknown.True;
			}
		}

		public string LevelDescriptor
		{
			get
			{
				switch (GetLevelForUI())
				{
				case 0:
					return "Skill0".Translate();
				case 1:
					return "Skill1".Translate();
				case 2:
					return "Skill2".Translate();
				case 3:
					return "Skill3".Translate();
				case 4:
					return "Skill4".Translate();
				case 5:
					return "Skill5".Translate();
				case 6:
					return "Skill6".Translate();
				case 7:
					return "Skill7".Translate();
				case 8:
					return "Skill8".Translate();
				case 9:
					return "Skill9".Translate();
				case 10:
					return "Skill10".Translate();
				case 11:
					return "Skill11".Translate();
				case 12:
					return "Skill12".Translate();
				case 13:
					return "Skill13".Translate();
				case 14:
					return "Skill14".Translate();
				case 15:
					return "Skill15".Translate();
				case 16:
					return "Skill16".Translate();
				case 17:
					return "Skill17".Translate();
				case 18:
					return "Skill18".Translate();
				case 19:
					return "Skill19".Translate();
				case 20:
					return "Skill20".Translate();
				default:
					return "Unknown";
				}
			}
		}

		public bool LearningSaturatedToday => xpSinceMidnight > 4000f;

		public int Aptitude
		{
			get
			{
				if (!aptitudeCached.HasValue)
				{
					aptitudeCached = 0;
					if (!ModsConfig.BiotechActive)
					{
						return aptitudeCached.Value;
					}
					if (pawn.genes != null)
					{
						foreach (Gene item in pawn.genes.GenesListForReading)
						{
							if (item.Active)
							{
								aptitudeCached += item.def.AptitudeFor(def);
							}
						}
					}
				}
				return aptitudeCached.Value;
			}
		}

		public SkillRecord()
		{
		}

		public SkillRecord(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public SkillRecord(Pawn pawn, SkillDef def)
		{
			this.pawn = pawn;
			this.def = def;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref levelInt, "level", 0);
			Scribe_Values.Look(ref xpSinceLastLevel, "xpSinceLastLevel", 0f);
			Scribe_Values.Look(ref passion, "passion", Passion.None);
			Scribe_Values.Look(ref xpSinceMidnight, "xpSinceMidnight", 0f);
		}

		public void Interval()
		{
			float num = (pawn.story.traits.HasTrait(TraitDefOf.GreatMemory) ? 0.5f : 1f);
			switch (levelInt)
			{
			case 10:
				Learn(-0.1f * num);
				break;
			case 11:
				Learn(-0.2f * num);
				break;
			case 12:
				Learn(-0.4f * num);
				break;
			case 13:
				Learn(-0.6f * num);
				break;
			case 14:
				Learn(-1f * num);
				break;
			case 15:
				Learn(-1.8f * num);
				break;
			case 16:
				Learn(-2.8f * num);
				break;
			case 17:
				Learn(-4f * num);
				break;
			case 18:
				Learn(-6f * num);
				break;
			case 19:
				Learn(-8f * num);
				break;
			case 20:
				Learn(-12f * num);
				break;
			}
		}

		public static float XpRequiredToLevelUpFrom(int startingLevel)
		{
			return XpForLevelUpCurve.Evaluate(startingLevel);
		}

		public void Learn(float xp, bool direct = false)
		{
			if (TotallyDisabled || (xp < 0f && levelInt == 0))
			{
				return;
			}
			bool flag = false;
			if (xp > 0f)
			{
				xp *= LearnRateFactor(direct);
			}
			xpSinceLastLevel += xp;
			if (!direct)
			{
				xpSinceMidnight += xp;
			}
			if (levelInt == 20 && xpSinceLastLevel > XpRequiredForLevelUp - 1f)
			{
				xpSinceLastLevel = XpRequiredForLevelUp - 1f;
			}
			while (xpSinceLastLevel >= XpRequiredForLevelUp)
			{
				xpSinceLastLevel -= XpRequiredForLevelUp;
				levelInt++;
				flag = true;
				if (levelInt == 14)
				{
					if (passion == Passion.None)
					{
						TaleRecorder.RecordTale(TaleDefOf.GainedMasterSkillWithoutPassion, pawn, def);
					}
					else
					{
						TaleRecorder.RecordTale(TaleDefOf.GainedMasterSkillWithPassion, pawn, def);
					}
				}
				if (levelInt >= 20)
				{
					levelInt = 20;
					xpSinceLastLevel = Mathf.Clamp(xpSinceLastLevel, 0f, XpRequiredForLevelUp - 1f);
					break;
				}
			}
			while (xpSinceLastLevel <= -1000f)
			{
				levelInt--;
				xpSinceLastLevel += XpRequiredForLevelUp;
				if (levelInt <= 0)
				{
					levelInt = 0;
					xpSinceLastLevel = 0f;
					break;
				}
			}
			if (flag && pawn.IsColonist && pawn.SpawnedOrAnyParentSpawned)
			{
				MoteMaker.ThrowText(pawn.DrawPosHeld ?? pawn.PositionHeld.ToVector3Shifted(), pawn.MapHeld, def.LabelCap + "\n" + "TextMote_SkillUp".Translate(Level));
			}
		}

		public int CompareTo(SkillRecord other)
		{
			int result = Level - other.Level;
			if (Level == other.Level)
			{
				if (xpSinceLastLevel == other.xpSinceLastLevel)
				{
					return 0;
				}
				if (!(xpSinceLastLevel > other.xpSinceLastLevel))
				{
					return -1;
				}
				return 1;
			}
			return result;
		}

		public int GetLevel(bool includeAptitudes = true)
		{
			if (TotallyDisabled)
			{
				return 0;
			}
			int num = levelInt;
			if (includeAptitudes)
			{
				num += Aptitude;
			}
			return Mathf.Clamp(num, 0, 20);
		}

		public int GetLevelForUI(bool includeAptitudes = true)
		{
			if (PermanentlyDisabled)
			{
				return 0;
			}
			int num = levelInt;
			if (includeAptitudes)
			{
				num += Aptitude;
			}
			return Mathf.Clamp(num, 0, 20);
		}

		public int GetUnclampedLevel()
		{
			if (PermanentlyDisabled)
			{
				return 0;
			}
			return levelInt + Aptitude;
		}

		public float LearnRateFactor(bool direct = false)
		{
			if (DebugSettings.fastLearning)
			{
				return 200f;
			}
			float num;
			switch (passion)
			{
			case Passion.None:
				num = 0.35f;
				break;
			case Passion.Minor:
				num = 1f;
				break;
			case Passion.Major:
				num = 1.5f;
				break;
			default:
				throw new NotImplementedException("Passion level " + passion);
			}
			if (!direct)
			{
				num *= pawn.GetStatValue(StatDefOf.GlobalLearningFactor);
				if (def == SkillDefOf.Animals)
				{
					num *= pawn.GetStatValue(StatDefOf.AnimalsLearningFactor);
				}
				if (LearningSaturatedToday)
				{
					num *= 0.2f;
				}
			}
			return num;
		}

		public void EnsureMinLevelWithMargin(int minLevel)
		{
			if (!TotallyDisabled && (levelInt < minLevel || (levelInt == minLevel && xpSinceLastLevel < XpRequiredForLevelUp / 2f)))
			{
				levelInt = minLevel;
				xpSinceLastLevel = XpRequiredForLevelUp / 2f;
			}
		}

		public void Notify_SkillDisablesChanged()
		{
			cachedTotallyDisabled = BoolUnknown.Unknown;
			cachedPermanentlyDisabled = BoolUnknown.Unknown;
		}

		public void Notify_GenesChanged()
		{
			aptitudeCached = null;
		}

		private bool CalculateTotallyDisabled()
		{
			return def.IsDisabled(pawn.story.DisabledWorkTagsBackstoryTraitsAndGenes, pawn.GetDisabledWorkTypes());
		}

		private bool CalculatePermanentlyDisabled()
		{
			return def.IsDisabled(pawn.story.DisabledWorkTagsBackstoryAndTraits, pawn.GetDisabledWorkTypes(permanentOnly: true));
		}

		public override string ToString()
		{
			return def.defName + ": " + levelInt + " (" + xpSinceLastLevel + "xp)";
		}
	}
}
