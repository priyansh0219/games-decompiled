using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public class Hediff : IExposable, ILoadReferenceable
	{
		public HediffDef def;

		public int ageTicks;

		private BodyPartRecord part;

		public ThingDef source;

		public BodyPartGroupDef sourceBodyPartGroup;

		public HediffDef sourceHediffDef;

		public int loadID = -1;

		protected float severityInt;

		private bool recordedTale;

		protected bool causesNoPain;

		private bool visible;

		public WeakReference<LogEntry> combatLogEntry;

		public string combatLogText;

		public int temp_partIndexToSetLater = -1;

		[Unsaved(false)]
		public Pawn pawn;

		public virtual string LabelBase => CurStage?.overrideLabel ?? def.label;

		public string LabelBaseCap => LabelBase.CapitalizeFirst(def);

		public virtual string Label
		{
			get
			{
				string labelInBrackets = LabelInBrackets;
				return LabelBase + (labelInBrackets.NullOrEmpty() ? "" : (" (" + labelInBrackets + ")"));
			}
		}

		public string LabelCap => Label.CapitalizeFirst(def);

		public virtual Color LabelColor => def.defaultLabelColor;

		public virtual string LabelInBrackets
		{
			get
			{
				if (CurStage != null && !CurStage.label.NullOrEmpty())
				{
					return CurStage.label;
				}
				return null;
			}
		}

		public virtual string SeverityLabel
		{
			get
			{
				if (!(def.lethalSeverity <= 0f))
				{
					return (Severity / def.lethalSeverity).ToStringPercent();
				}
				return null;
			}
		}

		public virtual int UIGroupKey => Label.GetHashCode();

		public virtual string TipStringExtra
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (StatDrawEntry item in HediffStatsUtility.SpecialDisplayStats(CurStage, this))
				{
					if (item.ShouldDisplay)
					{
						stringBuilder.AppendLine("  - " + item.LabelCap + ": " + item.ValueString);
					}
				}
				return stringBuilder.ToString();
			}
		}

		public virtual HediffStage CurStage
		{
			get
			{
				if (!def.stages.NullOrEmpty())
				{
					return def.stages[CurStageIndex];
				}
				return null;
			}
		}

		public virtual bool ShouldRemove => Severity <= 0f;

		public virtual bool Visible
		{
			get
			{
				if (!visible && CurStage != null)
				{
					return CurStage.becomeVisible;
				}
				return true;
			}
		}

		public virtual float BleedRate => 0f;

		public virtual float BleedRateScaled => BleedRate / pawn.HealthScale;

		public bool Bleeding => BleedRate > 1E-05f;

		public virtual float PainOffset
		{
			get
			{
				if (CurStage != null && !causesNoPain)
				{
					return CurStage.painOffset;
				}
				return 0f;
			}
		}

		public virtual float PainFactor
		{
			get
			{
				if (CurStage != null)
				{
					return CurStage.painFactor;
				}
				return 1f;
			}
		}

		public List<PawnCapacityModifier> CapMods
		{
			get
			{
				if (CurStage != null)
				{
					return CurStage.capMods;
				}
				return null;
			}
		}

		public virtual float SummaryHealthPercentImpact => 0f;

		public virtual float TendPriority
		{
			get
			{
				float a = 0f;
				HediffStage curStage = CurStage;
				if (curStage != null && curStage.lifeThreatening)
				{
					a = Mathf.Max(a, 1f);
				}
				a = Mathf.Max(a, BleedRate * 1.5f);
				HediffComp_TendDuration hediffComp_TendDuration = this.TryGetComp<HediffComp_TendDuration>();
				if (hediffComp_TendDuration != null && hediffComp_TendDuration.TProps.severityPerDayTended < 0f)
				{
					a = Mathf.Max(a, 0.025f);
				}
				return a;
			}
		}

		public virtual TextureAndColor StateIcon => TextureAndColor.None;

		public virtual int CurStageIndex => def.StageAtSeverity(Severity);

		public virtual float Severity
		{
			get
			{
				return severityInt;
			}
			set
			{
				bool flag = false;
				if (def.lethalSeverity > 0f && value >= def.lethalSeverity)
				{
					value = def.lethalSeverity;
					flag = true;
				}
				bool flag2 = this is Hediff_Injury && value > severityInt && Mathf.RoundToInt(value) != Mathf.RoundToInt(severityInt);
				int curStageIndex = CurStageIndex;
				severityInt = Mathf.Clamp(value, def.minSeverity, def.maxSeverity);
				if ((CurStageIndex != curStageIndex || flag || flag2) && pawn.health.hediffSet.hediffs.Contains(this))
				{
					pawn.health.Notify_HediffChanged(this);
					if (!pawn.Dead && pawn.needs.mood != null)
					{
						pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
					}
				}
			}
		}

		public BodyPartRecord Part
		{
			get
			{
				return part;
			}
			set
			{
				if (pawn == null && part != null)
				{
					Log.Error("Hediff: Cannot set Part without setting pawn first.");
				}
				else
				{
					part = value;
				}
			}
		}

		public virtual string Description => def.Description;

		public virtual bool TendableNow(bool ignoreTimer = false)
		{
			if (!def.tendable || Severity <= 0f || this.FullyImmune() || !Visible || this.IsPermanent() || !pawn.RaceProps.IsFlesh)
			{
				return false;
			}
			if (!ignoreTimer)
			{
				HediffComp_TendDuration hediffComp_TendDuration = this.TryGetComp<HediffComp_TendDuration>();
				if (hediffComp_TendDuration != null && !hediffComp_TendDuration.AllowTend)
				{
					return false;
				}
			}
			return true;
		}

		public virtual void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving && combatLogEntry != null)
			{
				LogEntry target = combatLogEntry.Target;
				if (target == null || !Current.Game.battleLog.IsEntryActive(target))
				{
					combatLogEntry = null;
				}
			}
			Scribe_Values.Look(ref loadID, "loadID", 0);
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
			Scribe_Defs.Look(ref source, "source");
			Scribe_Defs.Look(ref sourceBodyPartGroup, "sourceBodyPartGroup");
			Scribe_Defs.Look(ref sourceHediffDef, "sourceHediffDef");
			Scribe_BodyParts.Look(ref part, "part");
			Scribe_Values.Look(ref severityInt, "severity", 0f);
			Scribe_Values.Look(ref recordedTale, "recordedTale", defaultValue: false);
			Scribe_Values.Look(ref causesNoPain, "causesNoPain", defaultValue: false);
			Scribe_Values.Look(ref visible, "visible", defaultValue: false);
			Scribe_References.Look(ref combatLogEntry, "combatLogEntry");
			Scribe_Values.Look(ref combatLogText, "combatLogText");
			BackCompatibility.PostExposeData(this);
		}

		public virtual void Tick()
		{
			ageTicks++;
			if (def.hediffGivers != null && pawn.IsHashIntervalTick(60))
			{
				for (int i = 0; i < def.hediffGivers.Count; i++)
				{
					def.hediffGivers[i].OnIntervalPassed(pawn, this);
				}
			}
			if (Visible && !visible)
			{
				visible = true;
				if (def.taleOnVisible != null)
				{
					TaleRecorder.RecordTale(def.taleOnVisible, pawn, def);
				}
			}
			HediffStage curStage = CurStage;
			if (curStage == null)
			{
				return;
			}
			if (curStage.hediffGivers != null && pawn.IsHashIntervalTick(60))
			{
				for (int j = 0; j < curStage.hediffGivers.Count; j++)
				{
					curStage.hediffGivers[j].OnIntervalPassed(pawn, this);
				}
			}
			if (curStage.mentalStateGivers != null && pawn.IsHashIntervalTick(60) && !pawn.InMentalState)
			{
				for (int k = 0; k < curStage.mentalStateGivers.Count; k++)
				{
					MentalStateGiver mentalStateGiver = curStage.mentalStateGivers[k];
					if (Rand.MTBEventOccurs(mentalStateGiver.mtbDays, 60000f, 60f))
					{
						pawn.mindState.mentalStateHandler.TryStartMentalState(mentalStateGiver.mentalState, "MentalStateReason_Hediff".Translate(Label));
					}
				}
			}
			if (curStage.mentalBreakMtbDays > 0f && pawn.IsHashIntervalTick(60) && !pawn.InMentalState && !pawn.Downed && Rand.MTBEventOccurs(curStage.mentalBreakMtbDays, 60000f, 60f))
			{
				TryDoRandomMentalBreak();
			}
			if (curStage.vomitMtbDays > 0f && pawn.IsHashIntervalTick(600) && Rand.MTBEventOccurs(curStage.vomitMtbDays, 60000f, 600f) && pawn.Spawned && pawn.Awake() && pawn.RaceProps.IsFlesh)
			{
				pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Vomit), JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
			}
			if (curStage.forgetMemoryThoughtMtbDays > 0f && pawn.needs != null && pawn.needs.mood != null && pawn.IsHashIntervalTick(400) && Rand.MTBEventOccurs(curStage.forgetMemoryThoughtMtbDays, 60000f, 400f) && pawn.needs.mood.thoughts.memories.Memories.TryRandomElement(out var result))
			{
				pawn.needs.mood.thoughts.memories.RemoveMemory(result);
			}
			if (!recordedTale && curStage.tale != null)
			{
				TaleRecorder.RecordTale(curStage.tale, pawn);
				recordedTale = true;
			}
			if (curStage.destroyPart && Part != null && Part != pawn.RaceProps.body.corePart)
			{
				pawn.health.AddHediff(HediffDefOf.MissingBodyPart, Part);
			}
			if (curStage.deathMtbDays > 0f && pawn.IsHashIntervalTick(200) && Rand.MTBEventOccurs(curStage.deathMtbDays, 60000f, 200f))
			{
				DoMTBDeath();
			}
		}

		private void DoMTBDeath()
		{
			HediffStage curStage = CurStage;
			if (!curStage.mtbDeathDestroysBrain && ModsConfig.BiotechActive)
			{
				Pawn_GeneTracker genes = pawn.genes;
				if (genes != null && genes.HasGene(GeneDefOf.Deathless))
				{
					return;
				}
			}
			pawn.Kill(null, this);
			if (curStage.mtbDeathDestroysBrain)
			{
				BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
				if (brain != null)
				{
					Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn, brain);
					pawn.health.AddHediff(hediff, brain);
				}
			}
		}

		private void TryDoRandomMentalBreak()
		{
			HediffStage curStage = CurStage;
			if (curStage != null && DefDatabase<MentalBreakDef>.AllDefsListForReading.Where((MentalBreakDef x) => x.Worker.BreakCanOccur(pawn) && (curStage.allowedMentalBreakIntensities == null || curStage.allowedMentalBreakIntensities.Contains(x.intensity))).TryRandomElementByWeight((MentalBreakDef x) => x.Worker.CommonalityFor(pawn), out var result))
			{
				TaggedString taggedString = "MentalStateReason_Hediff".Translate(Label);
				if (!curStage.mentalBreakExplanation.NullOrEmpty())
				{
					taggedString += "\n\n" + curStage.mentalBreakExplanation.Formatted(pawn.Named("PAWN"));
				}
				result.Worker.TryStart(pawn, taggedString.Resolve(), causedByMood: false);
			}
		}

		public virtual void PostMake()
		{
			Severity = Mathf.Max(Severity, def.initialSeverity);
			causesNoPain = Rand.Value < def.chanceToCauseNoPain;
		}

		public virtual void PostAdd(DamageInfo? dinfo)
		{
			if (!def.disablesNeeds.NullOrEmpty())
			{
				pawn.needs.AddOrRemoveNeedsAsAppropriate();
			}
			if (def.removeWithTags.NullOrEmpty())
			{
				return;
			}
			for (int num = pawn.health.hediffSet.hediffs.Count - 1; num >= 0; num--)
			{
				Hediff hediff = pawn.health.hediffSet.hediffs[num];
				if (hediff != this && !hediff.def.tags.NullOrEmpty())
				{
					for (int i = 0; i < def.removeWithTags.Count; i++)
					{
						if (hediff.def.tags.Contains(def.removeWithTags[i]))
						{
							pawn.health.RemoveHediff(hediff);
							break;
						}
					}
				}
			}
		}

		public virtual void PreRemoved()
		{
		}

		public virtual void PostRemoved()
		{
			if ((def.causesNeed != null || !def.disablesNeeds.NullOrEmpty()) && !pawn.Dead)
			{
				pawn.needs.AddOrRemoveNeedsAsAppropriate();
			}
		}

		public virtual void PostTick()
		{
		}

		public virtual void Tended(float quality, float maxQuality, int batchPosition = 0)
		{
		}

		public virtual void Heal(float amount)
		{
			if (!(amount <= 0f))
			{
				Severity -= amount;
				pawn.health.Notify_HediffChanged(this);
			}
		}

		public virtual void ModifyChemicalEffect(ChemicalDef chem, ref float effect)
		{
		}

		public virtual bool TryMergeWith(Hediff other)
		{
			if (other == null || other.def != def || other.Part != Part)
			{
				return false;
			}
			Severity += other.Severity;
			ageTicks = 0;
			return true;
		}

		public virtual bool CauseDeathNow()
		{
			if (def.lethalSeverity >= 0f)
			{
				bool flag = Severity >= def.lethalSeverity;
				if (flag && DebugViewSettings.logCauseOfDeath)
				{
					Log.Message("CauseOfDeath: lethal severity exceeded " + Severity + " >= " + def.lethalSeverity);
				}
				return flag;
			}
			return false;
		}

		public virtual void Notify_PawnDied()
		{
		}

		public virtual void Notify_PawnKilled()
		{
		}

		public virtual void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
		}

		public virtual void Notify_PawnUsedVerb(Verb verb, LocalTargetInfo targets)
		{
		}

		public virtual void Notify_EntropyGained(float baseAmount, float finalAmount, Thing source = null)
		{
		}

		public virtual void Notify_RelationAdded(Pawn otherPawn, PawnRelationDef relationDef)
		{
		}

		public virtual void Notify_ImplantUsed(string violationSourceName, float detectionChance, int violationSourceLevel = -1)
		{
		}

		public virtual void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
		{
		}

		public virtual void Notify_IngestedThing(Thing thing, int amount)
		{
		}

		public virtual void Notify_Resurrected()
		{
		}

		public virtual IEnumerable<Gizmo> GetGizmos()
		{
			return null;
		}

		public virtual string GetTooltip(Pawn pawn, bool showHediffsDebugInfo)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (string.IsNullOrWhiteSpace(def.overrideTooltip))
			{
				if (string.IsNullOrWhiteSpace(CurStage?.overrideTooltip))
				{
					string severityLabel = SeverityLabel;
					bool flag = showHediffsDebugInfo && !DebugString().NullOrEmpty();
					string description = Description;
					if (!Label.NullOrEmpty() || !severityLabel.NullOrEmpty() || !CapMods.NullOrEmpty() || flag || !description.NullOrEmpty())
					{
						stringBuilder.AppendTagged(LabelCap.Colorize(ColoredText.TipSectionTitleColor));
						if (!severityLabel.NullOrEmpty())
						{
							stringBuilder.Append(": " + severityLabel);
						}
						if (!description.NullOrEmpty())
						{
							stringBuilder.AppendLine();
							stringBuilder.AppendInNewLine(description);
						}
						stringBuilder.AppendLine();
						string tipStringExtra = TipStringExtra;
						if (!tipStringExtra.NullOrEmpty())
						{
							stringBuilder.AppendLine();
							stringBuilder.AppendLine(tipStringExtra.TrimEndNewlines());
						}
						if (flag)
						{
							stringBuilder.AppendLine();
							stringBuilder.AppendLine(DebugString().TrimEndNewlines());
						}
					}
					string text = Cause();
					if (text != null)
					{
						stringBuilder.AppendLine().AppendTagged(("Cause".Translate() + ": " + text).Colorize(ColoredText.SubtleGrayColor));
					}
				}
				else
				{
					stringBuilder.Append(CurStage.overrideTooltip.Formatted(pawn.Named("PAWN"), TipStringExtra.Named("TipStringExtra"), Cause().Named("CAUSE")));
				}
			}
			else
			{
				stringBuilder.Append(def.overrideTooltip.Formatted(pawn.Named("PAWN"), TipStringExtra.Named("TipStringExtra"), Cause().Named("CAUSE")));
			}
			if (!string.IsNullOrWhiteSpace(def.extraTooltip))
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(def.extraTooltip.Formatted(pawn.Named("PAWN"), TipStringExtra.Named("TipStringExtra"), Cause().Named("CAUSE")));
			}
			if (!string.IsNullOrWhiteSpace(CurStage?.extraTooltip))
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(CurStage.extraTooltip.Formatted(pawn.Named("PAWN"), TipStringExtra.Named("TipStringExtra"), Cause().Named("CAUSE")));
			}
			return stringBuilder.ToString().TrimEnd();
			string Cause()
			{
				if (HealthCardUtility.GetCombatLogInfo(Gen.YieldSingle(this), out var taggedString, out var _))
				{
					return taggedString.Resolve();
				}
				return null;
			}
		}

		public virtual void PostDebugAdd()
		{
		}

		public virtual string DebugString()
		{
			string text = "";
			if (!Visible)
			{
				text += "hidden\n";
			}
			text = text + "severity: " + Severity.ToString("F3") + ((Severity >= def.maxSeverity) ? " (reached max)" : "");
			if (TendableNow())
			{
				text = text + "\ntend priority: " + TendPriority;
			}
			return text.Indented();
		}

		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			foreach (StatDrawEntry item in def.SpecialDisplayStats(req))
			{
				yield return item;
			}
		}

		public override string ToString()
		{
			return "(" + (def?.defName ?? GetType().Name) + ((part != null) ? (" " + part.Label) : "") + " ticksSinceCreation=" + ageTicks + ")";
		}

		public string GetUniqueLoadID()
		{
			return "Hediff_" + loadID;
		}
	}
}
