using UnityEngine;

public abstract class LazyUpdate : MonoBehaviour
{
	public PlayerDistanceTracker distanceTracker;

	public Renderer[] renderers;

	private float timeLastUpdate;

	protected float minDistance = 5f;

	protected float distanceMultiplier = 0.05f;

	protected float minDistanceVisible = 20f;

	protected float distanceMultiplierVisible = 0.01f;

	protected float maxDistance = 50f;

	protected virtual void Start()
	{
		timeLastUpdate = Time.time - Random.value * 0.4f;
		renderers = GetComponentsInChildren<Renderer>();
	}

	private void OnEnable()
	{
		timeLastUpdate = Time.time - Random.value * 0.4f;
	}

	protected abstract void UpdateLazy(float deltaTime);

	private void Update()
	{
		float num = 0.5f;
		float num2 = minDistanceVisible;
		float num3 = distanceMultiplierVisible;
		bool flag = false;
		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i].isVisible)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			num2 = minDistance;
			num3 = distanceMultiplier;
		}
		if (distanceTracker != null)
		{
			num = Mathf.Clamp(distanceTracker.distanceToPlayer - num2, 0f, maxDistance) * num3;
		}
		if (timeLastUpdate + num <= Time.time)
		{
			float deltaTime = Time.time - timeLastUpdate;
			UpdateLazy(deltaTime);
			timeLastUpdate = Time.time;
		}
	}
}
