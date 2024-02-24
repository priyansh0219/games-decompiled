using System;
using ProtoBuf;
using UWE;
using UnityEngine;

[Serializable]
[ProtoContract]
public class FogSettings : ISerializationCallbackReceiver
{
	public const int currentVersion = 2;

	public static readonly Color defaultColor = new Color(0f, 44f / 85f, 0.6745098f);

	[ProtoMember(1)]
	public bool enabled;

	[ProtoMember(2)]
	public Color color = defaultColor;

	[ProtoMember(3)]
	public float startDistance = 35f;

	[ProtoMember(4)]
	public float maxDistance = 350f;

	[ProtoMember(5)]
	public float absorptionSpeed = 1f;

	[ProtoMember(6)]
	public float sunGlowAmount = 1f;

	[ProtoMember(7)]
	public int version;

	[ProtoMember(8)]
	public Gradient dayNightColor = new Gradient();

	[ProtoMember(9)]
	public float depthDispersion = 0.003f;

	[ProtoMember(10)]
	public float scatteringScale = 1f;

	[ProtoBeforeSerialization]
	public void OnBeforeSerialization()
	{
		version = 2;
	}

	[ProtoAfterDeserialization]
	public void OnAfterDeserialization()
	{
		UpgradeData();
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		UpgradeData();
	}

	private void UpgradeData()
	{
		if (version < 2)
		{
			dayNightColor = UWE.Utils.DayNightGradient(color);
			version = 2;
		}
	}

	public Color EvaluateColor(float dayScalar)
	{
		return dayNightColor.Evaluate(dayScalar);
	}

	public void CopyFrom(FogSettings other)
	{
		enabled = other.enabled;
		color = other.color;
		startDistance = other.startDistance;
		maxDistance = other.maxDistance;
		absorptionSpeed = other.absorptionSpeed;
		sunGlowAmount = other.sunGlowAmount;
		version = other.version;
		dayNightColor = UWE.Utils.Clone(other.dayNightColor);
		depthDispersion = other.depthDispersion;
		scatteringScale = other.scatteringScale;
	}

	public override string ToString()
	{
		if (!enabled)
		{
			return string.Empty;
		}
		return string.Concat("(", dayNightColor.Evaluate(0.5f), " [", startDistance, ", ", maxDistance, "])");
	}
}
