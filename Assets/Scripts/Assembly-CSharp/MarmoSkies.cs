using System.Collections.Generic;
using UnityEngine;
using mset;

public class MarmoSkies : MonoBehaviour
{
	public static MarmoSkies main;

	public GameObject skySafeShallowsPrefab;

	public GameObject skyBaseInteriorPrefab;

	public GameObject skyBaseGlassPrefab;

	public GameObject skyExplorableWreckPrefab;

	private Dictionary<GameObject, Sky> skies = new Dictionary<GameObject, Sky>();

	private Sky skySafeShallow;

	private Sky skyBaseInterior;

	private Sky skyBaseGlass;

	private Sky skyExplorableWreck;

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		skySafeShallow = GetSky(skySafeShallowsPrefab);
		skyBaseInterior = GetSky(skyBaseInteriorPrefab);
		skyBaseGlass = GetSky(skyBaseGlassPrefab);
		skyExplorableWreck = GetSky(skyExplorableWreckPrefab);
	}

	public Sky GetSky(GameObject skyPrefab)
	{
		if (skyPrefab == null)
		{
			return null;
		}
		if (!skies.TryGetValue(skyPrefab, out var value))
		{
			GameObject obj = Object.Instantiate(skyPrefab);
			obj.transform.SetParent(base.transform);
			value = obj.GetComponent<Sky>();
			skies.Add(skyPrefab, value);
		}
		return value;
	}

	public Sky GetSky(Skies sky)
	{
		switch (sky)
		{
		case Skies.SafeShallow:
			return skySafeShallow;
		case Skies.BaseInterior:
			return skyBaseInterior;
		case Skies.BaseGlass:
			return skyBaseGlass;
		case Skies.ExplorableWreck:
			return skyExplorableWreck;
		default:
			return null;
		}
	}
}
