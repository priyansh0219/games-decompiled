using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Beacon : PlayerTool, IProtoEventListener
{
	private const float dropTorqueAmount = 5f;

	private const float dropForceAmount = 20f;

	public float throwDuration = 2f;

	[AssertNotNull]
	public BeaconLabel beaconLabel;

	[AssertNotNull]
	public FMOD_CustomEmitter beaconOnLoop;

	[AssertNotNull]
	public FMOD_CustomEmitter beaonDraw;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public string label;

	private Sequence sequence;

	private bool beaconActiveState;

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
		sequence = new Sequence();
		if (label == null)
		{
			string text2 = (base.name = string.Format("{0} {1}", Language.main.Get("BeaconDefaultPrefix"), BeaconManager.GetCount() + 1));
			label = text2;
			beaconLabel.SetLabel(label);
		}
		BeaconManager.Add(this);
	}

	private void Start()
	{
		beaconLabel.SetLabel(label);
	}

	private void Update()
	{
		if (beaconActiveState)
		{
			sequence.Update();
		}
	}

	protected override void OnDestroy()
	{
		BeaconManager.Remove(this);
		base.OnDestroy();
	}

	public override void OnDraw(Player p)
	{
		base.OnDraw(p);
		beaonDraw.Play();
	}

	public override bool OnRightHandDown()
	{
		if (Player.main.currentSub != null)
		{
			return false;
		}
		_isInUse = true;
		return true;
	}

	public override void OnToolUseAnim(GUIHand hand)
	{
		if (!beaconActiveState)
		{
			SetBeaconActiveState(newState: true);
			sequence.Set(throwDuration, target: true, Throw);
		}
	}

	private void Throw()
	{
		_isInUse = false;
		pickupable.Drop(base.transform.position);
		base.transform.rotation = Quaternion.LookRotation(Player.main.transform.position);
		GetComponent<WorldForces>().enabled = true;
		beaconOnLoop.Play();
	}

	private void OnPickedUp(Pickupable p)
	{
		SetBeaconActiveState(newState: false);
		beaconLabel.OnPickedUp();
		beaconOnLoop.Stop();
	}

	private void OnDropped(Pickupable pickupable)
	{
		SetBeaconActiveState(newState: true);
		Rigidbody component = GetComponent<Rigidbody>();
		component.AddForce(Player.main.camRoot.GetAimingTransform().forward * 20f);
		component.AddTorque(base.transform.right * 5f);
		beaconLabel.OnDropped();
	}

	private void SetBeaconActiveState(bool newState)
	{
		if (beaconActiveState != newState)
		{
			beaconActiveState = newState;
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		label = beaconLabel.GetLabel();
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		beaconLabel.SetLabel(label);
	}
}
