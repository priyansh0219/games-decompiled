using System;
using UnityEngine;

public class CreateConsoleCommand : MonoBehaviour
{
	public GameObject[] prefabList;

	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "create");
	}

	private void OnConsoleCommand_create(NotificationCenter.Notification n)
	{
		if (n == null || n.data == null || n.data.Count <= 0)
		{
			return;
		}
		string text = (string)n.data[0];
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		int number = 1;
		if (n.data.Count > 1 && int.TryParse((string)n.data[1], out var result))
		{
			number = result;
		}
		DevConsole.ParseFloat(n, 2, out var value, 12f);
		bool flag = false;
		for (int i = 0; i < prefabList.Length; i++)
		{
			GameObject gameObject = prefabList[i];
			if (gameObject != null && string.Equals(gameObject.name, text, StringComparison.OrdinalIgnoreCase))
			{
				Utils.CreateNPrefabs(gameObject, value, number);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			ErrorMessage.AddDebug("Could not find prefab with name " + text);
		}
	}
}
