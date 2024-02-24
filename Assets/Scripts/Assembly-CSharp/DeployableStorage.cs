using UWE;
using UnityEngine;

public class DeployableStorage : PlayerTool, ICompileTimeCheckable, IProtoEventListener
{
	[AssertNotNull]
	public LargeWorldEntity largeWorldEntity;

	[AssertNotNull]
	public Rigidbody rigidbody;

	private const float dropTorqueAmount = 5f;

	private const float dropForceAmount = 20f;

	public float throwDuration = 2f;

	public FMODAsset throwSound;

	private Sequence sequence = new Sequence();

	private bool continerActiveState;

	private const float physicsDistance = 48f;

	private const float physicsDistanceSq = 2304f;

	private bool lastDisablePhysics;

	public string CompileTimeCheck()
	{
		if (mainCollider == null)
		{
			return $"mainCollider field must not be null";
		}
		if (pickupable == null)
		{
			return $"pickupable field must not be null";
		}
		return null;
	}

	public override void Awake()
	{
		base.Awake();
		if (pickupable.attached)
		{
			OnPickedUp(pickupable);
		}
		else
		{
			OnDropped(pickupable);
		}
		pickupable.pickedUpEvent.AddHandler(base.gameObject, OnPickedUp);
		pickupable.droppedEvent.AddHandler(base.gameObject, OnDropped);
	}

	private void Update()
	{
		if (continerActiveState)
		{
			sequence.Update();
		}
		LargeWorldStreamer main = LargeWorldStreamer.main;
		if ((bool)main && pickupable.isPickupable)
		{
			bool flag = (base.transform.position - main.cachedCameraPosition).sqrMagnitude > 2304f;
			if (flag != lastDisablePhysics)
			{
				lastDisablePhysics = flag;
				UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, flag);
			}
		}
	}

	public override bool OnRightHandDown()
	{
		if (Inventory.CanDropItemHere(pickupable, notify: true))
		{
			_isInUse = true;
			return true;
		}
		return false;
	}

	public override void OnToolUseAnim(GUIHand hand)
	{
		if (!continerActiveState)
		{
			SetContainerActiveState(newState: true);
			sequence.Set(throwDuration, target: true, Throw);
		}
	}

	private void Throw()
	{
		_isInUse = false;
		pickupable.Drop(base.transform.position, MainCameraControl.main.transform.forward * 2f);
		if ((bool)throwSound && Player.main.IsUnderwater())
		{
			Utils.PlayFMODAsset(throwSound, base.transform);
		}
	}

	private void OnPickedUp(Pickupable p)
	{
		SetContainerActiveState(newState: false);
	}

	private void OnDropped(Pickupable pickupable)
	{
		SetContainerActiveState(newState: false);
	}

	private void SetContainerActiveState(bool newState)
	{
		if (continerActiveState != newState)
		{
			continerActiveState = newState;
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		LargeWorldStreamer main = LargeWorldStreamer.main;
		if (largeWorldEntity.cellLevel != LargeWorldEntity.CellLevel.Global && (bool)main && main.cellManager != null)
		{
			largeWorldEntity.cellLevel = LargeWorldEntity.CellLevel.Global;
			main.cellManager.RegisterEntity(largeWorldEntity);
		}
	}
}
