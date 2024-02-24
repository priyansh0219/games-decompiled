using UnityEngine;

public class PrecursorGunAim : MonoBehaviour
{
	[AssertNotNull]
	public Transform gunBase;

	[AssertNotNull]
	public Transform gunArm;

	public float damper = 6f;

	public Transform target;

	private Quaternion oldBaseRot;

	private Quaternion oldArmRot;

	private void LateUpdate()
	{
		if ((bool)target)
		{
			Quaternion quaternion = Quaternion.LookRotation(target.position - gunArm.position, Vector3.up);
			Quaternion b = Quaternion.Inverse(gunBase.parent.rotation) * quaternion;
			Quaternion quaternion2 = Quaternion.Slerp(oldBaseRot, b, Time.deltaTime / damper);
			gunBase.localRotation = Quaternion.Euler(0f, quaternion2.eulerAngles.y, 0f);
			oldBaseRot = gunBase.localRotation;
			Quaternion b2 = Quaternion.Inverse(gunArm.parent.rotation) * quaternion;
			quaternion2 = Quaternion.Slerp(oldBaseRot, b2, Time.deltaTime / damper);
			gunArm.localRotation = Quaternion.Euler(quaternion2.eulerAngles.x, 0f, 0f);
			oldArmRot = gunArm.localRotation;
		}
	}

	public void SetStartingRotation()
	{
		oldBaseRot = gunBase.localRotation;
		oldArmRot = gunArm.localRotation;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(gunBase.position, gunBase.forward * 5000f);
		Gizmos.DrawRay(gunArm.position, gunArm.forward * 5000f);
		if ((bool)target)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(target.position, 20f);
			Gizmos.DrawLine(gunArm.position, target.position);
		}
	}
}
