using System;
using System.Collections;
using ProtoBuf;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[ProtoContract]
[DisallowMultipleComponent]
public class CreatureDeath : MonoBehaviour, IProtoEventListener, IManagedFixedUpdateBehaviour, IManagedBehaviour, IOnTakeDamage
{
	public float removeCorpseAfterSeconds = -1f;

	public bool respawn = true;

	public bool respawnOnlyIfKilledByCreature;

	public float respawnInterval = 300f;

	[AssertNotNull]
	public LiveMixin liveMixin;

	public Eatable eatable;

	[AssertNotNull]
	public Rigidbody useRigidbody;

	private bool lastDamageWasHeat;

	[SerializeField]
	[AssertNotNull]
	private GameObject respawnerPrefab;

	[NonSerialized]
	[ProtoMember(3)]
	public bool hasSpawnedRespawner;

	public int managedFixedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "CreatureDeath";
	}

	private void Start()
	{
		if (IsDead())
		{
			respawn = false;
			base.gameObject.BroadcastMessage("OnKill");
		}
		SyncFixedUpdatingState();
		Pickupable component = GetComponent<Pickupable>();
		if (component != null)
		{
			component.pickedUpEvent.AddHandler(base.gameObject, OnPickedUp);
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (!IsDead())
		{
			return;
		}
		if (base.gameObject.GetComponentInParent<Player>() != null || base.gameObject.GetComponentInParent<Constructable>() != null)
		{
			if (eatable != null)
			{
				eatable.SetDecomposes(value: true);
			}
		}
		else
		{
			RemoveCorpse();
		}
	}

	private void OnPickedUp(Pickupable p)
	{
		if (respawn && !respawnOnlyIfKilledByCreature)
		{
			SpawnRespawner();
		}
	}

	private void OnKill()
	{
		StartCoroutine(OnKillAsync());
	}

	private IEnumerator OnKillAsync()
	{
		GameObject gameObject = base.gameObject;
		if (respawn && !respawnOnlyIfKilledByCreature)
		{
			SpawnRespawner();
		}
		TechType processed = TechData.GetProcessed(CraftData.GetTechType(base.gameObject));
		if (processed != 0 && lastDamageWasHeat)
		{
			TaskResult<GameObject> result = new TaskResult<GameObject>();
			yield return CraftData.InstantiateFromPrefabAsync(processed, result);
			gameObject = result.Get();
			gameObject.transform.position = base.gameObject.transform.position;
			gameObject.transform.rotation = base.gameObject.transform.rotation;
			gameObject.GetComponent<Rigidbody>().mass = base.gameObject.GetComponent<Rigidbody>().mass;
			gameObject.GetComponent<Rigidbody>().velocity = base.gameObject.GetComponent<Rigidbody>().velocity;
			gameObject.GetComponent<Rigidbody>().angularDrag = base.gameObject.GetComponent<Rigidbody>().angularDrag * 3f;
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (removeCorpseAfterSeconds >= 0f)
		{
			Invoke("RemoveCorpse", removeCorpseAfterSeconds);
		}
		if (eatable != null)
		{
			eatable.SetDecomposes(value: true);
		}
		Rigidbody component = gameObject.GetComponent<Rigidbody>();
		component.isKinematic = false;
		component.constraints = RigidbodyConstraints.None;
		WorldForces component2 = GetComponent<WorldForces>();
		if (component2 != null)
		{
			component2.handleDrag = false;
		}
		component.drag = Mathf.Max(GetComponent<Rigidbody>().drag, 1f);
		component.angularDrag = Mathf.Max(GetComponent<Rigidbody>().angularDrag, 1f);
		base.gameObject.EnsureComponent<EcoTarget>().SetTargetType(EcoTargetType.DeadMeat);
		SyncFixedUpdatingState();
	}

	private void SpawnRespawner()
	{
		if (hasSpawnedRespawner)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(respawnerPrefab);
		Vector3 position = base.transform.position;
		Creature component = GetComponent<Creature>();
		if (component != null)
		{
			position = component.leashPosition;
		}
		gameObject.transform.position = position;
		gameObject.transform.rotation = base.transform.rotation;
		Respawn component2 = gameObject.GetComponent<Respawn>();
		component2.spawnTime = DayNightCycle.main.timePassedAsFloat + respawnInterval;
		component2.techType = CraftData.GetTechType(base.gameObject);
		LargeWorldEntity component3 = GetComponent<LargeWorldEntity>();
		LargeWorldEntity component4 = gameObject.GetComponent<LargeWorldEntity>();
		if (component3 != null && component4 != null)
		{
			component4.cellLevel = component3.cellLevel;
		}
		if (base.transform.parent == null || base.transform.parent.GetComponentInParent<LargeWorldEntity>() == null)
		{
			if ((bool)LargeWorldStreamer.main)
			{
				LargeWorldStreamer.main.cellManager.RegisterEntity(gameObject);
			}
		}
		else
		{
			if ((bool)LargeWorldStreamer.main)
			{
				LargeWorldStreamer.main.cellManager.UnregisterEntity(gameObject);
			}
			gameObject.transform.parent = base.transform.parent;
		}
		respawn = false;
		base.gameObject.SendMessage("OnRespawnerSpawned", component2, SendMessageOptions.DontRequireReceiver);
		hasSpawnedRespawner = true;
	}

	private bool IsDead()
	{
		if (liveMixin != null)
		{
			return !liveMixin.IsAlive();
		}
		return false;
	}

	private void RemoveCorpse()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void OnAttackByCreature()
	{
		if (IsDead() && respawn && respawnOnlyIfKilledByCreature)
		{
			SpawnRespawner();
		}
	}

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		lastDamageWasHeat = damageInfo.type == DamageType.Heat || damageInfo.type == DamageType.Fire;
	}

	private void SyncFixedUpdatingState()
	{
		if (IsDead())
		{
			BehaviourUpdateUtils.Register(this);
		}
		else
		{
			BehaviourUpdateUtils.Deregister(this);
		}
	}

	public void ManagedFixedUpdate()
	{
		if (!IsDead())
		{
			SyncFixedUpdatingState();
			return;
		}
		float num = Mathf.Clamp(0f - useRigidbody.mass - base.transform.position.y, -1f, 1f);
		useRigidbody.AddForce(Vector3.up * num, ForceMode.Acceleration);
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}
}
