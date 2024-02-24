using System;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class PropulseCannonAmmoHandler : MonoBehaviour, IProtoEventListener, IMovementPlatform
{
	public GameObject fxTrailPrefab;

	private GameObject fxTrailInstance;

	private const float maxTime = 3f;

	private const int currentVersion = 1;

	private DealDamageOnImpact damageOnImpact;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public bool addedDamageOnImpact;

	[NonSerialized]
	[ProtoMember(3)]
	public bool behaviorWasEnabled;

	[NonSerialized]
	[ProtoMember(4)]
	public bool wasShot;

	[NonSerialized]
	[ProtoMember(5)]
	public float timeShot = -1f;

	[NonSerialized]
	[ProtoMember(6)]
	public bool locomotionWasEnabled;

	[NonSerialized]
	[ProtoMember(7)]
	public Vector3 velocity;

	private CollisionDetectionMode collisionDetectionMode;

	private List<Collider> disabledColliders = new List<Collider>();

	private PropulsionCannon cannon;

	private bool selfDestruct;

	private float shotDetectionModeDelayTime = 7f;

	private void Start()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (!base.gameObject.GetComponent<SetRigidBodyModeAfterDelay>())
		{
			collisionDetectionMode = component.collisionDetectionMode;
		}
		component.collisionDetectionMode = CollisionDetectionMode.Continuous;
		LargeWorldEntity component2 = GetComponent<LargeWorldEntity>();
		if (component2 != null && !LargeWorldStreamer.main.IsTransientEntity(base.gameObject))
		{
			base.transform.parent = null;
			LargeWorldStreamer.main.cellManager.UnregisterEntity(component2);
		}
	}

	bool IMovementPlatform.IsPlatform()
	{
		return false;
	}

	public void ResetHandler(bool disableColliders = false, bool deserializing = false)
	{
		if (!deserializing)
		{
			CleanUpHandler();
		}
		Rigidbody component = GetComponent<Rigidbody>();
		if (component.useGravity)
		{
			Debug.LogWarningFormat(this, "Propulsion Cannon ammo '{0}' is using gravity. Disabling now.", base.gameObject);
			component.useGravity = false;
		}
		if (GetComponent<WorldForces>() == null)
		{
			Debug.LogWarningFormat(this, "Propulsion Cannon ammo '{0}' is missing WorldForces component. Adding one now.", base.gameObject);
			WorldForces worldForces = base.gameObject.AddComponent<WorldForces>();
			worldForces.useRigidbody = component;
			worldForces.RegisterWorldForces();
		}
		IPropulsionCannonAmmo[] components = base.gameObject.GetComponents<IPropulsionCannonAmmo>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].OnGrab();
		}
		Living component2 = GetComponent<Living>();
		if (component2 != null && (!deserializing || !(component2 is Creature)))
		{
			behaviorWasEnabled = component2.enabled;
			component2.enabled = false;
		}
		Locomotion component3 = GetComponent<Locomotion>();
		if (component3 != null)
		{
			locomotionWasEnabled = component3.enabled;
			component3.enabled = false;
		}
		if (!disableColliders)
		{
			return;
		}
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (componentsInChildren[j].enabled)
			{
				componentsInChildren[j].enabled = false;
				disabledColliders.Add(componentsInChildren[j]);
			}
		}
	}

	public void OnShot(bool deserializing = false)
	{
		for (int i = 0; i < disabledColliders.Count; i++)
		{
			disabledColliders[i].enabled = true;
		}
		disabledColliders.Clear();
		damageOnImpact = GetComponent<DealDamageOnImpact>();
		if (damageOnImpact == null)
		{
			damageOnImpact = base.gameObject.AddComponent<DealDamageOnImpact>();
			addedDamageOnImpact = true;
		}
		IPropulsionCannonAmmo[] components = base.gameObject.GetComponents<IPropulsionCannonAmmo>();
		for (int j = 0; j < components.Length; j++)
		{
			components[j].OnShoot();
		}
		if (fxTrailPrefab != null)
		{
			fxTrailInstance = Utils.SpawnPrefabAt(fxTrailPrefab, null, base.transform.position);
			if (fxTrailInstance != null)
			{
				fxTrailInstance.SetActive(value: true);
				ParticleSystem component = fxTrailInstance.GetComponent<ParticleSystem>();
				if (component != null)
				{
					component.Play();
				}
			}
		}
		if (!deserializing)
		{
			wasShot = true;
			timeShot = DayNightCycle.main.timePassedAsFloat;
			LargeWorldEntity component2 = GetComponent<LargeWorldEntity>();
			if (component2 != null && !LargeWorldStreamer.main.IsTransientEntity(base.gameObject))
			{
				LargeWorldStreamer.main.cellManager.RegisterEntity(component2);
			}
		}
	}

	private void TriggerAmmoEndEvent()
	{
		IPropulsionCannonAmmo[] components = base.gameObject.GetComponents<IPropulsionCannonAmmo>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].OnRelease();
		}
	}

	private void TriggerAmmoImpactEvent()
	{
		IPropulsionCannonAmmo[] components = base.gameObject.GetComponents<IPropulsionCannonAmmo>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].OnImpact();
		}
	}

	private void CleanUpHandler()
	{
		for (int i = 0; i < disabledColliders.Count; i++)
		{
			disabledColliders[i].enabled = true;
		}
		if (addedDamageOnImpact && damageOnImpact != null)
		{
			UnityEngine.Object.Destroy(damageOnImpact);
		}
		if (behaviorWasEnabled)
		{
			Living component = GetComponent<Living>();
			if (component != null)
			{
				component.enabled = true;
			}
		}
		if (locomotionWasEnabled)
		{
			Locomotion component2 = GetComponent<Locomotion>();
			if (component2 != null)
			{
				component2.enabled = true;
			}
		}
		damageOnImpact = null;
		addedDamageOnImpact = false;
		timeShot = -1f;
		behaviorWasEnabled = false;
		wasShot = false;
		locomotionWasEnabled = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (wasShot && !selfDestruct)
		{
			TriggerAmmoImpactEvent();
			selfDestruct = true;
		}
	}

	private void OnExamine()
	{
		UndoChanges();
		UnityEngine.Object.Destroy(this);
	}

	private void Update()
	{
		if (wasShot && fxTrailInstance != null)
		{
			fxTrailInstance.transform.position = base.transform.position;
		}
		if (selfDestruct || (wasShot && (bool)DayNightCycle.main && (double)(timeShot + 3f) <= DayNightCycle.main.timePassed))
		{
			UndoChanges();
			UnityEngine.Object.Destroy(this);
		}
	}

	public void SetCannon(PropulsionCannon setCannon)
	{
		PropulsionCannon propulsionCannon = cannon;
		cannon = setCannon;
		if (propulsionCannon != null && propulsionCannon != setCannon)
		{
			propulsionCannon.ReleaseGrabbedObject();
		}
	}

	public bool IsGrabbedBy(PropulsionCannon cannon)
	{
		return this.cannon == cannon;
	}

	public void UndoChanges()
	{
		if (wasShot)
		{
			SetRigidBodyModeAfterDelay component = base.gameObject.GetComponent<SetRigidBodyModeAfterDelay>();
			if ((bool)component)
			{
				component.TriggerStart(shotDetectionModeDelayTime, collisionDetectionMode);
			}
			else
			{
				base.gameObject.AddComponent<SetRigidBodyModeAfterDelay>().TriggerStart(shotDetectionModeDelayTime, collisionDetectionMode);
			}
		}
		else
		{
			Rigidbody component2 = GetComponent<Rigidbody>();
			if ((bool)component2)
			{
				component2.collisionDetectionMode = collisionDetectionMode;
			}
		}
		TriggerAmmoEndEvent();
		CleanUpHandler();
		if (cannon != null)
		{
			cannon.OnAmmoHandlerDestroyed(base.gameObject);
		}
		cannon = null;
		Creature component3 = GetComponent<Creature>();
		if (component3 != null)
		{
			component3.leashPosition = base.transform.position;
		}
		LargeWorldEntity component4 = GetComponent<LargeWorldEntity>();
		if (component4 != null && !LargeWorldStreamer.main.IsTransientEntity(base.gameObject))
		{
			Pickupable component5 = GetComponent<Pickupable>();
			if (component5 == null || !component5.attached)
			{
				LargeWorldStreamer.main.cellManager.RegisterEntity(component4);
			}
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			velocity = component.velocity;
		}
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(component, isKinematic: false);
			component.useGravity = false;
			component.velocity = velocity;
		}
		if (wasShot)
		{
			ResetHandler(disableColliders: false, deserializing: true);
			OnShot(deserializing: true);
		}
		else
		{
			selfDestruct = true;
		}
	}

	private void OnKill()
	{
		behaviorWasEnabled = false;
		locomotionWasEnabled = false;
	}

	private void OnDisable()
	{
		if (cannon != null)
		{
			cannon.OnAmmoHandlerDestroyed(base.gameObject);
		}
	}
}
