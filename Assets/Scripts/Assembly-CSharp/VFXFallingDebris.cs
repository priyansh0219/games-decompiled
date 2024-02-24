using UnityEngine;

[ExecuteInEditMode]
public class VFXFallingDebris : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour
{
	public int maxDebrisCount = 10;

	public float spawnFrequency = 1f;

	public float seaLevel;

	public VFXParticlesPool groundImpactFX;

	public VFXParticlesPool waterImpactFX;

	public float damageOnImpact = 10f;

	public float radius = 6f;

	public DamageType type = DamageType.Heat;

	public FMODAsset groundImpactSound;

	public FMODAsset waterImpactSound;

	private Quaternion impactOrientation;

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	private int prevMaxDebrisCount = 10;

	private float prevSpawnFrequency = 1f;

	private float prevSealevel;

	private bool particlesChanged;

	private ParticleCollisionEvent[] collisionEvents;

	private int pCount;

	public int managedUpdateIndex { get; set; }

	private void OnEnable()
	{
		BehaviourUpdateUtils.Register(this);
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	public string GetProfileTag()
	{
		return "FallingDebris";
	}

	public void Init()
	{
		ps = (ParticleSystem)base.transform.GetComponent(typeof(ParticleSystem));
		ps.maxParticles = maxDebrisCount;
		ps.emissionRate = spawnFrequency;
		particles = new ParticleSystem.Particle[maxDebrisCount];
		ps.SetParticles(particles, maxDebrisCount);
		prevMaxDebrisCount = maxDebrisCount;
		prevSpawnFrequency = spawnFrequency;
		prevSealevel = seaLevel;
		collisionEvents = new ParticleCollisionEvent[8];
	}

	public void ManagedUpdate()
	{
		if (!(ps != null))
		{
			return;
		}
		pCount = ps.GetParticles(particles);
		for (int i = 0; i < pCount; i++)
		{
			Vector3 position = particles[i].position;
			if (!(position.y < seaLevel))
			{
				continue;
			}
			position.y = seaLevel;
			if (waterImpactFX != null)
			{
				if (waterImpactSound != null)
				{
					Utils.PlayFMODAsset(waterImpactSound, position);
				}
				waterImpactFX.Play(position, null);
			}
			particles[i].remainingLifetime = 0f;
			particles[i].startLifetime = particles[i].startLifetime;
			particlesChanged = true;
		}
		if (particlesChanged)
		{
			ps.SetParticles(particles, pCount);
			particlesChanged = false;
		}
	}

	private void Awake()
	{
		impactOrientation.eulerAngles = new Vector3(-90f, 0f, 0f);
		Init();
	}

	private void OnParticleCollision(GameObject other)
	{
		int safeCollisionEventSize = ps.GetSafeCollisionEventSize();
		if (collisionEvents.Length < safeCollisionEventSize)
		{
			collisionEvents = new ParticleCollisionEvent[safeCollisionEventSize];
		}
		int num = ps.GetCollisionEvents(other, collisionEvents);
		for (int i = 0; i < num; i++)
		{
			if ((bool)other && groundImpactFX != null)
			{
				Vector3 intersection = collisionEvents[i].intersection;
				if (groundImpactSound != null)
				{
					Utils.PlayFMODAsset(groundImpactSound, intersection);
				}
				groundImpactFX.Play(intersection, null);
				DamageSystem.RadiusDamage(damageOnImpact, intersection, radius, type, base.gameObject);
			}
		}
	}
}
