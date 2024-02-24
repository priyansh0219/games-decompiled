using UnityEngine;

public class DockingBayDoor : MonoBehaviour
{
	public Openable openable;

	public float animTime = 0.1f;

	private void OnLaunchBayOpening()
	{
		openable.PlayOpenAnimation(openState: false, animTime);
	}
}
