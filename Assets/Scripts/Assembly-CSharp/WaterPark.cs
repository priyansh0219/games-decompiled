using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

[ProtoContract]
[ProtoInclude(1000, typeof(LargeRoomWaterPark))]
public class WaterPark : MonoBehaviour, IBaseModule, IProtoEventListener
{
	public const Base.Direction kDirection = Base.Direction.Below;

	private static readonly Int3 floorCellStep = new Int3(0, 1, 0);

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version;

	[NonSerialized]
	[ProtoMember(2)]
	public float _constructed = 1f;

	[NonSerialized]
	[ProtoMember(3)]
	public Base.Face _moduleFace;

	private int height;

	protected bool isDirty;

	protected List<WaterParkItem> items = new List<WaterParkItem>();

	protected int usedSpace;

	private Base _hostBase;

	private static GameObject roomWaterParkPrefab;

	private static GameObject largeRoomWaterParkPrefab;

	public Transform itemsRoot;

	public Planter planter;

	public double spreadInfectionInterval = 60.0;

	public int wpPieceCapacity = 10;

	public float externalRadius = 2.8f;

	public float internalRadius = 2.2f;

	private double timeNextInfectionSpread = -1.0;

	public virtual WaterPark rootWaterPark => this;

	public Base hostBase
	{
		get
		{
			if (_hostBase == null)
			{
				_hostBase = base.transform.GetComponentInParent<Base>();
			}
			return _hostBase;
		}
		protected set
		{
			_hostBase = value;
		}
	}

	public Base.Face moduleFace
	{
		get
		{
			return _moduleFace;
		}
		set
		{
			_moduleFace = value;
		}
	}

