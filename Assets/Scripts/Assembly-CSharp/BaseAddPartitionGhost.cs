using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseAddPartitionGhost : BaseGhost, IProtoEventListener
{
	private const Base.FaceType faceType = Base.FaceType.Partition;

	[NonSerialized]
	[ProtoMember(1)]
	public Int3? anchoredCell;

	private Base.Direction partitionDirection = (Base.Direction)(-1);

	public override void SetupGhost()
	{
		base.SetupGhost();
		UpdateSize(Int3.one);
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
		ConstructableBase componentInParent = GetComponentInParent<ConstructableBase>();
		float num = ((componentInParent != null) ? componentInParent.placeDefaultDistance : 0f);
		Vector3 point = camera.position + camera.forward * num;
		Vector3 vector = targetBase.WorldToLocal(point);
		Int3 @int = targetBase.LocalToGrid(vector);
		Vector3 vector2 = targetBase.GridToLocal(@int);
		Vector3 vector3 = vector - vector2;
		Base.Direction direction = ((!(vector3.x > 0f - vector3.z)) ? ((vector3.x > vector3.z) ? Base.Direction.South : Base.Direction.West) : ((vector3.x > vector3.z) ? Base.Direction.East : Base.Direction.North));
		geometryChanged = UpdateDirection(direction);
		if (!targetBase.CanSetPartition(@int, partitionDirection))
		{
			geometryChanged |= SetupInvalid();
			return false;
		}
		Int3 int2 = targetBase.NormalizeCell(@int);
		Base.CellType cell = targetBase.GetCell(int2);
		Int3 int3 = Base.CellSize[(uint)cell];
		Int3.Bounds a = new Int3.Bounds(@int, @int);
		Int3.Bounds b = new Int3.Bounds(int2, int2 + int3 - 1);
		Int3.Bounds sourceRange = Int3.Bounds.Union(a, b);
		geometryChanged |= UpdateSize(sourceRange.size);
		Int3 int4 = @int - targetBase.GetAnchor();
		if (!anchoredCell.HasValue || anchoredCell.Value != int4)
		{
			anchoredCell = int4;
			ghostBase.CopyFrom(targetBase, sourceRange, sourceRange.mins * -1);
			ghostBase.ClearMasks();
			Int3 cell2 = @int - int2;
			Base.Face face = new Base.Face(cell2, partitionDirection);
			ghostBase.SetFaceMask(face, isMasked: true);
			ghostBase.SetFaceType(face, Base.FaceType.Partition);
			RebuildGhostGeometry();
			geometryChanged = true;
		}
		ConstructableBase componentInParent2 = GetComponentInParent<ConstructableBase>();
		componentInParent2.transform.position = targetBase.GridToWorld(int2);
		componentInParent2.transform.rotation = targetBase.transform.rotation;
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
		if (ghostModelParentConstructableBase.transform.position.y > float.PositiveInfinity && BaseGhost.GetDistanceToGround(ghostModelParentConstructableBase.transform.position) > 25f)
		{
			return false;
		}
		return true;
	}

	private bool UpdateDirection(Base.Direction direction)
	{
		if (partitionDirection == direction)
		{
			return false;
		}
		partitionDirection = direction;
		ghostBase.ClearGeometry();
		Int3.RangeEnumerator allCells = ghostBase.AllCells;
		while (allCells.MoveNext())
		{
			ghostBase.ClearCell(allCells.Current);
		}
		RebuildGhostGeometry();
		anchoredCell = null;
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
		anchoredCell = null;
		return true;
	}

	private bool SetupInvalid()
	{
		if (!anchoredCell.HasValue)
		{
			return false;
		}
		Int3.RangeEnumerator allCells = ghostBase.AllCells;
		while (allCells.MoveNext())
		{
			ghostBase.ClearCell(allCells.Current);
		}
		RebuildGhostGeometry();
		anchoredCell = null;
		return true;
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		Vector3 point = ghostBase.GridToWorld(Int3.zero);
		targetOffset = targetBase.WorldToGrid(point);
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
	}
}
