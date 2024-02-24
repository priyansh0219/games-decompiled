using UnityEngine;

[SkipProtoContractCheck]
public class BaseLadder : HandTarget, IHandTarget
{
	private BoxCollider boxCollider;

	[AssertLocalization]
	private const string climbUpHandText = "ClimbUp";

	[AssertLocalization]
	private const string climbDownHandText = "ClimbDown";

	private void Start()
	{
		boxCollider = GetComponent<BoxCollider>();
	}

	public void OnHandHover(GUIHand hand)
	{
		if (base.enabled)
		{
			hand.gameObject.GetComponent<Player>();
			if (GetExitPoint(out var _, out var direction))
			{
				string text = ((direction == Base.Direction.Above) ? "ClimbUp" : "ClimbDown");
				HandReticle.main.SetText(HandReticle.TextType.Hand, text, translate: true, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
				HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			}
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (base.enabled && GetExitPoint(out var position, out var _))
		{
			hand.player.SetPosition(position);
		}
	}

	private bool GetExitPoint(out Vector3 position, out Base.Direction direction)
	{
		position = Vector3.zero;
		direction = Base.Direction.Above;
		BaseDeconstructable componentInParent = GetComponentInParent<BaseDeconstructable>();
		if (componentInParent == null)
		{
			Debug.LogError("BaseLadder : GetExitPoint() : No BaseDeconstructable component found in parent!");
			return false;
		}
		Base.Face? face = componentInParent.face;
		if (!face.HasValue)
		{
			Debug.LogError("BaseLadder : GetExitPoint() : BaseDeconstructable.face is null!");
			return false;
		}
		Base.Face value = face.Value;
		direction = value.direction;
		Base componentInParent2 = GetComponentInParent<Base>();
		if (componentInParent2 == null)
		{
			Debug.LogError("BaseLadder : GetExitPoint() : No Base component found in parent!");
			return false;
		}
		return componentInParent2.GetLadderExitPosition(value, out position);
	}
}
