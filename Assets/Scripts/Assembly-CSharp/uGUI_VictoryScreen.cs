using UnityEngine;

public class uGUI_VictoryScreen : MonoBehaviour
{
	public static uGUI_VictoryScreen main;

	[AssertNotNull]
	public GameObject victoryScreen;

	private void Start()
	{
		main = this;
	}

	public static void EnableVictoryScreen()
	{
		main.victoryScreen.SetActive(value: true);
	}
}
