using UnityEngine;

public class ResourceTracker : MonoBehaviour, ICompileTimeCheckable, ILocalizationCheckable
{
	public TechType overrideTechType;

	[AssertNotNull]
	public PrefabIdentifier prefabIdentifier;

	[Tooltip("Optional")]
	public Rigidbody rb;

	[Tooltip("Optional")]
	public Pickupable pickupable;

	private string uniqueId = "";

	private TechType techType;

	private void StartUpdatePosition()
	{
		if (!GetComponent<ResourceTrackerUpdater>())
		{
			base.gameObject.AddComponent<ResourceTrackerUpdater>();
		}
	}

	private void StopUpdatePosition()
	{
		ResourceTrackerUpdater component = GetComponent<ResourceTrackerUpdater>();
		if ((bool)component)
		{
			Object.Destroy(component);
		}
	}

	private void Register()
	{
		ResourceTrackerDatabase.Register(uniqueId, base.transform.position, techType);
	}

	private void Unregister()
	{
		StopUpdatePosition();
		ResourceTrackerDatabase.Unregister(uniqueId, techType);
	}

	private void Start()
	{
		uniqueId = prefabIdentifier.Id;
		techType = ((overrideTechType == TechType.None) ? CraftData.GetTechType(base.gameObject) : overrideTechType);
		if ((bool)pickupable)
		{
			pickupable.pickedUpEvent.AddHandler(base.gameObject, OnPickedUp);
			pickupable.droppedEvent.AddHandler(base.gameObject, OnDropped);
		}
		if (!pickupable || !pickupable.attached)
		{
			Register();
			if ((bool)rb && !rb.isKinematic)
			{
				StartUpdatePosition();
			}
		}
	}

	public void OnKill()
	{
		Unregister();
	}

	public void OnExamine()
	{
		Unregister();
	}

	public void OnPickedUp(Pickupable p)
	{
		Unregister();
	}

	public void OnShinyPickUp(GameObject obj)
	{
		StartUpdatePosition();
	}

	public void OnDropped(Pickupable p)
	{
		Register();
		if ((bool)rb && !rb.isKinematic)
		{
			StartUpdatePosition();
		}
	}

	public void OnBreakResource()
	{
		Unregister();
	}

	public void OnBlueprintHandTargetUsed()
	{
		Unregister();
		base.enabled = false;
	}

	public void UpdatePosition()
	{
		Register();
	}

	private void OnScanned(PDAScanner.EntryData entryData)
	{
		if (entryData.destroyAfterScan)
		{
			Unregister();
		}
	}

	public string CompileTimeCheck()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (rb != component)
		{
			if (!component)
			{
				return "rb field must be empty";
			}
			return "rb field must reference existing Rigidbody component";
		}
		Pickupable component2 = GetComponent<Pickupable>();
		if (pickupable != component2)
		{
			pickupable = component2;
			if (!component2)
			{
				return "pickupable field must be empty";
			}
			return "pickupable field must reference existing Pickupable component";
		}
		return null;
	}

	public string CompileTimeCheck(ILanguage language)
	{
		TechType techType = ((overrideTechType == TechType.None) ? CraftData.GetTechType(base.gameObject) : overrideTechType);
		return language.CheckTechType(techType);
	}
}
