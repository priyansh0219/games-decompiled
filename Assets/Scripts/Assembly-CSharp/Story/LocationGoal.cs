using System;
using UnityEngine;

namespace Story
{
	[Serializable]
	public class LocationGoal : StoryGoal
	{
		[Tooltip("Human readable name for maintenance. No functionality.")]
		public string location;

		public Vector3 position;

		public float range;

		public float minStayDuration;

		private float timeRangeEntered = -1f;

		public bool Trigger(Vector3 playerPosition, float time)
		{
			if (Vector3.SqrMagnitude(playerPosition - position) > range * range)
			{
				timeRangeEntered = -1f;
				return false;
			}
			if (timeRangeEntered < 0f)
			{
				timeRangeEntered = time;
			}
			if (time - timeRangeEntered < minStayDuration)
			{
				return false;
			}
			Trigger();
			return true;
		}
	}
}
