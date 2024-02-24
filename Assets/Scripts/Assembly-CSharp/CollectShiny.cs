using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
[RequireComponent(typeof(SwimBehaviour))]
public class CollectShiny : CreatureAction, IProtoTreeEventListener, IManagedLateUpdateBehaviour, IManagedBehaviour
{
	[AssertNotNull]
	public Transform mouth;

	[AssertNotNull]
	public Transform shinyTargetAttach;

	public string eventQualifier = "";

	private GameObject shinyTarget;

	private bool targetPickedUp;

	private float timeNextFindShiny;

	public float swimVelocity = 3f;

	public float swimInterval = 5f;

	public float updateTargetInterval = 1f;

	private float timeNextSwim;

	private EcoRegion.TargetFilter isTargetValidFilter;

	public int managedLateUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "CollectShiny";
	}

	private void Start()
	{
		isTargetValidFilter = IsTargetValid;
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (timeNextFindShiny < time)
		{
			UpdateShinyTarget();
			timeNextFindShiny = time + updateTargetInterval * (1f + 0.2f * Random.value);
		}
		if (shinyTarget != null && shinyTarget.activeInHierarchy)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public override void StopPerform(Creature creature, float time)
	{
		DropShinyTarget();
	}

	private void TryPickupShinyTarget()
	{
		if (shinyTarget != null && shinyTarget.activeInHierarchy)
		{
			if (shinyTarget.GetComponentInParent<Player>() != null)
			{
				shinyTarget = null;
				targetPickedUp = false;
				timeNextFindShiny = Time.time + 6f;
				return;
			}
			SendMessage("OnShinyPickUp", shinyTarget, SendMessageOptions.DontRequireReceiver);
			shinyTarget.gameObject.SendMessage("OnShinyPickUp", base.gameObject, SendMessageOptions.DontRequireReceiver);
			UWE.Utils.SetCollidersEnabled(shinyTarget, enabled: false);
			shinyTarget.transform.parent = shinyTargetAttach;
			shinyTarget.transform.localPosition = Vector3.zero;
			targetPickedUp = true;
			UWE.Utils.SetIsKinematic(shinyTarget.GetComponent<Rigidbody>(), state: true);
			UWE.Utils.SetEnabled(shinyTarget.GetComponent<LargeWorldEntity>(), state: false);
			SendMessage("OnShinyPickedUp", shinyTarget, SendMessageOptions.DontRequireReceiver);
			base.swimBehaviour.SwimTo(base.transform.position + Vector3.up * 5f + Random.onUnitSphere, Vector3.up, swimVelocity);
			timeNextSwim = Time.time + 1f;
			BehaviourUpdateUtils.Register(this);
		}
	}

	private void DropShinyTarget()
	{
		if (shinyTarget != null && targetPickedUp)
		{
			DropShinyTarget(shinyTarget);
			shinyTarget = null;
			targetPickedUp = false;
			BehaviourUpdateUtils.Deregister(this);
		}
	}

	private void DropShinyTarget(GameObject target)
	{
		target.transform.parent = null;
		UWE.Utils.SetCollidersEnabled(target, enabled: true);
		UWE.Utils.SetIsKinematic(target.GetComponent<Rigidbody>(), state: false);
		LargeWorldEntity component = target.GetComponent<LargeWorldEntity>();
		if ((bool)component && (bool)LargeWorldStreamer.main)
		{
			LargeWorldStreamer.main.cellManager.RegisterEntity(component);
		}
		target.gameObject.SendMessage("OnShinyDropped", base.gameObject, SendMessageOptions.DontRequireReceiver);
	}

	private bool CloseToShinyTarget()
	{
		return (base.transform.position - shinyTarget.transform.position).sqrMagnitude < 16f;
	}

	private bool CloseToNest()
	{
		return (base.transform.position - creature.leashPosition).sqrMagnitude < 16f;
	}

	private bool IsTargetValid(IEcoTarget target)
	{
		return (target.GetPosition() - creature.leashPosition).sqrMagnitude > 64f;
	}

	private void UpdateShinyTarget()
	{
		GameObject gameObject = null;
		if (EcoRegionManager.main != null)
		{
			IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Shiny, base.transform.position, isTargetValidFilter);
			if (ecoTarget != null)
			{
				gameObject = ecoTarget.GetGameObject();
				Debug.DrawLine(base.transform.position, ecoTarget.GetPosition(), Color.red, 2f);
			}
			else
			{
				gameObject = null;
			}
		}
		if ((bool)gameObject)
		{
			Vector3 direction = gameObject.transform.position - base.transform.position;
			float num = direction.magnitude - 0.5f;
			if (num > 0f && Physics.Raycast(base.transform.position, direction, num, Voxeland.GetTerrainLayerMask()))
			{
				gameObject = null;
			}
		}
		if (!(shinyTarget != gameObject) || !(gameObject != null) || !(gameObject.GetComponent<Rigidbody>() != null) || !(gameObject.GetComponent<Pickupable>() != null))
		{
			return;
		}
		if (shinyTarget != null)
		{
			if ((gameObject.transform.position - base.transform.position).magnitude > (shinyTarget.transform.position - base.transform.position).magnitude)
			{
				DropShinyTarget();
				shinyTarget = gameObject;
			}
		}
		else
		{
			shinyTarget = gameObject;
		}
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (!(shinyTarget != null))
		{
			return;
		}
		if (!targetPickedUp)
		{
			if (time > timeNextSwim)
			{
				timeNextSwim = time + swimInterval;
				base.swimBehaviour.SwimTo(shinyTarget.transform.position, -Vector3.up, swimVelocity);
			}
			if (CloseToShinyTarget())
			{
				TryPickupShinyTarget();
			}
			return;
		}
		if (shinyTarget.transform.parent != shinyTargetAttach)
		{
			if (shinyTarget.transform.parent != null && shinyTarget.transform.parent.GetComponentInParent<Stalker>() != null)
			{
				targetPickedUp = false;
				shinyTarget = null;
			}
			else
			{
				TryPickupShinyTarget();
			}
		}
		if (time > timeNextSwim)
		{
			timeNextSwim = time + swimInterval;
			base.swimBehaviour.SwimTo(creature.leashPosition + new Vector3(0f, 2f, 0f), swimVelocity);
		}
		if (CloseToNest())
		{
			DropShinyTarget();
			creature.Happy.Add(1f);
		}
	}

	public void ManagedLateUpdate()
	{
		if (shinyTarget != null && targetPickedUp)
		{
			shinyTargetAttach.position = mouth.position;
		}
		else
		{
			BehaviourUpdateUtils.Deregister(this);
		}
	}

	private void OnDisable()
	{
		DropShinyTarget();
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		foreach (Transform item in shinyTargetAttach)
		{
			DropShinyTarget(item.gameObject);
		}
	}
}
