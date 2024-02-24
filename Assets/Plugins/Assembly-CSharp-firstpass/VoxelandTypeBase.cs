using System;
using UnityEngine;

[Serializable]
public class VoxelandTypeBase
{
	public float grassDensity = 1f;

	public bool grassZUp;

	public float grassJitter = 0.5f;

	public float grassMinScale = 0.5f;

	public float grassMaxScale = 1f;

	public int grassMinTilt;

	public int grassMaxTilt = 45;

	public bool grassRandomSpin = true;

	public bool perlinGrass;

	public float perlinPeriod = 10f;

	public virtual string Check()
	{
		if (grassDensity <= 0f)
		{
			return "Density must be greater than zero";
		}
		if (grassMinScale <= 0f || grassMaxScale <= 0f)
		{
			return "Scale must be greater than zero";
		}
		if (grassMinScale > grassMaxScale)
		{
			return "Min scale must be less than max scale";
		}
		return null;
	}

	public static bool ApproxEqual(VoxelandTypeBase a, VoxelandTypeBase b)
	{
		if (Mathf.Approximately(a.grassDensity, b.grassDensity) && a.grassZUp == b.grassZUp && Mathf.Approximately(a.grassJitter, b.grassJitter) && Mathf.Approximately(a.grassMinScale, b.grassMinScale) && Mathf.Approximately(a.grassMaxScale, b.grassMaxScale) && a.grassMinTilt == b.grassMinTilt && a.grassMaxTilt == b.grassMaxTilt && a.grassRandomSpin == b.grassRandomSpin && a.perlinGrass == b.perlinGrass)
		{
			return Mathf.Approximately(a.perlinPeriod, b.perlinPeriod);
		}
		return false;
	}

	public static void Copy(VoxelandTypeBase src, VoxelandTypeBase dst)
	{
		dst.grassDensity = src.grassDensity;
		dst.grassZUp = src.grassZUp;
		dst.grassJitter = src.grassJitter;
		dst.grassMinScale = src.grassMinScale;
		dst.grassMaxScale = src.grassMaxScale;
		dst.grassMinTilt = src.grassMinTilt;
		dst.grassMaxTilt = src.grassMaxTilt;
		dst.grassRandomSpin = src.grassRandomSpin;
		dst.perlinGrass = src.perlinGrass;
		dst.perlinPeriod = src.perlinPeriod;
	}
}
