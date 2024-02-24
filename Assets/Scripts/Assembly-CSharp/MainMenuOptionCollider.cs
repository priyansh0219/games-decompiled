using UnityEngine;

public class MainMenuOptionCollider : MonoBehaviour
{
	public MainMenuPrimaryOption option;

	private void OnMouseEnter()
	{
		option.isOpening = true;
		option.isClosing = false;
	}

	private void OnMouseExit()
	{
		option.isClosing = true;
		option.isOpening = false;
	}

	private void OnMouseDown()
	{
		option.ClickAction();
	}
}
