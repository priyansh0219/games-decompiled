using UWE;
using UnityEngine;

public class AvatarInputHandler : MonoBehaviour
{
	public static AvatarInputHandler main;

	public bool clicked;

	private void Awake()
	{
		main = this;
	}

	private void OnEnable()
	{
		UWE.Utils.lockCursor = true;
	}

	private void OnDisable()
	{
	}

	private void Update()
	{
		FPSInputModule.current.EscapeMenu();
		if (Input.GetMouseButtonDown(0) && GUIUtility.hotControl == 0)
		{
			clicked = true;
			UWE.Utils.lockCursor = true;
		}
	}

	public bool IsEnabled()
	{
		if (base.gameObject.activeInHierarchy)
		{
			return UWE.Utils.lockCursor;
		}
		return false;
	}
}
