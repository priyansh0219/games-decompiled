using UWE;
using UnityEngine;

public class VFXUnparentAfterSeconds : MonoBehaviour, GameObjectPool.IPooledObject
{
	public float timer;

	private float currentTime;

	private void LateUpdate()
	{
		currentTime -= Time.deltaTime;
		if (currentTime < 0f)
		{
			base.transform.parent = null;
		}
	}

	private void Awake()
	{
		currentTime = timer;
	}

	public void Spawn(float time = 0f, bool active = true)
	{
		currentTime = timer;
	}

	public void Despawn(float time = 0f)
	{
	}
}
