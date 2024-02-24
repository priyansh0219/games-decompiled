using UnityEngine;

public class BuildBotBeamPoints : MonoBehaviour
{
	public Transform[] beamPoints;

	public Transform GetClosestTransform(Vector3 toPoint)
	{
		float num = float.PositiveInfinity;
		Transform result = null;
		for (int i = 0; i < beamPoints.Length; i++)
		{
			float magnitude = (beamPoints[i].transform.position - toPoint).magnitude;
			if (magnitude < num)
			{
				result = beamPoints[i];
				num = magnitude;
			}
		}
		return result;
	}
}
