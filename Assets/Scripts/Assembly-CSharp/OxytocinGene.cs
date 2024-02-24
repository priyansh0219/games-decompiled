using System.Collections.Generic;
using UnityEngine;

public class OxytocinGene : Gene
{
	public float updateRate = 0.5f;

	public static float kMinRange = 5f;

	public static float kMaxRange = 50f;

	private static List<OxytocinGene> sources = new List<OxytocinGene>();

	private void Start()
	{
		InvokeRepeating("UpdateOxy", 0f, updateRate);
		sources.Add(this);
	}

	protected override void OnDestroy()
	{
		sources.Remove(this);
		base.OnDestroy();
	}

	private void UpdateOxy()
	{
		_ = base.gameObject.activeInHierarchy;
	}

	public static bool FindOxyTarget(Vector3 attracteePosition, out Vector3 targetPosition)
	{
		bool result = false;
		targetPosition = Vector3.zero;
		float num = -1f;
		foreach (OxytocinGene source in sources)
		{
			float magnitude = (attracteePosition - targetPosition).magnitude;
			if ((num < 0f || magnitude < num) && magnitude > 1f)
			{
				targetPosition = source.gameObject.transform.position;
				Debug.DrawLine(attracteePosition, targetPosition, Color.red, 2f);
				result = true;
			}
		}
		return result;
	}
}
