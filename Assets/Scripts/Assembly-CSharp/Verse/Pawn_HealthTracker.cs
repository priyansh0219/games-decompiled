using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

namespace Verse
{
	public class Pawn_HealthTracker : IExposable
	{
		private Pawn pawn;

		private PawnHealthState healthState = PawnHealthState.Mobile;

		[Unsaved(false)]
		public Effecter woundedEffecter;

		[Unsaved(false)]
		public Effecter deflectionEffecter;

		[LoadAlias("forceIncap")]
		public bool forceDowned;

		public bool beCarriedByCaravanIfSick;

		public bool killedByRitual;

		public int lastReceivedNeuralSuperchargeTick = -1;

		public HediffSet hediffSet;

		public PawnCapacitiesHandler capacities;

		public BillStack surgeryBills;

		public SummaryHealthHandler summaryHealth;

		public ImmunityHandler immunity;

		private List<Hediff_Injury> tmpMechInjuries = new List<Hediff_Injury>();

		private List<Hediff_Injury> tmpHediffInjuries = new List<Hediff_Injury>();

		public PawnHealthState State => healthState;

		public bool Downed => healthState == PawnHealthState.Down;

		public bool Dead => healthState == PawnHealthState.Dead;

		public float LethalDamageThreshold => 150f * pawn.HealthScale;

		public bool InPainShock => hediffSet.PainTotal >= pawn.GetStatValue(StatDefOf.PainShockThreshold);

		public Pawn_HealthTracker(Pawn pawn)
		{
			this.pawn = pawn;
			hediffSet = new HediffSet(pawn);
			capacities = new PawnCapacitiesHandler(pawn);
			summaryHealth = new SummaryHealthHandler(pawn);
			surgeryBills = new BillStack(pawn);
			immunity = new ImmunityHandler(pawn);
			beCarriedByCaravanIfSick = pawn.RaceProps.Humanlike;
		}

		public void Reset()
		{
			healthState = PawnHealthState.Mobile;
			hediffSet.Clear();
			capacities.Clear();
			summaryHealth.Notify_HealthChanged();
			surgeryBills.Clear();
			immunity = new ImmunityHandler(pawn);
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref healthState, "healthState", PawnHealthState.Mobile);
			Scribe_Values.Look(ref forceDowned, "forceDowned", defaultValue: false);
			Scribe_Values.Look(ref beCarriedByCaravanIfSick, "beCarriedByCaravanIfSick", defaultValue: true);
			Scribe_Values.Look(ref killedByRitual, "killedByRitual", defaultValue: false);
			Scribe_Values.Look(ref lastReceivedNeuralSuperchargeTick, "lastReceivedNeuralSuperchargeTick", -1);
			Scribe_Deep.Look(ref hediffSet, "hediffSet", pawn);
			Scribe_Deep.Look(ref surgeryBills, "surgeryBills", pawn);
			Scribe_Deep.Look(ref immunity, "immunity", pawn);
		}

