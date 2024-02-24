using UnityEngine;

public class DeadAnimationOnEnable : MonoBehaviour
{
	[AssertNotNull]
	public LiveMixin liveMixin;

	[AssertNotNull]
	public Animator animator;

	public bool disableAnimatorInstead;

	private void OnEnable()
	{
		bool flag = !liveMixin.IsAlive();
		if (!disableAnimatorInstead)
		{
			animator.SetBool(AnimatorHashID.dead, flag);
		}
		else
		{
			animator.enabled = !flag;
		}
	}
}
