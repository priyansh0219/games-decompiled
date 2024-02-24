using System;
using ProtoBuf;
using UnityEngine;

[Serializable]
[ProtoContract]
public class SunlightSettings
{
	public static readonly Color defaultColor = new Color(1f, 1f, 0.9843137f);

	[ProtoMember(1)]
	public bool enabled;

	[ProtoMember(2)]
	public float fade = 1f;

	[ProtoMember(3)]
	public Color color = defaultColor;

	[ProtoMember(4)]
	public float replaceFraction;

	[ProtoMember(5)]
	public bool shadowed;

	public void CopyFrom(SunlightSettings other)
	{
		enabled = other.enabled;
		fade = other.fade;
		color = other.color;
		replaceFraction = other.replaceFraction;
		shadowed = other.shadowed;
	}

	public override string ToString()
	{
		if (!enabled)
		{
			return string.Empty;
		}
		return string.Concat("(fade ", fade, ", ", color, " * ", replaceFraction, ", shadowed ", shadowed.ToString(), ")");
	}
}
