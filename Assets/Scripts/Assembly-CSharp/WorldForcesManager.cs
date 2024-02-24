using System.Collections.Generic;
using UnityEngine;

public class WorldForcesManager : MonoBehaviour
{
	private static WorldForcesManager instance;

	public const int invalidIndex = -1;

	private List<WorldForces> m_AllForces = new List<WorldForces>();

	public static WorldForcesManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Object.FindObjectOfType<WorldForcesManager>();
				if (instance == null)
				{
					instance = new GameObject("World Forces Manager").AddComponent<WorldForcesManager>();
				}
			}
			return instance;
		}
	}

	public void AddWorldForces(WorldForces forces)
	{
		forces.updaterIndex = m_AllForces.Count;
		m_AllForces.Add(forces);
	}

	public void RemoveWorldForces(WorldForces forces)
	{
		int updaterIndex = forces.updaterIndex;
		int index = m_AllForces.Count - 1;
		m_AllForces[updaterIndex] = m_AllForces[index];
		m_AllForces[updaterIndex].updaterIndex = updaterIndex;
		m_AllForces.RemoveAt(index);
		forces.updaterIndex = -1;
	}

	private void OnDisable()
	{
		while (m_AllForces.Count > 0)
		{
			m_AllForces[m_AllForces.Count - 1].updaterIndex = -1;
			m_AllForces.RemoveAt(m_AllForces.Count - 1);
		}
	}

	private void FixedUpdate()
	{
		int count = m_AllForces.Count;
		for (int i = 0; i < count; i++)
		{
			m_AllForces[i].DoFixedUpdate();
		}
	}
}
