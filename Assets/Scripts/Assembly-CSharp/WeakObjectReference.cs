using System.Collections.Generic;
using UnityEngine;

public class WeakObjectReference : MonoBehaviour
{
	private static Dictionary<string, Transform> objects = new Dictionary<string, Transform>();

	public string identifier = "";

	private void Awake()
	{
		Register();
	}

	private void Register()
	{
		if (string.IsNullOrEmpty(identifier))
		{
			Debug.LogError("WeakObjectReference : Identifier is not set!");
		}
		else if (objects.ContainsKey(identifier))
		{
			Debug.LogError("WeakObjectReference : Object reference for identifier '" + identifier + "' is already defined!");
		}
		else
		{
			objects.Add(identifier, GetComponent<Transform>());
		}
	}

	public static Transform TryGet(string id)
	{
		objects.TryGetValue(id, out var value);
		return value;
	}
}
