using UWE;
using UnityEngine;

public class PrecursorDoorMotorModeSetter : MonoBehaviour
{
	public PrecursorDoorMotorMode setToMotorModeOnEnter;

	private void OnTriggerEnter(Collider col)
	{
		if (setToMotorModeOnEnter == PrecursorDoorMotorMode.None || (col.gameObject != null && col.gameObject.GetComponentInChildren<IgnoreTrigger>() != null))
		{
			return;
		}
		GameObject entityRoot = UWE.Utils.GetEntityRoot(col.gameObject);
		if (!entityRoot)
		{
			entityRoot = col.gameObject;
		}
		Player componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Player>(entityRoot);
		if ((bool)componentInHierarchy)
		{
			switch (setToMotorModeOnEnter)
			{
			case PrecursorDoorMotorMode.Auto:
				componentInHierarchy.precursorOutOfWater = false;
				break;
			case PrecursorDoorMotorMode.ForceWalk:
				componentInHierarchy.precursorOutOfWater = true;
				break;
			}
		}
		Exosuit componentInHierarchy2 = UWE.Utils.GetComponentInHierarchy<Exosuit>(entityRoot);
		if ((bool)componentInHierarchy2)
		{
			switch (setToMotorModeOnEnter)
			{
			case PrecursorDoorMotorMode.Auto:
				componentInHierarchy2.precursorOutOfWater = false;
				break;
			case PrecursorDoorMotorMode.ForceWalk:
				componentInHierarchy2.precursorOutOfWater = true;
				break;
			}
		}
	}
}
