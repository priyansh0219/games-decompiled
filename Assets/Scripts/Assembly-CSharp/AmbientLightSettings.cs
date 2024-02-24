using System;
using ProtoBuf;
using UWE;
using UnityEngine;

[Serializable]
[ProtoContract]
public class AmbientLightSettings : ISerializationCallbackReceiver
{
	private const int currentVersion = 2;

	public static readonly Color defaultColor = new Color(0.20392157f, 0.20392157f, 0.20392157f);

	[ProtoMember(1)]
	public bool enabled;

	[ProtoMember(2)]
	public Color color = defaultColor;

	[ProtoMember(3)]
	public int version;

	[ProtoMember(4)]
	public Gradient dayNightColor = new Gradient();

	[ProtoMember(5)]
	public float scale = 1f;

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

	public void CopyFrom(AmbientLightSettings other)
	{
		enabled = other.enabled;
		color = other.color;
		version = other.version;
		scale = other.scale;
		dayNightColor = UWE.Utils.Clone(other.dayNightColor);
	}

	public override string ToString()
	{
		if (!enabled)
		{
			return string.Empty;
		}
		return string.Concat("(", dayNightColor.Evaluate(0.5f), ")");
	}
}
