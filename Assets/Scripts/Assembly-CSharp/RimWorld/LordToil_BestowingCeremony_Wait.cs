using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_BestowingCeremony_Wait : LordToil_Wait
	{
		public Pawn target;

		public Pawn bestower;

		public LordToil_BestowingCeremony_Wait(Pawn target, Pawn bestower)
		{
			this.target = target;
			this.bestower = bestower;
		}

		public override void Init()
		{
			Messages.Message("MessageBestowerWaiting".Translate(target.Named("TARGET"), lord.ownedPawns[0].Named("BESTOWER")), new LookTargets(new Pawn[2]
			{
				target,
				lord.ownedPawns[0]
			}), MessageTypeDefOf.NeutralEvent);
		}

		protected override void DecoratePawnDuty(PawnDuty duty)
		{
			duty.focus = target;
		}

		public override void DrawPawnGUIOverlay(Pawn pawn)
		{
			pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.QuestionMark);
		}

		public override IEnumerable<Gizmo> GetPawnGizmos(Pawn p)
		{
			if (p == bestower)
			{
				LordJob_BestowingCeremony job = (LordJob_BestowingCeremony)lord.LordJob;
				yield return new Command_BestowerCeremony(job, bestower, target, StartRitual);
			}
		}

		public override IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn target, Pawn forPawn)
		{
			if (target != bestower)
			{
				yield break;
			}
			yield return new FloatMenuOption("BeginRitual".Translate("RitualBestowingCeremony".Translate()), delegate
			{
				LordJob_BestowingCeremony lordJob_BestowingCeremony = (LordJob_BestowingCeremony)lord.LordJob;
				Find.WindowStack.Add(new Dialog_BeginRitual("ChooseParticipantsBestow".Translate(), "RitualBestowingCeremony".Translate(), null, lordJob_BestowingCeremony.targetSpot.ToTargetInfo(bestower.Map), bestower.Map, delegate(RitualRoleAssignments assignments)
				{
					StartRitual(assignments.Participants.Where((Pawn p) => p != bestower).ToList());
					return true;
				}, bestower, null, delegate(Pawn pawn, bool voluntary, bool allowOtherIdeos)
				{
					Lord lord = pawn.GetLord();
					if (lord != null && lord.LordJob is LordJob_Ritual)
					{
						return false;
					}
					return !pawn.IsPrisonerOfColony && !pawn.RaceProps.Animal;
				}, "Begin".Translate(), new List<Pawn> { bestower, forPawn }, null, outcome: RitualOutcomeEffectDefOf.BestowingCeremony, ritualName: "RitualBestowingCeremony".Translate()));
			});
		}

		private void StartRitual(List<Pawn> pawns)
		{
			lord.AddPawns(pawns);
			((LordJob_BestowingCeremony)lord.LordJob).colonistParticipants.AddRange(pawns);
			lord.ReceiveMemo(LordJob_BestowingCeremony.MemoCeremonyStarted);
			foreach (Pawn pawn in pawns)
			{
				if (pawn.drafter != null)
				{
					pawn.drafter.Drafted = false;
				}
				if (!pawn.Awake())
				{
					RestUtility.WakeUp(pawn);
				}
			}
		}
	}
}
