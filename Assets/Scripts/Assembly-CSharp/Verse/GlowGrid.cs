using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public sealed class GlowGrid
	{
		private Map map;

		public ColorInt[] glowGrid;

		public ColorInt[] glowGridNoCavePlants;

		private bool glowGridDirty;

		private HashSet<CompGlower> litGlowers = new HashSet<CompGlower>();

		private List<IntVec3> initialGlowerLocs = new List<IntVec3>();

		public const int AlphaOfNotOverlit = 0;

		public const int AlphaOfOverlit = 1;

		private const float GameGlowLitThreshold = 0.3f;

		private const float GameGlowOverlitThreshold = 0.9f;

		private const float GroundGameGlowFactor = 3.6f;

		private const float MaxGameGlowFromNonOverlitGroundLights = 0.5f;

		public GlowGrid(Map map)
		{
			this.map = map;
			glowGrid = new ColorInt[map.cellIndices.NumGridCells];
			glowGridNoCavePlants = new ColorInt[map.cellIndices.NumGridCells];
		}

		public Color32 VisualGlowAt(IntVec3 c)
		{
			return glowGrid[map.cellIndices.CellToIndex(c)].ProjectToColor32;
		}

		public float GameGlowAt(IntVec3 c, bool ignoreCavePlants = false)
		{
			float num = 0f;
			if (!map.roofGrid.Roofed(c))
			{
				num = map.skyManager.CurSkyGlow;
				if (num == 1f)
				{
					return num;
				}
			}
			ColorInt colorInt = (ignoreCavePlants ? glowGridNoCavePlants : glowGrid)[map.cellIndices.CellToIndex(c)];
			if (colorInt.a == 1)
			{
				return 1f;
			}
			float b = (float)Mathf.Max(colorInt.r, colorInt.g, colorInt.b) / 255f * 3.6f;
			b = Mathf.Min(0.5f, b);
			return Mathf.Max(num, b);
		}

		public PsychGlow PsychGlowAt(IntVec3 c)
		{
			return PsychGlowAtGlow(GameGlowAt(c));
		}

		public static PsychGlow PsychGlowAtGlow(float glow)
		{
			if (glow > 0.9f)
			{
				return PsychGlow.Overlit;
			}
			if (glow > 0.3f)
			{
				return PsychGlow.Lit;
			}
			return PsychGlow.Dark;
		}

		public void RegisterGlower(CompGlower newGlow)
		{
			litGlowers.Add(newGlow);
			MarkGlowGridDirty(newGlow.parent.Position);
			if (Current.ProgramState != ProgramState.Playing)
			{
				initialGlowerLocs.Add(newGlow.parent.Position);
			}
		}

		public void DeRegisterGlower(CompGlower oldGlow)
		{
			litGlowers.Remove(oldGlow);
			MarkGlowGridDirty(oldGlow.parent.Position);
		}

		public void MarkGlowGridDirty(IntVec3 loc)
		{
			glowGridDirty = true;
			map.mapDrawer.MapMeshDirty(loc, MapMeshFlag.GroundGlow);
		}

		public void GlowGridUpdate_First()
		{
			if (glowGridDirty)
			{
				RecalculateAllGlow();
				glowGridDirty = false;
			}
		}

		private void RecalculateAllGlow()
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return;
			}
			if (initialGlowerLocs != null)
			{
				foreach (IntVec3 initialGlowerLoc in initialGlowerLocs)
				{
					MarkGlowGridDirty(initialGlowerLoc);
				}
				initialGlowerLocs = null;
			}
			int numGridCells = map.cellIndices.NumGridCells;
			for (int i = 0; i < numGridCells; i++)
			{
				glowGrid[i] = new ColorInt(0, 0, 0, 0);
				glowGridNoCavePlants[i] = new ColorInt(0, 0, 0, 0);
			}
			foreach (CompGlower litGlower in litGlowers)
			{
				map.glowFlooder.AddFloodGlowFor(litGlower, glowGrid);
				if (litGlower.parent.def.category != ThingCategory.Plant || !litGlower.parent.def.plant.cavePlant)
				{
					map.glowFlooder.AddFloodGlowFor(litGlower, glowGridNoCavePlants);
				}
			}
		}
	}
}
