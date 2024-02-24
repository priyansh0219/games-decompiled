using UnityEngine;

public class AnimatorLink : MonoBehaviour
{
	[AssertNotNull]
	public SkinnedMeshRenderer meshRenderer;

	[AssertNotNull]
	public Animator meshAnimator;

	private void Start()
	{
		meshAnimator.enabled = meshRenderer.isVisible;
	}

	private void OnBecameVisible()
	{
		meshAnimator.enabled = true;
	}

	private void OnBecameInvisible()
	{
		meshAnimator.enabled = false;
	}
}
