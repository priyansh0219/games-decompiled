using System.Collections.Generic;
using System.Linq;

namespace Verse.AI.Group
{
	public abstract class LordJob : IExposable
	{
		public Lord lord;

		public virtual bool LostImportantReferenceDuringLoading => false;

		public virtual bool AllowStartNewGatherings => true;

		public virtual bool AllowStartNewRituals => AllowStartNewGatherings;

		public virtual bool NeverInRestraints => false;

		public virtual bool GuiltyOnDowned => false;

		public virtual bool CanBlockHostileVisitors => true;

		public virtual bool AddFleeToil => true;

		public virtual bool OrganizerIsStartingPawn => false;

		public virtual bool KeepExistingWhileHasAnyBuilding => false;

		public virtual bool AlwaysShowWeapon => false;

		public virtual bool IsCaravanSendable => false;

		public virtual bool ManagesRopableAnimals => false;

		public virtual bool DontInterruptLayingPawnsOnCleanup => false;

		public Map Map => lord.lordManager.map;

		public abstract StateGraph CreateGraph();

		public virtual void LordJobTick()
		{
		}

		public virtual void ExposeData()
		{
		}

		public virtual void Cleanup()
		{
		}

		public virtual void PostCleanup()
		{
		}

		public virtual void Notify_AddedToLord()
		{
		}

		public virtual void Notify_PawnAdded(Pawn p)
		{
		}

		public virtual void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
		}

		public virtual void Notify_PawnJobDone(Pawn p, JobCondition condition)
		{
		}

		public virtual void Notify_InMentalState(Pawn pawn, MentalStateDef stateDef)
		{
		}

		public virtual void Notify_BuildingAdded(Building b)
		{
		}

		public virtual void Notify_BuildingLost(Building b)
		{
		}

		public virtual void Notify_LordDestroyed()
		{
		}

		public virtual string GetJobReport(Pawn pawn)
		{
			return pawn.GetJobReport();
		}

		public virtual string GetReport(Pawn pawn)
		{
			return null;
		}

		public virtual bool CanOpenAnyDoor(Pawn p)
		{
			return false;
		}

		public virtual bool ShouldRemovePawn(Pawn p, PawnLostCondition reason)
		{
			return true;
		}

		public virtual IEnumerable<Gizmo> GetPawnGizmos(Pawn p)
		{
			return Enumerable.Empty<Gizmo>();
		}

		public virtual bool EndPawnJobOnCleanup(Pawn p)
		{
			return true;
		}

		public virtual bool ValidateAttackTarget(Pawn searcher, Thing target)
		{
			return true;
		}
	}
}
