using System;
using UnityEngine;

public class OVRProfile : UnityEngine.Object
{
	[Obsolete]
	public enum State
	{
		NOT_TRIGGERED = 0,
		LOADING = 1,
		READY = 2,
		ERROR = 3
	}

	[Obsolete]
	public string id => "000abc123def";

	[Obsolete]
	public string userName => "Oculus User";

	[Obsolete]
	public string locale => "en_US";

	public float ipd => Vector3.Distance(OVRPlugin.GetNodePose(OVRPlugin.Node.EyeLeft, usePhysicsPose: false).ToOVRPose().position, OVRPlugin.GetNodePose(OVRPlugin.Node.EyeRight, usePhysicsPose: false).ToOVRPose().position);

	public float eyeHeight => OVRPlugin.eyeHeight;

	public float eyeDepth => OVRPlugin.eyeDepth;

	public float neckHeight => eyeHeight - 0.075f;

	[Obsolete]
	public State state => State.READY;
}
