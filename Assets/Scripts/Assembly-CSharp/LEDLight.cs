using System;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class LEDLight : PlaceTool, IProtoEventListener
{
	public Animator animator;

	public ToggleLights toggleLights;

	public Rigidbody rigidBody;

	public LargeWorldEntity lwe;

	[AssertNotNull]
	public GameObject lights;

	[NonSerialized]
	[ProtoMember(1)]
	public bool deployed;

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		UpdateState(deployed, !deployed, registerInCellManager: true);
	}

	private void OnExamine()
	{
		UpdateState(deployed: false, enablePhysics: false, registerInCellManager: false);
	}

	private void OnDrop()
	{
		UpdateState(deployed: false, enablePhysics: true, registerInCellManager: true);
	}

	private void OnReload()
	{
		UpdateState(deployed: false, enablePhysics: false, registerInCellManager: false);
	}

	public override void OnPlace()
	{
		UpdateState(deployed: true, enablePhysics: false, registerInCellManager: true);
	}

	private void Update()
	{
		if ((bool)usingPlayer)
		{
			animator.SetBool("using_tool", GetUsedToolThisFrame());
		}
	}

	private void SetLightsActive(bool active)
	{
		lights.SetActive(active);
	}

	private void UpdateState(bool deployed, bool enablePhysics, bool registerInCellManager)
	{
		this.deployed = deployed;
		SetLightsActive(deployed);
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidBody, !enablePhysics);
		if (registerInCellManager)
		{
			LargeWorldEntity.CellLevel cellLevel = lwe.cellLevel;
			lwe.cellLevel = (deployed ? LargeWorldEntity.CellLevel.Global : LargeWorldEntity.CellLevel.Near);
			if (LargeWorldStreamer.main != null && LargeWorldStreamer.main.cellManager != null && lwe.cellLevel != cellLevel)
			{
				LargeWorldStreamer.main.cellManager.RegisterEntity(lwe);
			}
		}
	}
}
