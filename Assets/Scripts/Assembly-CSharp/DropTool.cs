using UnityEngine;

public class DropTool : PlayerTool
{
	public float pushForce = 8f;

	public override void OnToolUseAnim(GUIHand guiHand)
	{
		if (Inventory.CanDropItemHere(pickupable, notify: true) && guiHand.GetTool() == this)
		{
			pickupable.Drop(GetDropPosition(), MainCameraControl.main.transform.forward * pushForce, checkPosition: false);
		}
	}

	public Vector3 GetDropPosition()
	{
		Vector3 result = MainCameraControl.main.transform.forward * 1.07f + MainCameraControl.main.transform.position;
		if (Physics.Raycast(new Ray(MainCameraControl.main.transform.position, MainCameraControl.main.transform.forward), out var hitInfo, 1.07f, -1, QueryTriggerInteraction.Ignore))
		{
			result = hitInfo.point + hitInfo.normal * 0.2f;
		}
		return result;
	}

	public override bool OnRightHandDown()
	{
		return Inventory.CanDropItemHere(pickupable, notify: true);
	}
}
