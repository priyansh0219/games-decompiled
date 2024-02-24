using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseAddCorridorGhost : BaseGhost
{
	public enum Corridor
	{
		I = 0,
		L = 1,
		T = 2,
		X = 3
	}

	public bool isGlass;

	public Corridor corridor;

	public float maxHeightFromTerrain = 10f;

	public float minHeightFromTerrain = 1f;

	private int corridorType;

	private static bool[,] Shapes = new bool[4, 4]
	{
		{ true, false, true, false },
		{ true, true, false, false },
		{ true, true, false, true },
		{ true, true, true, true }
	};

	public override void Awake()
	{
		Builder.ClampRotation(4);
		base.Awake();
	}

	protected override Base.CellType GetCellType()
	{
		return Base.CellType.Corridor;
	}

	public override void SetupGhost()
	{
		base.SetupGhost();
		ghostBase.SetSize(new Int3(1));
		corridorType = CalculateCorridorType();
		ghostBase.SetCorridor(Int3.zero, corridorType, isGlass);
		RebuildGhostGeometry();
	}

	public override bool UpdatePlacement(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
	{
		positionFound = false;
		geometryChanged = false;
		UpdateRotation(ref geometryChanged);
		float placeDefaultDistance = ghostModelParentConstructableBase.placeDefaultDistance;
		Vector3 position = camera.position;
		Vector3 forward = camera.forward;
		targetBase = BaseGhost.FindBase(camera);
		bool flag;
		if (targetBase != null)
		{
			targetBase.SetPlacementGhost(this);
			positionFound = true;
			flag = true;
			Vector3 point = position + forward * placeDefaultDistance;
			Int3 size = new Int3(1);
			Int3 @int = targetBase.WorldToGrid(point);
			if (targetBase.GetCell(@int) != 0 || targetBase.IsCellUnderConstruction(@int))
			{
				@int = targetBase.PickCell(camera, point, size);
			}
			if (targetBase.GetCell(@int) != 0 && targetBase.WorldToGrid(camera.position) == @int && targetBase.PickFace(camera, out var face))
			{
				@int = Base.GetAdjacent(face);
			}
			int y = targetBase.Bounds.mins.y;
			Int3 cell = @int;
			if (!CheckCorridorConnection(@int, out var adjoinedFacesAreUsed))
			{
				for (int num = @int.y - 1; num >= y; num--)
				{
					cell.y = num;
					if (targetBase.IsCellUnderConstruction(cell))
					{
						flag = false;
						break;
					}
					if (targetBase.GetCell(cell) != 0)
					{
						if (num < @int.y - 1)
						{
							flag = false;
						}
						break;
					}
				}
			}
			else if (adjoinedFacesAreUsed)
			{
				flag = false;
			}
			if (!targetBase.HasSpaceFor(@int, size))
			{
				flag = false;
			}
			Base.CellType cell2 = targetBase.GetCell(Base.GetAdjacent(@int, Base.Direction.Above));
			Base.CellType cell3 = targetBase.GetCell(Base.GetAdjacent(@int, Base.Direction.Below));
			if (cell2 == Base.CellType.Room || cell2 == Base.CellType.Observatory || cell2 == Base.CellType.Moonpool || cell2 == Base.CellType.MoonpoolRotated || cell2 == Base.CellType.MapRoom || cell2 == Base.CellType.MapRoomRotated || cell2 == Base.CellType.LargeRoom || cell2 == Base.CellType.LargeRoomRotated || cell3 == Base.CellType.Room || cell3 == Base.CellType.Observatory || cell3 == Base.CellType.Moonpool || cell3 == Base.CellType.MoonpoolRotated || cell3 == Base.CellType.MapRoom || cell3 == Base.CellType.MapRoomRotated || cell3 == Base.CellType.LargeRoom || cell3 == Base.CellType.LargeRoomRotated)
			{
				flag = false;
			}
			if (IsBlockingHatch(@int, @int))
			{
				flag = false;
			}
			if (targetOffset != @int)
			{
				targetOffset = @int;
				RebuildGhostGeometry();
				geometryChanged = true;
			}
			ghostModelParentConstructableBase.transform.position = targetBase.GridToWorld(@int);
			ghostModelParentConstructableBase.transform.rotation = targetBase.transform.rotation;
		}
		else
		{
			flag = PlaceWithBoundsCast(position, forward, Builder.aaBounds.extents, placeDefaultDistance, minHeightFromTerrain, maxHeightFromTerrain, out var center);
			ghostModelParentConstructableBase.transform.position = center;
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

	private bool CheckCorridorConnection(Int3 cell, out bool adjoinedFacesAreUsed)
	{
		bool result = false;
		adjoinedFacesAreUsed = false;
		for (int i = 0; i < 4; i++)
		{
			if (!Shapes[(int)corridor, i])
			{
				continue;
			}
			Base.Direction direction = Base.HorizontalDirections[(i + Builder.lastRotation) % 4];
			Int3 adjacent = Base.GetAdjacent(cell, direction);
			Base.Direction direction2 = Base.ReverseDirection(direction);
			Base.CellType cell2 = targetBase.GetCell(adjacent);
			if (cell2 == Base.CellType.Observatory)
			{
				if (!ObservatoryAcceptsCorridors(adjacent))
				{
					adjoinedFacesAreUsed = true;
				}
				result = true;
			}
			else if ((targetBase.GetCellConnections(adjacent) & (1 << (int)direction2)) != 0)
			{
				if (cell2 == Base.CellType.Corridor)
				{
					Base.FaceType face = targetBase.GetFace(new Base.Face(adjacent, direction2));
					adjoinedFacesAreUsed |= face != Base.FaceType.Solid;
				}
				else
				{
					Base.FaceType face2 = targetBase.GetFace(new Base.Face(adjacent, direction2));
					adjoinedFacesAreUsed |= face2 == Base.FaceType.Hatch;
				}
				result = true;
			}
		}
		return result;
	}

	private int CalculateCorridorType()
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if (Shapes[(int)corridor, i])
			{
				num |= 1 << (int)Base.HorizontalDirections[(i + Builder.lastRotation) % 4];
			}
		}
		return num;
	}

	private void UpdateRotation(ref bool geometryChanged)
	{
		if (Builder.UpdateRotation(4))
		{
			corridorType = CalculateCorridorType();
			ghostBase.SetCorridor(Int3.zero, corridorType, isGlass);
			RebuildGhostGeometry();
			geometryChanged = true;
		}
	}
}
