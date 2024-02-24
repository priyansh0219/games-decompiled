using UnityEngine;

public class Sandworm : MonoBehaviour
{
	public enum SandwormState
	{
		Idle = 0,
		Rising = 1,
		Lowering = 2
	}

	public Animation animation;

	public SandwormState state;

	public float velocitySensitivity = 0.1f;

	public float yRiseDistance = 20f;

	public float riseTime = 3f;

	public float fallTime = 5f;

	public SandwormCollider sandwormCollider;

	private void Awake()
	{
	}

	private void Start()
	{
		animation.Play("idle");
	}

	private void OnTriggerStay(Collider collider)
	{
		if (state == SandwormState.Idle && (bool)Utils.FindAncestorWithComponent<LiveMixin>(collider.gameObject))
		{
			Rigidbody rigidbody = Utils.FindAncestorWithComponent<Rigidbody>(collider.gameObject);
			if ((bool)rigidbody && rigidbody.velocity.magnitude > velocitySensitivity && rigidbody.mass >= 1f)
			{
				state = SandwormState.Rising;
				animation.Play("attack");
				iTween.MoveTo(sandwormCollider.gameObject, iTween.Hash("y", sandwormCollider.gameObject.transform.position.y + yRiseDistance, "time", riseTime, "easetype", iTween.EaseType.easeInOutCubic, "oncomplete", "RiseComplete", "oncompletetarget", base.gameObject));
			}
		}
	}

	private void RiseComplete()
	{
		state = SandwormState.Lowering;
		iTween.MoveTo(sandwormCollider.gameObject, iTween.Hash("y", sandwormCollider.gameObject.transform.position.y - yRiseDistance, "time", fallTime, "easetype", iTween.EaseType.easeInOutCubic, "oncomplete", "FallComplete", "oncompletetarget", base.gameObject));
	}

	private void FallComplete()
	{
		animation.Play("idle");
		state = SandwormState.Idle;
	}
}
