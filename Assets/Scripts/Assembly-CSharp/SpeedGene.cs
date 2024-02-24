using UnityEngine;

public class SpeedGene : Gene
{
	public float GetSpeedScalar()
	{
		if (base.Scalar > 0f)
		{
			Debug.Log("GetSpeedScalar: " + (1f + base.Scalar * 3f));
		}
		return 1f + base.Scalar * 3f;
	}
}
