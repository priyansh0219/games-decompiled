using System;
using Gendarme;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class LiveMixin : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour, IShouldSerialize, ISerializationCallbackReceiver
{
	[AssertNotNull]
	public LiveMixinData data;

	private const float defaultTempDamage = -1f;

	[ProtoMember(1)]
	public float health;

	[NonSerialized]
	[ProtoMember(3)]
	public float tempDamage = -1f;

	private bool cinematicModeActive;

	[ReadOnly]
	[Obsolete("This is a nasty hack. Set health instead!")]
	public float startHealthPercent = 1f;

	private static float tempDamageHealRate = 2.5f;

	public FMOD_StudioEventEmitter damageClip;

	public FMOD_StudioEventEmitter deathClip;

	private float timeLastDamageEffect;

	private float timeLastElecDamageEffect;

	private float defaultHealth = -1f;

	private GameObject loopingDamageEffectObj;

	private static readonly ObjectPool<DamageInfo> damageInfoPool = ObjectPoolHelper.CreatePool<DamageInfo>(5000);

	private DamageInfo damageInfo;

	private IOnTakeDamage[] damageReceivers = new IOnTakeDamage[0];

	private CreatureDeath[] OnAttackByCreatureReceivers = new CreatureDeath[0];

	private static readonly ObjectPool<Event<float>> floatEventPool = ObjectPoolHelper.CreatePool<Event<float>>(7500);

	public Event<float> onHealDamage;

	public Event<float> onHealTempDamage;

	[NonSerialized]
	public bool invincible;

	[NonSerialized]
	public bool shielded;

	public Player player;

	public float maxHealth => data.maxHealth;

	public float minDamageForSound => data.minDamageForSound;

	public float loopEffectBelowPercent => data.loopEffectBelowPercent;

	public GameObject damageEffect => data.damageEffect;

	public GameObject deathEffect => data.deathEffect;

	public GameObject electricalDamageEffect => data.electricalDamageEffect;

	public GameObject loopingDamageEffect => data.loopingDamageEffect;

	public bool destroyOnDeath => data.destroyOnDeath;

	public bool weldable => data.weldable;

	public bool knifeable => data.knifeable;

	public bool canResurrect => data.canResurrect;

	public bool passDamageDataOnDeath => data.passDamageDataOnDeath;

	public bool broadcastKillOnDeath => data.broadcastKillOnDeath;

	public bool invincibleInCreative => data.invincibleInCreative;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "LiveMixin";
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		defaultHealth = health;
	}

	private void Start()
	{
		damageReceivers = GetComponents<IOnTakeDamage>();
		OnAttackByCreatureReceivers = GetComponents<CreatureDeath>();
		if (health <= 0f && tempDamage < 0f)
		{
			health = maxHealth * startHealthPercent;
			tempDamage = 0f;
		}
		if ((bool)loopingDamageEffect && !loopingDamageEffectObj && GetHealthFraction() < loopEffectBelowPercent)
		{
			loopingDamageEffectObj = Utils.SpawnZeroedAt(loopingDamageEffect, base.transform);
		}
		if (player != null)
		{
			player.playerRespawnEvent.AddHandler(base.gameObject, OnRespawn);
		}
		SyncUpdatingState();
	}

	public void OnRespawn(Player p)
	{
		health = maxHealth * startHealthPercent;
	}

	public void ResetHealth()
	{
		health = maxHealth;
	}

	public bool IsFullHealth()
	{
		return Mathf.Approximately(health, maxHealth);
	}

	public bool IsWeldable()
	{
		return weldable;
	}

	public float GetHealthFraction()
	{
		return health / maxHealth;
	}

	private bool IsCinematicActive()
	{
		if ((bool)player)
		{
			return player.cinematicModeActive;
		}
		return false;
	}

	private bool ShouldKillInCinematic()
	{
		if ((bool)player)
		{
			if (player.IsInSub())
			{
				return player.GetMode() == Player.Mode.Piloting;
			}
			return false;
		}
		return false;
	}

	public void NotifyAllAttachedDamageReceivers(DamageInfo inDamage)
	{
		if (base.gameObject.activeInHierarchy)
		{
			for (int i = 0; i < damageReceivers.Length; i++)
			{
				damageReceivers[i]?.OnTakeDamage(inDamage);
			}
		}
	}

	public void NotifyCreatureDeathsOfCreatureAttack()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		for (int i = 0; i < OnAttackByCreatureReceivers.Length; i++)
		{
			CreatureDeath creatureDeath = OnAttackByCreatureReceivers[i];
			if (creatureDeath != null)
			{
				creatureDeath.OnAttackByCreature();
			}
		}
	}

	public bool TakeDamage(float originalDamage, Vector3 position = default(Vector3), DamageType type = DamageType.Normal, GameObject dealer = null)
	{
		bool result = false;
		bool flag = GetComponent<BaseCell>() != null;
		bool flag2 = GameModeUtils.IsInvisible() && (invincibleInCreative || flag);
		if (health > 0f && !invincible && !flag2)
		{
			float num = 0f;
			if (!shielded)
			{
				num = DamageSystem.CalculateDamage(originalDamage, type, base.gameObject, dealer);
			}
			health = Mathf.Max(0f, health - num);
			if (type == DamageType.Cold || type == DamageType.Poison)
			{
				tempDamage += num;
				SyncUpdatingState();
			}
			damageInfo.Clear();
			damageInfo.originalDamage = originalDamage;
			damageInfo.damage = num;
			damageInfo.position = ((position == default(Vector3)) ? base.transform.position : position);
			damageInfo.type = type;
			damageInfo.dealer = dealer;
			NotifyAllAttachedDamageReceivers(damageInfo);
			if (shielded)
			{
				return result;
			}
			if ((bool)damageClip && num > 0f && num >= minDamageForSound && type != DamageType.Radiation)
			{
				Utils.PlayEnvSound(damageClip, damageInfo.position);
			}
			if ((bool)loopingDamageEffect && !loopingDamageEffectObj && GetHealthFraction() < loopEffectBelowPercent)
			{
				loopingDamageEffectObj = UWE.Utils.InstantiateWrap(loopingDamageEffect, base.transform.position, Quaternion.identity);
				loopingDamageEffectObj.transform.parent = base.transform;
			}
			if (Time.time > timeLastElecDamageEffect + 2.5f && type == DamageType.Electrical && electricalDamageEffect != null)
			{
				FixedBounds component = base.gameObject.GetComponent<FixedBounds>();
				Bounds bounds = ((!(component != null)) ? UWE.Utils.GetEncapsulatedAABB(base.gameObject) : component.bounds);
				GameObject obj = UWE.Utils.InstantiateWrap(electricalDamageEffect, bounds.center, Quaternion.identity);
				obj.transform.parent = base.transform;
				obj.transform.localScale = bounds.size * 0.65f;
				timeLastElecDamageEffect = Time.time;
			}
			else if (Time.time > timeLastDamageEffect + 1f && num > 0f && damageEffect != null && (type == DamageType.Normal || type == DamageType.Collide || type == DamageType.Explosive || type == DamageType.Puncture || type == DamageType.LaserCutter || type == DamageType.Drill))
			{
				Utils.SpawnPrefabAt(damageEffect, base.transform, damageInfo.position);
				timeLastDamageEffect = Time.time;
			}
			if (health <= 0f || health - tempDamage <= 0f)
			{
				result = true;
				if (!IsCinematicActive() || ShouldKillInCinematic())
				{
					Kill(type);
				}
				else
				{
					cinematicModeActive = true;
					SyncUpdatingState();
				}
			}
		}
		return result;
	}

	public float AddHealth(float healthBack)
	{
		float result = 0f;
		if ((IsAlive() || canResurrect) && health < maxHealth)
		{
			float num = health;
			health = Mathf.Min(health + healthBack, maxHealth);
			float num2 = health - num;
			result = num2;
			onHealDamage.Trigger(num2);
			if (health == maxHealth)
			{
				base.gameObject.SendMessage("OnRepair", SendMessageOptions.DontRequireReceiver);
			}
			if ((bool)loopingDamageEffect && (bool)loopingDamageEffectObj && GetHealthFraction() > loopEffectBelowPercent)
			{
				UnityEngine.Object.Destroy(loopingDamageEffectObj);
			}
		}
		return result;
	}

	public bool IsAlive()
	{
		return health > 0f;
	}

	public void Kill(DamageType damageType = DamageType.Normal)
	{
		health = 0f;
		tempDamage = 0f;
		SyncUpdatingState();
		if ((bool)deathClip)
		{
			Utils.PlayEnvSound(deathClip, base.transform.position, 25f);
		}
		if (deathEffect != null)
		{
			UWE.Utils.InstantiateWrap(deathEffect, base.transform.position, Quaternion.identity);
		}
		if (passDamageDataOnDeath)
		{
			base.gameObject.BroadcastMessage("OnKill", damageType, SendMessageOptions.DontRequireReceiver);
		}
		else if (broadcastKillOnDeath)
		{
			base.gameObject.BroadcastMessage("OnKill", SendMessageOptions.DontRequireReceiver);
		}
		if (destroyOnDeath)
		{
			CleanUp();
			UWE.Utils.DestroyWrap(base.gameObject);
		}
	}

	private void HealTempDamage(float timePassed)
	{
		if (tempDamage > 0f && health < maxHealth)
		{
			if (timePassed > 0f)
			{
				float b = timePassed * tempDamageHealRate;
				float num = Mathf.Min(Mathf.Min(tempDamage, b), maxHealth - health);
				health += num;
				tempDamage -= num;
				onHealTempDamage.Trigger(num);
				health = Mathf.Min(health, maxHealth);
			}
		}
		else
		{
			tempDamage = 0f;
		}
		if (tempDamage == 0f)
		{
			SyncUpdatingState();
		}
	}

	public void Awake()
	{
		damageInfo = damageInfoPool.Get();
		onHealDamage = floatEventPool.Get();
		onHealTempDamage = floatEventPool.Get();
	}

	private void OnEnable()
	{
		SyncUpdatingState();
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void CleanUp()
	{
		if (onHealDamage != null)
		{
			onHealDamage.Clear();
			floatEventPool.Return(onHealDamage);
		}
		if (onHealTempDamage != null)
		{
			onHealTempDamage.Clear();
			floatEventPool.Return(onHealTempDamage);
		}
		if (damageInfo != null)
		{
			damageInfo.Clear();
			damageInfoPool.Return(damageInfo);
		}
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		CleanUp();
	}

	private void SyncUpdatingState()
	{
		if (cinematicModeActive || tempDamage > 0f)
		{
			BehaviourUpdateUtils.Register(this);
		}
		else
		{
			BehaviourUpdateUtils.Deregister(this);
		}
	}

	public void ManagedUpdate()
	{
		HealTempDamage(Time.deltaTime);
		if (cinematicModeActive && !IsCinematicActive())
		{
			Kill();
			cinematicModeActive = false;
			SyncUpdatingState();
		}
	}

	[SuppressMessage("Gendarme.Rules.Correctness", "AvoidFloatingPointEqualityRule")]
	public bool ShouldSerialize()
	{
		if (health == defaultHealth)
		{
			return tempDamage != -1f;
		}
		return true;
	}

	[ContextMenu("MakeScriptableObject")]
	public void MakeScriptableObject()
	{
		MakeAndGetScriptableObject(string.Empty);
	}

	public LiveMixinData MakeAndGetScriptableObject(string overrideStr)
	{
		return null;
	}
}
