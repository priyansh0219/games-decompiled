using Verse;

namespace RimWorld
{
	public class CompUseEffect_CallBossgroup : CompUseEffect
	{
		private Effecter prepareEffecter;

		private int delayTicks = -1;

		public CompProperties_Useable_CallBossgroup Props => (CompProperties_Useable_CallBossgroup)props;

		public bool ShouldSendSpawnLetter
		{
			get
			{
				if (Props.spawnLetterLabelKey.NullOrEmpty() || Props.spawnLetterTextKey.NullOrEmpty())
				{
					return false;
				}
				if (!MechanitorUtility.AnyMechanitorInPlayerFaction())
				{
					return false;
				}
				if (Find.BossgroupManager.lastBossgroupCalled > 0)
				{
					return false;
				}
				return true;
			}
		}

		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
			if (Props.effecterDef != null)
			{
				Effecter effecter = new Effecter(Props.effecterDef);
				effecter.Trigger(new TargetInfo(parent.Position, parent.Map), TargetInfo.Invalid);
				effecter.Cleanup();
			}
			prepareEffecter?.Cleanup();
			prepareEffecter = null;
			if (Props.delayTicks <= 0)
			{
				CallBossgroup();
			}
			else
			{
				delayTicks = Props.delayTicks;
			}
		}

		private void CallBossgroup()
		{
			GameComponent_Bossgroup component = Current.Game.GetComponent<GameComponent_Bossgroup>();
			if (component == null)
			{
				Log.Error("Trying to call bossgroup with no GameComponent_Bossgroup.");
			}
			else
			{
				Props.bossgroupDef.Worker.Resolve(parent.Map, component.NumTimesCalledBossgroup(Props.bossgroupDef));
			}
		}

		public override TaggedString ConfirmMessage(Pawn p)
		{
			GameComponent_Bossgroup component = Current.Game.GetComponent<GameComponent_Bossgroup>();
			return "BossgroupWarningDialog".Translate(NamedArgumentUtility.Named(Props.bossgroupDef.boss.kindDef, "LEADERKIND"), Props.bossgroupDef.GetWaveDescription(component.NumTimesCalledBossgroup(Props.bossgroupDef)).Named("PAWNS"));
		}

		public override void PrepareTick()
		{
			if (Props.prepareEffecterDef != null && prepareEffecter == null)
			{
				prepareEffecter = Props.prepareEffecterDef.Spawn(parent.Position, parent.MapHeld);
			}
			prepareEffecter?.EffectTick(parent, TargetInfo.Invalid);
		}

		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
			if (delayTicks >= 0)
			{
				failReason = "AlreadyUsed".Translate();
				return false;
			}
			if (!MechanitorUtility.IsMechanitor(p))
			{
				failReason = "RequiresMechanitor".Translate();
				return false;
			}
			AcceptanceReport acceptanceReport = Props.bossgroupDef.Worker.CanResolve(p);
			if (!acceptanceReport)
			{
				failReason = acceptanceReport.Reason;
				return false;
			}
			failReason = null;
			return true;
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!ModLister.CheckBiotech("Call bossgroup"))
			{
				parent.Destroy();
			}
			else if (!respawningAfterLoad && ShouldSendSpawnLetter)
			{
				Props.SendBossgroupDetailsLetter(Props.spawnLetterLabelKey, Props.spawnLetterTextKey, parent.def);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref delayTicks, "delayTicks", -1);
		}

		public override void CompTick()
		{
			base.CompTick();
			if (delayTicks >= 0)
			{
				delayTicks--;
			}
			if (delayTicks == 0)
			{
				CallBossgroup();
			}
		}
	}
}
