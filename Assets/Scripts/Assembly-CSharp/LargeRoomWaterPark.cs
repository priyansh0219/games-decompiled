using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class LargeRoomWaterPark : WaterPark
{
	public LargeRoomWaterParkPlanter planters;

	public float positionXOffset = 0.5f;

	public float wpLength = 5f;

	public float baseSwitchWPChance = 0.2f;

	private LargeRoomWaterPark[] connectedWaterParks = new LargeRoomWaterPark[6];

	private Int2 currentCell2d;

	private LargeRoomWaterPark _rootWaterPark;

	private List<LargeRoomWaterPark> segments = new List<LargeRoomWaterPark>();

	private int usedLocalSpace;

	private int size;

	private bool rebuiltThisFrame;

	private bool connectionsRecalculatedThisFrame;

	private static List<LargeRoomWaterPark> sConnectedWaterParks = new List<LargeRoomWaterPark>();

	private const int upLeft = 0;

	private const int up = 1;

	private const int upRight = 2;

	private const int downLeft = 3;

	private const int down = 4;

	private const int downRight = 5;

	private static Int3[] cellOffset = new Int3[6]
	{
		new Int3(-1, 1, 0),
		new Int3(0, 1, 0),
		new Int3(1, 1, 0),
		new Int3(-1, -1, 0),
		new Int3(0, -1, 0),
		new Int3(1, -1, 0)
	};

	private static Int3[] cellOffsetRotated = new Int3[6]
	{
		new Int3(0, 1, -1),
		new Int3(0, 1, 0),
		new Int3(0, 1, 1),
		new Int3(0, -1, -1),
		new Int3(0, -1, 0),
		new Int3(0, -1, 1)
	};

	public override WaterPark rootWaterPark
	{
		get
		{
			if (!(_rootWaterPark != null))
			{
				return this;
			}
			return _rootWaterPark;
		}
	}

	private LargeRoomWaterPark GetRoot()
	{
		return rootWaterPark as LargeRoomWaterPark;
	}

	private void SetRoot(LargeRoomWaterPark newRoot)
	{
		LargeRoomWaterPark root = GetRoot();
		_rootWaterPark = newRoot;
		if (newRoot != null && newRoot != root)
		{
			for (int num = root.items.Count - 1; num >= 0; num--)
			{
				WaterParkItem waterParkItem = root.items[num];
				if (waterParkItem.GetWaterPark() == this)
				{
					root.items.Remove(waterParkItem);
					root.usedSpace -= waterParkItem.GetSize();
					newRoot.items.Add(waterParkItem);
					newRoot.usedSpace += waterParkItem.GetSize();
					waterParkItem.transform.parent = newRoot.itemsRoot;
				}
			}
		}
		if (root == this && newRoot != this)
		{
			segments.Clear();
		}
	}

	public override void EnsureLocalPointIsInside(ref Vector3 localPoint)
	{
		GetSegment(localPoint).EnsureLocalPointIsInsideSegment(ref localPoint);
	}

	private void EnsureLocalPointIsInsideSegment(ref Vector3 localPoint)
	{
		Int2 @int = GetRoot().currentCell2d;
		Vector3 vector = new Vector3(Base.cellSize.x * (float)(currentCell2d.x - @int.x), Base.cellSize.y * (float)(currentCell2d.y - @int.y), 0f);
		localPoint -= vector;
		float min = ((GetWaterParkBelow(localPoint, includeAdjacent: false) != null) ? (0f - Base.halfCellSize.y) : (-1.3f));
		float max = ((GetWaterParkAbove(localPoint, includeAdjacent: false) != null) ? Base.halfCellSize.y : 1.3f);
		localPoint.y = Mathf.Clamp(localPoint.y, min, max);
		if (localPoint.x > positionXOffset && localPoint.x < positionXOffset + wpLength)
		{
			localPoint.z = Mathf.Clamp(localPoint.z, 0f - internalRadius, internalRadius);
		}
		else
		{
			float num = ((localPoint.x > positionXOffset) ? (positionXOffset + wpLength) : positionXOffset);
			Vector3 vector2 = new Vector3(localPoint.x - num, 0f, localPoint.z);
			if (vector2.magnitude > internalRadius)
			{
				vector2 = internalRadius * vector2.normalized;
				localPoint.x = vector2.x + num;
				localPoint.z = vector2.z;
			}
		}
		localPoint += vector;
	}

	public override Vector3 GetRandomSwimTarget(WaterParkCreature creature)
	{
		return GetRandomSwimTargetInternal(creature);
	}

	private Vector3 GetRandomSwimTargetInternal(WaterParkCreature creature)
	{
		Vector3 zero = Vector3.zero;
		if (UnityEngine.Random.value < baseSwitchWPChance)
		{
			Int2 @int = GetRoot().currentCell2d;
			Vector3 vector = new Vector3(Base.cellSize.x * (float)(currentCell2d.x - @int.x), Base.cellSize.y * (float)(currentCell2d.y - @int.y), 0f);
			Vector3 localPosition = creature.transform.localPosition - vector;
			LargeRoomWaterPark largeRoomWaterPark = ((localPosition.y > 0f) ? GetWaterParkAbove(localPosition, includeAdjacent: false) : GetWaterParkBelow(localPosition, includeAdjacent: false));
			if (largeRoomWaterPark != null)
			{
				int num = largeRoomWaterPark.usedLocalSpace;
				int num2 = usedLocalSpace;
				if (UnityEngine.Random.value * (float)(num + num2) > (float)num)
				{
					zero.x = ((localPosition.x > Base.halfCellSize.x) ? (Base.cellSize.x - positionXOffset) : positionXOffset);
					Vector3 vector2 = base.transform.TransformPoint(zero);
					vector2.y = largeRoomWaterPark.transform.position.y;
					return vector2 + UnityEngine.Random.insideUnitSphere;
				}
			}
		}
		zero.y = UnityEngine.Random.Range(-1.3f, 1.3f);
		float num3 = wpLength * 2f;
		float num4 = (float)Math.PI * internalRadius;
		if (UnityEngine.Random.value * (num3 + num4) <= num3)
		{
			zero.x = UnityEngine.Random.Range(0f, wpLength);
			zero.z = UnityEngine.Random.Range(0f - internalRadius, internalRadius);
		}
		else
		{
			Vector2 vector3 = UnityEngine.Random.insideUnitCircle * internalRadius;
			float num5 = ((vector3.x > positionXOffset) ? (positionXOffset + wpLength) : positionXOffset);
			zero.x = vector3.x + num5;
			zero.z = vector3.y;
		}
		return base.transform.TransformPoint(zero);
	}

	public override Int2 GetCell(WaterParkItem item)
	{
		if (item == null || item.GetWaterPark() != this)
		{
			return new Int2(0);
		}
		LargeRoomWaterPark segment = GetSegment(item.transform.localPosition);
		if (segment != item.GetWaterPark())
		{
			item.SetWaterPark(segment);
		}
		return segment.currentCell2d;
	}

	public override void VerifyPlayerWaterPark(Player player)
	{
		if (!(player == null))
		{
			Vector3 position = player.transform.position;
			LargeRoomWaterPark segment = GetSegment(GetRoot().transform.InverseTransformPoint(position));
			player.currentWaterPark = (segment.IsPointInside(position) ? segment : null);
		}
	}

	public override bool HasFreeSpace()
	{
		LargeRoomWaterPark root = GetRoot();
		return wpPieceCapacity * root.segments.Count > root.usedSpace;
	}

	public override bool IsPointInside(Vector3 point)
	{
		Vector3 vector = base.transform.InverseTransformPoint(point);
		float num = 0f - Base.halfCellSize.y;
		float num2 = num + Base.cellSize.y;
		if (vector.y < num || vector.y > num2)
		{
			return false;
		}
		if (vector.x > 0f && vector.x < wpLength)
		{
			if (vector.z > 0f - externalRadius)
			{
				return vector.z > 0f - externalRadius;
			}
			return false;
		}
		float num3 = ((vector.x > 0f) ? wpLength : 0f);
		vector.x -= num3;
		vector.y = 0f;
		return vector.magnitude < externalRadius;
	}

	public override void Rebuild(Base hostBase, Int3 cell)
	{
		base.hostBase = hostBase;
		if (rebuiltThisFrame)
		{
			return;
		}
		rebuiltThisFrame = true;
		isDirty = true;
		RecalculateConnectionsRecursive(hostBase, cell);
		sConnectedWaterParks.Clear();
		sConnectedWaterParks.Add(this);
		GetConnectedWaterParks(sConnectedWaterParks);
		Int2 @int = currentCell2d;
		LargeRoomWaterPark largeRoomWaterPark = this;
		foreach (LargeRoomWaterPark sConnectedWaterPark in sConnectedWaterParks)
		{
			if (sConnectedWaterPark.currentCell2d.y < @int.y || (sConnectedWaterPark.currentCell2d.y == @int.y && sConnectedWaterPark.currentCell2d.x < @int.x))
			{
				@int = sConnectedWaterPark.currentCell2d;
				largeRoomWaterPark = sConnectedWaterPark;
			}
		}
		int count = sConnectedWaterParks.Count;
		largeRoomWaterPark.segments = new List<LargeRoomWaterPark>(sConnectedWaterParks);
		foreach (LargeRoomWaterPark sConnectedWaterPark2 in sConnectedWaterParks)
		{
			sConnectedWaterPark2.rebuiltThisFrame = true;
			sConnectedWaterPark2.isDirty = true;
			sConnectedWaterPark2.SetRoot(largeRoomWaterPark);
			sConnectedWaterPark2.size = count;
		}
		sConnectedWaterParks.Clear();
	}

	public override bool IsInitialized()
	{
		return true;
	}

	public override bool IsConnected()
	{
		LargeRoomWaterPark[] array = connectedWaterParks;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	protected override void Update()
	{
		base.Update();
		rebuiltThisFrame = false;
		connectionsRecalculatedThisFrame = false;
	}

	protected override void ValidateContent()
	{
		base.ValidateContent();
		int num = 0;
		LargeRoomWaterPark largeRoomWaterPark = this;
		while (largeRoomWaterPark != null)
		{
			num++;
			largeRoomWaterPark = largeRoomWaterPark.connectedWaterParks[1];
		}
		planters.SetMaxPlantsHeight(Base.cellSize.y * ((float)num - 0.5f));
		bool leftPlanterActive = connectedWaterParks[4] == null && connectedWaterParks[3] == null;
		bool rightPlanterActive = connectedWaterParks[4] == null && connectedWaterParks[5] == null;
		planters.SetLeftPlanterActive(leftPlanterActive);
		planters.SetRightPlanterActive(rightPlanterActive);
	}

	protected override void OnAdd(WaterParkItem item)
	{
		base.OnAdd(item);
		if (item is WaterParkCreature)
		{
			usedLocalSpace += item.GetSize();
		}
	}

	protected override void OnRemove(WaterParkItem item)
	{
		base.OnRemove(item);
		if (item is WaterParkCreature)
		{
			usedLocalSpace -= item.GetSize();
		}
	}

	private LargeRoomWaterPark GetWaterParkBelow(Vector3 localPosition, bool includeAdjacent)
	{
		int num = 4;
		if (connectedWaterParks[4] == null)
		{
			bool flag = localPosition.x > Base.halfCellSize.x;
			num = (flag ? 5 : 3);
			if (connectedWaterParks[num] == null && includeAdjacent)
			{
				num = (flag ? 3 : 5);
			}
		}
		return connectedWaterParks[num];
	}

	private LargeRoomWaterPark GetWaterParkAbove(Vector3 localPosition, bool includeAdjacent)
	{
		int num = 1;
		if (connectedWaterParks[1] == null)
		{
			bool flag = localPosition.x > Base.halfCellSize.x;
			num = (flag ? 2 : 0);
			if (connectedWaterParks[num] == null && includeAdjacent)
			{
				num = ((!flag) ? 2 : 0);
			}
		}
		return connectedWaterParks[num];
	}

	private LargeRoomWaterPark GetSegment(Vector3 localPoint)
	{
		Int2 @int = GetRoot().currentCell2d;
		Int2 int2 = default(Int2);
		Int2 int3 = default(Int2);
		int2.y = (int3.y = @int.y + Mathf.RoundToInt(localPoint.y / Base.cellSize.y));
		int2.x = @int.x + Mathf.RoundToInt(localPoint.x / Base.cellSize.x);
		int3.x = int2.x - 1;
		if (currentCell2d == int2 || currentCell2d == int3)
		{
			return this;
		}
		float num = float.PositiveInfinity;
		LargeRoomWaterPark result = this;
		LargeRoomWaterPark root = GetRoot();
		Vector3 zero = Vector3.zero;
		foreach (LargeRoomWaterPark segment in root.segments)
		{
			if (segment.currentCell2d == int2 || segment.currentCell2d == int3)
			{
				return segment;
			}
			zero.x = Base.cellSize.x * (float)(segment.currentCell2d.x - @int.x) + Base.halfCellSize.x;
			zero.y = Base.cellSize.y * (float)(segment.currentCell2d.y - @int.y);
			float sqrMagnitude = (localPoint - zero).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = segment;
			}
		}
		return result;
	}

	private void RecalculateConnectionsRecursive(Base hostBase, Int3 cell)
	{
		connectionsRecalculatedThisFrame = true;
		Base.CellType cell2 = hostBase.GetCell(cell);
		currentCell2d.x = cell[(cell2 != Base.CellType.LargeRoom) ? 2 : 0];
		currentCell2d.y = cell.y;
		Int3[] array = ((cell2 == Base.CellType.LargeRoom) ? cellOffset : cellOffsetRotated);
		for (int i = 0; i < 6; i++)
		{
			Int3 cell3 = cell + array[i];
			LargeRoomWaterPark largeRoomWaterPark = WaterPark.GetWaterParkModule(hostBase, cell3) as LargeRoomWaterPark;
			if (largeRoomWaterPark != null && !WaterPark.IsCellContainWaterPark(hostBase, cell3))
			{
				if (!largeRoomWaterPark.connectionsRecalculatedThisFrame)
				{
					largeRoomWaterPark.RecalculateConnectionsRecursive(hostBase, cell3);
					largeRoomWaterPark.OnDeconstructionStart();
				}
				largeRoomWaterPark = null;
			}
			connectedWaterParks[i] = largeRoomWaterPark;
			if (largeRoomWaterPark != null && (!largeRoomWaterPark.connectionsRecalculatedThisFrame || Array.IndexOf(largeRoomWaterPark.connectedWaterParks, this) < 0))
			{
				largeRoomWaterPark.RecalculateConnectionsRecursive(hostBase, cell3);
			}
		}
	}

	private void GetConnectedWaterParks(List<LargeRoomWaterPark> waterParks)
	{
		LargeRoomWaterPark[] array = connectedWaterParks;
		foreach (LargeRoomWaterPark largeRoomWaterPark in array)
		{
			if (largeRoomWaterPark != null && !waterParks.Contains(largeRoomWaterPark))
			{
				waterParks.Add(largeRoomWaterPark);
				largeRoomWaterPark.GetConnectedWaterParks(waterParks);
			}
		}
	}

	private void OnDeconstructionStart()
	{
		if (size == 1)
		{
			return;
		}
		LargeRoomWaterPark root = GetRoot();
		for (int num = root.items.Count - 1; num >= 0; num--)
		{
			WaterParkItem waterParkItem = root.items[num];
			if (!(waterParkItem.GetWaterPark() != this))
			{
				Vector3 localPosition = base.transform.InverseTransformPoint(waterParkItem.transform.position);
				LargeRoomWaterPark waterParkAbove = GetWaterParkAbove(localPosition, includeAdjacent: true);
				LargeRoomWaterPark waterParkBelow = GetWaterParkBelow(localPosition, includeAdjacent: true);
				LargeRoomWaterPark largeRoomWaterPark = ((waterParkBelow == null || (localPosition.y > 0f && waterParkAbove != null)) ? waterParkAbove : waterParkBelow);
				if (largeRoomWaterPark != null)
				{
					waterParkItem.SetWaterPark(largeRoomWaterPark);
				}
			}
		}
		size = 1;
		segments.Clear();
		segments.Add(this);
		_rootWaterPark = this;
	}
}
