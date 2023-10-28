using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class FactionRelationKindUtility
	{
		public static string GetLabel(this FactionRelationKind kind)
		{
			switch (kind)
			{
			case FactionRelationKind.Hostile:
				return "HostileLower".Translate();
			case FactionRelationKind.Neutral:
				return "NeutralLower".Translate();
			case FactionRelationKind.Ally:
				return "AllyLower".Translate();
			default:
				return "error";
			}
		}

		public static string GetLabelCap(this FactionRelationKind kind)
		{
			switch (kind)
			{
			case FactionRelationKind.Hostile:
				return "Hostile".Translate();
			case FactionRelationKind.Neutral:
				return "Neutral".Translate();
			case FactionRelationKind.Ally:
				return "Ally".Translate();
			default:
				return "error";
			}
		}

		public static Color GetColor(this FactionRelationKind kind)
		{
			switch (kind)
			{
			case FactionRelationKind.Hostile:
				return ColorLibrary.RedReadable;
			case FactionRelationKind.Neutral:
				return new Color(0f, 0.75f, 1f);
			case FactionRelationKind.Ally:
				return Color.green;
			default:
				return Color.white;
			}
		}
	}
}
