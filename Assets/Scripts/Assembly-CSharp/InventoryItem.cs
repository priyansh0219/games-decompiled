using System;
using UnityEngine;

public class InventoryItem
{
	public bool ignoreForSorting;

	public IItemsContainer container;

	public bool isEnabled = true;

	public bool isBarVisible = true;

	private TechType _techType;

	private int _x = -1;

	private int _y = -1;

	private int _width = 1;

	private int _height = 1;

	public TechType techType => _techType;

	public int x => _x;

	public int y => _y;

	public int width => _width;

	public int height => _height;

	public bool isBindable => TechData.GetEquipmentType(techType) == EquipmentType.Hand;

	public Pickupable item { get; private set; }

	public InventoryItem(Pickupable pickupable)
	{
		if (pickupable != null)
		{
			item = pickupable;
			item.SetInventoryItem(this);
		}
		else
		{
			Debug.LogException(new Exception("Attempt to initialize InventoryItem instance with null Pickupable object!"));
		}
	}

	public InventoryItem(int w, int h)
	{
		item = null;
		_width = w;
		_height = h;
	}

	public void SetTechType(TechType value)
	{
		_techType = value;
		Vector2int itemSize = TechData.GetItemSize(value);
		_width = itemSize.x;
		_height = itemSize.y;
	}

	public void SetGhostDims(int w, int h)
	{
		_width = w;
		_height = h;
	}

	public void SetPosition(int x, int y)
	{
		_x = x;
		_y = y;
	}

	public bool CanDrag(bool verbose)
	{
		if (item != null)
		{
			return container.AllowedToRemove(item, verbose);
		}
		return false;
	}
}
