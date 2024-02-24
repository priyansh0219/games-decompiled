using System;
using System.Collections.Generic;
using System.IO;
using UWE;
using UnityEngine;

public class TectonicPlates
{
	private FiniteGrid<int> grid = new FiniteGrid<int>();

	private List<int> plate2count = new List<int>();

	private FiniteGrid<int> grid2frontId = new FiniteGrid<int>();

	private List<Int2> frontCellList = new List<Int2>();

	private List<Int2> _filledNbors = new List<Int2>();

	private static Color[] DebugPlateColors = new Color[5]
	{
		Color.red,
		Color.green,
		Color.yellow,
		Color.cyan,
		Color.black
	};

	public FiniteGrid<int> GetGrid()
	{
		return grid;
	}

	public Int2 NormToCell(Vector2 uv)
	{
		int x = Mathf.FloorToInt(uv.x * (float)grid.xRes);
		int y = Mathf.FloorToInt(uv.y * (float)grid.yRes);
		return new Int2(x, y);
	}

	public Vector2 GetCellCenterUV(Int2 u)
	{
		return new Vector2(((float)u.x + 0.5f) / (float)grid.xRes, ((float)u.y + 0.5f) / (float)grid.yRes);
	}

	public int Get(Int2 u)
	{
		return Get(u.x, u.y);
	}

	public int Get(int x, int y)
	{
		if (!grid.IsInbound(x, y))
		{
			return -1;
		}
		return grid.Get(x, y);
	}

	public int Get(float u, float v)
	{
		Int2 @int = NormToCell(new Vector2(u, v));
		return Get(@int.x, @int.y);
	}

	public int Get(Vector2 uv)
	{
		Int2 @int = NormToCell(uv);
		return Get(@int.x, @int.y);
	}

	public int GetNumPlateCells(int pid)
	{
		return plate2count[pid];
	}

	public void Set(int x, int y, int plateId)
	{
		x = x.Clamp(0, grid.xRes - 1);
		y = y.Clamp(0, grid.yRes - 1);
		int num = grid.Get(x, y);
		if (num == plateId)
		{
			return;
		}
		Int2 @int = new Int2(x, y);
		grid.Set(@int, plateId);
		if (plateId != -1)
		{
			if (num == -1 && grid2frontId[@int] != -1)
			{
				int num2 = grid2frontId[@int];
				grid2frontId[@int] = -1;
				Int2 last = frontCellList.GetLast();
				frontCellList[num2] = last;
				frontCellList.RemoveAt(frontCellList.Count - 1);
				grid2frontId[last] = num2;
			}
			for (int i = 0; i < 4; i++)
			{
				Int2 int2 = @int.Get4Nbor(i);
				if (grid.IsInbound(int2) && grid.Get(int2) == -1 && grid2frontId[int2] == -1)
				{
					frontCellList.Add(int2);
					grid2frontId[int2] = frontCellList.Count - 1;
				}
			}
		}
		else
		{
			Debug.LogError("Not handling this case yet! We didn't need it for world generation");
		}
		while (plateId >= plate2count.Count)
		{
			plate2count.Add(0);
		}
		if (num != -1)
		{
			plate2count[num]--;
		}
		if (plateId != -1)
		{
			plate2count[plateId]++;
		}
	}

	public void Set(Vector2 uv, int plateId)
	{
		Int2 @int = NormToCell(uv);
		Set(@int.x, @int.y, plateId);
	}

	public void Set(float u, float v, int plateId)
	{
		Set(new Vector2(u, v), plateId);
	}

	public void Reset(int xRes, int yRes)
	{
		grid.Reset(xRes, yRes);
		grid2frontId.Reset(xRes, yRes);
		frontCellList.Clear();
		for (int i = 0; i < grid.xRes; i++)
		{
			for (int j = 0; j < grid.yRes; j++)
			{
				grid[i, j] = -1;
				grid2frontId[i, j] = -1;
			}
		}
	}

	private int GetDX(int direction)
	{
		switch (direction)
		{
		case 0:
			return 0;
		case 1:
			return 1;
		case 2:
			return 0;
		default:
			return -1;
		}
	}

	private int GetDY(int direction)
	{
		switch (direction)
		{
		case 0:
			return 1;
		case 1:
			return 0;
		case 2:
			return -1;
		default:
			return 0;
		}
	}

