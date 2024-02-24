using System;
using UnityEngine;

public class FloatingStone : MonoBehaviour
{
	private void Awake()
	{
		PrefabPlaceholdersGroup component = GetComponent<PrefabPlaceholdersGroup>();
		if (component != null)
		{
			component.OnPrefabGroupSpawned = (Action)Delegate.Combine(component.OnPrefabGroupSpawned, new Action(OnPrefabGroupSpawned));
			GetComponent<Rigidbody>().isKinematic = true;
		}
	}

	private void OnPrefabGroupSpawned()
	{
		GetComponent<Rigidbody>().isKinematic = false;
	}
}
