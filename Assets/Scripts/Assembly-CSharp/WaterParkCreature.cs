using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AddressableAssets;

[ProtoContract]
public class WaterParkCreature : WaterParkItem, IProtoEventListener
{
	[SerializeField]
	[AssertNotNull]
	private WaterParkCreatureData data;

	private bool isMature;

	private double matureTime;

	private float scaleInside = 0.6f;

	private SwimBehaviour swimBehaviour;

	private float swimMinVelocity = 0.5f;

	private float swimMaxVelocity = 1f;

	private Vector3 swimTarget;

	private float swimInterval = 2f;

	private float timeNextSwim;

	private double breedInterval;

	private bool locomotionParametersOverrode;

	private float locomotionDriftFactor;

	private float outsideTurnSpeed;

	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public float age = -1f;

	[NonSerialized]
	[ProtoMember(3)]
	public float timeNextBreed = -1f;

	[NonSerialized]
	[ProtoMember(4)]
	public bool bornInside;

	private static readonly Type[] behavioursToDisableInside = new Type[6]
	{
		typeof(FMOD_CustomLoopingEmitter),
		typeof(MeleeAttack),
		typeof(WorldForces),
		typeof(HeroPeeperHealingTrigger),
		typeof(ItemPrefabData),
		typeof(EcoTarget)
	};

	private List<Behaviour> disabledBehaviours;

	private float outsideMoveMaxSpeed;

	private bool isInside;

	[ContextMenu("Make Scriptable Object")]
	public void MakeScriptableObject()
	{
	}

	public static int GetCreatureSize(TechType creature)
	{
		return TechData.GetItemSize(creature).x;
	}

	public static void Born(AssetReferenceGameObject creaturePrefabReference, WaterPark waterPark, Vector3 position)
	{
		waterPark.StartCoroutine(BornAsync(creaturePrefabReference, waterPark, position));
	}

	private static IEnumerator BornAsync(AssetReferenceGameObject creaturePrefabReference, WaterPark waterPark, Vector3 position)
	{
		if (creaturePrefabReference != null && creaturePrefabReference.RuntimeKeyIsValid())
		{
			CoroutineTask<GameObject> task = AddressablesUtility.InstantiateAsync(creaturePrefabReference.RuntimeKey as string, null, position, Quaternion.identity, awake: false);
			yield return task;
			GameObject result = task.GetResult();
			WaterParkCreature component = result.GetComponent<WaterParkCreature>();
			if (component != null)
			{
				component.age = 0f;
				component.bornInside = true;
				component.InitializeCreatureBornInWaterPark();
				result.transform.localScale = component.data.initialSize * Vector3.one;
			}
			Pickupable pickupable = result.EnsureComponent<Pickupable>();
			result.SetActive(value: true);
			waterPark.AddItem(pickupable);
		}
	}

	private void SetMatureTime()
	{
		isMature = false;
		matureTime = DayNightCycle.main.timePassed + (double)(data.growingPeriod * (1f - age));
	}

	public void ResetBreedTime()
	{
		timeNextBreed = (float)(DayNightCycle.main.timePassed + breedInterval);
	}

	public bool GetCanBreed()
	{
		if (data.canBreed)
		{
			return isMature;
		}
		return false;
	}

	private void InitializeCreatureBornInWaterPark()
	{
		Creature component = base.gameObject.GetComponent<Creature>();
		if (component != null)
		{
			component.friendlyToPlayer = true;
		}
		CreatureDeath component2 = base.gameObject.GetComponent<CreatureDeath>();
		if (component2 != null)
		{
			component2.respawn = false;
		}
	}

