using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class HediffDef : Def
	{
		public Type hediffClass = typeof(Hediff);

		public List<HediffCompProperties> comps;

		[MustTranslate]
		public string descriptionShort;

		public float initialSeverity = 0.5f;

		public float lethalSeverity = -1f;

		public List<HediffStage> stages;

		public bool tendable;

		public bool isBad = true;

		public ThingDef spawnThingOnRemoved;

		public float chanceToCauseNoPain;

		public bool makesSickThought;

		public bool makesAlert = true;

		public NeedDef causesNeed;

		public List<NeedDef> disablesNeeds;

		public float minSeverity;

		public float maxSeverity = float.MaxValue;

		public bool scenarioCanAdd;

		public List<HediffGiver> hediffGivers;

		public bool cureAllAtOnceIfCuredByItem;

		public TaleDef taleOnVisible;

		public bool recordDownedTale = true;

		public bool everCurableByItem = true;

		public List<string> tags;

		public bool priceImpact;

		public float priceOffset;

		public bool chronic;

		public bool keepOnBodyPartRestoration;

		public bool countsAsAddedPartOrImplant;

		public bool blocksSocialInteraction;

		public bool blocksSleeping;

		[MustTranslate]
		public string overrideTooltip;

		[MustTranslate]
		public string extraTooltip;

		public bool levelIsQuantity;

		public bool removeOnDeathrestStart;

		public bool preventsPregnancy;

		public bool preventsLungRot;

		public bool pregnant;

		public bool allowMothballIfLowPriorityWorldPawn;

		public List<string> removeWithTags;

		public SimpleCurve removeOnRedressChanceByDaysCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(1f, 0f)
		};

		public bool removeOnQuestLodgers;

		public List<PawnKindDef> removeOnRedressIfNotOfKind;

		public bool displayWound;

		public float? woundAnchorRange;

		public Color defaultLabelColor = Color.white;

		public GraphicData eyeGraphicSouth;

		public GraphicData eyeGraphicEast;

		public float eyeGraphicScale = 1f;

		public InjuryProps injuryProps;

		public AddedBodyPartProps addedPartProps;

		[MustTranslate]
		public string labelNoun;

		[MustTranslate]
		public string battleStateLabel;

		[MustTranslate]
		public string labelNounPretty;

		[MustTranslate]
		public string targetPrefix;

		private bool alwaysAllowMothballCached;

		private bool alwaysAllowMothball;

		private string descriptionCached;

		private Hediff concreteExampleInt;

		public bool IsAddiction => typeof(Hediff_Addiction).IsAssignableFrom(hediffClass);

		public bool AlwaysAllowMothball
		{
			get
			{
				if (!alwaysAllowMothballCached)
				{
					alwaysAllowMothball = true;
					if (comps != null && comps.Count > 0)
					{
						alwaysAllowMothball = false;
					}
					if (stages != null)
					{
						for (int i = 0; i < stages.Count; i++)
						{
							HediffStage hediffStage = stages[i];
							if (hediffStage.deathMtbDays > 0f || (hediffStage.hediffGivers != null && hediffStage.hediffGivers.Count > 0))
							{
								alwaysAllowMothball = false;
							}
						}
					}
					alwaysAllowMothballCached = true;
				}
				return alwaysAllowMothball;
			}
		}

		public Hediff ConcreteExample
		{
			get
			{
				if (concreteExampleInt == null)
				{
					concreteExampleInt = HediffMaker.Debug_MakeConcreteExampleHediff(this);
				}
				return concreteExampleInt;
			}
		}

		public string Description
		{
			get
			{
				if (descriptionCached == null)
				{
					if (!descriptionShort.NullOrEmpty())
					{
						descriptionCached = descriptionShort;
					}
					else
					{
						descriptionCached = description;
					}
					descriptionCached = Regex.Replace(descriptionCached, "\\r\\n?|\\n", " ");
				}
				return descriptionCached;
			}
		}

		public bool HasComp(Type compClass)
		{
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (comps[i].compClass == compClass)
					{
						return true;
					}
				}
			}
			return false;
		}

		public HediffCompProperties CompPropsFor(Type compClass)
		{
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (comps[i].compClass == compClass)
					{
						return comps[i];
					}
				}
			}
			return null;
		}

		public T CompProps<T>() where T : HediffCompProperties
		{
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (comps[i] is T result)
					{
						return result;
					}
				}
			}
			return null;
		}

		public bool PossibleToDevelopImmunityNaturally()
		{
			HediffCompProperties_Immunizable hediffCompProperties_Immunizable = CompProps<HediffCompProperties_Immunizable>();
			if (hediffCompProperties_Immunizable != null && (hediffCompProperties_Immunizable.immunityPerDayNotSick > 0f || hediffCompProperties_Immunizable.immunityPerDaySick > 0f))
			{
				return true;
			}
			return false;
		}

		public string PrettyTextForPart(BodyPartRecord bodyPart)
		{
			if (labelNounPretty.NullOrEmpty())
			{
				return null;
			}
			return labelNounPretty.Formatted(label, bodyPart.Label);
		}

		public override void ResolveReferences()
		{
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].ResolveReferences(this);
				}
			}
		}

		public int StageAtSeverity(float severity)
		{
			if (stages == null)
			{
				return 0;
			}
			for (int num = stages.Count - 1; num >= 0; num--)
			{
				if (severity >= stages[num].minSeverity)
				{
					return num;
				}
			}
			return 0;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (hediffClass == null)
			{
				yield return "hediffClass is null";
			}
			if (!comps.NullOrEmpty() && !typeof(HediffWithComps).IsAssignableFrom(hediffClass))
			{
				yield return "has comps but hediffClass is not HediffWithComps or subclass thereof";
			}
			if (minSeverity > initialSeverity)
			{
				yield return "minSeverity is greater than initialSeverity";
			}
			if (maxSeverity < initialSeverity)
			{
				yield return "maxSeverity is lower than initialSeverity";
			}
			if (!tendable && HasComp(typeof(HediffComp_TendDuration)))
			{
				yield return "has HediffComp_TendDuration but tendable = false";
			}
			if (string.IsNullOrEmpty(description))
			{
				yield return "Hediff with defName " + defName + " has no description!";
			}
			if (comps != null)
			{
				for (int l = 0; l < comps.Count; l++)
				{
					foreach (string item2 in comps[l].ConfigErrors(this))
					{
						yield return string.Concat(comps[l], ": ", item2);
					}
				}
			}
			if (stages != null)
			{
				if (!typeof(Hediff_Addiction).IsAssignableFrom(hediffClass))
				{
					for (int l = 0; l < stages.Count; l++)
					{
						if (l >= 1 && stages[l].minSeverity <= stages[l - 1].minSeverity)
						{
							yield return "stages are not in order of minSeverity";
						}
					}
				}
				for (int l = 0; l < stages.Count; l++)
				{
					if (stages[l].hediffGivers != null)
					{
						for (int m = 0; m < stages[l].hediffGivers.Count; m++)
						{
							foreach (string item3 in stages[l].hediffGivers[m].ConfigErrors())
							{
								yield return item3;
							}
						}
					}
					if (stages[l].minSeverity > maxSeverity)
					{
						yield return "minSeverity of stage " + l + " is greater than maxSeverity.";
					}
				}
			}
			if (hediffGivers == null)
			{
				yield break;
			}
			for (int l = 0; l < hediffGivers.Count; l++)
			{
				foreach (string item4 in hediffGivers[l].ConfigErrors())
				{
					yield return item4;
				}
			}
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			if (stages == null || stages.Count != 1)
			{
				yield break;
			}
			foreach (StatDrawEntry item in stages[0].SpecialDisplayStats())
			{
				yield return item;
			}
		}

		public static HediffDef Named(string defName)
		{
			return DefDatabase<HediffDef>.GetNamed(defName);
		}
	}
}
