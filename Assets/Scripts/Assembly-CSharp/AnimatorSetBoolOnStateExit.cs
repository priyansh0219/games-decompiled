using UnityEngine;

public class AnimatorSetBoolOnStateExit : StateMachineBehaviour
{
	[SerializeField]
	private string parameter;

	[SerializeField]
	private bool value;

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetBool(parameter, value);
	}
}
