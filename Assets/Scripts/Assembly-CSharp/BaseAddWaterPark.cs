using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseAddWaterPark : BaseGhost
{
	[NonSerialized]
	[ProtoMember(1)]
	public Base.Face? anchoredFace;

	public override void SetupGhost()
	{
		base.SetupGhost();
		UpdateSize(Int3.one);
	}

	private Int3 GetCell(Transform camera, Base targetBase, float distance)
	{
		Int3 cell = targetBase.WorldToGrid(camera.position);
		int y = cell.y;
		Vector3 direction = new Vector3(0f, 0f, 0f);
		switch (targetBase.GetCell(cell))
		{
		case Base.CellType.LargeRoom:
			direction.Set(0f - Base.halfCellSize.x, 0f, 0f);
			break;
		case Base.CellType.LargeRoomRotated:
			direction.Set(0f, 0f, 0f - Base.halfCellSize.z);
			break;
		}
		direction = camera.position + camera.forward * distance + targetBase.transform.TransformDirection(direction);
		cell = targetBase.WorldToGrid(direction);
		cell.y = y;
		return cell;
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
		float distance = ((componentInParent != null) ? componentInParent.placeDefaultDistance : 0f);
		Base.Face face = new Base.Face(GetCell(camera, targetBase, distance), Base.Direction.Below);
		if (!targetBase.CanSetWaterPark(face.cell))
		{
			geometryChanged = SetupInvalid();
			return false;
		}
		Int3 @int = (targetOffset = targetBase.NormalizeCell(face.cell));
		Base.CellType cell = targetBase.GetCell(@int);
		Base.Face face2 = new Base.Face(face.cell - targetBase.GetAnchor(), face.direction);
		if (!anchoredFace.HasValue || anchoredFace.Value != face2)
		{
			anchoredFace = face2;
			Int3 int2 = Base.CellSize[(uint)cell];
			geometryChanged = UpdateSize(int2);
			ghostBase.CopyFrom(targetBase, new Int3.Bounds(@int, @int + int2 - 1), @int * -1);
			ghostBase.ClearMasks();
			Int3 cell2 = face.cell - @int;
			switch (cell)
			{
			case Base.CellType.Room:
			{
				Base.Face face4 = new Base.Face(cell2, face.direction);
				for (int l = 0; l < 2; l++)
				{
					ghostBase.SetFaceType(face4, Base.FaceType.WaterPark);
					ghostBase.SetFaceMask(face4, isMasked: true);
					face4.direction = Base.OppositeDirections[(int)face4.direction];
				}
				Base.Direction[] horizontalDirections = Base.HorizontalDirections;
				foreach (Base.Direction direction2 in horizontalDirections)
				{
					face4.direction = direction2;
					ghostBase.SetFaceType(face4, Base.FaceType.Solid);
					ghostBase.SetFaceMask(face4, isMasked: true);
				}
				break;
			}
			case Base.CellType.LargeRoom:
			case Base.CellType.LargeRoomRotated:
			{
				Base.Face face3 = default(Base.Face);
				int index = ((cell != Base.CellType.LargeRoom) ? 2 : 0);
				for (int i = 0; i < 2; i++)
				{
					face3.cell = cell2;
					face3.cell[index] += i;
					Base.Direction[] horizontalDirections = Base.HorizontalDirections;
					foreach (Base.Direction direction in horizontalDirections)
					{
						if (cell == Base.CellType.LargeRoomRotated)
						{
							if ((i == 0 && direction == Base.Direction.North) || (i == 1 && direction == Base.Direction.South))
							{
								continue;
							}
						}
						else if ((i == 0 && direction == Base.Direction.East) || (i == 1 && direction == Base.Direction.West))
						{
							continue;
						}
						face3.direction = direction;
						ghostBase.SetFaceMask(face3, isMasked: true);
						ghostBase.SetFaceType(face3, Base.FaceType.Solid);
					}
					face3.direction = face.direction;
					for (int k = 0; k < 2; k++)
					{
						ghostBase.SetFaceType(face3, Base.FaceType.WaterPark);
						ghostBase.SetFaceMask(face3, isMasked: true);
						face3.direction = Base.OppositeDirections[(int)face3.direction];
					}
				}
				break;
			}
			}
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
		if (cell == Base.CellType.LargeRoom || cell == Base.CellType.LargeRoomRotated)
		{
			Int3 int3 = new Int3(0, 1, 0);
			if (targetBase.IsCellUnderConstruction(face.cell + int3) || targetBase.IsCellUnderConstruction(face.cell - int3))
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
