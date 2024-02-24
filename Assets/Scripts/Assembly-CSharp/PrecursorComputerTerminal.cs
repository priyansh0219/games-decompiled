using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PrecursorComputerTerminal : MonoBehaviour
{
	[AssertNotNull]
	public GameObject fx;

	[AssertNotNull]
	public VFXLerpScale scaleControl;

	[AssertNotNull]
	public VFXController fxControl;

	private int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public bool used;

	public void OnStoryHandTarget()
	{
		if (!used)
		{
			fxControl.Play();
			scaleControl.Play();
			Invoke("DestroyFX", scaleControl.duration);
		}
		used = true;
	}

	private void Start()
	{
		if (used)
		{
			DestroyFX();
		}
	}

	private void DestroyFX()
	{
		UnityEngine.Object.Destroy(fx);
	}
}
