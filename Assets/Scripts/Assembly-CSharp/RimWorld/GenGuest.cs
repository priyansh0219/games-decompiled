using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class GenGuest
	{
		public static void PrisonerRelease(Pawn p)
		{
			if ((p.Faction == Faction.OfPlayer || p.IsWildMan()) && p.needs.mood != null)
			{
				p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.WasImprisoned);
			}
			if (p.SlaveFaction != null)
			{
				Faction hostFaction = p.HostFaction;
				p.SetFaction(p.SlaveFaction);
				p.guest.SetGuestStatus(hostFaction, GuestStatus.Prisoner);
			}
			GuestRelease(p);
		}

		public static void SlaveRelease(Pawn p)
		{
			if ((p.Faction == Faction.OfPlayer || p.IsWildMan()) && p.needs.mood != null)
			{
				p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.WasEnslaved);
			}
			GuestRelease(p);
		}

		private static bool ShouldStayOnMapOnRelease(Pawn pawn)
		{
			if (pawn.IsWildMan())
			{
				return true;
			}
			if (pawn.HomeFaction != null)
			{
				return pawn.HomeFaction.IsPlayer;
			}
			return false;
		}

		private static void GuestRelease(Pawn p)
		{
			if (p.ownership != null)
			{
				p.ownership.UnclaimAll();
			}
			if (p.Drafted)
			{
				p.drafter.Drafted = false;
			}
			if (ShouldStayOnMapOnRelease(p))
			{
				int interactionsToday = p.mindState.interactionsToday;
				int lastAssignedInteractTime = p.mindState.lastAssignedInteractTime;
				if (p.HomeFaction != null)
				{
					p.guest.SetGuestStatus(null);
				}
				p.mindState.interactionsToday = interactionsToday;
				p.mindState.lastAssignedInteractTime = lastAssignedInteractTime;
				if (p.IsWildMan())
				{
					p.mindState.WildManEverReachedOutside = false;
				}
			}
			else
			{
				p.guest.Released = true;
				if (RCellFinder.TryFindBestExitSpot(p, out var spot))
				{
					Job job = JobMaker.MakeJob(JobDefOf.Goto, spot);
					job.exitMapOnArrival = true;
					p.jobs.StartJob(job, JobCondition.InterruptForced);
				}
			}
		}

		public static void AddHealthyPrisonerReleasedThoughts(Pawn prisoner)
		{
			if (prisoner.IsColonist)
			{
				return;
			}
			foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
			{
				if (allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood != null && allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner != prisoner)
				{
					allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.ReleasedHealthyPrisoner, prisoner);
				}
			}
		}

		public static void RemoveHealthyPrisonerReleasedThoughts(Pawn prisoner)
		{
			foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_FreeColonist in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists)
			{
				if (allMapsCaravansAndTravelingTransportPods_Alive_FreeColonist.needs.mood != null && allMapsCaravansAndTravelingTransportPods_Alive_FreeColonist != prisoner)
				{
					allMapsCaravansAndTravelingTransportPods_Alive_FreeColonist.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.ReleasedHealthyPrisoner, prisoner);
				}
			}
		}

		public static void EmancipateSlave(Pawn warden, Pawn slave)
		{
			if (slave.IsSlave)
			{
				SlaveRelease(slave);
				if (slave.IsWildMan())
				{
					slave.mindState.WildManEverReachedOutside = false;
				}
				Messages.Message("MessageSlaveEmancipated".Translate(slave, warden), new LookTargets(slave, warden), MessageTypeDefOf.NeutralEvent);
			}
		}

		public static void EnslavePrisoner(Pawn warden, Pawn prisoner)
		{
			if (!prisoner.IsSlave)
			{
				bool everEnslaved = prisoner.guest.EverEnslaved;
				prisoner.guest.SetGuestStatus(warden.Faction, GuestStatus.Slave);
				Messages.Message("MessagePrisonerEnslaved".Translate(prisoner, warden), new LookTargets(prisoner, warden), MessageTypeDefOf.NeutralEvent);
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.EnslavedPrisoner, warden.Named(HistoryEventArgsNames.Doer)));
				if (!everEnslaved)
				{
					Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.EnslavedPrisonerNotPreviouslyEnslaved, warden.Named(HistoryEventArgsNames.Doer)));
				}
				prisoner.apparel.UnlockAll();
			}
		}
	}
}
