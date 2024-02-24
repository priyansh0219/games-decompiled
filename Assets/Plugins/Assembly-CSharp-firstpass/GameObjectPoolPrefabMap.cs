using System;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPoolPrefabMap : ScriptableObject, ISerializationCallbackReceiver
{
	[Serializable]
	public class prefabInfo
	{
		public string prefabId;

		public int count;
	}

	public int[] namesHash;

	public prefabInfo[] prefabInfos;

	private static Dictionary<int, prefabInfo> m_map;

	public static Dictionary<int, prefabInfo> Map
	{
		get
		{
			if (m_map == null)
			{
				Resources.Load<GameObjectPoolPrefabMap>("GameObjectPools");
				if (m_map == null)
				{
					m_map = new Dictionary<int, prefabInfo>();
				}
			}
			return m_map;
		}
		private set
		{
			m_map = value;
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (m_map == null)
		{
			Map = new Dictionary<int, prefabInfo>();
		}
		m_map.Clear();
		for (int i = 0; i < namesHash.Length; i++)
		{
			m_map.Add(namesHash[i], prefabInfos[i]);
		}
	}
}