	private void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "setwpcage");
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		version = 2;
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (version < 2)
		{
			bornInside = !data.isPickupableOutside;
		}
		if (bornInside)
		{
			InitializeCreatureBornInWaterPark();
		}
	}

	public override void ManagedUpdate()
	{
		base.ManagedUpdate();
		if (currentWaterPark == null)
		{
			return;
		}
		UpdateMovement();
		double timePassed = DayNightCycle.main.timePassed;
		if (!isMature)
		{
			float a = (float)(matureTime - (double)data.growingPeriod);
			age = Mathf.InverseLerp(a, (float)matureTime, (float)timePassed);
			base.transform.localScale = Mathf.Lerp(data.initialSize, data.maxSize, age) * Vector3.one;
			if (age == 1f)
			{
				isMature = true;
				if (data.canBreed)
				{
					breedInterval = data.growingPeriod * 0.5f;
					if (timeNextBreed < 0f)
					{
						ResetBreedTime();
					}
				}
				else
				{
					AssetReferenceGameObject adultPrefab = data.adultPrefab;
					if (adultPrefab != null && adultPrefab.RuntimeKeyIsValid())
					{
						Born(adultPrefab, currentWaterPark, base.transform.position);
						SetWaterPark(null);
						UnityEngine.Object.Destroy(base.gameObject);
						return;
					}
				}
			}
		}
		if (GetCanBreed() && timePassed > (double)timeNextBreed)
		{
			ResetBreedTime();
			WaterParkCreature breedingPartner = currentWaterPark.GetBreedingPartner(this);
			if (breedingPartner != null)
			{
				breedingPartner.ResetBreedTime();
				Born(data.eggOrChildPrefab, currentWaterPark, base.transform.position + Vector3.down);
			}
		}
	}

	protected virtual void UpdateMovement()
	{
		if (Time.time > timeNextSwim)
		{
			swimTarget = currentWaterPark.GetRandomSwimTarget(this);
			swimBehaviour.SwimTo(swimTarget, Mathf.Lerp(swimMinVelocity, swimMaxVelocity, age));
			timeNextSwim = Time.time + swimInterval * UnityEngine.Random.Range(1f, 2f);
		}
	}

	protected override void OnAddToWP()
	{
		swimBehaviour = base.gameObject.GetComponent<SwimBehaviour>();
		base.gameObject.GetComponent<Creature>().enabled = false;
		SetInsideState();
		if (age < 0f)
		{
			CraftData.GetTechType(base.gameObject);
			if (data.adultPrefab != null && data.adultPrefab.RuntimeKeyIsValid())
			{
				age = 0f;
			}
			else
			{
				float value = base.transform.localScale.x * scaleInside;
				age = Mathf.InverseLerp(data.initialSize, data.maxSize, value);
			}
		}
		SetMatureTime();
		InvokeRepeating("ValidatePosition", UnityEngine.Random.value * 10f, 10f);
		base.OnAddToWP();
	}

	private void OnDrop()
	{
		if (currentWaterPark == null)
		{
			SetOutsideState();
			if (!data.isPickupableOutside)
			{
				UnityEngine.Object.Destroy(pickupable);
			}
		}
	}

	protected override void OnRemoveFromWP()
	{
		Creature component = base.gameObject.GetComponent<Creature>();
		LiveMixin component2 = base.gameObject.GetComponent<LiveMixin>();
		if (component2 != null && component2.IsAlive())
		{
			component.enabled = true;
		}
		component.SetScale(data.outsideSize);
		timeNextBreed = -1f;
		CancelInvoke();
		base.OnRemoveFromWP();
	}

	private void SetInsideState()
	{
		if (isInside)
		{
			return;
		}
		isInside = true;
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		Animator animator = base.gameObject.GetComponent<Creature>().GetAnimator();
		if (animator != null)
		{
			AnimateByVelocity component = animator.GetComponent<AnimateByVelocity>();
			if (component != null)
			{
				outsideMoveMaxSpeed = component.animationMoveMaxSpeed;
				component.animationMoveMaxSpeed = swimMaxVelocity;
			}
		}
		Locomotion component2 = base.gameObject.GetComponent<Locomotion>();
		component2.canMoveAboveWater = true;
		locomotionParametersOverrode = true;
		locomotionDriftFactor = component2.driftFactor;
		component2.driftFactor = 0.1f;
		component2.forwardRotationSpeed = 0.6f;
		if (swimBehaviour != null)
		{
			outsideTurnSpeed = swimBehaviour.turnSpeed;
			swimBehaviour.turnSpeed = 1f;
		}
		disabledBehaviours = new List<Behaviour>();
		Behaviour[] componentsInChildren = GetComponentsInChildren<Behaviour>(includeInactive: true);
		foreach (Behaviour behaviour in componentsInChildren)
		{
			if (behaviour == null)
			{
				Debug.LogWarning("Discarded missing behaviour on a WaterParkCreature gameObject", this);
			}
			else
			{
				if (!behaviour.enabled)
				{
					continue;
				}
				Type type = behaviour.GetType();
				for (int j = 0; j < behavioursToDisableInside.Length; j++)
				{
					if (type.Equals(behavioursToDisableInside[j]) || type.IsSubclassOf(behavioursToDisableInside[j]))
					{
						behaviour.enabled = false;
						disabledBehaviours.Add(behaviour);
						break;
					}
				}
			}
		}
	}

	private void SetOutsideState()
	{
		if (!isInside)
		{
			return;
		}
		isInside = false;
		Animator animator = base.gameObject.GetComponent<Creature>().GetAnimator();
		if (animator != null)
		{
			AnimateByVelocity component = animator.GetComponent<AnimateByVelocity>();
			if (component != null)
			{
				component.animationMoveMaxSpeed = outsideMoveMaxSpeed;
			}
		}
		Locomotion component2 = base.gameObject.GetComponent<Locomotion>();
		component2.canMoveAboveWater = false;
		if (locomotionParametersOverrode)
		{
			component2.driftFactor = locomotionDriftFactor;
			locomotionParametersOverrode = false;
			if (swimBehaviour != null)
			{
				swimBehaviour.turnSpeed = outsideTurnSpeed;
			}
		}
		if (disabledBehaviours == null)
		{
			return;
		}
		for (int i = 0; i < disabledBehaviours.Count; i++)
		{
			if (disabledBehaviours[i] != null)
			{
				disabledBehaviours[i].enabled = true;
			}
		}
	}

	public override void ValidatePosition()
	{
		base.ValidatePosition();
		if (currentWaterPark != null)
		{
			currentWaterPark.EnsurePointIsInside(ref swimTarget);
		}
	}

	public override int GetSize()
	{
		return TechData.GetItemSize(CraftData.GetTechType(base.gameObject)).x;
	}

	private void OnConsoleCommand_setwpcage(NotificationCenter.Notification n)
	{
		if (DevConsole.ParseFloat(n, 0, out var value))
		{
			value = Mathf.Clamp01(value);
			ErrorMessage.AddDebug("Setting creature age to " + value + ".");
			age = value;
			if (age < 0f)
			{
				timeNextBreed = -1f;
			}
			SetMatureTime();
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (currentWaterPark != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(swimTarget, 0.5f);
			Gizmos.DrawLine(base.transform.position, swimTarget);
		}
	}
}
