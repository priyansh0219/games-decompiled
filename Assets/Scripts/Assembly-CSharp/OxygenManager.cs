using System;
using System.Collections.Generic;
using UnityEngine;

public class OxygenManager : MonoBehaviour
{
	private float oxygenUnitsPerSecondSurface = 30f;

	private List<IOxygenSource> sources = new List<IOxygenSource>();

	private static readonly Comparison<IOxygenSource> sourcesComparison = delegate(IOxygenSource a, IOxygenSource b)
	{
		int num = ((!a.IsPlayer()) ? (a.IsBreathable() ? 1 : 2) : 0);
		int num2 = ((!b.IsPlayer()) ? (b.IsBreathable() ? 1 : 2) : 0);
		return num - num2;
	};

	private void Update()
	{
		AddOxygenAtSurface(Time.deltaTime);
	}

	public void RegisterSource(IOxygenSource src)
	{
		if (!sources.Contains(src))
		{
			sources.Add(src);
			sources.Sort(sourcesComparison);
		}
	}

	public void UnregisterSource(IOxygenSource src)
	{
		sources.Remove(src);
	}

	public void GetTotal(out float available, out float capacity)
	{
		available = 0f;
		capacity = 0f;
		for (int i = 0; i < sources.Count; i++)
		{
			IOxygenSource oxygenSource = sources[i];
			if (oxygenSource.IsBreathable())
			{
				available += oxygenSource.GetOxygenAvailable();
				capacity += oxygenSource.GetOxygenCapacity();
			}
		}
	}

	public float GetOxygenAvailable()
	{
		float num = 0f;
		for (int i = 0; i < sources.Count; i++)
		{
			if (sources[i].IsBreathable())
			{
				num += sources[i].GetOxygenAvailable();
			}
		}
		return num;
	}

	public float GetOxygenCapacity()
	{
		float num = 0f;
		for (int i = 0; i < sources.Count; i++)
		{
			if (sources[i].IsBreathable())
			{
				num += sources[i].GetOxygenCapacity();
			}
		}
		return num;
	}

	public float GetOxygenFraction()
	{
		GetTotal(out var available, out var capacity);
		if (!(capacity > 0f))
		{
			return 0f;
		}
		return available / capacity;
	}

	public float AddOxygen(float secondsToAdd)
	{
		float num = 0f;
		for (int i = 0; i < sources.Count; i++)
		{
			float num2 = sources[i].AddOxygen(secondsToAdd);
			secondsToAdd -= num2;
			num += num2;
			if (Utils.NearlyEqual(secondsToAdd, 0f))
			{
				break;
			}
		}
		return num;
	}

	public float RemoveOxygen(float amountToRemove)
	{
		float num = 0f;
		for (int num2 = sources.Count - 1; num2 >= 0; num2--)
		{
			IOxygenSource oxygenSource = sources[num2];
			if (oxygenSource.IsBreathable())
			{
				float num3 = oxygenSource.RemoveOxygen(amountToRemove);
				num += num3;
				amountToRemove -= num3;
			}
		}
		return num;
	}

	public bool HasOxygenTank()
	{
		for (int i = 0; i < sources.Count; i++)
		{
			if (sources[i].IsBreathable() && !sources[i].IsPlayer())
			{
				return true;
			}
		}
		return false;
	}

	public void Restore()
	{
		AddOxygen(GetOxygenCapacity());
	}

	private void AddOxygenAtSurface(float timeInterval)
	{
		float secondsToAdd = timeInterval * oxygenUnitsPerSecondSurface;
		bool flag = false;
		for (int i = 0; i < sources.Count; i++)
		{
			if (sources[i].gameObject.transform.position.y > Ocean.GetOceanLevel() - 1f)
			{
				flag = true;
				break;
			}
		}
		Player component = GetComponent<Player>();
		bool flag2 = false;
		if (component != null)
		{
			if (component.currentWaterPark != null)
			{
				flag = false;
			}
			flag = flag || component.CanBreathe();
			flag2 = component.cinematicModeActive;
		}
		if (flag && !flag2)
		{
			AddOxygen(secondsToAdd);
		}
	}
}
