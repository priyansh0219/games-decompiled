using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class CompTargetable : CompUseEffect
	{
		private Thing target;

		public CompProperties_Targetable Props => (CompProperties_Targetable)props;

		protected abstract bool PlayerChoosesTarget { get; }

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_References.Look(ref target, "target");
		}

		public override bool SelectedUseOption(Pawn p)
		{
			if (PlayerChoosesTarget)
			{
				Find.Targeter.BeginTargeting(GetTargetingParameters(), delegate(LocalTargetInfo t)
				{
					target = t.Thing;
					parent.GetComp<CompUsable>().TryStartUseJob(p, target);
				}, p);
				return true;
			}
			target = null;
			return false;
		}

		public override void DoEffect(Pawn usedBy)
		{
			if ((PlayerChoosesTarget && target == null) || (target != null && !GetTargetingParameters().CanTarget(target)))
			{
				return;
			}
			base.DoEffect(usedBy);
			foreach (Thing target in GetTargets(target))
			{
				foreach (CompTargetEffect comp in parent.GetComps<CompTargetEffect>())
				{
					comp.DoEffectOn(usedBy, target);
				}
				if (Props.moteOnTarget != null)
				{
					MoteMaker.MakeAttachedOverlay(target, Props.moteOnTarget, Vector3.zero);
				}
				if (Props.fleckOnTarget != null)
				{
					FleckMaker.AttachedOverlay(target, Props.fleckOnTarget, Vector3.zero);
				}
				if (Props.moteConnecting != null)
				{
					MoteMaker.MakeConnectingLine(usedBy.DrawPos, target.DrawPos, Props.moteConnecting, usedBy.Map);
				}
				if (Props.fleckConnecting != null)
				{
					FleckMaker.ConnectingLine(usedBy.DrawPos, target.DrawPos, Props.fleckConnecting, usedBy.Map);
				}
			}
			this.target = null;
		}

		protected abstract TargetingParameters GetTargetingParameters();

		public abstract IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null);

		public bool BaseTargetValidator(Thing t)
		{
			if (t is Pawn pawn)
			{
				if (Props.psychicSensitiveTargetsOnly && pawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0f)
				{
					return false;
				}
				if (Props.ignoreQuestLodgerPawns && pawn.IsQuestLodger())
				{
					return false;
				}
				if (Props.ignorePlayerFactionPawns && pawn.Faction == Faction.OfPlayer)
				{
					return false;
				}
			}
			if (Props.fleshCorpsesOnly && t is Corpse corpse && !corpse.InnerPawn.RaceProps.IsFlesh)
			{
				return false;
			}
			if (Props.nonDessicatedCorpsesOnly && t is Corpse t2 && t2.GetRotStage() == RotStage.Dessicated)
			{
				return false;
			}
			return true;
		}
	}
}
