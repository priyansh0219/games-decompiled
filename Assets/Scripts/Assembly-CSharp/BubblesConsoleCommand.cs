using UnityEngine;

public class BubblesConsoleCommand : MonoBehaviour
{
	public GameObject bubblePrefab;

	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "bubbles");
	}

	private void OnConsoleCommand_bubbles(NotificationCenter.Notification n)
	{
		for (int i = 0; i < Random.Range(5, 10); i++)
		{
			Utils.SpawnFromPrefab(bubblePrefab, null).transform.position = Utils.GetRandomPosInView(15f);
		}
	}
}
