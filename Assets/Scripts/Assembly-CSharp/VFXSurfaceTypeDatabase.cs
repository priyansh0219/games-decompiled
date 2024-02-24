using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VFXSurfaceTypeDatabase.asset", menuName = "Subnautica/Create VFXSurfaceTypeDatabase Asset")]
public class VFXSurfaceTypeDatabase : ScriptableObject, ISerializationCallbackReceiver
{
	[Serializable]
	private struct SurfaceEventMapping
	{
		public string surfaceType;

		public string eventType;

		public GameObject prefab;
	}

	[HideInInspector]
	[SerializeField]
	private List<SurfaceEventMapping> mappings;

	private GameObject[] prefabs;

	private int numEventTypes;

	private int numSurfaceTypes;

	private VFXSurfaceTypeDatabase()
	{
		VFXSurfaceTypes[] array = (VFXSurfaceTypes[])Enum.GetValues(typeof(VFXSurfaceTypes));
		VFXEventTypes[] array2 = (VFXEventTypes[])Enum.GetValues(typeof(VFXEventTypes));
		numEventTypes = array2.Length;
		numSurfaceTypes = array.Length;
		prefabs = new GameObject[numSurfaceTypes * numEventTypes];
	}

	public void OnBeforeSerialize()
	{
		mappings = new List<SurfaceEventMapping>();
		VFXSurfaceTypes[] obj = (VFXSurfaceTypes[])Enum.GetValues(typeof(VFXSurfaceTypes));
		VFXEventTypes[] array = (VFXEventTypes[])Enum.GetValues(typeof(VFXEventTypes));
		VFXSurfaceTypes[] array2 = obj;
		SurfaceEventMapping item = default(SurfaceEventMapping);
		for (int i = 0; i < array2.Length; i++)
		{
			VFXSurfaceTypes surfaceType = array2[i];
			item.surfaceType = surfaceType.ToString();
			VFXEventTypes[] array3 = array;
			for (int j = 0; j < array3.Length; j++)
			{
				VFXEventTypes eventType = array3[j];
				GameObject prefab = GetPrefab(surfaceType, eventType);
				if (prefab != null)
				{
					item.eventType = eventType.ToString();
					item.prefab = prefab;
					mappings.Add(item);
				}
			}
		}
	}

	public void OnAfterDeserialize()
	{
		foreach (SurfaceEventMapping mapping in mappings)
		{
			try
			{
				VFXSurfaceTypes surfaceType = (VFXSurfaceTypes)Enum.Parse(typeof(VFXSurfaceTypes), mapping.surfaceType);
				VFXEventTypes eventType = (VFXEventTypes)Enum.Parse(typeof(VFXEventTypes), mapping.eventType);
				SetPrefab(surfaceType, eventType, mapping.prefab);
			}
			catch (Exception message)
			{
				Debug.LogWarning(message);
			}
		}
		mappings = null;
	}

	public GameObject GetPrefab(VFXSurfaceTypes surfaceType, VFXEventTypes eventType)
	{
		int num = (int)((int)surfaceType * numEventTypes + eventType);
		if (num < 0 || num >= prefabs.Length)
		{
			return null;
		}
		return prefabs[num];
	}

	public void SetPrefab(VFXSurfaceTypes surfaceType, VFXEventTypes eventType, GameObject prefab)
	{
		prefabs[(int)((int)surfaceType * numEventTypes + eventType)] = prefab;
	}
}
