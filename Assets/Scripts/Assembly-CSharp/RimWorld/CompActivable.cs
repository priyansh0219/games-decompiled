using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public abstract class CompActivable : ThingComp, ITargetingSource
	{
		protected int cooldownTicks;

		private int activeTicks;

		[Unsaved(false)]
		private Effecter progressBarEffecter;

		[Unsaved(false)]
		private Texture2D activateTex;

		[Unsaved(false)]
		private CompRefuelable refuelable;

		[Unsaved(false)]
		private CompPowerTrader power;

		public CompProperties_Activable Props => (CompProperties_Activable)props;

		public bool OnCooldown => cooldownTicks > 0;

		public bool Active => activeTicks > 0;

		public bool CasterIsPawn => true;

		public bool IsMeleeAttack => false;

		public bool Targetable => true;

		public bool MultiSelect => false;

		public bool HidePawnTooltips => false;

		public Thing Caster => parent;

		public Pawn CasterPawn => null;

		public Verb GetVerb => null;

		public TargetingParameters targetParams => Props.targetingParameters;

		public virtual ITargetingSource DestinationSelector => null;

		public Texture2D UIIcon
		{
			get
			{
				if (activateTex == null)
				{
					activateTex = ContentFinder<Texture2D>.Get(Props.activateTexPath);
				}
				return activateTex;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			refuelable = parent.TryGetComp<CompRefuelable>();
			power = parent.TryGetComp<CompPowerTrader>();
		}

		public override void CompTick()
		{
			if (Active)
			{
				if (TryUse())
				{
					Deactivate();
				}
				else
				{
					if (ShouldDeactivate())
					{
						SendDeactivateMessage();
						Deactivate();
						return;
					}
					activeTicks--;
				}
				if (activeTicks <= 0)
				{
					StartCooldown();
				}
			}
			else if (OnCooldown)
			{
				cooldownTicks--;
				if (progressBarEffecter == null)
				{
					progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
				}
				progressBarEffecter.EffectTick(parent, TargetInfo.Invalid);
				MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
				mote.progress = 1f - (float)cooldownTicks / (float)Props.cooldownTicks;
				mote.offsetZ = -0.8f;
				if (Props.cooldownFleck != null && parent.IsHashIntervalTick(Props.cooldownFleckSpawnIntervalTicks))
				{
					FleckCreationData dataStatic = FleckMaker.GetDataStatic(parent.DrawPos, parent.Map, Props.cooldownFleck, Props.cooldownFleckScale);
					parent.Map.flecks.CreateFleck(dataStatic);
				}
				if (cooldownTicks <= 0)
				{
					CooldownEnded();
				}
			}
		}

		protected virtual void SendDeactivateMessage()
		{
			Messages.Message("MessageActivationCanceled".Translate(parent), parent, MessageTypeDefOf.NeutralEvent);
		}

		public virtual AcceptanceReport CanActivate(Pawn activateBy = null)
		{
			if (Active)
			{
				return "AlreadyActive".Translate();
			}
			if (OnCooldown)
			{
				return Props.onCooldownString + " (" + "DurationLeft".Translate(cooldownTicks.ToStringSecondsFromTicks("F0")) + ")";
			}
			if (refuelable != null && !refuelable.HasFuel)
			{
				return refuelable.Props.outOfFuelMessage ?? ((string)"NoFuel".Translate());
			}
			if (power != null && !power.PowerOn)
			{
				return "NoPower".Translate().CapitalizeFirst();
			}
			if (activateBy != null)
			{
				if (parent.IsForbidden(activateBy))
				{
					return "CannotPrioritizeCellForbidden".Translate();
				}
				if (activateBy.Downed)
				{
					return "MessageRitualPawnDowned".Translate(activateBy);
				}
				if (activateBy.Deathresting)
				{
					return "IsDeathresting".Translate(activateBy.Named("PAWN"));
				}
				if (!activateBy.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
				{
					return "MessageIncapableOfManipulation".Translate(activateBy);
				}
				if (!activateBy.CanReach(parent, PathEndMode.ClosestTouch, Danger.Deadly))
				{
					return "CannotReach".Translate();
				}
			}
			return true;
		}

		public virtual void Activate()
		{
			if (CanActivate().Accepted)
			{
				activeTicks = Props.activeTicks;
				if (!Props.soundActivate.NullOrUndefined())
				{
					Props.soundActivate.PlayOneShot(SoundInfo.InMap(parent));
				}
			}
		}

		protected abstract bool TryUse();

		protected virtual bool ShouldDeactivate()
		{
			return false;
		}

		protected virtual void Deactivate()
		{
			activeTicks = 0;
		}

		protected virtual void StartCooldown()
		{
			cooldownTicks = Props.cooldownTicks;
		}

		protected virtual void CooldownEnded()
		{
			progressBarEffecter?.Cleanup();
			progressBarEffecter = null;
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			progressBarEffecter?.Cleanup();
			progressBarEffecter = null;
			base.PostDestroy(mode, previousMap);
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (parent.Spawned)
			{
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = "OrderActivation".Translate() + "...";
				command_Action.defaultDesc = "OrderActivationDesc".Translate(parent.Named("THING"));
				command_Action.icon = UIIcon;
				command_Action.action = delegate
				{
					SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
					Find.Targeter.BeginTargeting(this);
				};
				AcceptanceReport acceptanceReport = CanActivate();
				if (!acceptanceReport.Accepted)
				{
					command_Action.Disable(acceptanceReport.Reason.CapitalizeFirst());
				}
				yield return command_Action;
			}
			if (DebugSettings.ShowDevGizmos && OnCooldown)
			{
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "DEV: Reset cooldown";
				command_Action2.action = delegate
				{
					cooldownTicks = 0;
					CooldownEnded();
				};
				yield return command_Action2;
			}
		}

		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			if (OnCooldown)
			{
				text += (Props.onCooldownString.CapitalizeFirst() + ": " + "DurationLeft".Translate(cooldownTicks.ToStringSecondsFromTicks("F0")).CapitalizeFirst() + ".").Resolve();
			}
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			return text + "MustBeActivatedByColonist".Translate();
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			if ((bool)CanActivate(selPawn))
			{
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(Props.jobString.CapitalizeFirst(), delegate
				{
					Job job = JobMaker.MakeJob(JobDefOf.ActivateThing, parent);
					selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				}), selPawn, parent);
			}
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref cooldownTicks, "cooldownTicks", 0);
			Scribe_Values.Look(ref activeTicks, "activeTicks", 0);
		}

		public bool CanHitTarget(LocalTargetInfo target)
		{
			return ValidateTarget(target, showMessages: false);
		}

		public bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
		{
			if (!target.IsValid || target.Pawn == null)
			{
				return false;
			}
			Pawn pawn = target.Pawn;
			AcceptanceReport acceptanceReport = CanActivate(pawn);
			if (!acceptanceReport.Accepted)
			{
				if (showMessages && !acceptanceReport.Reason.NullOrEmpty())
				{
					Messages.Message("CannotGenericWorkCustom".Translate(Props.jobString) + ": " + acceptanceReport.Reason.CapitalizeFirst(), pawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}

		public void DrawHighlight(LocalTargetInfo target)
		{
			if (target.IsValid)
			{
				GenDraw.DrawTargetHighlight(target);
			}
		}

		public virtual void OrderForceTarget(LocalTargetInfo target)
		{
			if (ValidateTarget(target, showMessages: false))
			{
				Job job = JobMaker.MakeJob(JobDefOf.ActivateThing, parent);
				target.Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		}

		public void OnGUI(LocalTargetInfo target)
		{
			Widgets.MouseAttachedLabel("ChooseWhoShouldActivate".Translate());
			if (ValidateTarget(target, showMessages: false) && Props.targetingParameters.CanTarget(target.Pawn, this))
			{
				GenUI.DrawMouseAttachment(UIIcon);
			}
			else
			{
				GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
			}
		}
	}
}
