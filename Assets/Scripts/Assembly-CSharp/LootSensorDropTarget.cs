using System.Collections.Generic;
using UnityEngine;

public class LootSensorDropTarget : DropTarget
{
	public Transform lootSensorDisplay;

	public AudioClip lootPlacedClip;

	public List<TechType> validTargets = new List<TechType>();

	private Pickupable contents;

	private void Awake()
	{
	}

	public override bool DoesAcceptDrop(Pickupable p)
	{
		bool result = false;
		if (contents == null && validTargets.Contains(p.GetTechType()))
		{
			p.Drop(lootSensorDisplay.position);
			p.transform.rotation = lootSensorDisplay.gameObject.transform.rotation;
			contents = p;
			if ((bool)lootPlacedClip)
			{
				AudioSource.PlayClipAtPoint(lootPlacedClip, base.gameObject.transform.position);
			}
			result = true;
		}
		return result;
	}

	public string GetTechName()
	{
		if (!(contents != null))
		{
			return "";
		}
		return contents.GetTechName();
	}

	private void Update()
	{
		if (contents != null && !contents.isPickupable)
		{
			Debug.Log("Clearing pickupable from LootSensor.");
			contents = null;
		}
	}
}
