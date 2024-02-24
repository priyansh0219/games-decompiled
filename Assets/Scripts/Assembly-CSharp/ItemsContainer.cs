using System.Collections;
using System.Collections.Generic;
using Gendarme;
using UnityEngine;

[SuppressMessage("Subnautica.Rules", "ValueTypeEnumeratorRule")]
public class ItemsContainer : IItemsContainer, IEnumerable<InventoryItem>, IEnumerable
{
	private class ItemGroup
	{
		public int id;

		public int width;

		public int height;

		public List<InventoryItem> items;

		public ItemGroup(int id, int width, int height)
		{
			this.id = id;
			this.width = width;
			this.height = height;
			items = new List<InventoryItem>();
		}

		public void SetGhostDims(int width, int height)
		{
			this.width = width;
			this.height = height;
		}
	}

	private class GroupComparer : IComparer<ItemGroup>
	{
		public int Compare(ItemGroup group1, ItemGroup group2)
		{
			int id = group1.id;
			int id2 = group2.id;
			if (id == id2)
			{
				return 0;
			}
			int width = group1.width;
			int width2 = group2.width;
			int height = group1.height;
			int height2 = group2.height;
			int num = ((width >= height) ? width : height);
			int num2 = ((width2 >= height2) ? width2 : height2);
			if (num < num2)
			{
				return 1;
			}
			if (num > num2)
			{
				return -1;
			}
			int num3 = width * height;
			int num4 = width2 * height2;
			if (num3 == num4)
			{
				if (height < height2)
				{
					return 1;
				}
				if (height > height2)
				{
					return -1;
				}
			}
			else
			{
				if (num3 < num4)
				{
					return 1;
				}
				if (num3 > num4)
				{
					return -1;
				}
			}
			if (id < id2)
			{
				return -1;
			}
			return 1;
		}
	}

	public delegate void OnChangeItemPosition(InventoryItem item);

	public delegate void OnResize(int width, int height);

	public ItemsContainerType containerType;

	private string _label = "";

	private FMODAsset errorSound;

	private static GroupComparer groupComparer = new GroupComparer();

	[AssertLocalization]
	private const string overflowMessage = "ContainerOverflow";

	public IsAllowedToAdd isAllowedToAdd;

	public IsAllowedToRemove isAllowedToRemove;

	private Dictionary<TechType, ItemGroup> _items;

	private InventoryItem[,] itemsMap;

	private bool unsorted;

	private HashSet<TechType> allowedTech;

	private List<ItemGroup> itemGroups = new List<ItemGroup>();

	private InventoryItem ghostItem = new InventoryItem(1, 1);

	private ItemGroup ghostGroup = new ItemGroup(0, 1, 1);

	string IItemsContainer.label => _label;

	public Transform tr { get; private set; }

	public int sizeX { get; private set; }

	public int sizeY { get; private set; }

	public int count { get; private set; }

	public event OnAddItem onAddItem;

	public event OnRemoveItem onRemoveItem;

	public event OnChangeItemPosition onChangeItemPosition;

	public event OnResize onResize;

	public ItemsContainer(int width, int height, Transform tr, string label, FMODAsset errorSoundEffect)
	{
		this.tr = tr;
		sizeX = width;
		sizeY = height;
		_items = new Dictionary<TechType, ItemGroup>(TechTypeExtensions.sTechTypeComparer);
		itemsMap = new InventoryItem[sizeX, sizeY];
		count = 0;
		_label = label;
		errorSound = errorSoundEffect;
	}

	public void SetAllowedTechTypes(TechType[] allowedTech)
	{
		if (this.allowedTech == null)
		{
			this.allowedTech = new HashSet<TechType>();
		}
		else
		{
			this.allowedTech.Clear();
		}
		for (int i = 0; i < allowedTech.Length; i++)
		{
			this.allowedTech.Add(allowedTech[i]);
		}
	}

	public bool Contains(TechType techType)
	{
		return _items.ContainsKey(techType);
	}

	public bool Contains(InventoryItem item)
	{
		TechType techType = item.item.GetTechType();
		if (!_items.TryGetValue(techType, out var value))
		{
			return false;
		}
		return value.items.Contains(item);
	}

