using UnityEngine;

public static class GameStats
{
	public static void UpdateMaxDepth(float maxDepth)
	{
		Debug.LogFormat("Max depth: {0:0}m", maxDepth);
	}

	public static void UpdateDistanceTraveled(float distance)
	{
		Debug.LogFormat("Distance traveled: {0:0}m", distance);
	}

	public static void UpdateTimePlayed(float timePlayed)
	{
		Debug.LogFormat("Time played: {0:0}s", timePlayed);
	}
}
