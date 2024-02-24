using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BasePipeConnector : Constructable, IPipeConnection
{
	[AssertNotNull]
	public Transform pipeAttachPoint;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter pumpSound;

	[NonSerialized]
	[ProtoMember(1, OverwriteList = true)]
	public string[] childPipeUID;

	private List<string> children = new List<string>();

	private bool prevPowered;

	private bool poweredChanged;

	private PowerRelay powerRelay;

	protected override void Start()
	{
		base.Start();
		powerRelay = GetComponentInParent<PowerRelay>();
		Initialize();
	}

	private bool CheckHasPower()
	{
		if (powerRelay != null)
		{
			return powerRelay.IsPowered();
		}
		return false;
	}

	private void Initialize()
	{
		UpdateChildren();
		if (GetProvidesOxygen())
		{
			pumpSound.Play();
		}
		else
		{
			pumpSound.Stop();
		}
	}

	private void Update()
	{
		bool flag = CheckHasPower();
		if (prevPowered != flag)
		{
			poweredChanged = true;
			prevPowered = flag;
		}
		if (poweredChanged)
		{
			UpdateChildren();
			poweredChanged = false;
		}
		if (GetProvidesOxygen())
		{
			pumpSound.Play();
		}
		else
		{
			pumpSound.Stop();
		}
	}

	private void UpdateChildren()
	{
		for (int i = 0; i < OxygenPipe.pipes.Count; i++)
		{
			OxygenPipe oxygenPipe = OxygenPipe.pipes[i];
			if ((bool)oxygenPipe && oxygenPipe.GetRoot() == this)
			{
				oxygenPipe.UpdateOxygen();
			}
		}
	}

	public void UpdateOxygen()
	{
	}

	public bool GetProvidesOxygen()
	{
		if (base.constructed)
		{
			return CheckHasPower();
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
	}

	public void RemoveChild(IPipeConnection child)
	{
		children.Remove(child.GetGameObject().GetComponent<UniqueIdentifier>().Id);
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
		if (!base.constructed)
		{
			return null;
		}
		return this;
	}

	public override bool UpdateGhostModel(Transform aimTransform, GameObject ghostModel, RaycastHit hit, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
	{
		bool result = false;
		geometryChanged = false;
		if ((bool)hit.collider && (bool)hit.collider.gameObject)
		{
			bool flag = Player.main.IsInsideWalkable();
			ghostModel.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(aimTransform.forward, hit.normal), hit.normal);
			result = Constructable.CheckFlags(allowedInBase, allowedInSub, allowedOutside, allowedUnderwater, hit.point) && !flag && hit.collider.gameObject.GetComponentInParent<Base>() != null;
		}
		return result;
	}
}
