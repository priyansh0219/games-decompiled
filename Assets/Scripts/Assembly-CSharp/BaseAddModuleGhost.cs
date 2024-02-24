using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseAddModuleGhost : BaseGhost, IProtoEventListener
{
	[NonSerialized]
	[ProtoMember(1)]
	public Base.Face? anchoredFace;

	public Base.FaceType faceType;

	public GameObject modulePrefab;

	public override void Awake()
	{
		Builder.ClampRotation(Base.HorizontalDirections.Length);
		base.Awake();
	}

	public override void SetupGhost()
	{
		base.SetupGhost();
		UpdateSize(Int3.one);
	}

	public override bool UpdatePlacement(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
	{
		positionFound = false;
		geometryChanged = false;
		geometryChanged |= Builder.UpdateRotation(Base.HorizontalDirections.Length);
		Base.Direction direction = Base.HorizontalDirections[Builder.lastRotation % Base.HorizontalDirections.Length];
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
		Base.Face face = new Base.Face(targetBase.WorldToGrid(camera.position + camera.forward * num), direction);
		if (!targetBase.CanSetModule(ref face, faceType))
		{
			geometryChanged = SetupInvalid();
			return false;
		}
		Int3 @int = targetBase.NormalizeCell(face.cell);
		Base.Face face2 = new Base.Face(face.cell - targetBase.GetAnchor(), face.direction);
		if (!anchoredFace.HasValue || anchoredFace.Value != face2)
		{
			anchoredFace = face2;
			Base.CellType cell = targetBase.GetCell(@int);
			Int3 int2 = Base.CellSize[(uint)cell];
			geometryChanged = UpdateSize(int2);
			ghostBase.CopyFrom(targetBase, new Int3.Bounds(@int, @int + int2 - 1), @int * -1);
			Int3 cell2 = face.cell - @int;
			Base.Face face3 = new Base.Face(cell2, face.direction);
			ghostBase.SetFaceType(face3, faceType);
			ghostBase.ClearMasks();
			ghostBase.SetFaceMask(face3, isMasked: true);
			RebuildGhostGeometry();
			geometryChanged = true;
		}
		ghostModelParentConstructableBase.transform.position = targetBase.GridToWorld(@int);
		ghostModelParentConstructableBase.transform.rotation = targetBase.transform.rotation;
		positionFound = true;
		if (targetBase.IsCellUnderConstruction(face.cell))
		{
			return false;
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

	public override void Finish()
	{
		Base.Face value = anchoredFace.Value;
		value.cell += targetBase.GetAnchor();
		if (targetBase.SpawnModule(modulePrefab, value) != null)
		{
			base.Finish();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
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