	public bool Contains(Pickupable pickupable)
	{
		TechType techType = pickupable.GetTechType();
		if (!_items.TryGetValue(techType, out var value))
		{
			return false;
		}
		List<InventoryItem> items = value.items;
		for (int i = 0; i < items.Count; i++)
		{
			InventoryItem inventoryItem = items[i];
			if (pickupable == inventoryItem.item)
			{
				return true;
			}
		}
		return false;
	}

	public ItemsContainerType GetContainerType()
	{
		return containerType;
	}

	public List<TechType> GetItemTypes()
	{
		return new List<TechType>(_items.Keys);
	}

	public int GetCount(TechType techType)
	{
		if (!_items.TryGetValue(techType, out var value))
		{
			return 0;
		}
		return value.items.Count;
	}

	public IList<InventoryItem> GetItems(TechType techType)
	{
		if (!_items.TryGetValue(techType, out var value))
		{
			return null;
		}
		return value.items.AsReadOnly();
	}

	public void GetItems(TechType techType, List<InventoryItem> dstItems)
	{
		if (!_items.TryGetValue(techType, out var value))
		{
			return;
		}
		List<InventoryItem> items = value.items;
		int i = 0;
		for (int num = items.Count; i < num; i++)
		{
			InventoryItem item = items[i];
			if (!dstItems.Contains(item))
			{
				dstItems.Add(item);
			}
		}
	}

	bool IItemsContainer.HasRoomFor(Pickupable pickupable, InventoryItem ignore)
	{
		if (ignore != null)
		{
			ignore.ignoreForSorting = true;
		}
		bool result = HasRoomFor(pickupable);
		if (ignore != null)
		{
			ignore.ignoreForSorting = false;
		}
		return result;
	}

	public bool HasRoomFor(Pickupable pickupable)
	{
		Vector2int itemSize = TechData.GetItemSize(pickupable.GetTechType());
		return HasRoomFor(itemSize.x, itemSize.y);
	}

	public bool HasRoomFor(TechType techType)
	{
		Vector2int itemSize = TechData.GetItemSize(techType);
		return HasRoomFor(itemSize.x, itemSize.y);
	}

	public bool HasRoomFor(int width, int height)
	{
		foreach (KeyValuePair<TechType, ItemGroup> item in _items)
		{
			itemGroups.Add(item.Value);
		}
		ghostItem.SetGhostDims(width, height);
		ghostGroup.SetGhostDims(width, height);
		ghostGroup.items.Add(ghostItem);
		itemGroups.Add(ghostGroup);
		bool result = TrySort(itemGroups, itemsMap, sendEvents: false);
		ResetItemsMap(itemsMap);
		unsorted = true;
		ghostGroup.items.Clear();
		itemGroups.Clear();
		return result;
	}

