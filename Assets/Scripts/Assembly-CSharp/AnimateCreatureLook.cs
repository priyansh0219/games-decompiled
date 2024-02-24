using UnityEngine;

public class AnimateCreatureLook : MonoBehaviour
{
	[SerializeField]
	[AssertNotNull]
	private Animator animator;

	[SerializeField]
	[AssertNotNull]
	private LastTarget lastTarget;

	[SerializeField]
	[AssertNotNull]
	private Transform rootTransform;

	[SerializeField]
	[AssertNotNull]
	private Transform lookDirectionTransform;

	[SerializeField]
	private float animationMaxLookPitch = 90f;

	[SerializeField]
	private float animationMaxLookTilt = 90f;

	[SerializeField]
	private float rotationSpeed = 1f;

	[SerializeField]
	private float rememberTargetTime = 20f;

	[SerializeField]
	private float fov;

	private float prevLookPitch;

	private float prevLookTilt;

	public void Update()
	{
		float value = 0f;
		float value2 = 0f;
		if (lastTarget != null && lastTarget.target != null && Time.time < lastTarget.targetTime + rememberTargetTime)
		{
			Vector3 normalized = (lastTarget.target.transform.position - rootTransform.position).normalized;
			if (Vector3.Dot(normalized, rootTransform.forward) > fov)
			{
				Vector3 eulerAngles = Quaternion.LookRotation(lookDirectionTransform.InverseTransformDirection(normalized)).eulerAngles;
				value = prevLookPitch - Mathf.DeltaAngle(0f, eulerAngles.x);
				value2 = prevLookTilt + Mathf.DeltaAngle(0f, eulerAngles.y);
			}
		}
		value = Mathf.Clamp(value, 0f - animationMaxLookPitch, animationMaxLookPitch);
		value2 = Mathf.Clamp(value2, 0f - animationMaxLookTilt, animationMaxLookTilt);
		value = Mathf.Lerp(prevLookPitch, value, rotationSpeed * Time.deltaTime);
		value2 = Mathf.Lerp(prevLookTilt, value2, rotationSpeed * Time.deltaTime);
		if (animationMaxLookPitch > 0f)
		{
			animator.SetFloat(AnimatorHashID.look_pitch, value / animationMaxLookPitch);
		}
		if (animationMaxLookTilt > 0f)
		{
			animator.SetFloat(AnimatorHashID.look_tilt, value2 / animationMaxLookTilt);
		}
		prevLookPitch = value;
		prevLookTilt = value2;
	}

	public void OnKill()
	{
		animator.SetFloat(AnimatorHashID.look_pitch, 0f);
		animator.SetFloat(AnimatorHashID.look_tilt, 0f);
	}
}
