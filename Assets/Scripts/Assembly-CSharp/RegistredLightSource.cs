using System.Collections.Generic;
using UnityEngine;

public class RegistredLightSource : MonoBehaviour
{
	[AssertNotNull]
	public Light hostLight;

	private static readonly HashSet<RegistredLightSource> lightSources = new HashSet<RegistredLightSource>();

	public static RegistredLightSource GetNearestLight(Vector3 position, float maxRange)
	{
		if (lightSources.Count < 1)
		{
			return null;
		}
		RegistredLightSource result = null;
		float num = maxRange;
		HashSet<RegistredLightSource>.Enumerator enumerator = lightSources.GetEnumerator();
		while (enumerator.MoveNext())
		{
			RegistredLightSource current = enumerator.Current;
			if (!Mathf.Approximately(0f, current.GetIntensity()))
			{
				float num2 = Vector3.Distance(position, current.transform.position);
				if (num2 < num && num2 < current.GetRange())
				{
					result = current;
					num = num2;
				}
			}
		}
		return result;
	}

	private void OnEnable()
	{
		lightSources.Add(this);
	}

	private void OnDisable()
	{
		lightSources.Remove(this);
	}

	public Vector3 GetPosition()
	{
		return hostLight.transform.position;
	}

	public float GetRange()
	{
		return hostLight.range;
	}

	public float GetIntensity()
	{
		if (!hostLight.enabled || !hostLight.gameObject.activeInHierarchy)
		{
			return 0f;
		}
		return hostLight.intensity;
	}

	public Light GetHostLight()
	{
		return hostLight;
	}
}
