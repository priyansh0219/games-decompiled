using UWE;
using UnityEngine;

public class InspectOnFirstPickup : MonoBehaviour
{
	[AssertNotNull]
	public Pickupable pickupAble;

	public string animParam;

	public bool restoreQuickSlot = true;

	public float inspectDuration = 4.34f;

	private GameObject dummy;

	private void Start()
	{
		pickupAble.pickedUpEvent.AddHandler(base.gameObject, OnPickedUp);
	}

	private void OnPickedUp(Pickupable p)
	{
		if (!GameOptions.GetVrAnimationMode() && !Player.main.isPiloting && (Player.main.AddUsedTool(pickupAble.GetTechType()) || PlayerToolConsoleCommands.debugFirstUse))
		{
			Player.main.armsController.StartInspectObjectAsync(this);
		}
	}

	private void CreateDummy()
	{
		dummy = Object.Instantiate(base.gameObject, Inventory.main.toolSocket);
		dummy.name = base.gameObject.name + " inspectdummy";
		dummy.transform.localPosition = Vector3.zero;
		dummy.transform.localRotation = Quaternion.identity;
		dummy.SetActive(value: true);
		MonoBehaviour[] componentsInChildren = dummy.GetComponentsInChildren<MonoBehaviour>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		Collider[] componentsInChildren2 = dummy.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = false;
		}
		Rigidbody component = dummy.GetComponent<Rigidbody>();
		if ((bool)component)
		{
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(component, isKinematic: true);
		}
		InventoryModel component2 = dummy.GetComponent<InventoryModel>();
		if ((bool)component2)
		{
			component2.UpdateModel(isPickedUp: true);
		}
	}

	public void OnInspectObjectBegin()
	{
		CreateDummy();
	}

	public void OnInspectObjectDone()
	{
		if ((bool)dummy)
		{
			Object.Destroy(dummy);
		}
	}
}
