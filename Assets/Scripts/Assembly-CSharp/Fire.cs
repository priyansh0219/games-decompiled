using System.Collections;
using FMOD.Studio;
using UWE;
using UnityEngine;

[SkipProtoContractCheck]
public class Fire : HandTarget, IManagedUpdateBehaviour, IManagedBehaviour
{
	public GameObject fireFXprefab;

	public Light fireLight;

	public float lightRange = 1.75f;

	public float lightIntensity = 2f;

	public FMOD_CustomEmitter fireSound;

	[AssertNotNull]
	public LiveMixin livemixin;

	public bool introFire;

	public float fireGrowRate;

	public SubRoot fireSubRoot;

	public Vector3 minScale = Vector3.zero;

	public VFXExtinguishableFire fireFX;

	private bool playerisInFire;

	private float lastTimeDoused;

	private bool isExtinguished;

	private PARAMETER_ID fmodIndexFireHealth = FMODUWE.invalidParameterId;

	private Collider collider;

	public int managedUpdateIndex { get; set; }

	public override void Awake()
	{
		base.Awake();
		collider = GetComponent<Collider>();
	}

	private IEnumerator Start()
	{
		_ = fireFX == null;
		isExtinguished = !livemixin.IsAlive();
		if (isExtinguished)
		{
			Extinguished();
		}
		else
		{
			if (fireFX == null && !isExtinguished)
			{
				Transform parent = base.transform.parent;
				DeferredSpawner.Task spawnTask = DeferredSpawner.instance.InstantiateAsync(fireFXprefab, this, parent);
				yield return spawnTask;
				GameObject result = spawnTask.GetResult();
				if (!spawnTask.cancelled)
				{
					UWE.Utils.ZeroTransform(result.transform);
					fireFX = result.GetComponent<VFXExtinguishableFire>();
				}
			}
			if (fireLight != null && !fireLight.gameObject.activeSelf)
			{
				fireLight.gameObject.SetActive(value: true);
			}
		}
		BehaviourUpdateUtils.Register(this);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.Equals(Player.main.gameObject) && !playerisInFire)
		{
			playerisInFire = true;
			InvokeRepeating("DamagePlayerAtInterval", 0f, 1f);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.Equals(Player.main.gameObject))
		{
			playerisInFire = false;
			CancelInvoke("DamagePlayerAtInterval");
		}
	}

	private void DamagePlayerAtInterval()
	{
		if (!(Player.main.currentSub != fireSubRoot))
		{
			Player.main.GetComponent<LiveMixin>().TakeDamage(5f, base.transform.position, DamageType.Fire);
		}
	}

	public void Douse(float amount)
	{
		float healthFraction = livemixin.GetHealthFraction();
		if (FMODUWE.IsInvalidParameterId(fmodIndexFireHealth))
		{
			fmodIndexFireHealth = fireSound.GetParameterIndex("fire_health");
		}
		fireSound.SetParameterValue(fmodIndexFireHealth, healthFraction);
		lastTimeDoused = Time.time;
		livemixin.health = Mathf.Max(livemixin.health - amount, 0f);
		if ((bool)fireFX)
		{
			fireFX.amount = healthFraction;
		}
		base.transform.localScale = Vector3.Lerp(minScale, Vector3.one, healthFraction);
		if (!livemixin.IsAlive() && !isExtinguished)
		{
			Extinguished();
		}
		if (RequiresUpdate())
		{
			BehaviourUpdateUtils.Register(this);
		}
	}

	private void Extinguished()
	{
		if (introFire)
		{
			IntroLifepodDirector.main.ConcludeIntroSequence();
		}
		Object.Destroy(base.transform.parent.gameObject, 4f);
		if ((bool)fireFX)
		{
			fireFX.StopAndDestroy();
		}
		if (fireLight != null)
		{
			Object.Destroy(fireLight.gameObject);
		}
		CancelInvoke("DamagePlayerAtInterval");
		Collider component = base.gameObject.GetComponent<Collider>();
		if (component != null)
		{
			Object.Destroy(component);
		}
		isExtinguished = true;
		SendMessageUpwards("FireExtinguished", null, SendMessageOptions.DontRequireReceiver);
	}

	public int GetExtinguishPercent()
	{
		return (int)(Mathf.Clamp01(livemixin.health / livemixin.maxHealth) * 100f);
	}

	public bool IsExtinguished()
	{
		return isExtinguished;
	}

	public void ManagedUpdate()
	{
		if (fireFX != null && fireGrowRate != 0f && Time.time - lastTimeDoused > 0.5f)
		{
			float healthFraction = livemixin.GetHealthFraction();
			if (!isExtinguished)
			{
				livemixin.health = Mathf.Clamp(livemixin.health + fireGrowRate * Time.deltaTime, 0f, livemixin.maxHealth);
			}
			fireFX.amount = healthFraction;
			base.transform.localScale = Vector3.Lerp(minScale, Vector3.one, healthFraction);
		}
		if (!RequiresUpdate())
		{
			BehaviourUpdateUtils.Deregister(this);
		}
	}

	public void LateUpdate()
	{
		if (livemixin.health > 0f)
		{
			float healthFraction = livemixin.GetHealthFraction();
			if (fireLight != null)
			{
				float range = (lightRange + Random.value * 0.25f) * healthFraction + 1f;
				FlashingLightHelpers.SafeRangeChangePreFrame(fireLight, range);
				float intensity = healthFraction * (lightIntensity + Random.value * 0.5f);
				FlashingLightHelpers.SafeIntensityChangePerFrame(fireLight, intensity);
			}
		}
		else if (!isExtinguished)
		{
			Extinguished();
		}
		if (playerisInFire && collider != null && Player.main != null && !UWE.Utils.IsInsideCollider(collider, Player.main.transform.position))
		{
			playerisInFire = false;
			CancelInvoke("DamagePlayerAtInterval");
		}
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
		playerisInFire = false;
		CancelInvoke("DamagePlayerAtInterval");
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
		playerisInFire = false;
		CancelInvoke("DamagePlayerAtInterval");
		if (fireFX != null)
		{
			Object.Destroy(fireFX.gameObject);
		}
		if (fireLight != null)
		{
			Object.Destroy(fireLight.gameObject);
		}
	}

	public string GetProfileTag()
	{
		return "Fire";
	}

	private bool RequiresUpdate()
	{
		if (fireFX != null && fireGrowRate != 0f && !isExtinguished)
		{
			return livemixin.health != livemixin.maxHealth;
		}
		return false;
	}
}
