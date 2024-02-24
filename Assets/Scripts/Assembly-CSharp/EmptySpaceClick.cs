using UnityEngine;

public class EmptySpaceClick : MonoBehaviour
{
	private void Awake()
	{
	}

	private void OnMouseOver()
	{
		if (Input.GetMouseButtonDown(1))
		{
			DropHeld();
		}
	}

	private void OnMouseDown()
	{
		DropHeld();
	}

	private void DropHeld()
	{
		base.transform.parent.SendMessage("OnEmptySpaceClick", this, SendMessageOptions.DontRequireReceiver);
	}
}
