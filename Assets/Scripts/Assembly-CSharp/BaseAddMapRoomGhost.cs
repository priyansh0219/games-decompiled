using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseAddMapRoomGhost : BaseGhost
{
	[AssertNotNull]
	public Base.CellType[] cellTypes;

	public float maxHeightFromTerrain = 10f;

	public float minHeightFromTerrain = 1f;

	public override void Awake()
	{
		Builder.ClampRotation(cellTypes.Length);
		base.Awake();
	}

	protected override Base.CellType GetCellType()
	{
		return cellTypes[Builder.lastRotation % cellTypes.Length];
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
		UpdateRotation(ref geometryChanged);
		float placeDefaultDistance = ghostModelParentConstructableBase.placeDefaultDistance;
		Int3 @int = Base.CellSize[8];
		Vector3 direction = Vector3.Scale((@int - 1).ToVector3(), Base.halfCellSize);
		Vector3 position = camera.position;
		Vector3 forward = camera.forward;
		targetBase = BaseGhost.FindBase(camera);
		bool flag;
		if (targetBase != null)
		{
			targetBase.SetPlacementGhost(this);
			positionFound = true;
			flag = true;
			Vector3 vector = position + forward * placeDefaultDistance;
			Vector3 vector2 = targetBase.transform.TransformDirection(direction);
			Int3 int2 = targetBase.WorldToGrid(vector - vector2);
			Int3 maxs = int2 + @int - 1;
			Int3.Bounds bounds = new Int3.Bounds(int2, maxs);
			foreach (Int3 item in bounds)
			{
				if (targetBase.GetCell(item) != 0 || targetBase.IsCellUnderConstruction(item))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				foreach (Int3 item2 in bounds)
				{
					Base.CellType cell = targetBase.GetCell(Base.GetAdjacent(item2, Base.Direction.Above));
					Base.CellType cell2 = targetBase.GetCell(Base.GetAdjacent(item2, Base.Direction.Below));
					flag = flag && cell == Base.CellType.Empty && cell2 == Base.CellType.Empty;
				}
			}
			if (targetOffset != int2)
			{
				targetOffset = int2;
				RebuildGhostGeometry();
				geometryChanged = true;
			}
			ghostModelParentConstructableBase.transform.position = targetBase.GridToWorld(int2);
			ghostModelParentConstructableBase.transform.rotation = targetBase.transform.rotation;
		}
		else
		{
			Bounds aaBounds = Builder.aaBounds;
			Vector3 vector3 = ghostModelParentConstructableBase.transform.TransformDirection(direction);
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

	private void UpdateRotation(ref bool geometryChanged)
	{
		if (cellTypes.Length >= 2 && Builder.UpdateRotation(cellTypes.Length))
		{
			Base.CellType cellType = GetCellType();
			ghostBase.SetCell(Int3.zero, cellType);
			RebuildGhostGeometry();
			geometryChanged = true;
		}
	}
}
