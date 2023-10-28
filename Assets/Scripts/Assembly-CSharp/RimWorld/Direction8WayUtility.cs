using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class Direction8WayUtility
	{
		public static string LabelShort(this Direction8Way dir)
		{
			switch (dir)
			{
			case Direction8Way.North:
				return "Direction8Way_North_Short".Translate();
			case Direction8Way.NorthEast:
				return "Direction8Way_NorthEast_Short".Translate();
			case Direction8Way.East:
				return "Direction8Way_East_Short".Translate();
			case Direction8Way.SouthEast:
				return "Direction8Way_SouthEast_Short".Translate();
			case Direction8Way.South:
				return "Direction8Way_South_Short".Translate();
			case Direction8Way.SouthWest:
				return "Direction8Way_SouthWest_Short".Translate();
			case Direction8Way.West:
				return "Direction8Way_West_Short".Translate();
			case Direction8Way.NorthWest:
				return "Direction8Way_NorthWest_Short".Translate();
			default:
				return "Unknown Direction8Way";
			}
		}

		public static float AsAngle(this Direction8Way dir)
		{
			switch (dir)
			{
			case Direction8Way.North:
				return 0f;
			case Direction8Way.NorthEast:
				return 45f;
			case Direction8Way.East:
				return 90f;
			case Direction8Way.SouthEast:
				return 135f;
			case Direction8Way.South:
				return 180f;
			case Direction8Way.SouthWest:
				return 225f;
			case Direction8Way.West:
				return 270f;
			case Direction8Way.NorthWest:
				return 315f;
			default:
				return float.MaxValue;
			}
		}

		public static Vector3 AsVector(this Direction8Way dir)
		{
			return Quaternion.AngleAxis(dir.AsAngle(), Vector3.up) * Vector3.forward;
		}
	}
}
