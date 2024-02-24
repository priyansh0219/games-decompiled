using System;
using System.Collections.Generic;
using Gendarme;
using UnityEngine;

[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
public class Leakable : MonoBehaviour
{
	private List<VFXSubLeakPoint> unusedLeakPoints = new List<VFXSubLeakPoint>();

	private List<VFXSubLeakPoint> leakingLeakPoints = new List<VFXSubLeakPoint>();

	[NonSerialized]
	public Vector3 lastDmgPoint;

	[AssertNotNull]
	public LiveMixin liveMixin;

	public static int ComputeNumLeakPoints(float healthFrac, int totalPts)
	{
		return Mathf.CeilToInt(Mathf.Clamp((1f - healthFrac) * (float)totalPts, 0f, totalPts));
	}

	public bool IsLeaking()
	{
		return leakingLeakPoints.Count > 0;
	}

	public void RefreshLeakPoints()
	{
		leakingLeakPoints.Clear();
		unusedLeakPoints.Clear();
		GetComponentsInChildren(includeInactive: true, unusedLeakPoints);
		for (int num = unusedLeakPoints.Count - 1; num >= 0; num--)
		{
			VFXSubLeakPoint vFXSubLeakPoint = unusedLeakPoints[num];
			if (vFXSubLeakPoint.pointActive)
			{
				unusedLeakPoints.RemoveAt(num);
				leakingLeakPoints.Add(vFXSubLeakPoint);
			}
		}
	}

	public void Start()
	{
		RefreshLeakPoints();
		DevConsole.RegisterConsoleCommand(this, "damagebase");
	}

	private void OnConsoleCommand_damagebase(NotificationCenter.Notification n)
	{
		DevConsole.ParseFloat(n, 0, out var value, 20f);
		liveMixin.TakeDamage(value);
	}

	public int GetLeakCount()
	{
		return leakingLeakPoints.Count;
	}

	private VFXSubLeakPoint GetClosest(List<VFXSubLeakPoint> list, Vector3 position)
	{
		float num = 10000f;
		VFXSubLeakPoint result = null;
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			VFXSubLeakPoint vFXSubLeakPoint = list[num2];
			if ((bool)vFXSubLeakPoint)
			{
				float magnitude = (vFXSubLeakPoint.transform.position - position).magnitude;
				if (magnitude < num)
				{
					result = vFXSubLeakPoint;
					num = magnitude;
				}
			}
			else
			{
				list.RemoveAt(num2);
			}
		}
		return result;
	}

	private void SpringNearestLeak(Vector3 point)
	{
		VFXSubLeakPoint closest = GetClosest(unusedLeakPoints, point);
		if (closest != null)
		{
			closest.pointActive = true;
			leakingLeakPoints.Add(closest);
			unusedLeakPoints.Remove(closest);
		}
	}

	private void PlugNearestLeak(Vector3 point)
	{
		VFXSubLeakPoint closest = GetClosest(leakingLeakPoints, point);
		if (closest != null)
		{
			closest.pointActive = false;
			leakingLeakPoints.Remove(closest);
			unusedLeakPoints.Add(closest);
		}
	}

	public List<VFXSubLeakPoint> GetLeakPoints()
	{
		return leakingLeakPoints;
	}

	public void UpdateLeakPoints()
	{
		int totalPts = unusedLeakPoints.Count + leakingLeakPoints.Count;
		int num = ComputeNumLeakPoints(liveMixin.GetHealthFraction(), totalPts);
		while (leakingLeakPoints.Count < num)
		{
			SpringNearestLeak(lastDmgPoint);
		}
		while (leakingLeakPoints.Count > num)
		{
			PlugNearestLeak(MainCamera.camera.transform.position);
		}
		foreach (VFXSubLeakPoint leakingLeakPoint in leakingLeakPoints)
		{
			leakingLeakPoint.UpdateEffects();
		}
	}
}
