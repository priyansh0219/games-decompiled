using System;
using System.Collections.Generic;
using UWE;
using UnityEngine;

public class Tile : MonoBehaviour
{
	public class UseInfo
	{
		public VoxelandUtils.TransformedGrid wrapped;
	}

	[Serializable]
	public class CellFace : IEquatable<CellFace>
	{
		public int x;

		public int y;

		public int z;

		public int faceNum;

		public Int3 cell => new Int3(x, y, z);

		public CubeFace face => new CubeFace(faceNum);

		public CellFace(Int3 cell, CubeFace face)
		{
			x = cell.x;
			y = cell.y;
			z = cell.z;
			faceNum = face.face;
		}

		public override string ToString()
		{
			return string.Concat(cell, "/", face);
		}

		public override int GetHashCode()
		{
			return (((12289 * 31 + x.GetHashCode()) * 31 + y.GetHashCode()) * 31 + z.GetHashCode()) * 31 + face.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as CellFace);
		}

		public bool Equals(CellFace other)
		{
			if (other != null && other.x == x && other.y == y && other.z == z)
			{
				return other.faceNum == faceNum;
			}
			return false;
		}
	}

	[Serializable]
	public class EditorEntry
	{
		public CellFace face;

		public List<string> tags = new List<string>();
	}

	public static int BlocksPerTileCell = 16;

	public static string ExitIcon = "Exit.jpg";

	public static string TileIcon = "tile.jpg";

	public static string TileSetSceneRootName = "__TILESET_SCENE_ROOT__";

	[HideInInspector]
	public Int3 sizeCells;

	[HideInInspector]
	public List<EditorEntry> editorTags = new List<EditorEntry>();

	private Dictionary<CellFace, List<string>> runtimeTags = new Dictionary<CellFace, List<string>>();

	private Int3 cachedSizeBlocks = new Int3(0, 0, 0);

	public bool doNotUse;

	public bool isTUXOIL = true;

	public TUXOIL.TileType tileType;

	public Voxeland land;

	public void RuntimeInit()
	{
		sizeCells = GetSizeCells();
	}

	private EditorEntry GetEntry(CellFace cf)
	{
		foreach (EditorEntry editorTag in editorTags)
		{
			if (editorTag.face.Equals(cf))
			{
				return editorTag;
			}
		}
		return null;
	}

	public List<string> GetEditorTags(CellFace cf)
	{
		EditorEntry entry = GetEntry(cf);
		if (entry != null)
		{
			return entry.tags;
		}
		entry = new EditorEntry();
		entry.face = cf;
		editorTags.Add(entry);
		return entry.tags;
	}

	public List<string> GetTags(CellFace globalCellFace, Int3 globalOffsetCells, byte globalTurns)
	{
		Int3 tileSize = GetCachedSizeBlocks() / BlocksPerTileCell;
		Int3 cell = Int3.InverseTileTransform(globalCellFace.cell, tileSize, globalOffsetCells, globalTurns);
		CubeFace face = globalCellFace.face.RotateXZ(4 - globalTurns);
		CellFace key = new CellFace(cell, face);
		if (runtimeTags.ContainsKey(key))
		{
			return runtimeTags[key];
		}
		return null;
	}

	public Voxeland GetVoxeland()
	{
		return land;
	}

	public VoxelandData GetVoxelandData()
	{
		if (GetVoxeland() != null)
		{
			return GetVoxeland().data;
		}
		return null;
	}

	public bool IsUseable()
	{
		return GetVoxelandData() != null;
	}

	public Int3 GetCachedSizeBlocks()
	{
		return cachedSizeBlocks;
	}

	public Int3 GetSizeCells()
	{
		return GetSizeBlocks() / BlocksPerTileCell;
	}

	public bool IsBasic()
	{
		if (GetSizeCells().x == 1)
		{
			return GetSizeCells().z == 1;
		}
		return false;
	}

	public Int3 GetSizeBlocks()
	{
		VoxelandData voxelandData = GetVoxelandData();
		if (voxelandData != null)
		{
			return new Int3(voxelandData.sizeX, voxelandData.sizeY, voxelandData.sizeZ);
		}
		Debug.LogError("Tile.GetSizeBlocks called, but tile did not have voxeland data! Game object name = " + base.gameObject.name);
		return new Int3(0, 0, 0);
	}

	public static bool AreTagsCompatible(List<string> a, List<string> b)
	{
		bool flag = a == null || a.Count == 0;
		bool flag2 = b == null || b.Count == 0;
		if (flag != flag2)
		{
			return false;
		}
		if (flag && flag2)
		{
			return true;
		}
		if (a.Count > 1 || b.Count > 1)
		{
			Debug.LogError("We don't support multiple tags per face yet....steve should get rid of this possibility.");
			return false;
		}
		string text = a[0];
		string text2 = b[0];
		if ((text[0] == '-' && text2[0] == '-') || (text[0] == '+' && text2[0] == '+'))
		{
			return false;
		}
		if ((text[0] == '+' && text2[0] == '-') || (text[0] == '-' && text2[0] == '+'))
		{
			return text.Substring(1) == text2.Substring(1);
		}
		return text == text2;
	}

	public void InstantiateFull(Voxeland outLand, Int3 bsOrigin, byte ccwRots, UseInfo ui)
	{
		Instantiate(outLand, bsOrigin, ccwRots, ui);
		InstantiateGameplay(bsOrigin, ccwRots, ui);
	}

	public void Instantiate(Voxeland outLand, Int3 bsOrigin, byte ccwRots, UseInfo ui = null)
	{
		Voxeland voxeland = GetVoxeland();
		VoxelandUtils.TransformedGrid transformedGrid = null;
		if (ui != null)
		{
			transformedGrid = ui.wrapped;
		}
		else
		{
			transformedGrid = new VoxelandUtils.TransformedGrid();
			transformedGrid.src = voxeland.data;
			transformedGrid.SetSize(voxeland.data);
			transformedGrid.typeMap = outLand.MergeTypes(voxeland.types);
		}
		transformedGrid.ofs = bsOrigin;
		transformedGrid.ccwRotations = ccwRots;
		Int3 @int = new Int3(voxeland.data.sizeX, voxeland.data.sizeY, voxeland.data.sizeZ);
		if (ccwRots % 2 == 1)
		{
			@int = @int.ZYX();
		}
		outLand.data.SetForRange(transformedGrid, bsOrigin.x, bsOrigin.y, bsOrigin.z, @int.x, @int.y, @int.z);
	}

	public void InstantiateGameplay(Int3 bsOrigin, byte ccwRots, UseInfo ui)
	{
		Voxeland voxeland = GetVoxeland();
		Int3 @int = new Int3(voxeland.data.sizeX, voxeland.data.sizeY, voxeland.data.sizeZ);
		if (ccwRots % 2 == 1)
		{
			@int = @int.ZYX();
		}
		Quaternion.Euler(0f, (float)(int)ccwRots * -90f, 0f);
		_ = bsOrigin.ToVector3() + @int.ToVector3() / 2f;
	}
}
