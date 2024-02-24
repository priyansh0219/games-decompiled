using System;
using System.Collections.Generic;
using ProtoBuf;
using Story;
using UnityEngine;

[ProtoContract]
public class RestoreAnimatorState : MonoBehaviour
{
	[AssertNotNull]
	public Animator animationController;

	[AssertNotNull]
	public string fallbackStateName;

	public bool isCapturing = true;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public int stateNameHash;

	[NonSerialized]
	[ProtoMember(3)]
	public float normalizedTime;

	[NonSerialized]
	[ProtoMember(4)]
	public readonly List<AnimatorParameterValue> parameterValues = new List<AnimatorParameterValue>();

	private AnimatorControllerParameter[] parameterCache;

	private void Start()
	{
		if (stateNameHash == 0)
		{
			StoryGoalManager main = StoryGoalManager.main;
			if ((bool)main && main.IsGoalComplete("Precursor_Prison_Aquarium_EmperorLog1"))
			{
				Debug.Log("Restoring emperor animation state after prison batch upgrade", this);
				stateNameHash = Animator.StringToHash("center");
			}
		}
		if (stateNameHash != 0)
		{
			RestoreState();
		}
	}

	private void Update()
	{
		if (isCapturing && animationController.isActiveAndEnabled)
		{
			CaptureState();
		}
	}

	public void CaptureState()
	{
		if (!animationController.isActiveAndEnabled)
		{
			Debug.LogWarningFormat(this, "Can not capture state of inactive animation controller.");
			return;
		}
		if (parameterCache == null)
		{
			parameterCache = animationController.parameters;
		}
		AnimatorStateInfo currentAnimatorStateInfo = animationController.GetCurrentAnimatorStateInfo(0);
		stateNameHash = currentAnimatorStateInfo.fullPathHash;
		normalizedTime = currentAnimatorStateInfo.normalizedTime;
		while (parameterValues.Count < parameterCache.Length)
		{
			parameterValues.Add(new AnimatorParameterValue());
		}
		for (int i = 0; i < parameterCache.Length; i++)
		{
			AnimatorParameterValue animatorParameterValue = parameterValues[i];
			AnimatorControllerParameter paramInfo = parameterCache[i];
			animatorParameterValue.UpdateFrom(paramInfo, animationController);
		}
	}

	public void RestoreState()
	{
		if (!animationController.HasState(0, stateNameHash))
		{
			Debug.LogWarningFormat(this, "Animator doesn't have saved state '{0}' anymore. Using fallback state '{1}' instead.", stateNameHash, fallbackStateName);
			stateNameHash = Animator.StringToHash(fallbackStateName);
		}
		for (int i = 0; i < parameterValues.Count; i++)
		{
			AnimatorParameterValue animatorParameterValue = parameterValues[i];
			switch (animatorParameterValue.paramType)
			{
			case AnimatorControllerParameterType.Float:
				animationController.SetFloat(animatorParameterValue.paramHash, animatorParameterValue.floatValue);
				break;
			case AnimatorControllerParameterType.Int:
				animationController.SetInteger(animatorParameterValue.paramHash, animatorParameterValue.intValue);
				break;
			case AnimatorControllerParameterType.Bool:
				animationController.SetBool(animatorParameterValue.paramHash, animatorParameterValue.boolValue);
				break;
			case AnimatorControllerParameterType.Trigger:
				if (animatorParameterValue.boolValue)
				{
					animationController.SetTrigger(animatorParameterValue.paramHash);
				}
				break;
			}
		}
		animationController.Play(stateNameHash, 0, normalizedTime);
	}
}
