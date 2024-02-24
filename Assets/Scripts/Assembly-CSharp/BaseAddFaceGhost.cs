using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[ProtoInclude(701, typeof(BaseAddFaceModuleGhost))]
public class BaseAddFaceGhost : BaseGhost, IProtoEventListener
{
	[NonSerialized]
	[ProtoMember(1)]
	public Base.Face? anchoredFace;

	public Base.FaceType faceType;

	public GameObject modulePrefab;

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
		Collider collider = hitInfo.collider;
		targetBase = collider.GetComponentInParent<Base>();
		if (!targetBase || targetBase.GetComponent<BaseGhost>() != null)
		{
			targetBase = BaseGhost.FindBase(camera);
		}
		if (!targetBase)
		{
			geometryChanged = SetupInvalid();
			return false;
		}
		targetBase.SetPlacementGhost(this);
		BaseExplicitFace componentInParent = collider.GetComponentInParent<BaseExplicitFace>();
		Base.Face face;
		if (componentInParent == null || !componentInParent.face.HasValue)
		{
			if (collider.GetComponentInParent<BaseDeconstructable>() != null || !targetBase.PickFace(camera, out face))
			{
				geometryChanged = SetupInvalid();
				return false;
			}
		}
		else
		{
			face = componentInParent.face.Value;
		}
		if (!targetBase.CanSetFace(face, faceType))
		{
			face = Base.GetAdjacentFace(face);
			if (!targetBase.CanSetFace(face, faceType))
			{
				geometryChanged = SetupInvalid();
				return false;
			}
		}
		Int3 @int = (targetOffset = targetBase.NormalizeCell(face.cell));
		Base.CellType cell = targetBase.GetCell(@int);
		Int3 int2 = Base.CellSize[(uint)cell];
		Int3.Bounds a = new Int3.Bounds(face.cell, face.cell);
		Int3.Bounds b = new Int3.Bounds(@int, @int + int2 - 1);
		Int3.Bounds sourceRange = Int3.Bounds.Union(a, b);
		geometryChanged = UpdateSize(sourceRange.size);
		if (ghostBase.GetComponentInChildren<IBaseAccessoryGeometry>() != null)
		{
			geometryChanged = true;
		}
		Base.Face face2 = new Base.Face(face.cell - targetBase.GetAnchor(), face.direction);
		if (!anchoredFace.HasValue || anchoredFace.Value != face2)
		{
			anchoredFace = face2;
			ghostBase.CopyFrom(targetBase, sourceRange, sourceRange.mins * -1);
			ghostBase.ClearMasks();
			Int3 cell2 = face.cell - @int;
			Base.Face face3 = new Base.Face(cell2, face.direction);
			ghostBase.SetFaceMask(face3, isMasked: true);
			ghostBase.SetFaceType(face3, faceType);
			RebuildGhostGeometry();
			geometryChanged = true;
		}
		ghostModelParentConstructableBase.transform.position = targetBase.GridToWorld(@int);
		ghostModelParentConstructableBase.transform.rotation = targetBase.transform.rotation;
		positionFound = true;
		if (faceType == Base.FaceType.Hatch)
		{
			Base.Face adjacentFace = Base.GetAdjacentFace(face);
			if (targetBase.GetFace(adjacentFace) == Base.FaceType.Hatch)
			{
				return false;
			}
		}
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

	private static bool FindFirstMaskedFace(Base targetBase, out Base.Face face)
	{
		foreach (Int3 bound in targetBase.Bounds)
		{
			int index = targetBase.baseShape.GetIndex(bound);
			if (index == -1)
			{
				continue;
			}
			Base.Direction[] allDirections = Base.AllDirections;
			foreach (Base.Direction direction in allDirections)
			{
				if (targetBase.IsFaceUsed(index, direction))
				{
					face = new Base.Face(bound, direction);
					return true;
				}
			}
		}
		face = default(Base.Face);
		return false;
	}

	public override void Finish()
	{
		if (modulePrefab != null)
		{
			Base.Face face;
			if (anchoredFace.HasValue)
			{
				face = anchoredFace.Value;
				face.cell += targetBase.GetAnchor();
			}
			else
			{
				if (!FindFirstMaskedFace(ghostBase, out face))
				{
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
				Vector3 point = ghostBase.GridToWorld(Int3.zero);
				targetOffset = targetBase.WorldToGrid(point);
				face.cell += targetOffset;
			}
			if (targetBase.SpawnModule(modulePrefab, face) == null)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
		}
		base.Finish();
	}

	public override void FindOverlappedObjects(List<GameObject> overlappedObjects)
	{
		if (!anchoredFace.HasValue || !targetBase)
		{
			return;
		}
		Base.Face value = anchoredFace.Value;
		value.cell += targetBase.GetAnchor();
		Transform cellObject = targetBase.GetCellObject(value.cell);
		if (cellObject != null)
		{
			cellObject.GetComponentsInChildren(includeInactive: false, BaseGhost.explicitFaces);
		}
		foreach (BaseExplicitFace explicitFace in BaseGhost.explicitFaces)
		{
			if (explicitFace != null && explicitFace.face.HasValue && explicitFace.face.Value == value)
			{
				overlappedObjects.Add(explicitFace.gameObject);
			}
		}
	}

	public override void AttachCorridorConnectors(bool disableColliders = true)
	{
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		RecalculateTargetOffset();
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
	}
}
