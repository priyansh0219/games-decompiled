using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class GasGrid : IExposable
	{
		private int[] gasDensity;

		private Map map;

		private int cycleIndexDiffusion;

		private int cycleIndexDissipation;

		[Unsaved(false)]
		private List<IntVec3> cardinalDirections;

		[Unsaved(false)]
		private List<IntVec3> cellsInRandomOrder;

		[Unsaved(false)]
		private bool anyGasEverAdded;

		public const int MaxGasPerCell = 255;

		private const float CellsToDissipatePerTickFactor = 1f / 64f;

		private const float CellsToDiffusePerTickFactor = 1f / 32f;

		private const float MaxOverflowFloodfillRadius = 40f;

		private const int DissipationAmount_BlindSmoke = 4;

		private const int DissipationAmount_ToxGas = 3;

		private const int DissipationAmount_RotStink = 4;

		private const int MinDiffusion = 17;

		private const int AnyGasCheckIntervalTicks = 600;

		private static readonly FloatRange AlphaRange = new FloatRange(0.2f, 0.8f);

		private static Color SmokeColor = new Color32(200, 200, 200, byte.MaxValue);

		private static Color ToxColor = new Color32(180, 214, 24, byte.MaxValue);

		private static Color RotColor = new Color32(214, 90, 24, byte.MaxValue);

		public bool CalculateGasEffects => anyGasEverAdded;

		public GasGrid(Map map)
		{
			this.map = map;
			gasDensity = new int[map.cellIndices.NumGridCells];
			cardinalDirections = new List<IntVec3>();
			cardinalDirections.AddRange(GenAdj.CardinalDirections);
			cycleIndexDiffusion = Rand.Range(0, map.Area / 2);
		}

		public void RecalculateEverHadGas()
		{
			anyGasEverAdded = false;
			for (int i = 0; i < gasDensity.Length; i++)
			{
				if (gasDensity[i] > 0)
				{
					anyGasEverAdded = true;
					break;
				}
			}
		}

		public void Tick()
		{
			if (!CalculateGasEffects)
			{
				return;
			}
			int area = map.Area;
			int num = Mathf.CeilToInt((float)area * (1f / 64f));
			cellsInRandomOrder = map.cellsInRandomOrder.GetAll();
			for (int i = 0; i < num; i++)
			{
				if (cycleIndexDissipation >= area)
				{
					cycleIndexDissipation = 0;
				}
				TryDissipateGasses(CellIndicesUtility.CellToIndex(cellsInRandomOrder[cycleIndexDissipation], map.Size.x));
				cycleIndexDissipation++;
			}
			num = Mathf.CeilToInt((float)area * (1f / 32f));
			for (int j = 0; j < num; j++)
			{
				if (cycleIndexDiffusion >= area)
				{
					cycleIndexDiffusion = 0;
				}
				TryDiffuseGasses(cellsInRandomOrder[cycleIndexDiffusion]);
				cycleIndexDiffusion++;
			}
			if (map.IsHashIntervalTick(600))
			{
				RecalculateEverHadGas();
			}
		}

		public bool AnyGasAt(IntVec3 cell)
		{
			return AnyGasAt(CellIndicesUtility.CellToIndex(cell, map.Size.x));
		}

		private bool AnyGasAt(int idx)
		{
			return gasDensity[idx] > 0;
		}

		public byte DensityAt(IntVec3 cell, GasType gasType)
		{
			return DensityAt(CellIndicesUtility.CellToIndex(cell, map.Size.x), gasType);
		}

		private byte DensityAt(int index, GasType gasType)
		{
			return (byte)((uint)(gasDensity[index] >> (int)gasType) & 0xFFu);
		}

		public float DensityPercentAt(IntVec3 cell, GasType gasType)
		{
			return (float)(int)DensityAt(cell, gasType) / 255f;
		}

		public void AddGas(IntVec3 cell, GasType gasType, int amount, bool canOverflow = true)
		{
			if (amount <= 0 || !GasCanMoveTo(cell))
			{
				return;
			}
			anyGasEverAdded = true;
			int index = CellIndicesUtility.CellToIndex(cell, map.Size.x);
			byte b = DensityAt(index, GasType.BlindSmoke);
			byte b2 = DensityAt(index, GasType.ToxGas);
			byte b3 = DensityAt(index, GasType.RotStink);
			int overflow = 0;
			switch (gasType)
			{
			case GasType.BlindSmoke:
				b = AdjustedDensity(b + amount, out overflow);
				break;
			case GasType.ToxGas:
				if (!ModLister.CheckBiotech("Tox gas"))
				{
					return;
				}
				b2 = AdjustedDensity(b2 + amount, out overflow);
				break;
			case GasType.RotStink:
				b3 = AdjustedDensity(b3 + amount, out overflow);
				break;
			default:
				Log.Error("Trying to add unknown gas type.");
				return;
			}
			SetDirect(index, b, b2, b3);
			map.mapDrawer.MapMeshDirty(cell, MapMeshFlag.Gas);
			if (canOverflow && overflow > 0)
			{
				Overflow(cell, gasType, overflow);
			}
		}

		private byte AdjustedDensity(int newDensity, out int overflow)
		{
			if (newDensity > 255)
			{
				overflow = newDensity - 255;
				return byte.MaxValue;
			}
			overflow = 0;
			if (newDensity < 0)
			{
				return 0;
			}
			return (byte)newDensity;
		}

		public Color ColorAt(IntVec3 cell)
		{
			int index = CellIndicesUtility.CellToIndex(cell, map.Size.x);
			float num = (int)DensityAt(index, GasType.BlindSmoke);
			float num2 = (int)DensityAt(index, GasType.ToxGas);
			float num3 = (int)DensityAt(index, GasType.RotStink);
			float num4 = num + num2 + num3;
			Color result = SmokeColor * (num / num4) + (ToxColor * (num2 / num4) + RotColor * (num3 / num4));
			result.a = AlphaRange.LerpThroughRange(num4 / 765f);
			return result;
		}

		public void Notify_ThingSpawned(Thing thing)
		{
			if (!thing.Spawned || thing.def.Fillage != FillCategory.Full)
			{
				return;
			}
			foreach (IntVec3 item in thing.OccupiedRect())
			{
				if (AnyGasAt(item))
				{
					gasDensity[CellIndicesUtility.CellToIndex(item, map.Size.x)] = 0;
					map.mapDrawer.MapMeshDirty(item, MapMeshFlag.Gas);
				}
			}
		}

		private void SetDirect(int index, byte smoke, byte toxic, byte rotStink)
		{
			if (!ModsConfig.BiotechActive)
			{
				toxic = 0;
			}
			gasDensity[index] = (rotStink << 16) | (toxic << 8) | smoke;
		}

		private void Overflow(IntVec3 cell, GasType gasType, int amount)
		{
			if (amount <= 0)
			{
				return;
			}
			int remainingAmount = amount;
			map.floodFiller.FloodFill(cell, (IntVec3 c) => GasCanMoveTo(c), delegate(IntVec3 c)
			{
				int num = Mathf.Min(remainingAmount, 255 - DensityAt(c, gasType));
				if (num > 0)
				{
					AddGas(c, gasType, num, canOverflow: false);
					remainingAmount -= num;
				}
				return remainingAmount <= 0;
			}, GenRadial.NumCellsInRadius(40f), rememberParents: true);
		}

		private void TryDissipateGasses(int index)
		{
			if (!AnyGasAt(index))
			{
				return;
			}
			bool flag = false;
			int num = DensityAt(index, GasType.BlindSmoke);
			if (num > 0)
			{
				num = Math.Max(num - 4, 0);
				if (num == 0)
				{
					flag = true;
				}
			}
			int num2 = DensityAt(index, GasType.ToxGas);
			if (num2 > 0)
			{
				num2 = Math.Max(num2 - 3, 0);
				if (num2 == 0)
				{
					flag = true;
				}
			}
			int num3 = DensityAt(index, GasType.RotStink);
			if (num3 > 0)
			{
				num3 = Math.Max(num3 - 4, 0);
				if (num3 == 0)
				{
					flag = true;
				}
			}
			SetDirect(index, (byte)num, (byte)num2, (byte)num3);
			if (flag)
			{
				map.mapDrawer.MapMeshDirty(CellIndicesUtility.IndexToCell(index, map.Size.x), MapMeshFlag.Gas);
			}
		}

		private void TryDiffuseGasses(IntVec3 cell)
		{
			int index = CellIndicesUtility.CellToIndex(cell, map.Size.x);
			int gasA = DensityAt(index, GasType.ToxGas);
			int gasA2 = DensityAt(index, GasType.RotStink);
			if (gasA + gasA2 < 17)
			{
				return;
			}
			bool flag = false;
			cardinalDirections.Shuffle();
			for (int i = 0; i < cardinalDirections.Count; i++)
			{
				IntVec3 intVec = cell + cardinalDirections[i];
				if (!GasCanMoveTo(intVec))
				{
					continue;
				}
				int index2 = CellIndicesUtility.CellToIndex(intVec, map.Size.x);
				int gasB = DensityAt(index2, GasType.ToxGas);
				int gasB2 = DensityAt(index2, GasType.RotStink);
				if (false | TryDiffuseIndividualGas(ref gasA, ref gasB) | TryDiffuseIndividualGas(ref gasA2, ref gasB2))
				{
					SetDirect(index2, DensityAt(index2, GasType.BlindSmoke), (byte)gasB, (byte)gasB2);
					map.mapDrawer.MapMeshDirty(intVec, MapMeshFlag.Gas);
					flag = true;
					if (gasA + gasA2 < 17)
					{
						break;
					}
				}
			}
			if (flag)
			{
				SetDirect(index, DensityAt(index, GasType.BlindSmoke), (byte)gasA, (byte)gasA2);
				map.mapDrawer.MapMeshDirty(cell, MapMeshFlag.Gas);
			}
		}

		private bool TryDiffuseIndividualGas(ref int gasA, ref int gasB)
		{
			if (gasA < 17)
			{
				return false;
			}
			int num = Mathf.Abs(gasA - gasB) / 2;
			if (gasA > gasB && num >= 17)
			{
				gasA -= num;
				gasB += num;
				return true;
			}
			return false;
		}

		public bool GasCanMoveTo(IntVec3 cell)
		{
			if (!cell.InBounds(map))
			{
				return false;
			}
			if (cell.Filled(map))
			{
				return cell.GetDoor(map)?.Open ?? false;
			}
			return true;
		}

		public void EqualizeGasThroughBuilding(Building b, bool twoWay)
		{
			if (!CalculateGasEffects)
			{
				return;
			}
			IntVec3[] beqCells = new IntVec3[4];
			for (int i = 0; i < beqCells.Length; i++)
			{
				beqCells[i] = IntVec3.Invalid;
			}
			int beqCellCount = 0;
			int totalRotStink = 0;
			int totalToxGas = 0;
			if (twoWay)
			{
				for (int j = 0; j < 2; j++)
				{
					IntVec3 cell2 = ((j == 0) ? (b.Position + b.Rotation.FacingCell) : (b.Position - b.Rotation.FacingCell));
					VisitCell(cell2);
				}
			}
			else
			{
				for (int k = 0; k < 4; k++)
				{
					IntVec3 cell3 = b.Position + GenAdj.CardinalDirections[k];
					VisitCell(cell3);
				}
			}
			if (beqCellCount <= 1)
			{
				return;
			}
			byte toxic = (byte)Mathf.Min(totalToxGas / beqCellCount, 255);
			byte rotStink = (byte)Mathf.Min(totalRotStink / beqCellCount, 255);
			for (int l = 0; l < beqCellCount; l++)
			{
				if (beqCells[l].IsValid)
				{
					SetDirect(map.cellIndices.CellToIndex(beqCells[l]), DensityAt(beqCells[l], GasType.BlindSmoke), toxic, rotStink);
					map.mapDrawer.MapMeshDirty(beqCells[l], MapMeshFlag.Gas);
				}
			}
			void VisitCell(IntVec3 cell)
			{
				if (cell.IsValid && GasCanMoveTo(cell))
				{
					if (AnyGasAt(cell))
					{
						totalRotStink += DensityAt(cell, GasType.RotStink);
						if (ModsConfig.BiotechActive)
						{
							totalToxGas += DensityAt(cell, GasType.ToxGas);
						}
					}
					beqCells[beqCellCount] = cell;
					beqCellCount++;
				}
			}
		}

		public void Debug_ClearAll()
		{
			for (int i = 0; i < gasDensity.Length; i++)
			{
				gasDensity[i] = 0;
			}
			anyGasEverAdded = false;
			map.mapDrawer.WholeMapChanged(MapMeshFlag.Gas);
		}

		public void Debug_FillAll()
		{
			for (int i = 0; i < gasDensity.Length; i++)
			{
				if (GasCanMoveTo(map.cellIndices.IndexToCell(i)))
				{
					SetDirect(i, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				}
			}
			anyGasEverAdded = true;
			map.mapDrawer.WholeMapChanged(MapMeshFlag.Gas);
		}

		public void ExposeData()
		{
			MapExposeUtility.ExposeInt(map, (IntVec3 c) => gasDensity[map.cellIndices.CellToIndex(c)], delegate(IntVec3 c, int val)
			{
				gasDensity[map.cellIndices.CellToIndex(c)] = val;
			}, "gasDensity");
			Scribe_Values.Look(ref cycleIndexDiffusion, "cycleIndexDiffusion", 0);
			Scribe_Values.Look(ref cycleIndexDissipation, "cycleIndexDissipation", 0);
		}
	}
}
