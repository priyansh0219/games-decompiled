using Story;
using UnityEngine;

public class TimeCapsuleHandScanner : HandTarget, IHandTarget
{
	[AssertNotNull]
	public RocketPreflightCheckManager preflightCheckManager;

	[AssertNotNull]
	public Animator handScanScreenAnimator;

	[AssertNotNull]
	public Animator playerAnimator;

	[AssertNotNull]
	public PlayerCinematicController capsuleInController;

	[AssertNotNull]
	public PlayerCinematicController capsuleOutContoller;

	[AssertNotNull]
	public FMOD_CustomEmitter timeCap_Open_FirstUse_SFX;

	[AssertNotNull]
	public FMOD_CustomEmitter timeCap_Open_SFX;

	[AssertNotNull]
	public FMOD_CustomEmitter timeCap_Close_SFX;

	[AssertNotNull]
	public StoryGoal timeCapsuleGoal;

	public GameObject collision;

	[AssertLocalization]
	public string toolTipString;

	private void Start()
	{
		playerAnimator.SetBool("capsule_first_use", value: true);
	}

	public void OnStartAnimation(GUIHand guiHand)
	{
		handScanScreenAnimator.SetTrigger("Activate");
	}

	public void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, toolTipString, translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnHandClick(GUIHand hand)
	{
		if (playerAnimator.GetBool("capsule_first_use"))
		{
			Player.main.playerAnimator.SetBool("rocketship_time_capsule_first", value: true);
			handScanScreenAnimator.SetTrigger("Activate");
			timeCap_Open_FirstUse_SFX.Play();
		}
		else
		{
			Player.main.playerAnimator.SetBool("rocketship_time_capsule_first", value: false);
			playerAnimator.SetBool("capsule_in", value: true);
			playerAnimator.SetBool("capsule_out", value: false);
			timeCap_Open_SFX.Play();
		}
		capsuleInController.StartCinematicMode(Player.main);
	}

	public void OnPlayerCinematicModeEnd(PlayerCinematicController controller)
	{
		playerAnimator.SetBool("capsule_first_use", value: false);
		if (controller.animParam == "capsule_in")
		{
			Player.main.GetPDA().Open(PDATab.TimeCapsule, base.transform, OnCloseCallback);
		}
		else if (controller.animParam == "capsule_out")
		{
			playerAnimator.SetBool("capsule_in", value: false);
			playerAnimator.SetBool("capsule_out", value: false);
		}
	}

	private void OnCloseCallback(PDA pda)
	{
		playerAnimator.SetBool("capsule_in", value: false);
		playerAnimator.SetBool("capsule_out", value: true);
		capsuleOutContoller.StartCinematicMode(Player.main);
		preflightCheckManager.SetTimeCapsuleReady(PlayerTimeCapsule.main.IsValid());
		timeCap_Close_SFX.Play();
	}

	public void LaunchTimeCapsule()
	{
		timeCapsuleGoal.Trigger();
	}
}
