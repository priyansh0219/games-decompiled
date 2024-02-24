using System;
using UWE;
using UnityEngine;

public class LootSensor : MonoBehaviour
{
	public TechType scanTech = TechType.ScrapMetal;

	public AudioClip scanClip;

	public AudioClip detectedClip;

	public float scanInterval = 6f;

	public Light powerLight;

	public int scanRange = 300;

	public LootSensorDropTarget lootSensorDropTarget;

	private float timeSinceScan;

	private bool on = true;

	private float originalLightIntensity;

	public static bool IsLootDetected(TechType[] techTypes, Vector3 center, int range, out TechType detectedType)
	{
		detectedType = TechType.None;
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(center, range);
		for (int i = 0; i < num; i++)
		{
			Collider collider = UWE.Utils.sharedColliderBuffer[i];
			Pickupable pickupable = collider.gameObject.GetComponent<Pickupable>();
			if (!pickupable)
			{
				pickupable = Utils.FindAncestorWithComponent<Pickupable>(collider.gameObject);
			}
			if ((bool)pickupable && Array.IndexOf(techTypes, pickupable.GetTechType()) > -1 && pickupable.isPickupable && (collider.gameObject.transform.position - center).magnitude > 1f)
			{
				detectedType = pickupable.GetTechType();
				return true;
			}
		}
		return false;
	}

	public static bool IsLootDetected(TechType techType, Vector3 center, int range)
	{
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(center, range);
		for (int i = 0; i < num; i++)
		{
			Collider collider = UWE.Utils.sharedColliderBuffer[i];
			Pickupable pickupable = collider.gameObject.GetComponent<Pickupable>();
			if (!pickupable)
			{
				pickupable = Utils.FindAncestorWithComponent<Pickupable>(collider.gameObject);
			}
			if ((bool)pickupable && pickupable.GetTechType() == techType && pickupable.isPickupable && (collider.gameObject.transform.position - center).magnitude > 1f)
			{
				return true;
			}
		}
		return false;
	}

	public string GetLootSensingTechName()
	{
		return lootSensorDropTarget.GetTechName();
	}

	private void Update()
	{
		timeSinceScan += Time.deltaTime;
		if (on && timeSinceScan > scanInterval)
		{
			bool num = IsLootDetected(scanTech, base.transform.position, scanRange);
			if (num)
			{
				ErrorMessage.AddMessage("LootSensor detected " + scanTech.AsString());
			}
			if (num)
			{
				FMODUWE.PlayOneShot("event:/interface/on_long", base.gameObject.transform.position, 0.5f);
			}
			if (!num)
			{
				FMODUWE.PlayOneShot("event:/interface/ping", base.gameObject.transform.position, 0.5f);
			}
			timeSinceScan = 0f;
		}
	}
}
