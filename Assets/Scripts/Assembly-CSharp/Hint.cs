using UnityEngine;

public class Hint : MonoBehaviour
{
	[AssertNotNull]
	public uGUI_PopupMessage message;

	[AssertNotNull]
	public uGUI_PopupMessage warning;

	public static Hint main { get; private set; }

	private void Awake()
	{
		if (main != null)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			main = this;
		}
	}
}
