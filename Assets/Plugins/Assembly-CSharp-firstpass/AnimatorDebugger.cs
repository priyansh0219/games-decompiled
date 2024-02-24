using UnityEngine;

public class AnimatorDebugger : MonoBehaviour
{
	[AssertNotNull]
	public Animator animationController;

	public AnimatorStateInfo GetState(int layerIndex)
	{
		return animationController.GetCurrentAnimatorStateInfo(layerIndex);
	}

	public void SetState(int stateNameHash, int layer, float normalizedTime)
	{
		animationController.Play(stateNameHash, layer, normalizedTime);
	}
}
