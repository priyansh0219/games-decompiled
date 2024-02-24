using System;
using UnityEngine;

namespace Story
{
	[Serializable]
	public class UnlockSignalData
	{
		public Vector3 targetPosition;

		public string targetDescription;

		public void Trigger(OnGoalUnlockTracker tracker)
		{
			SignalPing component = UnityEngine.Object.Instantiate(tracker.signalPrefab).GetComponent<SignalPing>();
			component.pos = targetPosition;
			component.descriptionKey = targetDescription;
			component.PlayVO();
		}

		public override string ToString()
		{
			return $"{targetDescription} {targetPosition}";
		}
	}
}
