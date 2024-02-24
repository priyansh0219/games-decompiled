using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseAddCellGhost : BaseGhost
{
	[AssertNotNull]
	public List<Base.CellType> cellTypes;

	public float maxHeightFromTerrain = 10f;

	public float minHeightFromTerrain = 1f;

	private static readonly List<KeyValuePair<Base.Face, Base.FaceType>> overrideFaces = new List<KeyValuePair<Base.Face, Base.FaceType>>();

	private static readonly Dictionary<Int3, int> score = new Dictionary<Int3, int>();

	public override void Awake()
	{
		Builder.ClampRotation(cellTypes.Count);
		base.Awake();
	}

	protected override Base.CellType GetCellType()
	{
		return cellTypes[Builder.lastRotation % cellTypes.Count];
	}

	public override void SetupGhost()
	{
		base.SetupGhost();
		Base.CellType cellType = GetCellType();
		Int3 size = Base.CellSize[(uint)cellType];
		ghostBase.SetSize(size);
		ghostBase.SetCell(Int3.zero, cellType);
		RebuildGhostGeometry();
	}

	public override bool UpdatePlacement(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
	{
		positionFound = false;
		geometryChanged = false;
		overrideFaces.Clear();
		UpdateRotation(ref geometryChanged);
		Base.CellType cellType = GetCellType();
		float placeDefaultDistance = ghostModelParentConstructableBase.placeDefaultDistance;
		Int3 @int = Base.CellSize[(uint)cellType];
		Vector3 direction = Vector3.Scale((@int - 1).ToVector3(), Base.halfCellSize);
		Vector3 position = camera.position;
		Vector3 forward = camera.forward;
		float searchDistance = 20f;
		switch (cellType)
		{
		case Base.CellType.Moonpool:
		case Base.CellType.MoonpoolRotated:
			searchDistance = 30f;
			break;
		case Base.CellType.LargeRoom:
		case Base.CellType.LargeRoomRotated:
			searchDistance = 50f;
			break;
		}
		prevTargetBase = targetBase;
		targetBase = BaseGhost.FindBase(camera, searchDistance);
		bool flag;
		if (targetBase != null)
		{
			targetBase.SetPlacementGhost(this);
			positionFound = true;
			flag = true;
			Vector3 vector = position + forward * placeDefaultDistance;
			Vector3 vector2 = targetBase.transform.TransformDirection(direction);
			Int3 cell = targetBase.WorldToGrid(vector - vector2);
			cell = Snap(cell, cellType);
			Int3 maxs = cell + @int - 1;
			Int3.Bounds bounds = new Int3.Bounds(cell, maxs);
			foreach (Int3 item in bounds)
			{
				if (targetBase.GetCell(item) != 0 || targetBase.IsCellUnderConstruction(item))
				{
					flag = false;
					break;
				}
			}
			if (flag && !allowedAboveWater)
			{
				flag = targetBase.GridToWorld(cell).y <= 0f;
			}
			if (flag)
			{
				switch (cellType)
				{
				case Base.CellType.Foundation:
				{
					Int3.Bounds bounds2 = targetBase.Bounds;
					int y = bounds2.mins.y;
					int y2 = bounds2.maxs.y;
					foreach (Int3 item2 in new Int3.Bounds(cell, new Int3(maxs.x, cell.y, maxs.z)))
					{
						Int3 current2 = item2;
						for (int num2 = cell.y - 1; num2 >= y; num2--)
						{
							current2.y = num2;
							if (targetBase.IsCellUnderConstruction(current2) || targetBase.GetCell(current2) != 0)
							{
								flag = false;
								break;
							}
						}
						if (!flag)
						{
							break;
						}
						for (int j = maxs.y + 1; j <= y2; j++)
						{
							current2.y = j;
							Base.CellType cell2 = targetBase.GetCell(current2);
							if (targetBase.IsCellUnderConstruction(current2) || cell2 == Base.CellType.Foundation || cell2 == Base.CellType.WallFoundationN || cell2 == Base.CellType.WallFoundationW || cell2 == Base.CellType.WallFoundationS || cell2 == Base.CellType.WallFoundationE || cell2 == Base.CellType.Moonpool || cell2 == Base.CellType.MoonpoolRotated)
							{
								flag = false;
								break;
							}
							if (j == maxs.y + 1 && (cell2 == Base.CellType.Observatory || cell2 == Base.CellType.MapRoom || cell2 == Base.CellType.MapRoomRotated))
							{
								flag = false;
								break;
							}
						}
						if (!flag)
						{
							break;
						}
					}
					break;
				}
				case Base.CellType.Moonpool:
				case Base.CellType.MoonpoolRotated:
					foreach (Int3 item3 in new Int3.Bounds(Int3.zero, @int - 1))
					{
						Base.CellType cell3 = targetBase.GetCell(Base.GetAdjacent(cell + item3, Base.Direction.Above));
						Base.CellType cell4 = targetBase.GetCell(Base.GetAdjacent(cell + item3, Base.Direction.Below));
						flag = flag && cell3 == Base.CellType.Empty && cell4 == Base.CellType.Empty;
					}
					if (flag && WouldHaveProhibitedCorridors())
					{
						flag = false;
					}
					if (flag && WouldCreateMoonpoolObstructions(cellType, cell))
					{
						flag = false;
					}
					break;
				case Base.CellType.Room:
				case Base.CellType.LargeRoom:
				case Base.CellType.LargeRoomRotated:
				{
					Int3 size = Base.CellSize[(uint)cellType];
					Int3 adjacent = Base.GetAdjacent(cell, Base.Direction.Below);
					bool flag2 = targetBase.GetRawCellType(adjacent) == cellType;
					bool flag3 = targetBase.CompareCellTypes(adjacent, size, Base.CellType.Empty, hasAny: false, includeGhosts: true);
					bool flag4 = targetBase.CompareCellTypes(adjacent, size, Base.sFoundationCheckTypes, hasAny: true);
					flag = flag && (flag2 || flag3 || flag4);
					if (flag && flag2)
					{
						Base.FaceType face = targetBase.GetFace(new Base.Face(adjacent, Base.Direction.Above));
						if (face == Base.FaceType.GlassDome || face == Base.FaceType.LargeGlassDome)
						{
							flag = false;
						}
					}
					if (flag && flag2)
					{
						targetBase.GetGhostFace(new Base.Face(adjacent, Base.Direction.Above), out var faceType);
						if (faceType == Base.FaceType.GlassDome || faceType == Base.FaceType.LargeGlassDome)
						{
							flag = false;
						}
					}
					Int3 adjacent2 = Base.GetAdjacent(cell, Base.Direction.Above);
					bool flag5 = targetBase.GetRawCellType(adjacent2) == cellType;
					bool flag6 = targetBase.CompareCellTypes(adjacent2, size, Base.CellType.Empty, hasAny: false, includeGhosts: true);
					flag = flag && (flag5 || flag6);
					if (flag && WouldHaveProhibitedCorridors())
					{
						flag = false;
					}
					if (flag && WouldCreateMoonpoolObstructions(cellType, cell))
					{
						flag = false;
					}
					if (flag)
					{
						if (flag5)
						{
							overrideFaces.Add(new KeyValuePair<Base.Face, Base.FaceType>(new Base.Face(Int3.zero, Base.Direction.Above), Base.FaceType.Hole));
						}
						if (flag2)
						{
							overrideFaces.Add(new KeyValuePair<Base.Face, Base.FaceType>(new Base.Face(Int3.zero, Base.Direction.Below), Base.FaceType.Hole));
						}
					}
					break;
				}
				case Base.CellType.Observatory:
					flag &= targetBase.GetCell(Base.GetAdjacent(cell, Base.Direction.Below)) == Base.CellType.Empty && targetBase.GetCell(Base.GetAdjacent(cell, Base.Direction.Above)) == Base.CellType.Empty;
					if (flag)
					{
						Base.Direction[] horizontalDirections = Base.HorizontalDirections;
						foreach (Base.Direction direction2 in horizontalDirections)
						{
							if (targetBase.GetCell(Base.GetAdjacent(cell, direction2)) == Base.CellType.Observatory)
							{
								flag = false;
								break;
							}
						}
					}
					if (flag)
					{
						int num = 0;
						if (targetBase.IsValidObsConnection(Base.GetAdjacent(cell, Base.Direction.North), Base.Direction.South))
						{
							num++;
						}
						if (targetBase.IsValidObsConnection(Base.GetAdjacent(cell, Base.Direction.East), Base.Direction.West))
						{
							num++;
						}
						if (targetBase.IsValidObsConnection(Base.GetAdjacent(cell, Base.Direction.South), Base.Direction.North))
						{
							num++;
						}
						if (targetBase.IsValidObsConnection(Base.GetAdjacent(cell, Base.Direction.West), Base.Direction.East))
						{
							num++;
						}
						if (num != 1)
						{
							flag = false;
						}
					}
					break;
				}
				if (IsBlockingHatch(bounds.mins, bounds.maxs))
				{
					flag = false;
				}
			}
			if (targetOffset != cell)
			{
				targetOffset = cell;
				ghostBase.SetCell(Int3.zero, cellType);
				for (int k = 0; k < overrideFaces.Count; k++)
				{
					KeyValuePair<Base.Face, Base.FaceType> keyValuePair = overrideFaces[k];
					ghostBase.SetFaceType(keyValuePair.Key, keyValuePair.Value);
				}
				overrideFaces.Clear();
				RebuildGhostGeometry();
				geometryChanged = true;
			}
			ghostModelParentConstructableBase.transform.position = targetBase.GridToWorld(cell);
			ghostModelParentConstructableBase.transform.rotation = targetBase.transform.rotation;
		}
		else
		{
			if (prevTargetBase != null)
			{
				SetupGhost();
				geometryChanged = true;
			}
			Bounds aaBounds = Builder.aaBounds;
			aaBounds.extents = 1.05f * aaBounds.extents;
			Vector3 vector3 = ghostModelParentConstructableBase.transform.TransformDirection(aaBounds.center);
			flag = PlaceWithBoundsCast(position, forward, aaBounds.extents, placeDefaultDistance, minHeightFromTerrain, maxHeightFromTerrain, out var center);
			ghostModelParentConstructableBase.transform.position = center - vector3;
			if (flag)
			{
				targetOffset = Int3.zero;
			}
		}
		if (flag && ghostModelParentConstructableBase.transform.position.y > float.PositiveInfinity)
		{
			flag = BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) <= 25f;
		}
		return flag;
	}

	private Int3 Snap(Int3 cell, Base.CellType cellType)
	{
		Int3 @int = Base.CellSize[(uint)cellType];
		score.Clear();
		Int3 adjacent = Base.GetAdjacent(cell, Base.Direction.Above);
		int value;
		foreach (Int3 item in Int3.Range(adjacent, adjacent + @int - 1))
		{
			if (targetBase.GetCell(item) == cellType)
			{
				Int3 key = targetBase.NormalizeCell(item);
				if (score.TryGetValue(key, out value))
				{
					score[key] = value + 1;
				}
				else
				{
					score.Add(key, 1);
				}
			}
		}
		Int3 adjacent2 = Base.GetAdjacent(cell, Base.Direction.Below);
		foreach (Int3 item2 in Int3.Range(adjacent2, adjacent2 + @int - 1))
		{
			if (targetBase.GetCell(item2) == cellType)
			{
				Int3 key2 = targetBase.NormalizeCell(item2);
				if (score.TryGetValue(key2, out value))
				{
					score[key2] = value + 1;
				}
				else
				{
					score.Add(key2, 1);
				}
			}
		}
		int num = 0;
		foreach (KeyValuePair<Int3, int> item3 in score)
		{
			if (item3.Value > num)
			{
				num = item3.Value;
				cell.x = item3.Key.x;
				cell.z = item3.Key.z;
			}
		}
		return cell;
	}

	private void UpdateRotation(ref bool geometryChanged)
	{
		if (cellTypes.Count >= 2 && Builder.UpdateRotation(cellTypes.Count))
		{
			Base.CellType cellType = GetCellType();
			Int3 @int = Base.CellSize[(uint)cellType];
			if (@int != ghostBase.GetSize())
			{
				ghostBase.ClearGeometry();
				ghostBase.SetSize(@int);
			}
			ghostBase.ClearCell(Int3.zero);
			ghostBase.SetCell(Int3.zero, cellType);
			RebuildGhostGeometry();
			geometryChanged = true;
		}
	}

	public override void FindOverlappedObjects(List<GameObject> overlappedObjects)
	{
		foreach (Int3 allCell in ghostBase.AllCells)
		{
			Base.Direction[] horizontalDirections = Base.HorizontalDirections;
			foreach (Base.Direction direction in horizontalDirections)
			{
				Int3 adjacent = Base.GetAdjacent(allCell + targetOffset, direction);
				Base.Direction direction2 = Base.ReverseDirection(direction);
				if (targetBase.GetCell(adjacent) == Base.CellType.Empty || (targetBase.GetCellConnections(adjacent) & (1 << (int)direction2)) == 0 || (ghostBase.GetCellConnections(allCell) & (1 << (int)direction)) == 0)
				{
					continue;
				}
				Base.Face face = new Base.Face(adjacent, direction2);
				Transform cellObject = targetBase.GetCellObject(face.cell);
				if (cellObject != null)
				{
					cellObject.GetComponentsInChildren(includeInactive: false, BaseGhost.explicitFaces);
				}
				foreach (BaseExplicitFace explicitFace in BaseGhost.explicitFaces)
				{
					if (explicitFace != null && explicitFace.face.HasValue && explicitFace.face.Value == face)
					{
						overlappedObjects.Add(explicitFace.gameObject);
					}
				}
			}
		}
	}
}