		public Hediff AddHediff(HediffDef def, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
		{
			Hediff hediff = HediffMaker.MakeHediff(def, pawn);
			AddHediff(hediff, part, dinfo, result);
			return hediff;
		}

		public void AddHediff(Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
		{
			if (part != null)
			{
				hediff.Part = part;
			}
			hediffSet.AddDirect(hediff, dinfo, result);
			CheckForStateChange(dinfo, hediff);
			if (pawn.RaceProps.hediffGiverSets == null)
			{
				return;
			}
			for (int i = 0; i < pawn.RaceProps.hediffGiverSets.Count; i++)
			{
				HediffGiverSetDef hediffGiverSetDef = pawn.RaceProps.hediffGiverSets[i];
				for (int j = 0; j < hediffGiverSetDef.hediffGivers.Count; j++)
				{
					hediffGiverSetDef.hediffGivers[j].OnHediffAdded(pawn, hediff);
				}
			}
		}

		public void RemoveHediff(Hediff hediff)
		{
			hediff.PreRemoved();
			hediffSet.hediffs.Remove(hediff);
			hediff.PostRemoved();
			Notify_HediffChanged(hediff);
		}

		public void RemoveAllHediffs()
		{
			for (int num = hediffSet.hediffs.Count - 1; num >= 0; num--)
			{
				RemoveHediff(hediffSet.hediffs[num]);
			}
		}

		public void Notify_HediffChanged(Hediff hediff)
		{
			hediffSet.DirtyCache();
			CheckForStateChange(null, hediff);
		}

		public void Notify_UsedVerb(Verb verb, LocalTargetInfo target)
		{
			foreach (Hediff hediff in hediffSet.hediffs)
			{
				hediff.Notify_PawnUsedVerb(verb, target);
			}
		}

		public void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			Faction homeFaction = this.pawn.HomeFaction;
			if (dinfo.Instigator != null && homeFaction != null && homeFaction.IsPlayer && !this.pawn.InAggroMentalState && !dinfo.Def.consideredHelpful)
			{
				Pawn pawn = dinfo.Instigator as Pawn;
				if (dinfo.InstigatorGuilty && pawn != null && pawn.guilt != null && pawn.mindState != null)
				{
					pawn.guilt.Notify_Guilty();
				}
			}
			if (this.pawn.Spawned)
			{
				if (!this.pawn.Position.Fogged(this.pawn.Map))
				{
					this.pawn.mindState.Active = true;
				}
				this.pawn.GetLord()?.Notify_PawnDamaged(this.pawn, dinfo);
				if (dinfo.Def.ExternalViolenceFor(this.pawn))
				{
					GenClamor.DoClamor(this.pawn, 18f, ClamorDefOf.Harm);
				}
				this.pawn.jobs.Notify_DamageTaken(dinfo);
			}
			if (homeFaction != null)
			{
				homeFaction.Notify_MemberTookDamage(this.pawn, dinfo);
				if (Current.ProgramState == ProgramState.Playing && homeFaction == Faction.OfPlayer && dinfo.Def.ExternalViolenceFor(this.pawn) && this.pawn.SpawnedOrAnyParentSpawned)
				{
					this.pawn.MapHeld.dangerWatcher.Notify_ColonistHarmedExternally();
				}
			}
			if (this.pawn.apparel != null && !dinfo.IgnoreArmor)
			{
				List<Apparel> wornApparel = this.pawn.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					if (wornApparel[i].CheckPreAbsorbDamage(dinfo))
					{
						absorbed = true;
						return;
					}
				}
			}
			if (this.pawn.Spawned)
			{
				this.pawn.stances.Notify_DamageTaken(dinfo);
				this.pawn.stances.stunner.Notify_DamageApplied(dinfo);
			}
			if (this.pawn.RaceProps.IsFlesh && dinfo.Def.ExternalViolenceFor(this.pawn))
			{
				Pawn pawn2 = dinfo.Instigator as Pawn;
				if (pawn2 != null)
				{
					if (pawn2.HostileTo(this.pawn))
					{
						this.pawn.relations.canGetRescuedThought = true;
					}
					if (this.pawn.RaceProps.Humanlike && pawn2.RaceProps.Humanlike && this.pawn.needs.mood != null && (!pawn2.HostileTo(this.pawn) || (pawn2.Faction == homeFaction && pawn2.InMentalState)))
					{
						this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.HarmedMe, pawn2);
					}
				}
				ThingDef thingDef = ((pawn2 != null && dinfo.Weapon != pawn2.def) ? dinfo.Weapon : null);
				TaleRecorder.RecordTale(TaleDefOf.Wounded, this.pawn, pawn2, thingDef);
			}
			absorbed = false;
		}

		public void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (ShouldBeDead())
			{
				if (!ShouldBeDeathrestingOrInComa())
				{
					if (!this.pawn.Destroyed)
					{
						this.pawn.Kill(dinfo);
					}
					return;
				}
				ForceDeathrestOrComa(dinfo, null);
			}
			if (dinfo.Def.additionalHediffs != null && (dinfo.Def.applyAdditionalHediffsIfHuntingForFood || !(dinfo.Instigator is Pawn pawn) || pawn.CurJob == null || pawn.CurJob.def != JobDefOf.PredatorHunt))
			{
				List<DamageDefAdditionalHediff> additionalHediffs = dinfo.Def.additionalHediffs;
				for (int i = 0; i < additionalHediffs.Count; i++)
				{
					DamageDefAdditionalHediff damageDefAdditionalHediff = additionalHediffs[i];
					if (damageDefAdditionalHediff.hediff == null)
					{
						continue;
					}
					float num = ((damageDefAdditionalHediff.severityFixed <= 0f) ? (totalDamageDealt * damageDefAdditionalHediff.severityPerDamageDealt) : damageDefAdditionalHediff.severityFixed);
					if (damageDefAdditionalHediff.victimSeverityScalingByInvBodySize)
					{
						num *= 1f / this.pawn.BodySize;
					}
					if (damageDefAdditionalHediff.victimSeverityScaling != null)
					{
						num *= (damageDefAdditionalHediff.inverseStatScaling ? Mathf.Max(1f - this.pawn.GetStatValue(damageDefAdditionalHediff.victimSeverityScaling), 0f) : this.pawn.GetStatValue(damageDefAdditionalHediff.victimSeverityScaling));
					}
					if (num >= 0f)
					{
						Hediff hediff = HediffMaker.MakeHediff(damageDefAdditionalHediff.hediff, this.pawn);
						hediff.Severity = num;
						AddHediff(hediff, null, dinfo);
						if (Dead)
						{
							return;
						}
					}
				}
			}
			for (int j = 0; j < hediffSet.hediffs.Count; j++)
			{
				hediffSet.hediffs[j].Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
			}
		}

		public void RestorePart(BodyPartRecord part, Hediff diffException = null, bool checkStateChange = true)
		{
			if (part == null)
			{
				Log.Error("Tried to restore null body part.");
				return;
			}
			RestorePartRecursiveInt(part, diffException);
			hediffSet.DirtyCache();
			if (checkStateChange)
			{
				CheckForStateChange(null, null);
			}
		}

		private void RestorePartRecursiveInt(BodyPartRecord part, Hediff diffException = null)
		{
			List<Hediff> hediffs = hediffSet.hediffs;
			for (int num = hediffs.Count - 1; num >= 0; num--)
			{
				Hediff hediff = hediffs[num];
				if (hediff.Part == part && hediff != diffException && !hediff.def.keepOnBodyPartRestoration)
				{
					hediffs.RemoveAt(num);
					hediff.PostRemoved();
				}
			}
			for (int i = 0; i < part.parts.Count; i++)
			{
				RestorePartRecursiveInt(part.parts[i], diffException);
			}
		}

		public void CheckForStateChange(DamageInfo? dinfo, Hediff hediff)
		{
			if (Dead)
			{
				return;
			}
			if (ModsConfig.BiotechActive && pawn.mechanitor != null)
			{
				pawn.mechanitor.Notify_HediffStateChange(hediff);
			}
			if (hediff != null && hediff.def.blocksSleeping && !pawn.Awake())
			{
				RestUtility.WakeUp(pawn);
			}
			else if (ShouldBeDead())
			{
				if (ShouldBeDeathrestingOrInComa())
				{
					ForceDeathrestOrComa(dinfo, hediff);
				}
				else if (!pawn.Destroyed)
				{
					pawn.Kill(dinfo, hediff);
				}
			}
			else if (!Downed)
			{
				if (ShouldBeDowned())
				{
					if (!forceDowned && dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(pawn) && !pawn.IsWildMan() && (pawn.Faction == null || !pawn.Faction.IsPlayer) && (pawn.HostFaction == null || !pawn.HostFaction.IsPlayer))
					{
						bool flag = ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.HasGene(GeneDefOf.Deathless);
						float num = ((flag && pawn.Faction == Faction.OfPlayer) ? 0f : (pawn.RaceProps.Animal ? 0.5f : ((!pawn.RaceProps.IsMechanoid) ? ((Find.Storyteller.difficulty.unwaveringPrisoners ? HealthTuning.DeathOnDownedChance_NonColonyHumanlikeFromPopulationIntentCurve : HealthTuning.DeathOnDownedChance_NonColonyHumanlikeFromPopulationIntentCurve_WaveringPrisoners).Evaluate(StorytellerUtilityPopulation.PopulationIntent) * Find.Storyteller.difficulty.enemyDeathOnDownedChanceFactor) : 1f)));
						if (Rand.Chance(num))
						{
							if (DebugViewSettings.logCauseOfDeath)
							{
								Log.Message("CauseOfDeath: chance on downed " + num.ToStringPercent());
							}
							if (flag && !pawn.Dead)
							{
								pawn.health.AddHediff(HediffDefOf.MissingBodyPart, pawn.health.hediffSet.GetBrain(), dinfo);
							}
							else
							{
								pawn.Kill(dinfo);
							}
							return;
						}
					}
					forceDowned = false;
					MakeDowned(dinfo, hediff);
				}
				else
				{
					if (capacities.CapableOf(PawnCapacityDefOf.Manipulation))
					{
						return;
					}
					if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null && pawn.jobs != null && pawn.CurJob != null)
					{
						pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
					if (pawn.equipment == null || pawn.equipment.Primary == null)
					{
						return;
					}
					if (pawn.kindDef.destroyGearOnDrop)
					{
						pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
					}
					else if (pawn.InContainerEnclosed)
					{
						pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.holdingOwner);
					}
					else if (pawn.SpawnedOrAnyParentSpawned)
					{
						pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out var _, pawn.PositionHeld);
					}
					else if (pawn.IsCaravanMember())
					{
						ThingWithComps primary = pawn.equipment.Primary;
						pawn.equipment.Remove(primary);
						if (!pawn.inventory.innerContainer.TryAdd(primary))
						{
							primary.Destroy();
						}
					}
					else
					{
						pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
					}
				}
			}
			else if (!ShouldBeDowned())
			{
				MakeUndowned(hediff);
			}
		}

		private bool ShouldBeDeathrestingOrInComa()
		{
			if (!ModsConfig.BiotechActive)
			{
				return false;
			}
			if (!SanguophageUtility.ShouldBeDeathrestingOrInComaInsteadOfDead(pawn))
			{
				return false;
			}
			return true;
		}

		private bool ShouldBeDowned()
		{
			if (!InPainShock && capacities.CanBeAwake && capacities.CapableOf(PawnCapacityDefOf.Moving))
			{
				return pawn.ageTracker.CurLifeStage.alwaysDowned;
			}
			return true;
		}

		public bool ShouldBeDead()
		{
			if (Dead)
			{
				return true;
			}
			for (int i = 0; i < hediffSet.hediffs.Count; i++)
			{
				if (hediffSet.hediffs[i].CauseDeathNow())
				{
					return true;
				}
			}
			if (ShouldBeDeadFromRequiredCapacity() != null)
			{
				return true;
			}
			if (PawnCapacityUtility.CalculatePartEfficiency(hediffSet, pawn.RaceProps.body.corePart) <= 0.0001f)
			{
				if (DebugViewSettings.logCauseOfDeath)
				{
					Log.Message("CauseOfDeath: zero efficiency of " + pawn.RaceProps.body.corePart.Label);
				}
				return true;
			}
			if (ShouldBeDeadFromLethalDamageThreshold())
			{
				return true;
			}
			return false;
		}

		public PawnCapacityDef ShouldBeDeadFromRequiredCapacity()
		{
			List<PawnCapacityDef> allDefsListForReading = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				PawnCapacityDef pawnCapacityDef = allDefsListForReading[i];
				if ((pawn.RaceProps.IsFlesh ? pawnCapacityDef.lethalFlesh : pawnCapacityDef.lethalMechanoids) && !capacities.CapableOf(pawnCapacityDef))
				{
					if (DebugViewSettings.logCauseOfDeath)
					{
						Log.Message("CauseOfDeath: no longer capable of " + pawnCapacityDef.defName);
					}
					return pawnCapacityDef;
				}
			}
			return null;
		}

		public bool ShouldBeDeadFromLethalDamageThreshold()
		{
			float num = 0f;
			for (int i = 0; i < hediffSet.hediffs.Count; i++)
			{
				if (hediffSet.hediffs[i] is Hediff_Injury)
				{
					num += hediffSet.hediffs[i].Severity;
				}
			}
			bool flag = num >= LethalDamageThreshold;
			if (flag && DebugViewSettings.logCauseOfDeath)
			{
				Log.Message("CauseOfDeath: lethal damage " + num + " >= " + LethalDamageThreshold);
			}
			return flag;
		}

		public bool WouldLosePartAfterAddingHediff(HediffDef def, BodyPartRecord part, float severity)
		{
			Hediff hediff = HediffMaker.MakeHediff(def, pawn, part);
			hediff.Severity = severity;
			return CheckPredicateAfterAddingHediff(hediff, () => hediffSet.PartIsMissing(part));
		}

		public bool WouldDieAfterAddingHediff(Hediff hediff)
		{
			if (Dead)
			{
				return true;
			}
			bool num = CheckPredicateAfterAddingHediff(hediff, ShouldBeDead);
			if (num && DebugViewSettings.logCauseOfDeath)
			{
				Log.Message("CauseOfDeath: WouldDieAfterAddingHediff=true for " + pawn.Name);
			}
			return num;
		}

		public bool WouldDieAfterAddingHediff(HediffDef def, BodyPartRecord part, float severity)
		{
			Hediff hediff = HediffMaker.MakeHediff(def, pawn, part);
			hediff.Severity = severity;
			return WouldDieAfterAddingHediff(hediff);
		}

		public bool WouldBeDownedAfterAddingHediff(Hediff hediff)
		{
			if (Dead)
			{
				return false;
			}
			return CheckPredicateAfterAddingHediff(hediff, ShouldBeDowned);
		}

		public bool WouldBeDownedAfterAddingHediff(HediffDef def, BodyPartRecord part, float severity)
		{
			Hediff hediff = HediffMaker.MakeHediff(def, pawn, part);
			hediff.Severity = severity;
			return WouldBeDownedAfterAddingHediff(hediff);
		}

		public void SetDead()
		{
			if (Dead)
			{
				Log.Error(string.Concat(pawn, " set dead while already dead."));
			}
			healthState = PawnHealthState.Dead;
		}

		private bool CheckPredicateAfterAddingHediff(Hediff hediff, Func<bool> pred)
		{
			HashSet<Hediff> missing = CalculateMissingPartHediffsFromInjury(hediff);
			hediffSet.hediffs.Add(hediff);
			if (missing != null)
			{
				hediffSet.hediffs.AddRange(missing);
			}
			hediffSet.DirtyCache();
			bool result = pred();
			if (missing != null)
			{
				hediffSet.hediffs.RemoveAll((Hediff x) => missing.Contains(x));
			}
			hediffSet.hediffs.Remove(hediff);
			hediffSet.DirtyCache();
			return result;
		}

		private HashSet<Hediff> CalculateMissingPartHediffsFromInjury(Hediff hediff)
		{
			HashSet<Hediff> missing = null;
			if (hediff.Part != null && hediff.Part != pawn.RaceProps.body.corePart && hediff.Severity >= hediffSet.GetPartHealth(hediff.Part))
			{
				missing = new HashSet<Hediff>();
				AddAllParts(hediff.Part);
			}
			return missing;
			void AddAllParts(BodyPartRecord part)
			{
				Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn);
				hediff_MissingPart.lastInjury = hediff.def;
				hediff_MissingPart.Part = part;
				missing.Add(hediff_MissingPart);
				foreach (BodyPartRecord part in part.parts)
				{
					AddAllParts(part);
				}
			}
		}

		private void ForceDeathrestOrComa(DamageInfo? dinfo, Hediff hediff)
		{
			if (pawn.CanDeathrest())
			{
				if (SanguophageUtility.TryStartDeathrest(pawn, DeathrestStartReason.LethalDamage))
				{
					GeneUtility.OffsetHemogen(pawn, -9999f);
				}
			}
			else
			{
				SanguophageUtility.TryStartRegenComa(pawn, DeathrestStartReason.LethalDamage);
			}
			if (!Downed)
			{
				forceDowned = true;
				MakeDowned(dinfo, hediff);
			}
		}

		private void MakeDowned(DamageInfo? dinfo, Hediff hediff)
		{
			if (Downed)
			{
				Log.Error(string.Concat(pawn, " tried to do MakeDowned while already downed."));
				return;
			}
			if (pawn.guilt != null && pawn.GetLord() != null && pawn.GetLord().LordJob != null && pawn.GetLord().LordJob.GuiltyOnDowned)
			{
				pawn.guilt.Notify_Guilty();
			}
			healthState = PawnHealthState.Down;
			PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(pawn, dinfo, PawnDiedOrDownedThoughtsKind.Downed);
			if (pawn.InMentalState && pawn.MentalStateDef.recoverFromDowned)
			{
				pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
			}
			pawn.mindState.droppedWeapon = null;
			if (pawn.Spawned)
			{
				pawn.DropAndForbidEverything(keepInventoryAndEquipmentIfInBed: true, rememberPrimary: true);
				pawn.stances.CancelBusyStanceSoft();
			}
			if (!pawn.DutyActiveWhenDown(onlyInBed: true))
			{
				pawn.ClearMind(ifLayingKeepLaying: true, clearInspiration: false, clearMentalState: false);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.Incapped, dinfo);
			}
			if (pawn.Drafted)
			{
				pawn.drafter.Drafted = false;
			}
			PortraitsCache.SetDirty(pawn);
			GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
			if (pawn.SpawnedOrAnyParentSpawned)
			{
				GenHostility.Notify_PawnLostForTutor(pawn, pawn.MapHeld);
			}
			if (pawn.RaceProps.Humanlike && Current.ProgramState == ProgramState.Playing && pawn.SpawnedOrAnyParentSpawned)
			{
				if (pawn.HostileTo(Faction.OfPlayer))
				{
					LessonAutoActivator.TeachOpportunity(ConceptDefOf.Capturing, pawn, OpportunityType.Important);
				}
				if (pawn.Faction == Faction.OfPlayer)
				{
					LessonAutoActivator.TeachOpportunity(ConceptDefOf.Rescuing, pawn, OpportunityType.Critical);
				}
			}
			if (dinfo.HasValue && dinfo.Value.Instigator != null && dinfo.Value.Instigator is Pawn instigator)
			{
				RecordsUtility.Notify_PawnDowned(pawn, instigator);
			}
			if (pawn.Spawned && (hediff == null || hediff.def.recordDownedTale))
			{
				TaleRecorder.RecordTale(TaleDefOf.Downed, pawn, dinfo.HasValue ? (dinfo.Value.Instigator as Pawn) : null, dinfo.HasValue ? dinfo.Value.Weapon : null);
				Find.BattleLog.Add(new BattleLogEntry_StateTransition(pawn, RulePackDefOf.Transition_Downed, dinfo.HasValue ? (dinfo.Value.Instigator as Pawn) : null, hediff, dinfo.HasValue ? dinfo.Value.HitPart : null));
			}
			Find.Storyteller.Notify_PawnEvent(pawn, AdaptationEvent.Downed, dinfo);
			pawn.mechanitor?.Notify_Downed();
		}

		private void MakeUndowned(Hediff hediff)
		{
			if (!Downed)
			{
				Log.Error(string.Concat(pawn, " tried to do MakeUndowned when already undowned."));
				return;
			}
			healthState = PawnHealthState.Mobile;
			if (PawnUtility.ShouldSendNotificationAbout(pawn) && (hediff == null || hediff.def != HediffDefOf.Deathrest))
			{
				Messages.Message("MessageNoLongerDowned".Translate(pawn.LabelCap, pawn), pawn, MessageTypeDefOf.PositiveEvent);
			}
			PortraitsCache.SetDirty(pawn);
			GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
			if (pawn.guest != null)
			{
				pawn.guest.Notify_PawnUndowned();
			}
		}

		public void NotifyPlayerOfKilled(DamageInfo? dinfo, Hediff hediff, Caravan caravan)
		{
			TaggedString taggedString = "";
			taggedString = (dinfo.HasValue ? dinfo.Value.Def.deathMessage.Formatted(pawn.LabelShortCap, pawn.Named("PAWN")) : ((hediff == null) ? "PawnDied".Translate(pawn.LabelShortCap, pawn.Named("PAWN")) : "PawnDiedBecauseOf".Translate(pawn.LabelShortCap, hediff.def.LabelCap, pawn.Named("PAWN"))));
			Quest quest = null;
			if (pawn.IsBorrowedByAnyFaction())
			{
				foreach (QuestPart_LendColonistsToFaction item in QuestUtility.GetAllQuestPartsOfType<QuestPart_LendColonistsToFaction>())
				{
					if (item.LentColonistsListForReading.Contains(pawn))
					{
						taggedString += "\n\n" + "LentColonistDied".Translate(pawn.Named("PAWN"), item.lendColonistsToFaction.Named("FACTION"));
						quest = item.quest;
						break;
					}
				}
			}
			taggedString = taggedString.AdjustedFor(pawn);
			if (pawn.Faction == Faction.OfPlayer)
			{
				TaggedString label = "Death".Translate() + ": " + pawn.LabelShortCap;
				if (caravan != null)
				{
					Messages.Message("MessageCaravanDeathCorpseAddedToInventory".Translate(pawn.Named("PAWN")), caravan, MessageTypeDefOf.PawnDeath);
				}
				if (pawn.Ideo != null)
				{
					foreach (Precept item2 in pawn.Ideo.PreceptsListForReading)
					{
						if (!string.IsNullOrWhiteSpace(item2.def.extraTextPawnDeathLetter))
						{
							taggedString += "\n\n" + item2.def.extraTextPawnDeathLetter.Formatted(pawn.Named("PAWN"));
						}
					}
				}
				if (pawn.Name != null && !pawn.Name.Numerical && pawn.RaceProps.Animal)
				{
					label += " (" + pawn.KindLabel + ")";
				}
				pawn.relations?.CheckAppendBondedAnimalDiedInfo(ref taggedString, ref label);
				Find.LetterStack.ReceiveLetter(label, taggedString, LetterDefOf.Death, pawn, null, quest);
			}
			else
			{
				Messages.Message(taggedString, pawn, MessageTypeDefOf.PawnDeath);
			}
		}

		public void Notify_Resurrected()
		{
			healthState = PawnHealthState.Mobile;
			hediffSet.hediffs.RemoveAll((Hediff x) => x.def.everCurableByItem && x.TryGetComp<HediffComp_Immunizable>() != null);
			hediffSet.hediffs.RemoveAll((Hediff x) => x.def.everCurableByItem && (x.def.lethalSeverity >= 0f || (x.def.stages != null && x.def.stages.Any((HediffStage y) => y.lifeThreatening))));
			if (!pawn.RaceProps.IsMechanoid)
			{
				hediffSet.hediffs.RemoveAll((Hediff x) => x.def.everCurableByItem && x is Hediff_Injury && !x.IsPermanent());
			}
			else
			{
				tmpMechInjuries.Clear();
				hediffSet.GetHediffs(ref tmpMechInjuries, (Hediff_Injury x) => x != null && x.def.everCurableByItem && !x.IsPermanent());
				if (tmpMechInjuries.Count > 0)
				{
					float num = tmpMechInjuries.Sum((Hediff_Injury x) => x.Severity) * 0.5f / (float)tmpMechInjuries.Count;
					for (int i = 0; i < tmpMechInjuries.Count; i++)
					{
						tmpMechInjuries[i].Severity -= num;
					}
					tmpMechInjuries.Clear();
				}
			}
			hediffSet.hediffs.RemoveAll((Hediff x) => x.def.everCurableByItem && x is Hediff_Injury && x.IsPermanent() && hediffSet.GetPartHealth(x.Part) <= 0f);
			while (true)
			{
				Hediff_MissingPart hediff_MissingPart = (from x in hediffSet.GetMissingPartsCommonAncestors()
					where !hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(x.Part)
					select x).FirstOrDefault();
				if (hediff_MissingPart == null)
				{
					break;
				}
				RestorePart(hediff_MissingPart.Part, null, checkStateChange: false);
			}
			for (int num2 = hediffSet.hediffs.Count - 1; num2 >= 0; num2--)
			{
				hediffSet.hediffs[num2].Notify_Resurrected();
			}
			hediffSet.DirtyCache();
			if (ShouldBeDead())
			{
				hediffSet.hediffs.RemoveAll((Hediff h) => !h.def.keepOnBodyPartRestoration);
			}
			Notify_HediffChanged(null);
		}

		public void HealthTick()
		{
			if (Dead)
			{
				return;
			}
			for (int num = hediffSet.hediffs.Count - 1; num >= 0; num--)
			{
				Hediff hediff = hediffSet.hediffs[num];
				try
				{
					hediff.Tick();
					hediff.PostTick();
				}
				catch (Exception ex)
				{
					Log.Error("Exception ticking hediff " + hediff.ToStringSafe() + " for pawn " + pawn.ToStringSafe() + ". Removing hediff... Exception: " + ex);
					try
					{
						RemoveHediff(hediff);
					}
					catch (Exception ex2)
					{
						Log.Error("Error while removing hediff: " + ex2);
					}
				}
				if (Dead)
				{
					return;
				}
			}
			bool flag = false;
			for (int num2 = hediffSet.hediffs.Count - 1; num2 >= 0; num2--)
			{
				Hediff hediff2 = hediffSet.hediffs[num2];
				if (hediff2.ShouldRemove)
				{
					hediff2.PreRemoved();
					hediffSet.hediffs.RemoveAt(num2);
					hediff2.PostRemoved();
					flag = true;
				}
			}
			if (flag)
			{
				Notify_HediffChanged(null);
			}
			if (Dead)
			{
				return;
			}
			immunity.ImmunityHandlerTick();
			if (pawn.RaceProps.IsFlesh && pawn.IsHashIntervalTick(600) && (pawn.needs.food == null || !pawn.needs.food.Starving))
			{
				bool flag2 = false;
				if (hediffSet.HasNaturallyHealingInjury())
				{
					float num3 = 8f;
					if (pawn.GetPosture() != 0)
					{
						num3 += 4f;
						Building_Bed building_Bed = pawn.CurrentBed();
						if (building_Bed != null)
						{
							num3 += building_Bed.def.building.bed_healPerDay;
						}
					}
					foreach (Hediff hediff3 in hediffSet.hediffs)
					{
						HediffStage curStage = hediff3.CurStage;
						if (curStage != null && curStage.naturalHealingFactor != -1f)
						{
							num3 *= curStage.naturalHealingFactor;
						}
					}
					hediffSet.GetHediffs(ref tmpHediffInjuries, (Hediff_Injury h) => h.CanHealNaturally());
					tmpHediffInjuries.RandomElement().Heal(num3 * pawn.HealthScale * 0.01f * pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
					flag2 = true;
				}
				if (hediffSet.HasTendedAndHealingInjury())
				{
					Need_Food food = pawn.needs.food;
					if (food == null || !food.Starving)
					{
						hediffSet.GetHediffs(ref tmpHediffInjuries, (Hediff_Injury h) => h.CanHealFromTending());
						Hediff_Injury hediff_Injury = tmpHediffInjuries.RandomElement();
						float tendQuality = hediff_Injury.TryGetComp<HediffComp_TendDuration>().tendQuality;
						float num4 = GenMath.LerpDouble(0f, 1f, 0.5f, 1.5f, Mathf.Clamp01(tendQuality));
						hediff_Injury.Heal(8f * num4 * pawn.HealthScale * 0.01f * pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
						flag2 = true;
					}
				}
				if (flag2 && !HasHediffsNeedingTendByPlayer() && !HealthAIUtility.ShouldSeekMedicalRest(pawn) && PawnUtility.ShouldSendNotificationAbout(pawn))
				{
					Messages.Message("MessageFullyHealed".Translate(pawn.LabelCap, pawn), pawn, MessageTypeDefOf.PositiveEvent);
				}
			}
			if (pawn.RaceProps.IsFlesh && hediffSet.BleedRateTotal >= 0.1f)
			{
				float num5 = hediffSet.BleedRateTotal * pawn.BodySize;
				num5 = ((pawn.GetPosture() != 0) ? (num5 * 0.0004f) : (num5 * 0.004f));
				if (Rand.Value < num5)
				{
					DropBloodFilth();
				}
			}
			if (!pawn.IsHashIntervalTick(60))
			{
				return;
			}
			List<HediffGiverSetDef> hediffGiverSets = pawn.RaceProps.hediffGiverSets;
			if (hediffGiverSets != null)
			{
				for (int i = 0; i < hediffGiverSets.Count; i++)
				{
					List<HediffGiver> hediffGivers = hediffGiverSets[i].hediffGivers;
					for (int j = 0; j < hediffGivers.Count; j++)
					{
						hediffGivers[j].OnIntervalPassed(pawn, null);
						if (pawn.Dead)
						{
							return;
						}
					}
				}
			}
			if (pawn.story == null)
			{
				return;
			}
			List<Trait> allTraits = pawn.story.traits.allTraits;
			for (int k = 0; k < allTraits.Count; k++)
			{
				if (allTraits[k].Suppressed)
				{
					continue;
				}
				TraitDegreeData currentData = allTraits[k].CurrentData;
				if (!(currentData.randomDiseaseMtbDays > 0f) || !Rand.MTBEventOccurs(currentData.randomDiseaseMtbDays, 60000f, 60f))
				{
					continue;
				}
				BiomeDef biome;
				if (pawn.Tile != -1)
				{
					biome = Find.WorldGrid[pawn.Tile].biome;
				}
				else
				{
					biome = DefDatabase<BiomeDef>.GetRandom();
				}
				IncidentDef incidentDef = DefDatabase<IncidentDef>.AllDefs.Where((IncidentDef d) => d.category == IncidentCategoryDefOf.DiseaseHuman).RandomElementByWeightWithFallback((IncidentDef d) => biome.CommonalityOfDisease(d));
				if (incidentDef == null)
				{
					continue;
				}
				string blockedInfo;
				List<Pawn> list = ((IncidentWorker_Disease)incidentDef.Worker).ApplyToPawns(Gen.YieldSingle(pawn), out blockedInfo);
				if (PawnUtility.ShouldSendNotificationAbout(pawn))
				{
					if (list.Contains(pawn))
					{
						Find.LetterStack.ReceiveLetter("LetterLabelTraitDisease".Translate(incidentDef.diseaseIncident.label), "LetterTraitDisease".Translate(pawn.LabelCap, incidentDef.diseaseIncident.label, pawn.Named("PAWN")).AdjustedFor(pawn), LetterDefOf.NegativeEvent, pawn);
					}
					else if (!blockedInfo.NullOrEmpty())
					{
						Messages.Message(blockedInfo, pawn, MessageTypeDefOf.NeutralEvent);
					}
				}
			}
		}

		public bool HasHediffsNeedingTend(bool forAlert = false)
		{
			return hediffSet.HasTendableHediff(forAlert);
		}

		public bool HasHediffsNeedingTendByPlayer(bool forAlert = false)
		{
			if (HasHediffsNeedingTend(forAlert))
			{
				if (pawn.NonHumanlikeOrWildMan())
				{
					if (pawn.Faction == Faction.OfPlayer)
					{
						return true;
					}
					Building_Bed building_Bed = pawn.CurrentBed();
					if (building_Bed != null && building_Bed.Faction == Faction.OfPlayer)
					{
						return true;
					}
				}
				else if ((pawn.Faction == Faction.OfPlayer && pawn.HostFaction == null) || pawn.HostFaction == Faction.OfPlayer)
				{
					return true;
				}
			}
			return false;
		}

		public void DropBloodFilth()
		{
			if ((pawn.Spawned || pawn.ParentHolder is Pawn_CarryTracker) && pawn.SpawnedOrAnyParentSpawned && pawn.RaceProps.BloodDef != null)
			{
				FilthMaker.TryMakeFilth(pawn.PositionHeld, pawn.MapHeld, pawn.RaceProps.BloodDef, pawn.LabelIndefinite());
			}
		}

		public IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Hediff hediff in hediffSet.hediffs)
			{
				IEnumerable<Gizmo> gizmos = hediff.GetGizmos();
				if (gizmos == null)
				{
					continue;
				}
				foreach (Gizmo item in gizmos)
				{
					yield return item;
				}
			}
		}
	}
}
