using UnityEngine;

namespace Verse
{
	public static class HumanlikeMeshPoolUtility
	{
		public static float HumanlikeBodyWidthForPawn(Pawn pawn)
		{
			if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.bodyWidth.HasValue)
			{
				return pawn.ageTracker.CurLifeStage.bodyWidth.Value;
			}
			return 1.5f;
		}

		public static GraphicMeshSet GetHumanlikeBodySetForPawn(Pawn pawn)
		{
			if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.bodyWidth.HasValue)
			{
				return MeshPool.GetMeshSetForWidth(pawn.ageTracker.CurLifeStage.bodyWidth.Value);
			}
			return MeshPool.humanlikeBodySet;
		}

		public static GraphicMeshSet GetHumanlikeHeadSetForPawn(Pawn pawn)
		{
			if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.bodyWidth.HasValue)
			{
				return MeshPool.GetMeshSetForWidth(pawn.ageTracker.CurLifeStage.bodyWidth.Value);
			}
			return MeshPool.humanlikeHeadSet;
		}

		public static GraphicMeshSet GetHumanlikeHairSetForPawn(Pawn pawn)
		{
			Vector2 hairMeshSize = pawn.story.headType.hairMeshSize;
			if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.headSizeFactor.HasValue)
			{
				hairMeshSize *= pawn.ageTracker.CurLifeStage.headSizeFactor.Value;
			}
			return MeshPool.GetMeshSetForWidth(hairMeshSize.x, hairMeshSize.y);
		}

		public static GraphicMeshSet GetHumanlikeBeardSetForPawn(Pawn pawn)
		{
			Vector2 beardMeshSize = pawn.story.headType.beardMeshSize;
			if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.headSizeFactor.HasValue)
			{
				beardMeshSize *= pawn.ageTracker.CurLifeStage.headSizeFactor.Value;
			}
			return MeshPool.GetMeshSetForWidth(beardMeshSize.x, beardMeshSize.y);
		}

		public static GraphicMeshSet GetSwaddledBabySet()
		{
			return MeshPool.humanlikeSwaddledBaby;
		}
	}
}
