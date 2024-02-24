using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseAddBulkheadGhost : BaseGhost
{
	private Base.Face? face;

	public override void SetupGhost()
	{
		base.SetupGhost();
		Int3 size = new Int3(1);
		ghostBase.SetSize(size);
		ghostBase.AllocateMasks();
		SetupInvalid();
	}

	public override bool UpdatePlacement(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
	{
		positionFound = false;
		geometryChanged = false;
		Player main = Player.main;
		if (main == null || main.currentSub == null || !main.currentSub.isBase)
		{
			geometryChanged = SetupInvalid();
			return false;
		}
		targetBase = BaseGhost.FindBase(camera);
		if (targetBase == null)
		{
			geometryChanged = SetupInvalid();
			return false;
		}
		targetBase.SetPlacementGhost(this);
		Vector3 normal = targetBase.transform.InverseTransformDirection(camera.forward);
		Base.Face face = new Base.Face(targetBase.WorldToGrid(camera.position), Base.NormalToDirection(normal));
		if (IsCorridorConnector(face))
		{
			face = Base.GetAdjacentFace(face);
			if (IsCorridorConnector(face))
			{
				geometryChanged = SetupInvalid();
				return false;
			}
		}
		if (!targetBase.CanSetBulkhead(face))
		{
			geometryChanged = SetupInvalid();
			return false;
		}
		Int3 @int = targetBase.NormalizeCell(face.cell);
		if (!this.face.HasValue || this.face.Value.cell != face.cell || this.face.Value.direction != face.direction)
		{
			Base.CellType cell = targetBase.GetCell(@int);
			Int3 int2 = Base.CellSize[(uint)cell];
			if (ghostBase.Shape.ToInt3() != int2)
			{
				ghostBase.SetSize(int2);
				ghostBase.AllocateMasks();
			}
			ghostBase.CopyFrom(targetBase, new Int3.Bounds(@int, @int + int2 - 1), @int * -1);
			Int3 cell2 = face.cell - @int;
			Base.Face face2 = new Base.Face(cell2, face.direction);
			ghostBase.SetFaceType(face2, Base.FaceType.BulkheadClosed);
			ghostBase.ClearMasks();
			ghostBase.SetFaceMask(face2, isMasked: true);
			RebuildGhostGeometry();
			geometryChanged = true;
			this.face = face;
		}
		ghostModelParentConstructableBase.transform.position = targetBase.GridToWorld(@int);
		ghostModelParentConstructableBase.transform.rotation = targetBase.transform.rotation;
		positionFound = true;
		if (targetBase.IsCellUnderConstruction(face.cell) || targetBase.IsCellUnderConstruction(Base.GetAdjacent(face)))
		{
			return false;
		}
		targetOffset = face.cell;
		if (ghostModelParentConstructableBase.transform.position.y > float.PositiveInfinity && BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) > 25f)
		{
			return false;
		}
		return true;
	}

	private bool IsCorridorConnector(Base.Face face)
	{
		switch (targetBase.GetCell(face.cell))
		{
		case Base.CellType.Room:
		case Base.CellType.Observatory:
		case Base.CellType.Moonpool:
		case Base.CellType.MapRoom:
		case Base.CellType.MapRoomRotated:
		case Base.CellType.ControlRoom:
		case Base.CellType.ControlRoomRotated:
		case Base.CellType.LargeRoom:
		case Base.CellType.LargeRoomRotated:
		case Base.CellType.MoonpoolRotated:
			return true;
		default:
			return false;
		}
	}

	private bool SetupInvalid()
	{
		if (!face.HasValue)
		{
			return false;
		}
		ghostBase.ClearCell(Int3.zero);
		RebuildGhostGeometry();
		face = null;
		return true;
	}
}