	public bool SpreadStep()
	{
		if (frontCellList.Count == 0)
		{
			return true;
		}
		Int2 random = frontCellList.GetRandom();
		_filledNbors.Clear();
		for (int i = 0; i < 4; i++)
		{
			Int2 @int = random.Get4Nbor(i);
			if (grid.IsInbound(@int) && grid.Get(@int) != -1)
			{
				_filledNbors.Add(@int);
			}
		}
		Int2 random2 = _filledNbors.GetRandom();
		Set(random.x, random.y, grid.Get(random2));
		if (frontCellList.Count == 0)
		{
			frontCellList.Capacity = 0;
			grid2frontId = null;
			return true;
		}
		return false;
	}

	public bool GetAreAllCellsSet()
	{
		return frontCellList.Count == 0;
	}

	public bool IsUniformAround(float u, float v, float uStep, float vStep, float radius, bool ignoreBorder, List<int> plate2type)
	{
		if (Get(u, v) == -1)
		{
			return false;
		}
		int num = plate2type[Get(u, v)];
		for (float num2 = 0f - radius; num2 <= radius; num2 += uStep)
		{
			for (float num3 = 0f - radius; num3 <= radius; num3 += vStep)
			{
				if (num2 * num2 + num3 * num3 > radius * radius)
				{
					continue;
				}
				float u2 = u + num2;
				float v2 = v + num3;
				int num4 = Get(u2, v2);
				if (!ignoreBorder || num4 != -1)
				{
					if (num4 == -1)
					{
						return false;
					}
					if (plate2type[num4] != num)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public bool IsCloseToEdge(Int2 center, int radialCellCount)
	{
		return !grid.ForNeighborhood(center, radialCellCount, (int x, int y, int plateId) => true);
	}

	public bool IsUniformAround(Int2 center, int radialCellCount, List<int> plate2type)
	{
		if (!grid.IsInbound(center))
		{
			return false;
		}
		int centerPlateType = ((plate2type == null) ? grid.Get(center) : plate2type[grid.Get(center)]);
		bool foundDiff = false;
		if (grid.ForNeighborhood(center, radialCellCount, delegate(int x, int y, int plateId)
		{
			if (((plate2type == null) ? grid.Get(x, y) : plate2type[grid.Get(x, y)]) != centerPlateType)
			{
				foundDiff = true;
				return false;
			}
			return true;
		}))
		{
			return !foundDiff;
		}
		return false;
	}

	public bool IsUniformAround(float u, float v, int radialCellCount, List<int> plate2type)
	{
		Int2 center = NormToCell(new Vector2(u, v));
		return IsUniformAround(center, radialCellCount, plate2type);
	}

	public void ComputeDistanceField(FiniteGrid<float> field, List<int> plate2type)
	{
		field.Reset(grid.xRes, grid.yRes);
		Queue<Int2> queue = new Queue<Int2>();
		for (int i = 0; i < grid.xRes; i++)
		{
			for (int j = 0; j < grid.yRes; j++)
			{
				Int2 @int = new Int2(i, j);
				if (!IsCloseToEdge(@int, 1))
				{
					if (!IsUniformAround(@int, 1, plate2type))
					{
						field.Set(@int, 0f);
						queue.Enqueue(@int);
					}
					else
					{
						field.Set(@int, float.PositiveInfinity);
					}
				}
			}
		}
		GridUtils.UpdateDistanceField(field, queue);
	}

	public void Write(StreamWriter writer, BinaryWriter binWriter)
	{
		writer.WriteLine("tectonics");
		writer.WriteLine(grid[0, 0]);
		GridUtils.Write(binWriter, grid);
	}

	public bool Read(StreamReader reader, BinaryReader binReader)
	{
		if (reader.ReadLine() != "tectonics")
		{
			Debug.LogError("Expected to read line 'tectonics' - input is corrupted.");
			return false;
		}
		Convert.ToInt32(reader.ReadLine());
		GridUtils.Read(binReader, grid);
		return true;
	}

	public void SaveProgressPNG(string pngPath)
	{
		Debug.Log("saving to " + pngPath + ", frontcells count = " + frontCellList.Count);
		grid.SaveToPNG(pngPath, delegate(Int2 u, int pid)
		{
			if (grid2frontId[u] != -1)
			{
				return Color.white;
			}
			return (pid == -1) ? Color.black : DebugPlateColors.GetWrap(pid);
		});
	}

	public void SavePNG(string pngPath)
	{
		Debug.Log("saving to " + pngPath + ", frontcells count = " + frontCellList.Count);
		grid.SaveToPNG(pngPath, delegate(Int2 u, int pid)
		{
			for (int i = 0; i < 8; i++)
			{
				Int2 p = u.Get8Nbor(i);
				if (grid.IsInbound(p) && grid[p] != pid)
				{
					return Color.white;
				}
			}
			return Color.black;
		});
	}
}
