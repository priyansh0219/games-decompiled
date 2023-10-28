using RimWorld;
using UnityEngine;

namespace Verse
{
	public class PawnTweener
	{
		private Pawn pawn;

		private Vector3 tweenedPos = new Vector3(0f, 0f, 0f);

		private int lastDrawFrame = -1;

		private Vector3 lastTickSpringPos;

		private const float SpringTightness = 0.09f;

		public Vector3 TweenedPos => tweenedPos;

		public Vector3 LastTickTweenedVelocity => TweenedPos - lastTickSpringPos;

		public PawnTweener(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void PreDrawPosCalculation()
		{
			if (lastDrawFrame == RealTime.frameCount)
			{
				return;
			}
			if (lastDrawFrame < RealTime.frameCount - 1)
			{
				ResetTweenedPosToRoot();
			}
			else
			{
				lastTickSpringPos = tweenedPos;
				float tickRateMultiplier = Find.TickManager.TickRateMultiplier;
				if (tickRateMultiplier < 5f)
				{
					Vector3 vector = TweenedPosRoot() - tweenedPos;
					float num = 0.09f * (RealTime.deltaTime * 60f * tickRateMultiplier);
					if (RealTime.deltaTime > 0.05f)
					{
						num = Mathf.Min(num, 1f);
					}
					tweenedPos += vector * num;
				}
				else
				{
					tweenedPos = TweenedPosRoot();
				}
			}
			lastDrawFrame = RealTime.frameCount;
		}

		public void ResetTweenedPosToRoot()
		{
			tweenedPos = TweenedPosRoot();
			lastTickSpringPos = tweenedPos;
		}

		private Vector3 TweenedPosRoot()
		{
			if (!pawn.Spawned)
			{
				return pawn.Position.ToVector3Shifted();
			}
			float z = 0f;
			if (pawn.Spawned && pawn.ageTracker.CurLifeStage.sittingOffset.HasValue && !pawn.pather.MovingNow && pawn.GetPosture() == PawnPosture.Standing)
			{
				Building edifice = pawn.Position.GetEdifice(pawn.Map);
				if (edifice != null && edifice.def.building != null && edifice.def.building.isSittable)
				{
					z = pawn.ageTracker.CurLifeStage.sittingOffset.Value;
				}
			}
			float num = MovedPercent();
			return pawn.pather.nextCell.ToVector3Shifted() * num + pawn.Position.ToVector3Shifted() * (1f - num) + new Vector3(0f, 0f, z) + PawnCollisionTweenerUtility.PawnCollisionPosOffsetFor(pawn);
		}

		private float MovedPercent()
		{
			if (!pawn.pather.Moving)
			{
				return 0f;
			}
			if (pawn.stances.FullBodyBusy)
			{
				return 0f;
			}
			if (pawn.pather.BuildingBlockingNextPathCell() != null)
			{
				return 0f;
			}
			if (pawn.pather.NextCellDoorToWaitForOrManuallyOpen() != null)
			{
				return 0f;
			}
			if (pawn.pather.WillCollideWithPawnOnNextPathCell())
			{
				return 0f;
			}
			return 1f - pawn.pather.nextCellCostLeft / pawn.pather.nextCellCostTotal;
		}
	}
}
