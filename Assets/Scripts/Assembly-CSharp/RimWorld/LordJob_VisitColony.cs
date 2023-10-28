using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_VisitColony : LordJob
	{
		private Faction faction;

		private IntVec3 chillSpot;

		private int? durationTicks;

		public List<Thing> gifts;

		public StateGraph exitSubgraph;

		public LordJob_VisitColony()
		{
		}

		public LordJob_VisitColony(Faction faction, IntVec3 chillSpot, int? durationTicks = null)
		{
			this.faction = faction;
			this.chillSpot = chillSpot;
			this.durationTicks = durationTicks;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil lordToil = (stateGraph.StartingToil = stateGraph.AttachSubgraph(new LordJob_Travel(chillSpot).CreateGraph()).StartingToil);
			LordToil_DefendPoint lordToil_DefendPoint = new LordToil_DefendPoint(chillSpot);
			stateGraph.AddToil(lordToil_DefendPoint);
			LordToil_TakeWoundedGuest lordToil_TakeWoundedGuest = new LordToil_TakeWoundedGuest();
			stateGraph.AddToil(lordToil_TakeWoundedGuest);
			exitSubgraph = new LordJob_TravelAndExit(IntVec3.Invalid).CreateGraph();
			LordToil startingToil2 = stateGraph.AttachSubgraph(exitSubgraph).StartingToil;
			LordToil target = exitSubgraph.lordToils[1];
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Walk, canDig: true);
			stateGraph.AddToil(lordToil_ExitMap);
			Transition transition = new Transition(lordToil, startingToil2);
			transition.AddSources(lordToil_DefendPoint);
			transition.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
			if (faction != null)
			{
				transition.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
			}
			transition.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil, lordToil_ExitMap);
			transition2.AddSources(lordToil_DefendPoint, lordToil_TakeWoundedGuest);
			transition2.AddSources(exitSubgraph.lordToils);
			transition2.AddTrigger(new Trigger_PawnCannotReachMapEdge());
			if (faction != null)
			{
				transition2.AddPreAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
			}
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_ExitMap, startingToil2);
			transition3.AddTrigger(new Trigger_PawnCanReachMapEdge());
			transition3.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			transition3.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil, lordToil_DefendPoint);
			transition4.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition4);
			if (faction != null)
			{
				Transition transition5 = new Transition(lordToil_DefendPoint, lordToil_TakeWoundedGuest);
				transition5.AddTrigger(new Trigger_WoundedGuestPresent());
				transition5.AddPreAction(new TransitionAction_Message("MessageVisitorsTakingWounded".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name)));
				stateGraph.AddTransition(transition5);
			}
			Transition transition6 = new Transition(lordToil_DefendPoint, target);
			transition6.AddSources(lordToil_TakeWoundedGuest, lordToil);
			transition6.AddTrigger(new Trigger_BecamePlayerEnemy());
			transition6.AddPreAction(new TransitionAction_SetDefendLocalGroup());
			transition6.AddPostAction(new TransitionAction_WakeAll());
			transition6.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition6);
			Transition transition7 = new Transition(lordToil_DefendPoint, startingToil2);
			int tickLimit = ((!DebugSettings.instantVisitorsGift || faction == null) ? ((!durationTicks.HasValue) ? Rand.Range(8000, 22000) : durationTicks.Value) : 0);
			transition7.AddTrigger(new Trigger_TicksPassed(tickLimit));
			if (faction != null)
			{
				transition7.AddPreAction(new TransitionAction_Message("VisitorsLeaving".Translate(faction.Name)));
			}
			if (gifts != null)
			{
				transition7.AddPreAction(new TransitionAction_GiveGift
				{
					gifts = gifts
				});
			}
			else
			{
				transition7.AddPreAction(new TransitionAction_CheckGiveGift());
			}
			transition7.AddPostAction(new TransitionAction_WakeAll());
			transition7.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			stateGraph.AddTransition(transition7);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.Look(ref faction, "faction");
			Scribe_Values.Look(ref chillSpot, "chillSpot");
			Scribe_Values.Look(ref durationTicks, "durationTicks");
			Scribe_Collections.Look(ref gifts, "gifts", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				gifts?.RemoveAll((Thing x) => x == null);
			}
		}
	}
}
