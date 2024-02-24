using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ClimbMotor : MonoBehaviour
{
	private float prevSlopeLimit;

	private void OnTriggerEnter(Collider collider)
	{
		Debug.Log("OnTriggerEnter");
		ClimbSurface climbSurface = (ClimbSurface)collider.GetComponent(typeof(ClimbSurface));
		if (climbSurface != null)
		{
			CharacterController characterController = (CharacterController)base.transform.GetComponent(typeof(CharacterController));
			prevSlopeLimit = characterController.slopeLimit;
			characterController.slopeLimit = climbSurface.slopeLimit;
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		Debug.Log("OnTriggerEnter");
		if (collider.GetComponent(typeof(ClimbSurface)) != null)
		{
			((CharacterController)base.transform.GetComponent(typeof(CharacterController))).slopeLimit = prevSlopeLimit;
		}
	}
}
