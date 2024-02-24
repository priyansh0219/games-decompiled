using System;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class PipeSurfaceFloater : DropTool, IPipeConnection, IProtoEventListener
{
	[AssertNotNull]
	public GameObject floater;

	[AssertNotNull]
	public Rigidbody rigidBody;

	[AssertNotNull]
	public GameObject turbine;

	[AssertNotNull]
	public Transform pipeAttachPoint;

	[AssertNotNull]
	public WorldForces worldForces;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter pumpSound;

	private float scale;

	private const float turbineRotTime = 0.5f;

	[NonSerialized]
	[ProtoMember(1, OverwriteList = true)]
	public string[] childPipeUID;

	[NonSerialized]
	[ProtoMember(2)]
	public bool deployed;

	private List<string> children = new List<string>();

	private void Start()
	{
		pickupable.isPickupable = children.Count == 0;
		if (rigidBody.isKinematic)
		{
			scale = 1f;
		}
		pickupable.pickedUpEvent.AddHandler(base.gameObject, OnPickedUp);
		pickupable.droppedEvent.AddHandler(base.gameObject, OnDropped);
		UpdateRigidBody();
	}

	private void OnPickedUp(Pickupable p)
	{
		children.Clear();
		deployed = false;
	}

	private void OnDropped(Pickupable p)
	{
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidBody, isKinematic: false);
		children.Clear();
		deployed = false;
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		childPipeUID = children.ToArray();
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (childPipeUID != null)
		{
			children = new List<string>(childPipeUID);
		}
		else
		{
			children = new List<string>();
		}
	}

	public void UpdateOxygen()
	{
	}

	public bool GetProvidesOxygen()
	{
		if (deployed)
		{
			return rigidBody.isKinematic;
		}
		return false;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public Vector3 GetAttachPoint()
	{
		return pipeAttachPoint.position;
	}

	public void SetParent(IPipeConnection parent)
	{
	}

	public void AddChild(IPipeConnection child)
	{
		children.Add(child.GetGameObject().GetComponent<UniqueIdentifier>().Id);
		pickupable.isPickupable = children.Count == 0;
	}

	public void RemoveChild(IPipeConnection child)
	{
		children.Remove(child.GetGameObject().GetComponent<UniqueIdentifier>().Id);
		pickupable.isPickupable = children.Count == 0;
	}

	public IPipeConnection GetParent()
	{
		return null;
	}

	public void SetRoot(IPipeConnection root)
	{
	}

	public IPipeConnection GetRoot()
	{
		if (!rigidBody.isKinematic || !deployed)
		{
			return null;
		}
		return this;
	}

	public override void OnToolUseAnim(GUIHand guiHand)
	{
		base.OnToolUseAnim(guiHand);
		scale = 0f;
		Vector3 position = base.transform.position;
		position.y = Mathf.Min(0f, position.y);
		base.transform.position = position;
		deployed = true;
	}

	public override bool OnRightHandDown()
	{
		if (base.OnRightHandDown() && Player.main.IsSwimming())
		{
			return GetDropPosition().y < 0f;
		}
		return false;
	}

	private void UpdateRigidBody()
	{
		bool isKinematic = pickupable.attached || children.Count > 0 || (deployed && base.transform.position.y >= -0.1f);
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidBody, isKinematic);
		worldForces.handleGravity = !deployed;
	}

	private void FixedUpdate()
	{
		UpdateRigidBody();
		if (usingPlayer == null && !rigidBody.isKinematic && deployed && rigidBody.velocity.magnitude <= 10f)
		{
			rigidBody.AddForce(Vector3.up * 5f, ForceMode.Acceleration);
		}
		if (!deployed && base.transform.position.y < -0.5f)
		{
			deployed = true;
		}
	}

	private void Update()
	{
		if (usingPlayer == null)
		{
			if (!pickupable.attached && deployed)
			{
				scale = Mathf.Clamp01(scale + Time.deltaTime * 1f);
				if (base.transform.position.y >= 0f)
				{
					float num = Time.time % 0.5f / 0.5f;
					turbine.transform.localEulerAngles = new Vector3(0f, num * 360f, 0f);
				}
				if (deployed)
				{
					Vector3 localEulerAngles = base.transform.localEulerAngles;
					localEulerAngles.x = 0f;
					localEulerAngles.z = 0f;
					base.transform.localEulerAngles = localEulerAngles;
				}
			}
			else
			{
				scale = 0f;
			}
		}
		else
		{
			scale = 0f;
		}
		floater.transform.localScale = Vector3.one * scale;
		if (GetProvidesOxygen())
		{
			pumpSound.Play();
		}
		else
		{
			pumpSound.Stop();
		}
	}
}
