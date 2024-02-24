using UnityEngine;

public class GenericDropper : PlayerTool
{
	public override void OnToolUseAnim(GUIHand hand)
	{
		Transform transform = MainCamera.camera.transform;
		if (!Physics.Raycast(transform.position, transform.forward, out var _, 3f, -2621441))
		{
			GetComponent<Pickupable>().Drop(transform.position + transform.forward * 2.75f);
		}
	}
}