	public bool HasRoomFor(List<Vector2int> sizes)
	{
		int id = 0;
		List<ItemGroup> list = new List<ItemGroup>(_items.Values);
		List<ItemGroup> list2 = new List<ItemGroup>();
		for (int i = 0; i < sizes.Count; i++)
		{
			Vector2int vector2int = sizes[i];
			InventoryItem item = new InventoryItem(vector2int.x, vector2int.y);
			bool flag = false;
			for (int j = 0; j < list2.Count; j++)
			{
				ItemGroup itemGroup = list2[j];
				if (itemGroup.width == vector2int.x && itemGroup.height == vector2int.y)
				{
					itemGroup.items.Add(item);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				ItemGroup itemGroup2 = new ItemGroup(id, vector2int.x, vector2int.y);
				itemGroup2.items.Add(item);
				list2.Add(itemGroup2);
			}
		}
		for (int k = 0; k < list2.Count; k++)
		{
			list.Add(list2[k]);
		}
		bool result = TrySort(list, itemsMap, sendEvents: false);
		ResetItemsMap(itemsMap);
		unsorted = true;
		return result;
	}

	public void UnsafeAdd(InventoryItem item)
	{
		TechType techType = item.item.GetTechType();
		if (_items.TryGetValue(techType, out var value))
		{
			value.items.Add(item);
		}
		else
		{
			Vector2int itemSize = TechData.GetItemSize(techType);
			value = new ItemGroup((int)techType, itemSize.x, itemSize.y);
			value.items.Add(item);
			_items.Add(techType, value);
		}
		item.container = this;
		item.item.Reparent(tr);
		item.item.onTechTypeChanged += UpdateItemTechType;
		count++;
		unsorted = true;
		NotifyAddItem(item);
	}

	public InventoryItem AddItem(Pickupable pickupable)
	{
		InventoryItem inventoryItem = new InventoryItem(pickupable);
		if (((IItemsContainer)this).AddItem(inventoryItem))
		{
			return inventoryItem;
		}
		return null;
	}

	IEnumerator<InventoryItem> IItemsContainer.GetEnumerator()
	{
		return ((IEnumerable<InventoryItem>)this).GetEnumerator();
	}

	bool IItemsContainer.AddItem(InventoryItem item)
	{
		if (item == null)
		{
			return false;
		}
		Pickupable item2 = item.item;
		if (item2 == null)
		{
			return false;
		}
		if (!((IItemsContainer)this).AllowedToAdd(item2, verbose: true) || !HasRoomFor(item.item))
		{
			if ((bool)errorSound)
			{
				FMODUWE.PlayOneShot(errorSound, Vector3.zero);
			}
			return false;
		}
		IItemsContainer container = item.container;
		if (container != null && !container.RemoveItem(item, forced: false, verbose: true))
		{
			return false;
		}
		UnsafeAdd(item);
		return true;
	}

	bool IItemsContainer.RemoveItem(InventoryItem item, bool forced, bool verbose)
	{
		Pickupable item2 = item.item;
		if (!forced && !((IItemsContainer)this).AllowedToRemove(item2, verbose))
		{
			return false;
		}
		TechType techType = item2.GetTechType();
		if (_items.TryGetValue(techType, out var value))
		{
			List<InventoryItem> items = value.items;
			if (items.Remove(item))
			{
				if (items.Count == 0)
				{
					_items.Remove(techType);
				}
				item.container = null;
				item.item.onTechTypeChanged -= UpdateItemTechType;
				count--;
				unsorted = true;
				NotifyRemoveItem(item);
				return true;
			}
		}
		return false;
	}

	bool IItemsContainer.AllowedToAdd(Pickupable pickupable, bool verbose)
	{
		if (!IsTechTypeAllowed(pickupable.GetTechType()))
		{
			return false;
		}
		if (isAllowedToAdd != null)
		{
			return isAllowedToAdd(pickupable, verbose);
		}
		return true;
	}

	bool IItemsContainer.AllowedToRemove(Pickupable pickupable, bool verbose)
	{
		if (isAllowedToRemove != null)
		{
			return isAllowedToRemove(pickupable, verbose);
		}
		return true;
	}

	void IItemsContainer.UpdateContainer()
	{
		Sort();
	}

	public bool RemoveItem(Pickupable pickupable, bool forced = false)
	{
		if (!forced && !((IItemsContainer)this).AllowedToRemove(pickupable, verbose: true))
		{
			return false;
		}
		TechType techType = pickupable.GetTechType();
		if (_items.TryGetValue(techType, out var value))
		{
			List<InventoryItem> items = value.items;
			for (int i = 0; i < items.Count; i++)
			{
				InventoryItem inventoryItem = items[i];
				if (inventoryItem.item == pickupable)
				{
					items.RemoveAt(i);
					if (items.Count == 0)
					{
						_items.Remove(techType);
					}
					inventoryItem.container = null;
					pickupable.onTechTypeChanged -= UpdateItemTechType;
					count--;
					unsorted = true;
					NotifyRemoveItem(inventoryItem);
					return true;
				}
			}
		}
		return false;
	}

	public Pickupable RemoveItem(TechType techType)
	{
		if (_items.TryGetValue(techType, out var value))
		{
			List<InventoryItem> items = value.items;
			int num = items.Count - 1;
			float num2 = float.MaxValue;
			for (int num3 = num; num3 >= 0; num3--)
			{
				InventoryItem inventoryItem = items[num3];
				if (!(inventoryItem.item == null))
				{
					IBattery component = inventoryItem.item.GetComponent<IBattery>();
					if (component == null)
					{
						break;
					}
					if (num2 > component.charge)
					{
						num2 = component.charge;
						_ = component.capacity;
						num = num3;
					}
				}
			}
			InventoryItem inventoryItem2 = items[num];
			Pickupable item = inventoryItem2.item;
			if (!((IItemsContainer)this).AllowedToRemove(item, verbose: true))
			{
				return null;
			}
			items.RemoveAt(num);
			if (items.Count == 0)
			{
				_items.Remove(techType);
			}
			inventoryItem2.container = null;
			item.onTechTypeChanged -= UpdateItemTechType;
			count--;
			unsorted = true;
			NotifyRemoveItem(inventoryItem2);
			return item;
		}
		return null;
	}

	public bool Clear(bool keepSecured = false)
	{
		bool result = false;
		Dictionary<TechType, ItemGroup>.Enumerator enumerator = _items.GetEnumerator();
		List<TechType> list = new List<TechType>();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TechType, ItemGroup> current = enumerator.Current;
			List<InventoryItem> items = current.Value.items;
			for (int num = items.Count - 1; num >= 0; num--)
			{
				InventoryItem inventoryItem = items[num];
				Pickupable item = inventoryItem.item;
				if (!keepSecured || item.destroyOnDeath)
				{
					items.RemoveAt(num);
					inventoryItem.container = null;
					item.onTechTypeChanged -= UpdateItemTechType;
					count--;
					unsorted = true;
					NotifyRemoveItem(inventoryItem);
					if (items.Count == 0)
					{
						list.Add(current.Key);
					}
					Object.Destroy(item.gameObject);
					result = true;
				}
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			TechType key = list[i];
			_items.Remove(key);
		}
		return result;
	}

	public void Resize(int width, int height)
	{
		if (sizeX == width && sizeY == height)
		{
			return;
		}
		InventoryItem[,] array = itemsMap;
		int num = Mathf.Min(sizeX, width);
		int num2 = Mathf.Min(sizeY, height);
		sizeX = width;
		sizeY = height;
		itemsMap = new InventoryItem[sizeX, sizeY];
		unsorted = true;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				itemsMap[i, j] = array[i, j];
			}
		}
		NotifyResize(sizeX, sizeY);
	}

	public bool DestroyItem(TechType techType)
	{
		Pickupable pickupable = RemoveItem(techType);
		if (pickupable == null)
		{
			return false;
		}
		Object.Destroy(pickupable.gameObject);
		return true;
	}

	public bool SecureItems()
	{
		bool result = false;
		Dictionary<TechType, ItemGroup>.Enumerator enumerator = _items.GetEnumerator();
		while (enumerator.MoveNext())
		{
			List<InventoryItem> items = enumerator.Current.Value.items;
			for (int i = 0; i < items.Count; i++)
			{
				Pickupable item = items[i].item;
				if (item.destroyOnDeath)
				{
					result = true;
					item.destroyOnDeath = false;
				}
			}
		}
		return result;
	}

	public void Sort()
	{
		if (unsorted)
		{
			List<ItemGroup> gr = new List<ItemGroup>(_items.Values);
			if (!TrySort(gr, itemsMap, sendEvents: true))
			{
				ErrorMessage.AddError(Language.main.Get("ContainerOverflow"));
			}
			unsorted = false;
		}
	}

	private bool IsTechTypeAllowed(TechType techType)
	{
		if (allowedTech != null && allowedTech.Count != 0)
		{
			return allowedTech.Contains(techType);
		}
		return true;
	}

	private bool TrySort(List<ItemGroup> gr, InventoryItem[,] map, bool sendEvents)
	{
		bool flag = true;
		ResetItemsMap(map);
		gr.Sort(groupComparer);
		for (int i = 0; i < gr.Count; i++)
		{
			ItemGroup itemGroup = gr[i];
			int width = itemGroup.width;
			int height = itemGroup.height;
			List<InventoryItem> items = itemGroup.items;
			for (int j = 0; j < items.Count; j++)
			{
				InventoryItem inventoryItem = items[j];
				if (!inventoryItem.ignoreForSorting)
				{
					if (!GetRoomFor(map, width, height, out var x, out var y))
					{
						flag = false;
						break;
					}
					AddItemToMap(map, x, y, inventoryItem);
					inventoryItem.SetPosition(x, y);
					if (sendEvents)
					{
						NotifyChangeItemPosition(inventoryItem);
					}
				}
			}
			if (!flag)
			{
				break;
			}
		}
		return flag;
	}

	private void UpdateItemTechType(Pickupable pickupable, TechType oldTechType)
	{
		TechType techType = pickupable.GetTechType();
		if (techType == oldTechType || !_items.TryGetValue(oldTechType, out var value))
		{
			return;
		}
		List<InventoryItem> items = value.items;
		for (int i = 0; i < items.Count; i++)
		{
			InventoryItem inventoryItem = items[i];
			if (inventoryItem.item == pickupable)
			{
				items.RemoveAt(i);
				if (items.Count == 0)
				{
					_items.Remove(oldTechType);
				}
				if (_items.TryGetValue(techType, out value))
				{
					items = value.items;
					items.Add(inventoryItem);
				}
				else
				{
					Vector2int itemSize = TechData.GetItemSize(techType);
					value = new ItemGroup((int)techType, itemSize.x, itemSize.y);
					value.items.Add(inventoryItem);
					_items.Add(techType, value);
				}
				unsorted = true;
				break;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		Dictionary<TechType, ItemGroup>.Enumerator e = _items.GetEnumerator();
		while (e.MoveNext())
		{
			IEnumerator e2 = e.Current.Value.items.GetEnumerator();
			while (e2.MoveNext())
			{
				yield return e2.Current;
			}
		}
	}

	IEnumerator<InventoryItem> IEnumerable<InventoryItem>.GetEnumerator()
	{
		Dictionary<TechType, ItemGroup>.Enumerator e = _items.GetEnumerator();
		while (e.MoveNext())
		{
			List<InventoryItem>.Enumerator e2 = e.Current.Value.items.GetEnumerator();
			while (e2.MoveNext())
			{
				yield return e2.Current;
			}
		}
	}

	private void NotifyAddItem(InventoryItem item)
	{
		if (this.onAddItem != null)
		{
			this.onAddItem(item);
		}
	}

	private void NotifyRemoveItem(InventoryItem item)
	{
		if (this.onRemoveItem != null)
		{
			this.onRemoveItem(item);
		}
	}

	private void NotifyChangeItemPosition(InventoryItem item)
	{
		if (this.onChangeItemPosition != null)
		{
			this.onChangeItemPosition(item);
		}
	}

	private void NotifyResize(int width, int height)
	{
		if (this.onResize != null)
		{
			this.onResize(width, height);
		}
	}

	private static void ResetItemsMap(InventoryItem[,] map)
	{
		int length = map.GetLength(0);
		int length2 = map.GetLength(1);
		for (int i = 0; i < length2; i++)
		{
			for (int j = 0; j < length; j++)
			{
				map[j, i] = null;
			}
		}
	}

	private static void AddItemToMap(InventoryItem[,] map, int x, int y, InventoryItem item)
	{
		int num = x + item.width;
		int num2 = y + item.height;
		for (int i = y; i < num2; i++)
		{
			for (int j = x; j < num; j++)
			{
				map[j, i] = item;
			}
		}
	}

	private static int NextFreeCell(InventoryItem[,] map, int x, int y, int w, int h)
	{
		for (int i = y; i < y + h; i++)
		{
			for (int j = x; j < x + w; j++)
			{
				InventoryItem inventoryItem = map[j, i];
				if (inventoryItem != null)
				{
					return inventoryItem.x + inventoryItem.width;
				}
			}
		}
		return x;
	}

	private static bool GetRoomFor(InventoryItem[,] map, int w, int h, out int x, out int y)
	{
		bool flag = false;
		x = -1;
		y = -1;
		int length = map.GetLength(0);
		int length2 = map.GetLength(1);
		for (int i = 0; i <= length2 - h; i++)
		{
			int num = 0;
			while (num <= length - w)
			{
				int num2 = NextFreeCell(map, num, i, w, h);
				if (num2 != num)
				{
					num = num2 - 1;
					num++;
					continue;
				}
				flag = true;
				x = num;
				y = i;
				break;
			}
			if (flag)
			{
				break;
			}
		}
		return flag;
	}
}
