using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseAddPartitionDoorGhost : BaseGhost, IProtoEventListener
{
	[NonSerialized]
	[ProtoMember(1)]
	public Base.Face? anchoredFace;

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
		Int3 @int = targetBase.WorldToGrid(camera.position + camera.forward * num);
		if (!targetBase.CanSetPartitionDoor(@int, out var doorFaceDirection))
		{
			geometryChanged = SetupInvalid();
			return false;
		}
		Int3 int2 = targetBase.NormalizeCell(@int);
		Base.CellType cell = targetBase.GetCell(int2);
		Int3 int3 = Base.CellSize[(uint)cell];
		Int3.Bounds a = new Int3.Bounds(@int, @int);
		Int3.Bounds b = new Int3.Bounds(int2, int2 + int3 - 1);
		Int3.Bounds sourceRange = Int3.Bounds.Union(a, b);
		geometryChanged = UpdateSize(sourceRange.size);
		Base.Face face = new Base.Face(@int - targetBase.GetAnchor(), doorFaceDirection);
		if (!anchoredFace.HasValue || anchoredFace.Value != face)
		{
			anchoredFace = face;
			ghostBase.CopyFrom(targetBase, sourceRange, sourceRange.mins * -1);
			ghostBase.ClearMasks();
			Int3 cell2 = @int - int2;
			Base.Face face2 = new Base.Face(cell2, doorFaceDirection);
			ghostBase.SetFaceType(face2, Base.FaceType.PartitionDoor);
			ghostBase.SetFaceMask(face2, isMasked: true);
			Base.Direction[] horizontalDirections = Base.HorizontalDirections;
			foreach (Base.Direction direction in horizontalDirections)
			{
				Base.Face face3 = new Base.Face(@int, direction);
				if (targetBase.GetFace(face3) == Base.FaceType.Partition)
				{
					Base.Face face4 = new Base.Face(cell2, direction);
					ghostBase.SetFaceMask(face4, isMasked: true);
				}
			}
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
		anchoredFace = null;
		return true;
	}

	private bool SetupInvalid()
	{
		if (!anchoredFace.HasValue)
		{
			return false;
		}
		Int3.RangeEnumerator allCells = ghostBase.AllCells;
		while (allCells.MoveNext())
		{
			ghostBase.ClearCell(allCells.Current);
		}
		RebuildGhostGeometry();
		anchoredFace = null;
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
