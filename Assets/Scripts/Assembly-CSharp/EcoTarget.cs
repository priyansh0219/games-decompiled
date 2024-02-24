using UnityEngine;

public class EcoTarget : MonoBehaviour, IEcoTarget, IScheduledUpdateBehaviour, IManagedBehaviour
{
	public EcoTargetType type;

	private EcoRegion currentRegion;

	private const float staggerUpdateTime = 1f;

	private float nextUpdateTime = -1f;

	private static EcoTargetTypeComparer sEcoTargetTypeComparer = new EcoTargetTypeComparer();

	public static EcoTargetTypeComparer EcoTargetTypeComparer => sEcoTargetTypeComparer;

	public int scheduledUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "EcoTarget";
	}

	public EcoTargetType GetTargetType()
	{
		return type;
	}

	public void SetTargetType(EcoTargetType newType)
	{
		if (newType != type)
		{
			if (currentRegion != null)
			{
				currentRegion.UnregisterTarget(this);
			}
			type = newType;
			if (currentRegion != null)
			{
				currentRegion.RegisterTarget(this);
			}
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public string GetName()
	{
		return base.name;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public void ScheduledUpdate()
	{
		if (Time.time < nextUpdateTime)
		{
			return;
		}
		while (nextUpdateTime < Time.time)
		{
			nextUpdateTime += 1f;
		}
		EcoRegion region = EcoRegionManager.main.GetRegion(base.transform.position, currentRegion);
		if (region != currentRegion)
		{
			if (currentRegion != null)
			{
				currentRegion.UnregisterTarget(this);
			}
			region?.RegisterTarget(this);
			currentRegion = region;
		}
	}

	private void OnEnable()
	{
		currentRegion = null;
		nextUpdateTime = Time.time + Random.Range(0f, 1f);
		UpdateSchedulerUtils.Register(this);
	}

	private void OnDisable()
	{
		if (currentRegion != null)
		{
			currentRegion.UnregisterTarget(this);
		}
		UpdateSchedulerUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		UpdateSchedulerUtils.Deregister(this);
	}
}
