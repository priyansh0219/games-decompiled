using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class AnimatorParameterValue
{
	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public int paramHash;

	[NonSerialized]
	[ProtoMember(3)]
	public AnimatorControllerParameterType paramType;

	[NonSerialized]
	[ProtoMember(4)]
	public bool boolValue;

	[NonSerialized]
	[ProtoMember(5)]
	public int intValue;

	[NonSerialized]
	[ProtoMember(6)]
	public float floatValue;

	public void UpdateFrom(AnimatorControllerParameter paramInfo, Animator animationController)
	{
		paramHash = paramInfo.nameHash;
		paramType = paramInfo.type;
		switch (paramInfo.type)
		{
		case AnimatorControllerParameterType.Float:
			floatValue = animationController.GetFloat(paramInfo.nameHash);
			break;
		case AnimatorControllerParameterType.Int:
			intValue = animationController.GetInteger(paramInfo.nameHash);
			break;
		case AnimatorControllerParameterType.Bool:
		case AnimatorControllerParameterType.Trigger:
			boolValue = animationController.GetBool(paramInfo.nameHash);
			break;
		}
	}

	public static AnimatorParameterValue Create(AnimatorControllerParameter paramInfo, Animator animationController)
	{
		switch (paramInfo.type)
		{
		case AnimatorControllerParameterType.Float:
		{
			float @float = animationController.GetFloat(paramInfo.nameHash);
			return Create(paramInfo, @float);
		}
		case AnimatorControllerParameterType.Int:
		{
			int integer = animationController.GetInteger(paramInfo.nameHash);
			return Create(paramInfo, integer);
		}
		case AnimatorControllerParameterType.Bool:
		case AnimatorControllerParameterType.Trigger:
		{
			bool @bool = animationController.GetBool(paramInfo.nameHash);
			return Create(paramInfo, @bool);
		}
		default:
			return null;
		}
	}

	public static AnimatorParameterValue Create(AnimatorControllerParameter info, bool value)
	{
		if (value == info.defaultBool)
		{
			return null;
		}
		AnimatorParameterValue animatorParameterValue = Create(info);
		animatorParameterValue.boolValue = value;
		return animatorParameterValue;
	}

	public static AnimatorParameterValue Create(AnimatorControllerParameter info, int value)
	{
		if (value == info.defaultInt)
		{
			return null;
		}
		AnimatorParameterValue animatorParameterValue = Create(info);
		animatorParameterValue.intValue = value;
		return animatorParameterValue;
	}

	public static AnimatorParameterValue Create(AnimatorControllerParameter info, float value)
	{
		if (value == info.defaultFloat)
		{
			return null;
		}
		AnimatorParameterValue animatorParameterValue = Create(info);
		animatorParameterValue.floatValue = value;
		return animatorParameterValue;
	}

	private static AnimatorParameterValue Create(AnimatorControllerParameter info)
	{
		return new AnimatorParameterValue
		{
			paramHash = info.nameHash,
			paramType = info.type
		};
	}
}
