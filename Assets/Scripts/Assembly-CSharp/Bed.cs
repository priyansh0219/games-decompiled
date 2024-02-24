using UnityEngine;

public class Bed : CinematicModeTrigger
{
	private enum BedSide
	{
		None = 0,
		Left = 1,
		Right = 2
	}

	private enum InUseMode
	{
		None = 0,
		Sleeping = 1
	}

	[AssertNotNull]
	public Animator animator;

	public Transform playerTarget;

	private PlayerCinematicController currentStandUpCinematicController;

	public PlayerCinematicController leftLieDownCinematicController;

	public PlayerCinematicController rightLieDownCinematicController;

	public PlayerCinematicController leftStandUpCinematicController;

	public PlayerCinematicController rightStandUpCinematicController;

	public Vector3 leftAnimPosition = Vector3.zero;

	public Vector3 rightAnimPosition = Vector3.zero;

	public GameObject leftObstacleCheck;

	public GameObject rightObstacleCheck;

	public float checkDistance;

	private LayerMask checkLayerMask;

	private float kSleepGameTimeDuration = 396.00003f;

	private float kSleepRealTimeDuration = 5f;

	private float kSleepInterval = 600f;

	private Player currentPlayer;

	[AssertLocalization]
	private const string notEnoughSpaceMessage = "NotEnoughSpaceToLieDown";

	[AssertLocalization]
	private const string sleepTimeOut = "BedSleepTimeOut";

	private InUseMode inUseMode;

	public void Start()
	{
		onCinematicEnd.AddListener(OnCinematicEnd);
		checkLayerMask = ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Trigger")));
	}

	private void Update()
	{
		switch (inUseMode)
		{
		case InUseMode.None:
			if (currentPlayer != null)
			{
				Subscribe(currentPlayer, state: false);
				currentPlayer = null;
			}
			break;
		case InUseMode.Sleeping:
			if (!DayNightCycle.main.IsInSkipTimeMode() || currentPlayer == null)
			{
				ExitInUseMode(currentPlayer);
			}
			break;
		}
	}

	public void OnCinematicEnd(CinematicModeEventData eventData)
	{
		if (eventData.cinematicController == cinematicController)
		{
			currentPlayer = eventData.player;
			Subscribe(currentPlayer, state: true);
			EnterInUseMode(currentPlayer);
		}
	}

	private void LateUpdate()
	{
		if (inUseMode != 0 && currentPlayer != null)
		{
			currentPlayer.SetPosition(playerTarget.position, playerTarget.rotation);
		}
	}

	public override void OnHandClick(GUIHand hand)
	{
		base.isValidHandTarget = GetCanSleep(hand.player, notify: true);
		if (base.isValidHandTarget)
		{
			switch (GetSide(hand.player))
			{
			case BedSide.None:
				ErrorMessage.AddWarning(Language.main.Get("NotEnoughSpaceToLieDown"));
				return;
			case BedSide.Right:
				cinematicController = rightLieDownCinematicController;
				currentStandUpCinematicController = rightStandUpCinematicController;
				animator.transform.localPosition = rightAnimPosition;
				break;
			default:
				cinematicController = leftLieDownCinematicController;
				currentStandUpCinematicController = leftStandUpCinematicController;
				animator.transform.localPosition = leftAnimPosition;
				break;
			}
			ResetAnimParams(hand.player.playerAnimator);
			base.OnHandClick(hand);
		}
	}

