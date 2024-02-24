using System;
using System.Collections.Generic;
using UnityEngine;

public class VFXPool : MonoBehaviour
{
	[Serializable]
	public class FX
	{
		public enum OnLimitReached
		{
			DeleteOldest = 0,
			Skip = 1
		}

		public GameObject prefab;

		public int maxQuantity;

		public float fxDuration;

		public bool instantiateFirstCalls;

		public OnLimitReached onLimitReached;

		public int currentInstancesCount;
	}

	private static VFXPool _main;

	private Vector3 defaultAngles = new Vector3(-90f, 0f, 0f);

	public FX[] effects;

	private Dictionary<FX, List<GameObject>> pooledFX = new Dictionary<FX, List<GameObject>>();

	private Dictionary<GameObject, FX> usedFX = new Dictionary<GameObject, FX>();

	public static VFXPool main
	{
		get
		{
			if (_main == null)
			{
				_main = UnityEngine.Object.FindObjectOfType<VFXPool>();
			}
			return _main;
		}
	}

	public FX GetFX(GameObject prefab)
	{
		FX result = null;
		for (int i = 0; i < effects.Length; i++)
		{
			if (effects[i].prefab == prefab)
			{
				result = effects[i];
			}
		}
		return result;
	}

	public void Warmup(FX fx)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < fx.maxQuantity; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(fx.prefab, base.transform.position, Quaternion.identity);
			gameObject.GetComponent<VFXPooledFX>().fx = fx;
			gameObject.transform.parent = base.transform;
			gameObject.SetActive(value: false);
			list.Add(gameObject);
		}
		pooledFX.Add(fx, list);
		fx.currentInstancesCount += fx.maxQuantity;
	}

	public void WarmupAll()
	{
		pooledFX = new Dictionary<FX, List<GameObject>>();
		usedFX = new Dictionary<GameObject, FX>();
		for (int i = 0; i < effects.Length; i++)
		{
			if (!effects[i].instantiateFirstCalls)
			{
				Warmup(effects[i]);
			}
		}
	}

	public GameObject Play(FX fx, Vector3 position, Vector3 angles, Transform parent)
	{
		GameObject gameObject = null;
		if (pooledFX.TryGetValue(fx, out var value) && value.Count > 0)
		{
			gameObject = value[0];
			gameObject.transform.position = position;
			gameObject.transform.eulerAngles = angles;
			gameObject.transform.parent = parent;
			gameObject.SetActive(value: true);
			gameObject.GetComponent<ParticleSystem>().Play();
			usedFX.Add(gameObject, fx);
			value.RemoveAt(0);
		}
		return gameObject;
	}

	public GameObject Play(FX fx, Vector3 position, Vector3 angles)
	{
		return Play(fx, position, angles, null);
	}

	public GameObject Play(FX fx, Vector3 position)
	{
		return Play(fx, position, defaultAngles, null);
	}

	public GameObject Play(GameObject prefab, Vector3 position, Vector3 angles, Transform parent)
	{
		GameObject result = null;
		FX fX = GetFX(prefab);
		if (fX != null)
		{
			result = Play(fX, position, angles, parent);
		}
		return result;
	}

	public GameObject Play(GameObject prefab, Vector3 position, Vector3 angles)
	{
		GameObject result = null;
		FX fX = GetFX(prefab);
		if (fX != null)
		{
			result = Play(fX, position, angles);
		}
		return result;
	}

	public GameObject Play(GameObject prefab, Vector3 position)
	{
		GameObject result = null;
		FX fX = GetFX(prefab);
		if (fX != null)
		{
			result = Play(fX, position);
		}
		return result;
	}

	public void Stop(GameObject fxInstance)
	{
		fxInstance.GetComponent<ParticleSystem>().Stop();
	}

	public void StopAndRecycle(GameObject fxInstance, FX fx)
	{
		fxInstance.GetComponent<ParticleSystem>().Stop();
		Recycle(fxInstance, fx);
	}

	public void StopAndRecycle(GameObject fxInstance)
	{
		fxInstance.GetComponent<ParticleSystem>().Stop();
		Recycle(fxInstance);
	}

	public void Recycle(GameObject fxInstance)
	{
		if (usedFX.TryGetValue(fxInstance, out var value))
		{
			Recycle(fxInstance, value);
		}
	}

	public void Recycle(GameObject fxInstance, FX fx)
	{
		pooledFX[fx].Add(fxInstance);
		usedFX.Remove(fxInstance);
		fxInstance.GetComponent<ParticleSystem>().Clear();
		fxInstance.SetActive(value: false);
		fxInstance.transform.parent = base.transform;
		fxInstance.transform.position = base.transform.position;
	}

	private void Awake()
	{
		WarmupAll();
	}
}
