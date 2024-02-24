using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class CurrentGenerator : MonoBehaviour
{
	public float force = 30f;

	public float pullInForce = 1f;

	public ParticleSystem[] particles;

	public ButtonHandTarget button;

	[NonSerialized]
	[ProtoMember(1)]
	public bool isActive;

	public FMOD_StudioEventEmitter activeSound;

	private List<Rigidbody> bodies = new List<Rigidbody>();
}
