using UnityEngine;

public class SimpleInputHandler : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Hide();
		}
	}

	public void Show()
	{
		if (!base.gameObject.activeSelf)
		{
			InputHandlerStack.main.Push(base.gameObject);
		}
	}

	public void Hide()
	{
		if (base.gameObject.activeSelf)
		{
			InputHandlerStack.main.Pop(base.gameObject);
		}
	}
}