	public float constructed
	{
		get
		{
			return _constructed;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (_constructed != value)
			{
				_constructed = value;
				if (!(_constructed >= 1f) && _constructed <= 0f)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}
	}

	private void OnGlobalEntitiesLoaded()
	{
		foreach (Transform item in itemsRoot.transform)
		{
			AddItem(item.GetComponent<Pickupable>());
		}
	}

	protected virtual void Update()
	{
		if (isDirty)
		{
			isDirty = false;
			ValidateContent();
		}
		double timePassed = DayNightCycle.main.timePassed;
		if (timeNextInfectionSpread > 0.0 && timePassed > timeNextInfectionSpread)
		{
			if (InfectCreature())
			{
				timeNextInfectionSpread = timePassed + spreadInfectionInterval;
			}
			else
			{
				timeNextInfectionSpread = -1.0;
			}
		}
	}

	protected virtual void ValidateContent()
	{
		for (int i = 0; i < items.Count; i++)
		{
			items[i].ValidatePosition();
		}
		if (planter != null)
		{
			planter.SetMaxPlantsHeight(Base.cellSize.y * ((float)height - 0.5f));
		}
	}

	public static void TransferValue(WaterPark srcWaterPark, WaterPark dstWaterPark)
	{
		List<WaterParkItem> list = new List<WaterParkItem>(srcWaterPark.items);
		for (int i = 0; i < list.Count; i++)
		{
			srcWaterPark.MoveItemTo(list[i], dstWaterPark);
		}
	}

	public static void Unite(WaterPark bottomWaterPark, WaterPark topWaterPark)
	{
		bottomWaterPark.height += topWaterPark.height;
		TransferValue(topWaterPark, bottomWaterPark);
		UnityEngine.Object.Destroy(topWaterPark.gameObject);
	}

	public static void Split(WaterPark bottomWaterPark, WaterPark topWaterPark)
	{
		float num = Base.cellSize.y * (float)bottomWaterPark.height;
		List<WaterParkItem> list = new List<WaterParkItem>();
		for (int i = 0; i < bottomWaterPark.items.Count; i++)
		{
			WaterParkItem waterParkItem = bottomWaterPark.items[i];
			if (waterParkItem.transform.localPosition.y > num)
			{
				list.Add(waterParkItem);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			bottomWaterPark.MoveItemTo(list[j], topWaterPark);
		}
	}

	public static IEnumerator InitializeAsync()
	{
		AsyncOperationHandle<GameObject> roomWPPrefabRequest = AddressablesUtility.LoadAsync<GameObject>("Submarine/Build/WaterPark.prefab");
		yield return roomWPPrefabRequest;
		roomWaterParkPrefab = roomWPPrefabRequest.Result;
		AsyncOperationHandle<GameObject> largeRoomWPPrefabRequest = AddressablesUtility.LoadAsync<GameObject>("Submarine/Build/WaterParkLarge.prefab");
		yield return largeRoomWPPrefabRequest;
		largeRoomWaterParkPrefab = largeRoomWPPrefabRequest.Result;
	}

	public virtual void Rebuild(Base hostBase, Int3 cell)
	{
		this.hostBase = hostBase;
		if (isDirty)
		{
			return;
		}
		isDirty = true;
		if (!IsInitialized())
		{
			WaterPark waterParkModule = GetWaterParkModule(hostBase, cell - floorCellStep);
			if (waterParkModule != null && waterParkModule.height > 1)
			{
				TransferValue(waterParkModule, this);
				height = waterParkModule.height - 1;
				waterParkModule.height = 1;
			}
		}
		int num = 0;
		Int3 cell2 = cell;
		WaterPark waterPark = null;
		do
		{
			num++;
			cell2 += floorCellStep;
		}
		while (IsCellContainWaterPark(hostBase, cell2) && (waterPark = GetWaterParkModule(hostBase, cell2)) == null);
		int num2 = height;
		height = num;
		if (waterPark != null)
		{
			waterPark.Rebuild(hostBase, cell2);
			Unite(this, waterPark);
		}
		else if (height < num2)
		{
			cell2 += floorCellStep;
			if (IsCellContainWaterPark(hostBase, cell2))
			{
				waterPark = GetWaterParkModule(hostBase, cell2, spawnIfNull: true);
				Split(this, waterPark);
			}
		}
	}

	public static WaterPark GetWaterParkModule(Base hostBase, Int3 cell, bool spawnIfNull = false)
	{
		WaterPark waterPark = hostBase.GetModule(new Base.Face(cell, Base.Direction.Below)) as WaterPark;
		if (spawnIfNull && waterPark == null)
		{
			waterPark = Spawn(hostBase, cell);
		}
		return waterPark;
	}

	private static WaterPark Spawn(Base hostBase, Int3 cell)
	{
		GameObject prefab = ((hostBase.GetCell(cell) == Base.CellType.Room) ? roomWaterParkPrefab : largeRoomWaterParkPrefab);
		hostBase.SpawnModule(prefab, new Base.Face(cell, Base.Direction.Below));
		return GetWaterParkModule(hostBase, cell);
	}

	protected static bool IsCellContainWaterPark(Base hostBase, Int3 cell)
	{
		return hostBase.GetFace(new Base.Face(cell, Base.Direction.Below)) == Base.FaceType.WaterPark;
	}

	public static bool CanDropItemInside(Pickupable item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.GetComponent<WaterParkItem>() == null)
		{
			return false;
		}
		LiveMixin component = item.GetComponent<LiveMixin>();
		if (!(component == null))
		{
			return component.IsAlive();
		}
		return true;
	}

	public void AddItem(Pickupable pickupable)
	{
		if (!(pickupable == null))
		{
			WaterParkItem waterParkItem = pickupable.gameObject.EnsureComponent<WaterParkItem>();
			waterParkItem.pickupable = pickupable;
			waterParkItem.infectedMixin = waterParkItem.GetComponent<InfectedMixin>();
			AddItem(waterParkItem);
		}
	}

	public void RemoveItem(Pickupable pickupable)
	{
		WaterParkItem component = pickupable.GetComponent<WaterParkItem>();
		if (component != null)
		{
			RemoveItem(component);
		}
	}

	public void AddItem(WaterParkItem item)
	{
		WaterPark waterPark = rootWaterPark;
		if (!waterPark.items.Contains(item))
		{
			waterPark.items.Add(item);
			item.enabled = true;
			item.transform.parent = waterPark.itemsRoot;
			item.SetWaterPark(this);
			UpdateInfectionSpreading();
			OnAdd(item);
		}
	}

	public void RemoveItem(WaterParkItem item, bool unparent = true)
	{
		WaterPark waterPark = rootWaterPark;
		if (waterPark.items.Contains(item))
		{
			waterPark.items.Remove(item);
			if (unparent && item.transform.parent == waterPark.itemsRoot)
			{
				item.transform.parent = null;
			}
			if (item.GetWaterPark() == this)
			{
				item.SetWaterPark(null);
			}
			UpdateInfectionSpreading();
			OnRemove(item);
		}
	}

	public void MoveItemTo(WaterParkItem item, WaterPark waterPark)
	{
		if (waterPark == null)
		{
			RemoveItem(item);
			return;
		}
		WaterPark waterPark2 = rootWaterPark;
		if (waterPark2.items.Contains(item))
		{
			if (waterPark2 == waterPark.rootWaterPark)
			{
				item.SetWaterPark(waterPark);
				OnRemove(item);
				waterPark.OnAdd(item);
			}
			else
			{
				waterPark2.items.Remove(item);
				OnRemove(item);
				waterPark.AddItem(item);
			}
		}
	}

	public bool Contains(GameObject item)
	{
		WaterParkItem component = item.GetComponent<WaterParkItem>();
		if (component != null)
		{
			return rootWaterPark.items.Contains(component);
		}
		return false;
	}

	public bool HasItemsInside()
	{
		foreach (WaterParkItem item in rootWaterPark.items)
		{
			if (item != null)
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool IsInitialized()
	{
		return height != 0;
	}

	public virtual bool IsConnected()
	{
		return height > 1;
	}

	protected virtual void OnAdd(WaterParkItem item)
	{
		rootWaterPark.usedSpace += item.GetSize();
	}

	protected virtual void OnRemove(WaterParkItem item)
	{
		rootWaterPark.usedSpace -= item.GetSize();
	}

	public virtual Vector3 GetRandomSwimTarget(WaterParkCreature creature)
	{
		Vector2 vector = UnityEngine.Random.insideUnitCircle * internalRadius;
		float max = Base.cellSize.y * (float)height - 2.2f;
		float y = UnityEngine.Random.Range(-0.3f, max);
		return base.transform.position + new Vector3(vector.x, y, vector.y);
	}

	public void EnsurePointIsInside(ref Vector3 point)
	{
		Vector3 localPoint = rootWaterPark.transform.InverseTransformPoint(point);
		EnsureLocalPointIsInside(ref localPoint);
		point = rootWaterPark.transform.TransformPoint(localPoint);
	}

	public virtual void EnsureLocalPointIsInside(ref Vector3 localPoint)
	{
		float min = -1f;
		float max = Base.cellSize.y * (float)height - 2.2f;
		localPoint.y = Mathf.Clamp(localPoint.y, min, max);
		Vector3 vector = new Vector3(localPoint.x, 0f, localPoint.z);
		if (vector.magnitude > internalRadius)
		{
			vector = internalRadius * vector.normalized;
			localPoint.x = vector.x;
			localPoint.z = vector.z;
		}
	}

	public virtual bool IsPointInside(Vector3 point)
	{
		float num = 0f - Base.halfCellSize.y;
		float num2 = num + Base.cellSize.y * (float)height;
		Vector3 vector = base.transform.InverseTransformPoint(point);
		if (vector.y < num || vector.y > num2)
		{
			return false;
		}
		vector.y = 0f;
		return vector.magnitude < externalRadius;
	}

	public virtual Int2 GetCell(WaterParkItem item)
	{
		if (item == null || item.GetWaterPark() != this)
		{
			return new Int2(0);
		}
		return new Int2(0, Mathf.RoundToInt(item.transform.localPosition.y / Base.cellSize.y));
	}

	public virtual void VerifyPlayerWaterPark(Player player)
	{
		if (player != null && !IsPointInside(player.transform.position))
		{
			player.currentWaterPark = null;
		}
	}

	public virtual bool HasFreeSpace()
	{
		return wpPieceCapacity * height > usedSpace;
	}

	public WaterParkCreature GetBreedingPartner(WaterParkCreature creature)
	{
		if (!rootWaterPark.items.Contains(creature) || !HasFreeSpace())
		{
			return null;
		}
		WaterParkCreature result = null;
		float num = float.MaxValue;
		TechType techType = creature.GetTechType();
		foreach (WaterParkItem item in rootWaterPark.items)
		{
			if (!(item == creature))
			{
				WaterParkCreature waterParkCreature = item as WaterParkCreature;
				if (waterParkCreature != null && waterParkCreature.GetCanBreed() && waterParkCreature.timeNextBreed < num && waterParkCreature.GetTechType() == techType)
				{
					num = waterParkCreature.timeNextBreed;
					result = waterParkCreature;
				}
			}
		}
		return result;
	}

	private bool ContainsHeroPeepers()
	{
		for (int i = 0; i < items.Count; i++)
		{
			Peeper component = items[i].GetComponent<Peeper>();
			if (component != null && component.isHero)
			{
				return true;
			}
		}
		return false;
	}

	private bool ContainsInfectedCreature()
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].infectedMixin != null && items[i].infectedMixin.GetInfectedAmount() > 0.25f)
			{
				return true;
			}
		}
		return false;
	}

	private bool InfectCreature()
	{
		bool result = false;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].infectedMixin != null && items[i].infectedMixin.GetInfectedAmount() < 1f)
			{
				items[i].infectedMixin.SetInfectedAmount(1f);
				result = true;
				break;
			}
		}
		return result;
	}

	private void CureAllCreatures()
	{
		InfectedMixin infectedMixin = null;
		for (int i = 0; i < items.Count; i++)
		{
			infectedMixin = items[i].infectedMixin;
			if (infectedMixin != null && infectedMixin.GetInfectedAmount() > 0.1f)
			{
				infectedMixin.SetInfectedAmount(0.1f);
			}
		}
	}

	private void UpdateInfectionSpreading()
	{
		if (ContainsHeroPeepers())
		{
			CureAllCreatures();
			timeNextInfectionSpread = -1.0;
		}
		else if (timeNextInfectionSpread < 0.0 && ContainsInfectedCreature())
		{
			timeNextInfectionSpread = DayNightCycle.main.timePassed + spreadInfectionInterval;
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		version = 1;
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (version >= 1)
		{
			return;
		}
		version = 1;
		Constructable component = GetComponent<Constructable>();
		if (component != null)
		{
			constructed = component.amount;
			UnityEngine.Object.Destroy(component);
		}
		Base componentInParent = GetComponentInParent<Base>();
		if (componentInParent != null)
		{
			Int3 cell = componentInParent.WorldToGrid(base.transform.position);
			Base.Face face = new Base.Face(cell, Base.Direction.Below);
			if (componentInParent.GetFaceRaw(face) == Base.FaceType.WaterPark)
			{
				face.cell -= componentInParent.GetAnchor();
				_moduleFace = face;
				return;
			}
		}
		Debug.LogError("Failed to upgrade savegame data. FiltrationMachine IBaseModule is not found", this);
	}
}
