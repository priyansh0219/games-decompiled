using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class WornGraphicData
	{
		public bool renderUtilityAsPack;

		public WornGraphicDirectionData north;

		public WornGraphicDirectionData south;

		public WornGraphicDirectionData east;

		public WornGraphicDirectionData west;

		public WornGraphicBodyTypeData male;

		public WornGraphicBodyTypeData female;

		public WornGraphicBodyTypeData thin;

		public WornGraphicBodyTypeData hulk;

		public WornGraphicBodyTypeData fat;

		public WornGraphicDirectionData GetDirectionalData(Rot4 facing)
		{
			switch (facing.AsInt)
			{
			case 0:
				return north;
			case 1:
				return east;
			case 2:
				return south;
			case 3:
				return west;
			default:
				throw new ArgumentException($"Unhandled Rot4: {facing}");
			}
		}

		public Vector2 BeltOffsetAt(Rot4 facing, BodyTypeDef bodyType)
		{
			WornGraphicDirectionData directionalData = GetDirectionalData(facing);
			Vector2 offset = directionalData.offset;
			if (bodyType == BodyTypeDefOf.Male)
			{
				offset += directionalData.male.offset;
			}
			else if (bodyType == BodyTypeDefOf.Female)
			{
				offset += directionalData.female.offset;
			}
			else if (bodyType == BodyTypeDefOf.Thin)
			{
				offset += directionalData.thin.offset;
			}
			else if (bodyType == BodyTypeDefOf.Hulk)
			{
				offset += directionalData.hulk.offset;
			}
			else if (bodyType == BodyTypeDefOf.Fat)
			{
				offset += directionalData.fat.offset;
			}
			return offset;
		}

		public Vector2 BeltScaleAt(Rot4 facing, BodyTypeDef bodyType)
		{
			Vector2 scale = GetDirectionalData(facing).Scale;
			if (bodyType == BodyTypeDefOf.Male)
			{
				scale *= male.Scale;
			}
			else if (bodyType == BodyTypeDefOf.Female)
			{
				scale *= female.Scale;
			}
			else if (bodyType == BodyTypeDefOf.Thin)
			{
				scale *= thin.Scale;
			}
			else if (bodyType == BodyTypeDefOf.Hulk)
			{
				scale *= hulk.Scale;
			}
			else if (bodyType == BodyTypeDefOf.Fat)
			{
				scale *= fat.Scale;
			}
			return scale;
		}
	}
}
