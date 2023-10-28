using UnityEngine;

namespace Verse
{
	public struct MoteAttachLink
	{
		private TargetInfo targetInt;

		private Vector3 offsetInt;

		private Vector3 lastDrawPosInt;

		public static readonly MoteAttachLink Invalid = new MoteAttachLink(TargetInfo.Invalid, Vector3.zero);

		public bool Linked => targetInt.IsValid;

		public TargetInfo Target => targetInt;

		public Vector3 LastDrawPos => lastDrawPosInt;

		public MoteAttachLink(TargetInfo target, Vector3 offset)
		{
			targetInt = target;
			offsetInt = offset;
			lastDrawPosInt = Vector3.zero;
			if (target.IsValid)
			{
				UpdateDrawPos();
			}
		}

		public void UpdateTarget(TargetInfo target, Vector3 offset)
		{
			targetInt = target;
			offsetInt = offset;
		}

		public void UpdateDrawPos()
		{
			if (targetInt.HasThing && targetInt.Thing.SpawnedOrAnyParentSpawned)
			{
				lastDrawPosInt = targetInt.Thing.SpawnedParentOrMe.DrawPos + offsetInt;
			}
			else
			{
				lastDrawPosInt = targetInt.Cell.ToVector3Shifted() + offsetInt;
			}
		}
	}
}
