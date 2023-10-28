using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class GeneGraphicData
	{
		[NoTranslate]
		public string graphicPath;

		[NoTranslate]
		public string graphicPathFemale;

		[NoTranslate]
		public List<string> graphicPaths;

		public GeneColorType colorType;

		public Color? color;

		public float colorRGBPostFactor = 1f;

		public float drawScale = 1f;

		public bool drawWhileDessicated;

		public bool visibleNorth = true;

		public bool useSkinShader;

		public bool drawIfFaceCovered;

		public bool skinIsHairColor;

		public bool tattoosVisible = true;

		public FurDef fur;

		public GeneDrawLoc drawLoc = GeneDrawLoc.HeadMiddle;

		public GeneDrawLayer layer;

		public bool drawNorthAfterHair;

		public bool drawOnEyes;

		private Vector3 drawOffset = Vector3.zero;

		private Vector3? drawOffsetNorth;

		private Vector3? drawOffsetSouth;

		private Vector3? drawOffsetEast;

		public float narrowCrownHorizontalOffset;

		private string GraphicPathFor(Pawn pawn)
		{
			if (!graphicPaths.NullOrEmpty())
			{
				return graphicPaths[pawn.thingIDNumber % graphicPaths.Count];
			}
			if (pawn.gender == Gender.Female && !graphicPathFemale.NullOrEmpty())
			{
				return graphicPathFemale;
			}
			return graphicPath;
		}

		private Color GetColorFor(Pawn pawn)
		{
			Color color;
			switch (colorType)
			{
			case GeneColorType.Hair:
				color = pawn.story.HairColor;
				break;
			case GeneColorType.Skin:
				color = pawn.story.SkinColor;
				break;
			default:
				color = this.color ?? Color.white;
				break;
			}
			return color * colorRGBPostFactor;
		}

		public (Graphic, Graphic) GetGraphics(Pawn pawn, Shader skinShader, Color rottingColor)
		{
			Shader shader = (useSkinShader ? skinShader : ShaderDatabase.Transparent);
			string path = GraphicPathFor(pawn);
			Graphic item = GraphicDatabase.Get<Graphic_Multi>(color: GetColorFor(pawn), path: path, shader: shader, drawSize: Vector2.one, colorTwo: Color.white);
			Graphic item2 = GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one, color ?? rottingColor, Color.white);
			return (item, item2);
		}

		public Vector3 DrawOffsetAt(Rot4 rot)
		{
			switch (rot.AsInt)
			{
			case 0:
				return drawOffsetNorth ?? drawOffset;
			case 1:
				return drawOffsetEast ?? drawOffset;
			case 2:
				return drawOffsetSouth ?? drawOffset;
			case 3:
			{
				Vector3 result = drawOffsetEast ?? drawOffset;
				result.x *= -1f;
				return result;
			}
			default:
				return Vector3.zero;
			}
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (!graphicPaths.NullOrEmpty())
			{
				if (!graphicPath.NullOrEmpty())
				{
					yield return "defines both graphicPaths and graphicPath.";
				}
				if (!graphicPathFemale.NullOrEmpty())
				{
					yield return "defines both graphicPaths and graphicPathFemale.";
				}
			}
		}
	}
}
