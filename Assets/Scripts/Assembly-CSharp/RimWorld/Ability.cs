using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	public class Ability : IVerbOwner, IExposable, ILoadReferenceable
	{
		public int Id = -1;

		public Pawn pawn;

		public AbilityDef def;

		public List<AbilityComp> comps;

		protected Command gizmo;

		private VerbTracker verbTracker;

		private int cooldownTicks;

		private int cooldownTicksDuration;

		private Mote warmupMote;

		private Effecter warmupEffecter;

		private Sustainer soundCast;

		private bool wasCastingOnPrevTick;

		private int charges;

		public int lastCastTick = -99999;

		public Precept sourcePrecept;

		private List<PreCastAction> preCastActions = new List<PreCastAction>();

		private List<Tuple<Effecter, TargetInfo, TargetInfo>> maintainedEffecters = new List<Tuple<Effecter, TargetInfo, TargetInfo>>();

		private List<Mote> customWarmupMotes = new List<Mote>();

		private bool needToRecacheWarmupMotes = true;

		private List<CompAbilityEffect> effectComps;

		private List<LocalTargetInfo> affectedTargetsCached = new List<LocalTargetInfo>();

		private TargetInfo verbTargetInfoTmp = null;

		public Verb verb => verbTracker.PrimaryVerb;

		public List<Tool> Tools { get; private set; }

		public Thing ConstantCaster => pawn;

		public List<VerbProperties> VerbProperties => new List<VerbProperties> { def.verbProperties };

		public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

		public int CooldownTicksRemaining => cooldownTicks;

		public int CooldownTicksTotal => cooldownTicksDuration;

		public VerbTracker VerbTracker
		{
			get
			{
				if (verbTracker == null)
				{
					verbTracker = new VerbTracker(this);
				}
				return verbTracker;
			}
		}

		public bool HasCooldown
		{
			get
			{
				if (!(def.cooldownTicksRange != default(IntRange)))
				{
					if (def.groupDef != null)
					{
						return def.groupDef.cooldownTicks > 0;
					}
					return false;
				}
				return true;
			}
		}

		public virtual bool CanCast
		{
			get
			{
				if (!comps.NullOrEmpty())
				{
					for (int i = 0; i < comps.Count; i++)
					{
						if (!comps[i].CanCast)
						{
							return false;
						}
					}
				}
				if (!def.cooldownPerCharge)
				{
					return cooldownTicks <= 0;
				}
				return charges > 0;
			}
		}

		public bool Casting
		{
			get
			{
				if (!verb.WarmingUp)
				{
					if (pawn.jobs?.curDriver is JobDriver_CastAbilityWorld)
					{
						return pawn.CurJob.ability == this;
					}
					return false;
				}
				return true;
			}
		}

		public bool CanCooldown
		{
			get
			{
				if (def.waitForJobEnd)
				{
					return pawn.jobs?.curJob?.def != def.jobDef;
				}
				return true;
			}
		}

		public string Tooltip
		{
			get
			{
				string text = def.GetTooltip(pawn);
				if (def.cooldownPerCharge)
				{
					text = text + "\n\n" + "Uses".Translate().ToString() + $": {charges} / {def.charges}";
				}
				else if (def.charges > 1 && CooldownTicksRemaining <= 0)
				{
					text = text + "\n\n" + "Charges".Translate().ToString() + $": {charges} / {def.charges}";
				}
				if (EffectComps != null)
				{
					foreach (CompAbilityEffect effectComp in EffectComps)
					{
						string text2 = effectComp.ExtraTooltipPart();
						if (!text2.NullOrEmpty())
						{
							text = text + "\n\n" + text2;
						}
					}
				}
				return text;
			}
		}

		public virtual bool CanQueueCast
		{
			get
			{
				if (HasCooldown)
				{
					if (!CanCast)
					{
						return false;
					}
					if (pawn.jobs == null)
					{
						return false;
					}
					int num = 0;
					foreach (Job item in pawn.jobs.AllJobs())
					{
						if (SameForQueueing(item))
						{
							num++;
							if (num >= charges)
							{
								return false;
							}
						}
					}
					return true;
				}
				return true;
				bool SameForQueueing(Job j)
				{
					if (j.verbToUse != verb)
					{
						if (def.groupDef != null && j.ability != null)
						{
							return j.ability.def.groupDef == def.groupDef;
						}
						return false;
					}
					return true;
				}
			}
		}

		public List<CompAbilityEffect> EffectComps
		{
			get
			{
				if (effectComps == null)
				{
					IEnumerable<CompAbilityEffect> enumerable = CompsOfType<CompAbilityEffect>();
					effectComps = ((enumerable == null) ? new List<CompAbilityEffect>() : enumerable.ToList());
				}
				return effectComps;
			}
		}

		public string UniqueVerbOwnerID()
		{
			return GetUniqueLoadID();
		}

		public bool VerbsStillUsableBy(Pawn p)
		{
			return true;
		}

		public Ability()
		{
		}

		public Ability(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public Ability(Pawn pawn, Precept sourcePrecept)
		{
			this.pawn = pawn;
			this.sourcePrecept = sourcePrecept;
		}

		public Ability(Pawn pawn, AbilityDef def)
		{
			this.pawn = pawn;
			this.def = def;
			Initialize();
		}

		public Ability(Pawn pawn, Precept sourcePrecept, AbilityDef def)
		{
			this.pawn = pawn;
			this.def = def;
			this.sourcePrecept = sourcePrecept;
			Initialize();
		}

		public virtual bool CanApplyOn(LocalTargetInfo target)
		{
			if (effectComps != null)
			{
				foreach (CompAbilityEffect effectComp in effectComps)
				{
					if (!effectComp.CanApplyOn(target, null))
					{
						return false;
					}
				}
			}
			return true;
		}

		public virtual bool CanApplyOn(GlobalTargetInfo target)
		{
			if (effectComps != null)
			{
				foreach (CompAbilityEffect effectComp in effectComps)
				{
					if (!effectComp.CanApplyOn(target))
					{
						return false;
					}
				}
			}
			return true;
		}

		public virtual bool AICanTargetNow(LocalTargetInfo target)
		{
			if (!def.aiCanUse || !CanCast)
			{
				return false;
			}
			if (!CanApplyOn(target))
			{
				return false;
			}
			if (effectComps != null)
			{
				foreach (CompAbilityEffect effectComp in effectComps)
				{
					if (!effectComp.AICanTargetNow(target))
					{
						return false;
					}
				}
			}
			return true;
		}

		public virtual LocalTargetInfo AIGetAOETarget()
		{
			if (def.ai_SearchAOEForTargets)
			{
				foreach (Thing item in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, verb.verbProps.range, useCenter: true))
				{
					if (!ValidAOEAffectedTarget(item))
					{
						continue;
					}
					bool flag = true;
					foreach (CompAbilityEffect effectComp in effectComps)
					{
						if (!effectComp.AICanTargetNow(item))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						return item;
					}
				}
			}
			return LocalTargetInfo.Invalid;
		}

		public Window ConfirmationDialog(LocalTargetInfo target, Action action)
		{
			if (EffectComps != null)
			{
				foreach (CompAbilityEffect effectComp in effectComps)
				{
					Window window = effectComp.ConfirmationDialog(target, action);
					if (window != null)
					{
						return window;
					}
				}
			}
			if (!def.confirmationDialogText.NullOrEmpty())
			{
				return Dialog_MessageBox.CreateConfirmation(def.confirmationDialogText.Formatted(pawn.Named("PAWN")), action);
			}
			return null;
		}

		public Window ConfirmationDialog(GlobalTargetInfo target, Action action)
		{
			if (EffectComps != null)
			{
				foreach (CompAbilityEffect effectComp in EffectComps)
				{
					Window window = effectComp.ConfirmationDialog(target, action);
					if (window != null)
					{
						return window;
					}
				}
			}
			if (!def.confirmationDialogText.NullOrEmpty())
			{
				return Dialog_MessageBox.CreateConfirmation(def.confirmationDialogText.Formatted(pawn.Named("PAWN")), action);
			}
			return null;
		}

		protected virtual void PreActivate(LocalTargetInfo? target)
		{
			if (HasCooldown)
			{
				if (def.groupDef != null)
				{
					int num = (def.overrideGroupCooldown ? def.cooldownTicksRange.RandomInRange : def.groupDef.cooldownTicks);
					foreach (Ability item in pawn.abilities.AllAbilitiesForReading)
					{
						item.Notify_GroupStartedCooldown(def.groupDef, num);
					}
					if (pawn.Ideo != null)
					{
						foreach (Precept_Ritual item2 in pawn.Ideo.PreceptsListForReading.OfType<Precept_Ritual>())
						{
							if (item2.def.useCooldownFromAbilityGroupDef == def.groupDef)
							{
								item2.Notify_CooldownFromAbilityStarted(num);
							}
						}
					}
				}
				else
				{
					charges--;
					if (def.cooldownPerCharge)
					{
						if (charges < def.charges && cooldownTicks == 0)
						{
							StartCooldown(def.cooldownTicksRange.RandomInRange);
						}
					}
					else if (charges <= 0)
					{
						StartCooldown(def.cooldownTicksRange.RandomInRange);
					}
				}
			}
			if (def.writeCombatLog)
			{
				Find.BattleLog.Add(new BattleLogEntry_AbilityUsed(pawn, target?.Thing, def, RulePackDefOf.Event_AbilityUsed));
			}
			customWarmupMotes.Clear();
		}

		public virtual bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
		{
			PreActivate(target);
			if (def.hostile && pawn.mindState != null)
			{
				pawn.mindState.lastCombatantTick = Find.TickManager.TicksGame;
			}
			if (EffectComps.Any())
			{
				affectedTargetsCached.Clear();
				affectedTargetsCached.AddRange(GetAffectedTargets(target));
				ApplyEffects(EffectComps, affectedTargetsCached, dest);
			}
			preCastActions.Clear();
			return true;
		}

		public virtual bool Activate(GlobalTargetInfo target)
		{
			PreActivate(null);
			if (def.hostile && pawn.mindState != null)
			{
				pawn.mindState.lastCombatantTick = Find.TickManager.TicksGame;
			}
			if (EffectComps.Any())
			{
				ApplyEffects(EffectComps, target);
			}
			preCastActions.Clear();
			return true;
		}

		public IEnumerable<LocalTargetInfo> GetAffectedTargets(LocalTargetInfo target)
		{
			if (def.HasAreaOfEffect && def.canUseAoeToGetTargets)
			{
				foreach (LocalTargetInfo item in from t in GenRadial.RadialDistinctThingsAround(target.Cell, pawn.Map, def.EffectRadius, useCenter: true).Where(ValidAOEAffectedTarget)
					select new LocalTargetInfo(t))
				{
					yield return item;
				}
			}
			else
			{
				yield return target;
			}
		}

		private bool ValidAOEAffectedTarget(Thing target)
		{
			if (!verb.targetParams.CanTarget(target))
			{
				return false;
			}
			if (target.Fogged())
			{
				return false;
			}
			for (int i = 0; i < EffectComps.Count; i++)
			{
				if (!EffectComps[i].Valid((LocalTargetInfo)target, throwMessages: false))
				{
					return false;
				}
			}
			return true;
		}

		public virtual void QueueCastingJob(LocalTargetInfo target, LocalTargetInfo destination)
		{
			if (CanQueueCast && CanApplyOn(target))
			{
				ShowCastingConfirmationIfNeeded(target, delegate
				{
					needToRecacheWarmupMotes = true;
					pawn.jobs.TryTakeOrderedJob(GetJob(target, destination), JobTag.Misc);
				});
			}
		}

		public virtual Job GetJob(LocalTargetInfo target, LocalTargetInfo destination)
		{
			Job job = JobMaker.MakeJob(def.jobDef ?? JobDefOf.CastAbilityOnThing);
			job.verbToUse = verb;
			job.targetA = target;
			job.targetB = destination;
			job.ability = this;
			needToRecacheWarmupMotes = true;
			return job;
		}

		public virtual void QueueCastingJob(GlobalTargetInfo target)
		{
			if (!CanQueueCast || !CanApplyOn(target))
			{
				return;
			}
			ShowCastingConfirmationIfNeeded(target, delegate
			{
				if (!pawn.IsCaravanMember())
				{
					Job job = JobMaker.MakeJob(def.jobDef ?? JobDefOf.CastAbilityOnWorldTile);
					job.verbToUse = verb;
					job.globalTarget = target;
					job.ability = this;
					needToRecacheWarmupMotes = true;
					pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				}
				else
				{
					Activate(target);
				}
			});
		}

		private void ShowCastingConfirmationIfNeeded(LocalTargetInfo target, Action cast)
		{
			Window window = ConfirmationDialog(target, cast);
			if (window == null)
			{
				cast();
			}
			else
			{
				Find.WindowStack.Add(window);
			}
		}

		private void ShowCastingConfirmationIfNeeded(GlobalTargetInfo target, Action cast)
		{
			Window window = ConfirmationDialog(target, cast);
			if (window == null)
			{
				cast();
			}
			else
			{
				Find.WindowStack.Add(window);
			}
		}

		public bool ValidateGlobalTarget(GlobalTargetInfo target)
		{
			for (int i = 0; i < EffectComps.Count; i++)
			{
				if (!EffectComps[i].Valid(target, throwMessages: true))
				{
					return false;
				}
			}
			return true;
		}

		public virtual bool GizmoDisabled(out string reason)
		{
			if (!CanCast)
			{
				if (CanCooldown)
				{
					reason = "AbilityOnCooldown".Translate(cooldownTicks.ToStringTicksToPeriod()).Resolve();
					return true;
				}
				reason = "AbilityAlreadyQueued".Translate();
				return true;
			}
			if (!CanQueueCast)
			{
				reason = "AbilityAlreadyQueued".Translate();
				return true;
			}
			if (!pawn.Drafted && def.disableGizmoWhileUndrafted && pawn.GetCaravan() == null && !DebugSettings.ShowDevGizmos)
			{
				reason = "AbilityDisabledUndrafted".Translate();
				return true;
			}
			if (pawn.DevelopmentalStage.Baby())
			{
				reason = "IsIncapped".Translate(pawn.LabelShort, pawn);
				return true;
			}
			if (pawn.Downed)
			{
				reason = "CommandDisabledUnconscious".TranslateWithBackup("CommandCallRoyalAidUnconscious").Formatted(pawn);
				return true;
			}
			if (pawn.Deathresting)
			{
				reason = "CommandDisabledDeathresting".Translate(pawn);
				return true;
			}
			if (!comps.NullOrEmpty())
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (comps[i].GizmoDisabled(out reason))
					{
						return true;
					}
				}
			}
			if (pawn.GetLord()?.LordJob is LordJob_Ritual lordJob_Ritual)
			{
				reason = "AbilityDisabledInRitual".Translate(pawn, lordJob_Ritual.RitualLabel);
				return true;
			}
			reason = null;
			return false;
		}

		public virtual void AbilityTick()
		{
			VerbTracker.VerbsTick();
			if (def.emittedFleck != null && Casting && pawn.IsHashIntervalTick(def.emissionInterval))
			{
				FleckMaker.ThrowMetaIcon(verb.CurrentTarget.Cell, pawn.Map, def.emittedFleck, 0.75f);
			}
			if ((def.warmupMote != null || def.WarmupMoteSocialSymbol != null) && Casting)
			{
				Vector3 vector = pawn.DrawPos + def.moteDrawOffset;
				vector += (verb.CurrentTarget.CenterVector3 - vector) * def.moteOffsetAmountTowardsTarget;
				if (warmupMote == null || warmupMote.Destroyed)
				{
					if (def.WarmupMoteSocialSymbol == null)
					{
						if (def.warmupMote.thingClass != typeof(MoteAttached))
						{
							warmupMote = MoteMaker.MakeStaticMote(vector, pawn.Map, def.warmupMote);
						}
						else
						{
							Corpse corpse = verb.CurrentTarget.Thing as Corpse;
							warmupMote = MoteMaker.MakeAttachedOverlay((corpse != null) ? corpse : verb.CurrentTarget.Thing, def.warmupMote, Vector3.zero);
						}
					}
					else
					{
						warmupMote = MoteMaker.MakeInteractionBubble(pawn, verb.CurrentTarget.Pawn, ThingDefOf.Mote_Speech, def.WarmupMoteSocialSymbol);
					}
				}
				else
				{
					if (!(warmupMote is MoteAttached))
					{
						warmupMote.exactPosition = vector;
					}
					warmupMote.Maintain();
				}
			}
			if (Casting)
			{
				Thing thing = verb.CurrentTarget.Thing;
				if (warmupEffecter == null && def.warmupEffecter != null)
				{
					if (!def.useAverageTargetPositionForWarmupEffecter || EffectComps.NullOrEmpty())
					{
						if (thing != null)
						{
							warmupEffecter = def.warmupEffecter.SpawnAttached(thing, pawn.MapHeld);
							verbTargetInfoTmp = thing;
						}
						else
						{
							warmupEffecter = def.warmupEffecter.Spawn(verb.CurrentTarget.Cell, pawn.MapHeld);
							verbTargetInfoTmp = new TargetInfo(verb.CurrentTarget.Cell, pawn.MapHeld);
						}
					}
					else
					{
						Vector3 zero = Vector3.zero;
						IEnumerable<LocalTargetInfo> affectedTargets = GetAffectedTargets(verb.CurrentTarget);
						foreach (LocalTargetInfo item4 in affectedTargets)
						{
							zero += item4.Cell.ToVector3Shifted();
						}
						zero /= (float)affectedTargets.Count();
						warmupEffecter = def.warmupEffecter.Spawn(zero.ToIntVec3(), pawn.MapHeld);
						verbTargetInfoTmp = new TargetInfo(zero.ToIntVec3(), pawn.MapHeld);
					}
					warmupEffecter.Trigger(verbTargetInfoTmp, verbTargetInfoTmp);
				}
				warmupEffecter?.EffectTick(verbTargetInfoTmp, verbTargetInfoTmp);
				if (needToRecacheWarmupMotes && !EffectComps.NullOrEmpty() && verb.CurrentTarget.Thing != null)
				{
					customWarmupMotes.Clear();
					foreach (CompAbilityEffect effectComp in effectComps)
					{
						foreach (Mote item5 in effectComp.CustomWarmupMotes(verb.CurrentTarget))
						{
							customWarmupMotes.Add(item5);
						}
					}
					needToRecacheWarmupMotes = false;
				}
				foreach (Mote customWarmupMote in customWarmupMotes)
				{
					customWarmupMote.Maintain();
				}
			}
			else if (warmupEffecter != null)
			{
				warmupEffecter.Cleanup();
				warmupEffecter = null;
			}
			if (verb.WarmingUp)
			{
				if (!(def.targetWorldCell ? CanApplyOn(pawn.CurJob.globalTarget) : CanApplyOn(verb.CurrentTarget)))
				{
					if (def.targetWorldCell)
					{
						pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
					verb.WarmupStance?.Interrupt();
					verb.Reset();
					customWarmupMotes.Clear();
					preCastActions.Clear();
				}
				else
				{
					for (int num = preCastActions.Count - 1; num >= 0; num--)
					{
						if (preCastActions[num].ticksAwayFromCast >= verb.WarmupTicksLeft)
						{
							preCastActions[num].action(verb.CurrentTarget, verb.CurrentDestination);
							preCastActions.RemoveAt(num);
						}
					}
				}
			}
			if (pawn.Spawned && Casting)
			{
				if (def.warmupSound != null)
				{
					if (soundCast == null || soundCast.Ended)
					{
						soundCast = def.warmupSound.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map), MaintenanceType.PerTick));
					}
					else
					{
						soundCast.Maintain();
					}
				}
				if (!wasCastingOnPrevTick && def.warmupStartSound != null)
				{
					def.warmupStartSound.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
				}
				if (def.warmupPreEndSound != null && verb.WarmupTicksLeft == def.warmupPreEndSoundSeconds.SecondsToTicks())
				{
					def.warmupPreEndSound.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
				}
			}
			if (cooldownTicks > 0 && CanCooldown)
			{
				cooldownTicks--;
				if (cooldownTicks == 0)
				{
					if (def.cooldownPerCharge)
					{
						charges = Mathf.Min(++charges, def.charges);
						if (charges < def.charges)
						{
							StartCooldown(def.cooldownTicksRange.RandomInRange);
						}
					}
					if (def.sendLetterOnCooldownComplete)
					{
						Find.LetterStack.ReceiveLetter("AbilityReadyLabel".Translate(def.LabelCap), "AbilityReadyText".Translate(pawn, def.label), LetterDefOf.NeutralEvent, new LookTargets(pawn));
					}
				}
			}
			for (int num2 = maintainedEffecters.Count - 1; num2 >= 0; num2--)
			{
				Effecter item = maintainedEffecters[num2].Item1;
				if (item.ticksLeft > 0)
				{
					TargetInfo item2 = maintainedEffecters[num2].Item2;
					TargetInfo item3 = maintainedEffecters[num2].Item3;
					item.EffectTick(item2, item3);
					item.ticksLeft--;
				}
				else
				{
					item.Cleanup();
					maintainedEffecters.RemoveAt(num2);
				}
			}
			if (!comps.NullOrEmpty())
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CompTick();
				}
			}
			if (wasCastingOnPrevTick && !Casting)
			{
				lastCastTick = Find.TickManager.TicksGame;
			}
			wasCastingOnPrevTick = Casting;
		}

		public void Notify_StartedCasting()
		{
			for (int i = 0; i < EffectComps.Count; i++)
			{
				preCastActions.AddRange(EffectComps[i].GetPreCastActions());
			}
		}

		public void DrawEffectPreviews(LocalTargetInfo target)
		{
			for (int i = 0; i < EffectComps.Count; i++)
			{
				EffectComps[i].DrawEffectPreview(target);
			}
		}

		public bool GizmosVisible()
		{
			if (!EffectComps.NullOrEmpty())
			{
				foreach (CompAbilityEffect effectComp in EffectComps)
				{
					if (effectComp.ShouldHideGizmo)
					{
						return false;
					}
				}
			}
			return true;
		}

		public virtual IEnumerable<Command> GetGizmos()
		{
			if (gizmo == null)
			{
				gizmo = (Command)Activator.CreateInstance(def.gizmoClass, this);
				gizmo.Order = def.uiOrder;
			}
			if (!pawn.Drafted || def.showWhenDrafted)
			{
				yield return gizmo;
			}
			if (DebugSettings.ShowDevGizmos && cooldownTicks > 0 && CanCooldown)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "DEV: Reset cooldown";
				command_Action.action = delegate
				{
					cooldownTicks = 0;
					charges = def.charges;
				};
				yield return command_Action;
			}
		}

		public virtual IEnumerable<Gizmo> GetGizmosExtra()
		{
			if (comps == null)
			{
				yield break;
			}
			foreach (AbilityComp comp in comps)
			{
				foreach (Gizmo item in comp.CompGetGizmosExtra())
				{
					yield return item;
				}
			}
		}

		public string GetInspectString()
		{
			if (comps.NullOrEmpty())
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < comps.Count; i++)
			{
				string text = comps[i].CompInspectStringExtra();
				if (!text.NullOrEmpty())
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.Append(text);
				}
			}
			return stringBuilder.ToString();
		}

		private void ApplyEffects(IEnumerable<CompAbilityEffect> effects, List<LocalTargetInfo> targets, LocalTargetInfo dest)
		{
			foreach (LocalTargetInfo target in targets)
			{
				ApplyEffects(effects, target, dest);
			}
			foreach (CompAbilityEffect effect in effects)
			{
				effect.PostApplied(targets, pawn.MapHeld);
			}
		}

		public void StartCooldown(int ticks)
		{
			cooldownTicksDuration = ticks;
			cooldownTicks = cooldownTicksDuration;
			if (!def.cooldownPerCharge)
			{
				charges = def.charges;
			}
		}

		public void Notify_GroupStartedCooldown(AbilityGroupDef group, int ticks)
		{
			if (group == def.groupDef)
			{
				StartCooldown(ticks);
			}
		}

		protected virtual void ApplyEffects(IEnumerable<CompAbilityEffect> effects, LocalTargetInfo target, LocalTargetInfo dest)
		{
			foreach (CompAbilityEffect effect in effects)
			{
				effect.Apply(target, dest);
			}
		}

		protected virtual void ApplyEffects(IEnumerable<CompAbilityEffect> effects, GlobalTargetInfo target)
		{
			foreach (CompAbilityEffect effect in effects)
			{
				effect.Apply(target);
			}
		}

		public virtual void OnGizmoUpdate()
		{
			foreach (CompAbilityEffect effectComp in EffectComps)
			{
				effectComp.OnGizmoUpdate();
			}
		}

		public IEnumerable<T> CompsOfType<T>() where T : AbilityComp
		{
			if (comps == null)
			{
				return null;
			}
			return comps.Where((AbilityComp c) => c is T).Cast<T>();
		}

		public T CompOfType<T>() where T : AbilityComp
		{
			if (comps == null)
			{
				return null;
			}
			return comps.FirstOrDefault((AbilityComp c) => c is T) as T;
		}

		public void Initialize()
		{
			if (def.comps.Any())
			{
				comps = new List<AbilityComp>();
				for (int i = 0; i < def.comps.Count; i++)
				{
					AbilityComp abilityComp = null;
					try
					{
						abilityComp = (AbilityComp)Activator.CreateInstance(def.comps[i].compClass);
						abilityComp.parent = this;
						comps.Add(abilityComp);
						abilityComp.Initialize(def.comps[i]);
					}
					catch (Exception ex)
					{
						Log.Error("Could not instantiate or initialize an AbilityComp: " + ex);
						comps.Remove(abilityComp);
					}
				}
			}
			if (Id == -1)
			{
				Id = Find.UniqueIDsManager.GetNextAbilityID();
			}
			if (VerbTracker.PrimaryVerb is Verb_CastAbility verb_CastAbility)
			{
				verb_CastAbility.ability = this;
			}
			charges = def.charges;
		}

		public float FinalPsyfocusCost(LocalTargetInfo target)
		{
			if (def.AnyCompOverridesPsyfocusCost)
			{
				foreach (AbilityComp comp in comps)
				{
					if (comp.props.OverridesPsyfocusCost)
					{
						return comp.PsyfocusCostForTarget(target);
					}
				}
			}
			return def.PsyfocusCost;
		}

		public float HemogenCost()
		{
			if (comps != null)
			{
				foreach (AbilityComp comp in comps)
				{
					if (comp is CompAbilityEffect_HemogenCost compAbilityEffect_HemogenCost)
					{
						return compAbilityEffect_HemogenCost.Props.hemogenCost;
					}
				}
			}
			return 0f;
		}

		public string WorldMapExtraLabel(GlobalTargetInfo t)
		{
			foreach (CompAbilityEffect effectComp in EffectComps)
			{
				string text = effectComp.WorldMapExtraLabel(t);
				if (text != null)
				{
					return text;
				}
			}
			return null;
		}

		public void AddEffecterToMaintain(Effecter eff, IntVec3 pos, int ticks, Map map = null)
		{
			eff.ticksLeft = ticks;
			TargetInfo targetInfo = new TargetInfo(pos, map ?? pawn.Map);
			maintainedEffecters.Add(new Tuple<Effecter, TargetInfo, TargetInfo>(eff, targetInfo, targetInfo));
		}

		public void AddEffecterToMaintain(Effecter eff, IntVec3 posA, IntVec3 posB, int ticks, Map map = null)
		{
			eff.ticksLeft = ticks;
			TargetInfo item = new TargetInfo(posA, map ?? pawn.Map);
			TargetInfo item2 = new TargetInfo(posB, map ?? pawn.Map);
			maintainedEffecters.Add(new Tuple<Effecter, TargetInfo, TargetInfo>(eff, item, item2));
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			if (def == null)
			{
				return;
			}
			Scribe_Values.Look(ref Id, "Id", -1);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (Id == -1)
				{
					Id = Find.UniqueIDsManager.GetNextAbilityID();
				}
				Initialize();
			}
			Scribe_References.Look(ref sourcePrecept, "sourcePrecept");
			Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
			Scribe_Values.Look(ref cooldownTicks, "cooldownTicks", 0);
			Scribe_Values.Look(ref cooldownTicksDuration, "cooldownTicksDuration", 0);
			Scribe_Values.Look(ref charges, "charges", def.charges);
			Scribe_Values.Look(ref lastCastTick, "lastCastTick", -99999);
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].PostExposeData();
				}
			}
		}

		public string GetUniqueLoadID()
		{
			return "Ability_" + Id;
		}
	}
}
