using UnityEngine;

[SkipProtoContractCheck]
public class BulkheadDoor : HandTarget, IHandTarget
{
	public delegate void OnStateChange(bool open);

	[AssertNotNull]
	[SerializeField]
	private Animator animator;

	[AssertNotNull]
	[SerializeField]
	private PlayerCinematicController frontOpenCinematicController;

	[AssertNotNull]
	[SerializeField]
	private PlayerCinematicController frontCloseCinematicController;

	[AssertNotNull]
	[SerializeField]
	private PlayerCinematicController backOpenCinematicController;

	[AssertNotNull]
	[SerializeField]
	private PlayerCinematicController backCloseCinematicController;

	[SerializeField]
	private bool initiallyOpen;

	[AssertNotNull]
	[SerializeField]
	private Transform frontSideDummy;

	private float sideDistanceThreshold = 0.7f;

	public OnStateChange onStateChange;

	private int quickSlot = -1;

	private static readonly int animOpened = Animator.StringToHash("opened");

	private static readonly int animPlayerInFront = Animator.StringToHash("player_in_front");

	[AssertLocalization]
	private const string openHandText = "Open";

	[AssertLocalization]
	private const string closeHandText = "Close";

	public bool opened { get; private set; }

	public override void Awake()
	{
		base.Awake();
		SetState(initiallyOpen);
	}

	private void OnEnable()
	{
		animator.SetBool(animOpened, opened);
	}

	public void OnHandHover(GUIHand hand)
	{
		if (base.enabled && PlayerCinematicController.cinematicModeCount <= 0)
		{
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			HandReticle.main.SetText(HandReticle.TextType.Hand, opened ? "Close" : "Open", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (base.enabled && PlayerCinematicController.cinematicModeCount <= 0)
		{
			if (GameOptions.GetVrAnimationMode())
			{
				SetState(!opened);
			}
			else
			{
				StartCinematic(hand.player);
			}
		}
	}

	private void StartCinematic(Player player)
	{
		quickSlot = Inventory.main.quickSlots.activeSlot;
		if (Inventory.main.ReturnHeld())
		{
			bool side = GetSide();
			animator.SetBool(animPlayerInFront, side);
			PlayerCinematicController playerCinematicController = ((!side) ? (opened ? backCloseCinematicController : backOpenCinematicController) : (opened ? frontCloseCinematicController : frontOpenCinematicController));
			playerCinematicController.StartCinematicMode(player);
		}
	}

	private void OnPlayerCinematicModeEnd()
	{
		SetState(!opened);
		Inventory.main.quickSlots.Select(quickSlot);
	}

	private bool GetSide()
	{
		Transform aimingTransform = SNCameraRoot.main.GetAimingTransform();
		Vector3 position = aimingTransform.position;
		Vector3 position2 = frontSideDummy.position;
		Vector3 forward = frontSideDummy.forward;
		float num = Vector3.Dot(position - position2, forward);
		if (Mathf.Abs(num) > sideDistanceThreshold)
		{
			return num < 0f;
		}
		num = Vector3.Dot(aimingTransform.forward, forward);
		if (Mathf.Approximately(num, 0f))
		{
			return true;
		}
		return num > 0f;
	}

	private void NotifyStateChange()
	{
		if (onStateChange != null)
		{
			onStateChange(opened);
		}
	}

	public void SetState(bool open)
	{
		opened = open;
		animator.SetBool(animOpened, opened);
		NotifyStateChange();
	}

	public void SetInitialyOpen(bool open)
	{
		initiallyOpen = open;
	}

	public bool GetInitialyOpen()
	{
		return initiallyOpen;
	}
}