	public override void OnHandHover(GUIHand hand)
	{
		base.isValidHandTarget = GetCanSleep(hand.player, notify: false);
		if (base.isValidHandTarget && hand.IsFreeToInteract())
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, handText, translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	private bool GetCanSleep(Player player, bool notify)
	{
		if (!base.enabled)
		{
			return false;
		}
		if (player.GetMode() != 0)
		{
			return false;
		}
		if (player.IsUnderwater())
		{
			return false;
		}
		if (DayNightCycle.main.IsInSkipTimeMode())
		{
			return false;
		}
		if ((double)(player.timeLastSleep + kSleepInterval) > DayNightCycle.main.timePassed || (CrashedShipExploder.main != null && !CrashedShipExploder.main.IsExploded()))
		{
			if (notify)
			{
				ErrorMessage.AddMessage(Language.main.Get("BedSleepTimeOut"));
			}
			return false;
		}
		return true;
	}

	private BedSide GetSide(Player player)
	{
		bool flag = CheckForSpace(leftObstacleCheck);
		bool flag2 = CheckForSpace(rightObstacleCheck);
		if (!flag && !flag2)
		{
			return BedSide.None;
		}
		if ((base.transform.InverseTransformPoint(player.transform.position).x >= 0f && flag2) || !flag)
		{
			return BedSide.Right;
		}
		return BedSide.Left;
	}

	private bool CheckForSpace(GameObject obstacleCheckObj)
	{
		return !Physics.Raycast(new Ray(obstacleCheckObj.transform.position, obstacleCheckObj.transform.forward), checkDistance, checkLayerMask);
	}

	private void EnterInUseMode(Player player)
	{
		if (inUseMode == InUseMode.None)
		{
			player.FreezeStats();
			if (GameOptions.GetVrAnimationMode())
			{
				ForcedSetAnimParams(lieDownParamValue: true, standUpParamValue: false, player.playerAnimator);
			}
			player.cinematicModeActive = true;
			MainCameraControl.main.viewModel.localRotation = Quaternion.identity;
			inUseMode = InUseMode.Sleeping;
			DayNightCycle.main.SkipTime(kSleepGameTimeDuration, kSleepRealTimeDuration);
			uGUI_PlayerSleep.main.StartSleepScreen();
		}
	}

	private void ExitInUseMode(Player player, bool skipCinematics = false)
	{
		if (inUseMode == InUseMode.None)
		{
			return;
		}
		player.UnfreezeStats();
		if (inUseMode == InUseMode.Sleeping)
		{
			if (DayNightCycle.main.IsInSkipTimeMode())
			{
				DayNightCycle.main.StopSkipTimeMode();
			}
			player.timeLastSleep = DayNightCycle.main.timePassedAsFloat;
			uGUI_PlayerSleep.main.StopSleepScreen();
		}
		if (player == currentPlayer)
		{
			if (!skipCinematics)
			{
				ResetAnimParams(player.playerAnimator);
				currentStandUpCinematicController.StartCinematicMode(player);
			}
			if (GameOptions.GetVrAnimationMode() || skipCinematics)
			{
				ForcedSetAnimParams(lieDownParamValue: false, standUpParamValue: true, player.playerAnimator);
			}
		}
		MainCameraControl.main.lookAroundMode = false;
		inUseMode = InUseMode.None;
	}

	private void Subscribe(Player player, bool state)
	{
		if (!(player == null))
		{
			if (state)
			{
				currentPlayer.playerDeathEvent.AddHandler(base.gameObject, OnPlayerDeath);
				currentPlayer.isUnderwater.changedEvent.AddHandler(base.gameObject, CheckIfUnderwater);
			}
			else
			{
				currentPlayer.playerDeathEvent.RemoveHandler(base.gameObject, OnPlayerDeath);
				currentPlayer.isUnderwater.changedEvent.RemoveHandler(base.gameObject, CheckIfUnderwater);
			}
		}
	}

	private void OnPlayerDeath(Player player)
	{
		if (!(currentPlayer != player))
		{
			animator.Rebind();
			ExitInUseMode(player, skipCinematics: true);
			player.cinematicModeActive = false;
		}
	}

	private void CheckIfUnderwater(Utils.MonitoredValue<bool> isUnderwater)
	{
		if (currentPlayer != null && currentPlayer.isUnderwater.value)
		{
			ExitInUseMode(currentPlayer);
		}
	}

	private void ResetAnimParams(Animator playerAnimator)
	{
		SafeAnimator.SetBool(animator, "cinematics", !GameOptions.GetVrAnimationMode());
		ForcedSetAnimParams(lieDownParamValue: false, standUpParamValue: false, playerAnimator);
	}

	private void ForcedSetAnimParams(bool lieDownParamValue, bool standUpParamValue, Animator playerAnimator)
	{
		SafeAnimator.SetBool(animator, cinematicController.animParam, lieDownParamValue);
		SafeAnimator.SetBool(animator, currentStandUpCinematicController.animParam, standUpParamValue);
		SafeAnimator.SetBool(playerAnimator, cinematicController.playerViewAnimationName, lieDownParamValue);
		SafeAnimator.SetBool(playerAnimator, currentStandUpCinematicController.playerViewAnimationName, standUpParamValue);
	}
}
