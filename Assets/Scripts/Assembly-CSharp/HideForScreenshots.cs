using System;
using System.Collections.Generic;
using UnityEngine;

public class HideForScreenshots : MonoBehaviour
{
	[Flags]
	public enum HideType
	{
		None = 0,
		Mask = 1,
		HUD = 2,
		ViewModel = 4
	}

	public bool recursive = true;

	public HideType type;

	public HashSet<Behaviour> behavioursToRenable = new HashSet<Behaviour>();

	public HashSet<Renderer> rendersToRenable = new HashSet<Renderer>();

	public HashSet<GameObject> objsToRenable = new HashSet<GameObject>();

	private void ProcessComponent(Behaviour b)
	{
		if (b != null && b.enabled)
		{
			b.enabled = false;
			behavioursToRenable.Add(b);
		}
	}

	private void ProcessComponent(Renderer r)
	{
		if (r != null && r.enabled)
		{
			r.enabled = false;
			rendersToRenable.Add(r);
		}
	}

	private void HideInternal(GameObject obj)
	{
		obj.BroadcastMessage("HideForScreenshots", SendMessageOptions.DontRequireReceiver);
		objsToRenable.Add(obj);
		ProcessComponent(obj.GetComponent<GUIText>());
		ProcessComponent(obj.GetComponent<GUITexture>());
		ProcessComponent(obj.GetComponent<Renderer>());
		if (recursive)
		{
			Transform transform = obj.transform;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				HideInternal(child.gameObject);
			}
		}
	}

	private void UnhideInternal()
	{
		foreach (Behaviour item in behavioursToRenable)
		{
			if (item != null)
			{
				item.enabled = true;
			}
		}
		foreach (Renderer item2 in rendersToRenable)
		{
			if (item2 != null)
			{
				item2.enabled = true;
			}
		}
		foreach (GameObject item3 in objsToRenable)
		{
			if (item3 != null)
			{
				item3.BroadcastMessage("UnhideForScreenshots", SendMessageOptions.DontRequireReceiver);
			}
		}
		behavioursToRenable.Clear();
		rendersToRenable.Clear();
		objsToRenable.Clear();
	}

	public static void Hide(HideType hide)
	{
		HideForScreenshots[] array = UnityEngine.Object.FindObjectsOfType<HideForScreenshots>();
		foreach (HideForScreenshots hideForScreenshots in array)
		{
			if ((hideForScreenshots.type & hide) != 0)
			{
				hideForScreenshots.HideInternal(hideForScreenshots.gameObject);
			}
			else
			{
				hideForScreenshots.UnhideInternal();
			}
		}
	}
}
