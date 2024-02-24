using System.Collections.Generic;
using UnityEngine;

public class BaseWaterPlaneManager : MonoBehaviour
{
	private List<BaseWaterPlane> waterPlanes = new List<BaseWaterPlane>();

	private List<VFXSubLeakPoint> leakPoints = new List<VFXSubLeakPoint>();

	private float _leakAmount;

	private float waterlevel;

	public float leakAmount
	{
		get
		{
			return _leakAmount;
		}
		set
		{
			if (_leakAmount == value)
			{
				return;
			}
			_leakAmount = value;
			foreach (BaseWaterPlane waterPlane in waterPlanes)
			{
				if ((bool)waterPlane)
				{
					waterPlane.leakAmount = _leakAmount;
				}
			}
		}
	}

	private void UpdateWaterLevel()
	{
		float num = (0f - Base.cellSize.y) * 0.5f;
		float num2 = Base.cellSize.y * 0.5f;
		waterlevel = base.transform.position.y + num + (num2 - num) * leakAmount;
	}

	private void Update()
	{
		UpdateWaterLevel();
		foreach (BaseWaterPlane waterPlane in waterPlanes)
		{
			waterPlane.waterlevel = waterlevel;
		}
		for (int i = 0; i < leakPoints.Count; i++)
		{
			leakPoints[i].waterlevel = waterlevel;
		}
	}

	public void SetHost(Transform host)
	{
		UpdateWaterLevel();
		host.GetComponentsInChildren(includeInactive: true, waterPlanes);
		foreach (BaseWaterPlane waterPlane in waterPlanes)
		{
			waterPlane.hostTrans = host;
			waterPlane.waterlevel = waterlevel;
			waterPlane.leakAmount = leakAmount;
		}
		GetComponentsInChildren(includeInactive: true, leakPoints);
	}
}
