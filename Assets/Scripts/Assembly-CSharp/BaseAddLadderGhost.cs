using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseAddLadderGhost : BaseGhost
{
	public static readonly Base.Direction ladderFaceDir;

	private bool isDirty;

	public override void SetupGhost()
	{
		base.SetupGhost();
		UpdateSize(Int3.one);
	}

	public override bool UpdatePlacement(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
	{
		positionFound = false;
		geometryChanged = false;
		if (!Physics.Raycast(camera.position, camera.forward, out var hitInfo, placeMaxDistance, BaseGhost.placeLayerMask.value))
		{
			geometryChanged = SetupInvalid();
			return false;
		}
		targetBase = hitInfo.collider.GetComponentInParent<Base>();
		if (!targetBase)
		{
			targetBase = BaseGhost.FindBase(camera);
		}
		if (!targetBase || !targetBase.PickFace(camera, out var face))
		{
			geometryChanged = SetupInvalid();
			return false;
		}
		if ((bool)targetBase)
		{
			targetBase.SetPlacementGhost(this);
		}
		if (!targetBase.CanSetLadder(face, out var faceEnd))
		{
			geometryChanged = SetupInvalid();
			return false;
		}
		if (face.direction == Base.Direction.Below)
		{
			Base.Face face2 = face;
			face = faceEnd;
			faceEnd = face2;
		}
		Int3 @int = targetBase.NormalizeCell(face.cell);
		Base.CellType cell = targetBase.GetCell(@int);
		Int3 int2 = Base.CellSize[(uint)cell];
		Int3.Bounds a = new Int3.Bounds(face.cell, faceEnd.cell);
		Int3.Bounds b = new Int3.Bounds(@int, @int + int2 - 1);
		Int3.Bounds sourceRange = Int3.Bounds.Union(a, b);
		Int3 size = sourceRange.size;
		geometryChanged = UpdateSize(size);
		if (isDirty || targetOffset != face.cell)
		{
			ghostBase.CopyFrom(targetBase, sourceRange, sourceRange.mins * -1);
			Base.Face face3 = new Base.Face(face.cell - sourceRange.mins, face.direction);
			Base.Face face4 = new Base.Face(faceEnd.cell - sourceRange.mins, faceEnd.direction);
			ghostBase.ClearMasks();
			ghostBase.SetFaceMask(face3, isMasked: true);
			ghostBase.SetFaceMask(face4, isMasked: true);
			ghostBase.SetFaceType(face3, Base.FaceType.Ladder);
			ghostBase.SetFaceType(face4, Base.FaceType.Ladder);
			for (int i = 1; i < face4.cell.y; i++)
			{
				Int3 cell2 = face4.cell;
				cell2.y = i;
				Base.Face face5 = new Base.Face(cell2, ladderFaceDir);
				ghostBase.SetFaceMask(face5, isMasked: true);
				ghostBase.SetFaceType(face5, Base.FaceType.Ladder);
			}
			RebuildGhostGeometry();
			isDirty = false;
			geometryChanged = true;
		}
		ghostModelParentConstructableBase.transform.position = targetBase.GridToWorld(sourceRange.mins);
		ghostModelParentConstructableBase.transform.rotation = targetBase.transform.rotation;
		positionFound = true;
		Int3.RangeEnumerator enumerator = sourceRange.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int3 current = enumerator.Current;
			if (targetBase.IsCellUnderConstruction(current))
			{
				return false;
			}
		}
		targetOffset = face.cell;
		if (ghostModelParentConstructableBase.transform.position.y > float.PositiveInfinity && BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) > 25f)
		{
			return false;
		}
		return true;
	}

	private bool UpdateSize(Int3 size)
	{
		if (size == ghostBase.GetSize())
		{
			return false;
		}
		ghostBase.ClearGeometry();
		ghostBase.SetSize(size);
		ghostBase.AllocateMasks();
		RebuildGhostGeometry();
		isDirty = true;
		return true;
	}

	private bool SetupInvalid()
	{
		if (isDirty)
		{
			return false;
		}
		Int3.RangeEnumerator allCells = ghostBase.AllCells;
		while (allCells.MoveNext())
		{
			ghostBase.ClearCell(allCells.Current);
		}
		RebuildGhostGeometry();
		isDirty = true;
		return true;
	}
}
