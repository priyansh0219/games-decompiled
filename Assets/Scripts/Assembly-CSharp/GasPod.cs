using UWE;
using UnityEngine;

public class GasPod : MonoBehaviour, IPropulsionCannonAmmo, IProtoTreeEventListener
{
	public float autoDetonateTime = 6f;

	public GameObject model;

	public Collider mainCollider;

	public GameObject gasEffectPrefab;

	public Pickupable pickup;

	private float detonateAtTime;

	private float detonateTime;

	private GameObject gasEffect;

	private bool detonated;

	public float damageRadius = 8f;

	public float damagePerSecond = 10f;

	public float damageInterval = 0.5f;

	public float smokeDuration = 15f;

	public bool isArtificial;

	private float timeLastDamageTick;

	private SphereCollider sphereCollider;

	public FMOD_StudioEventEmitter releaseSound;

	public FMOD_StudioEventEmitter burstSound;

	private TriggerStayTracker tracker;

	private bool grabbedByPropCannon;

	private bool wasShot;

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		if (pickup != null && !pickup.attached)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		if (releaseSound != null)
		{
			Utils.PlayEnvSound(releaseSound, base.transform.position, 5f);
		}
		tracker = GetComponent<TriggerStayTracker>();
		if (autoDetonateTime > 0f)
		{
			PrepareDetonationTime();
		}
		else
		{
			Detonate();
		}
	}

	private void PrepareDetonationTime()
	{
		detonateAtTime = Time.time + autoDetonateTime * 0.6f + Random.value * autoDetonateTime * 0.4f;
	}

	void IPropulsionCannonAmmo.OnGrab()
	{
		grabbedByPropCannon = true;
	}

	void IPropulsionCannonAmmo.OnShoot()
	{
		wasShot = true;
		PrepareDetonationTime();
	}

	void IPropulsionCannonAmmo.OnImpact()
	{
		Detonate();
	}

	void IPropulsionCannonAmmo.OnRelease()
	{
		if (!wasShot)
		{
			PrepareDetonationTime();
		}
		grabbedByPropCannon = false;
	}

	bool IPropulsionCannonAmmo.GetAllowedToGrab()
	{
		return !detonated;
	}

	bool IPropulsionCannonAmmo.GetAllowedToShoot()
	{
		return true;
	}

	private void OnDrop()
	{
		PrepareDetonationTime();
		if ((bool)LargeWorldStreamer.main)
		{
			LargeWorldStreamer.main.MakeEntityTransient(base.gameObject);
		}
	}

	private void Detonate()
	{
		if (!detonated)
		{
			detonated = true;
			detonateTime = Time.time;
			model.SetActive(value: false);
			mainCollider.enabled = false;
			gasEffect = Object.Instantiate(gasEffectPrefab);
			gasEffect.transform.parent = base.transform;
			UWE.Utils.ZeroTransform(gasEffect);
			sphereCollider = base.gameObject.AddComponent<SphereCollider>();
			sphereCollider.radius = damageRadius;
			sphereCollider.isTrigger = true;
			if (burstSound != null)
			{
				Utils.PlayEnvSound(burstSound, base.transform.position, 13f);
			}
		}
	}

	private void Update()
	{
		if (detonated)
		{
			if (timeLastDamageTick + damageInterval <= Time.time)
			{
				foreach (GameObject item in tracker.Get())
				{
					if (!item)
					{
						continue;
					}
					LiveMixin component = item.GetComponent<LiveMixin>();
					if (component == null || !component.IsAlive())
					{
						continue;
					}
					Player component2 = item.GetComponent<Player>();
					if ((component2 != null && component2.IsInside()) || (component2 == null && item.GetComponent<Living>() == null))
					{
						continue;
					}
					WaterParkItem component3 = item.GetComponent<WaterParkItem>();
					if (!(component3 != null) || !component3.IsInsideWaterPark())
					{
						component.TakeDamage(damagePerSecond * damageInterval, item.transform.position, DamageType.Poison);
						if (!isArtificial)
						{
							component.NotifyCreatureDeathsOfCreatureAttack();
						}
					}
				}
				timeLastDamageTick = Time.time;
			}
			if (detonateTime + smokeDuration <= Time.time)
			{
				Object.Destroy(base.gameObject);
			}
		}
		else if (!grabbedByPropCannon && detonateAtTime <= Time.time)
		{
			Detonate();
		}
	}
}
