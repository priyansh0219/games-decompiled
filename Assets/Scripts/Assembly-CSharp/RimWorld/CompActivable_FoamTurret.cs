using Verse;

namespace RimWorld
{
	public class CompActivable_FoamTurret : CompActivable
	{
		private Building_TurretGun ParentGun => (Building_TurretGun)parent;

		public override AcceptanceReport CanActivate(Pawn activateBy = null)
		{
			AcceptanceReport result = base.CanActivate(activateBy);
			if (!result.Accepted)
			{
				return result;
			}
			if (!ParentGun.TryFindNewTarget().IsValid)
			{
				return "NoNearbyFire".Translate();
			}
			return true;
		}

		protected override void SendDeactivateMessage()
		{
			Messages.Message("MessageActivationCanceled".Translate(parent) + ": " + "NoNearbyFireNoFuelUsed".Translate(), parent, MessageTypeDefOf.NeutralEvent);
		}

		protected override bool ShouldDeactivate()
		{
			return !CanActivate();
		}

		protected override bool TryUse()
		{
			ParentGun.TryActivateBurst();
			if (ParentGun.CurrentTarget.IsValid)
			{
				ParentGun.Top.ForceFaceTarget(ParentGun.CurrentTarget);
				return true;
			}
			return false;
		}
	}
}
