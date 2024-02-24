using System;
using ProtoBuf;
using Story;
using UnityEngine;

[ProtoContract]
public class PrecursorDisableGunTerminal : HandTarget, IHandTarget, ICompileTimeCheckable, IStoryGoalListener
{
	[AssertNotNull]
	public FMODAsset accessGrantedSound;

	[AssertNotNull]
	public FMODAsset accessDeniedSound;

	[AssertNotNull]
	public PlayerCinematicController cinematic;

	[AssertNotNull]
	public FMODAsset firstUseSound;

	[AssertNotNull]
	public FMODAsset useSound;

	[AssertNotNull]
	public FMODAsset curedUseSound;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter openLoopSound;

	[AssertNotNull]
	public StoryGoal onPlayerCuredGoal;

	[AssertNotNull]
	public StoryGoal gunDeactivate;

	[AssertNotNull]
	public StoryGoal disableDeniedGoal;

	[AssertNotNull]
	public StoryGoal lostRiverHintGoal;

	[AssertNotNull]
	public ParticleSystem glowRing;

	[AssertNotNull]
	public Material glowMaterial;

	private Player usingPlayer;

	private bool opened;

	private float maxDuration;

	private bool ignorePlayer;

	private bool triggeredDeniedStory;

	private bool playerCured;

	[NonSerialized]
	[ProtoMember(2)]
	public bool firstUse = true;

	private void Start()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			playerCured = main.IsGoalComplete(onPlayerCuredGoal.key);
			if (!playerCured)
			{
				main.AddListener(this);
			}
			main.IsGoalComplete(gunDeactivate.key);
		}
	}

	public void NotifyGoalComplete(string key)
	{
		if (string.Equals(key, onPlayerCuredGoal.key, StringComparison.OrdinalIgnoreCase))
		{
			playerCured = true;
		}
	}

	private void SetOpen(bool isOpen)
	{
		if (isOpen)
		{
			openLoopSound.Play();
		}
		else
		{
			openLoopSound.Stop();
		}
		opened = isOpen;
		cinematic.animator.SetBool("open", isOpen);
	}

	public void OnHandHover(GUIHand hand)
	{
		if (!StoryGoalCustomEventHandler.main.gunDisabled && !(usingPlayer != null) && opened)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Disable_Gun", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Interact);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (usingPlayer == null && PlayerCinematicController.cinematicModeCount <= 0 && opened)
		{
			usingPlayer = hand.player;
			Inventory.main.ReturnHeld();
			if (playerCured)
			{
				Utils.PlayFMODAsset(curedUseSound, base.transform);
			}
			else if (firstUse)
			{
				Utils.PlayFMODAsset(firstUseSound, base.transform);
			}
			else
			{
				Utils.PlayFMODAsset(useSound, base.transform);
			}
			usingPlayer.playerAnimator.SetBool("using_tool_first", firstUse);
			usingPlayer.playerAnimator.SetBool("cured", playerCured);
			cinematic.animator.SetBool("first_use", firstUse);
			cinematic.animator.SetBool("cured", playerCured);
			cinematic.StartCinematicMode(hand.player);
			Invoke(playerCured ? "SetLightAccessGranted" : "SetLightAccessDenied", firstUse ? 10.5f : 5.75f);
		}
	}

	private void SetLightAccessGranted()
	{
		glowMaterial.SetColor(ShaderPropertyID._Color, Color.green);
		glowRing.Play();
	}

	private void SetLightAccessDenied()
	{
		glowMaterial.SetColor(ShaderPropertyID._Color, Color.red);
		glowRing.Play();
	}

	public void OnPlayerCinematicModeEnd()
	{
		if ((bool)usingPlayer)
		{
			if (!playerCured)
			{
				Utils.PlayFMODAsset(accessDeniedSound, base.transform);
				disableDeniedGoal.Trigger();
				if (!triggeredDeniedStory)
				{
					lostRiverHintGoal.Trigger();
					triggeredDeniedStory = true;
				}
			}
			else
			{
				Utils.PlayFMODAsset(accessGrantedSound, base.transform);
				StoryGoalCustomEventHandler.main.DisableGun();
			}
			SetOpen(isOpen: false);
			ignorePlayer = true;
			usingPlayer.playerAnimator.SetBool("using_tool_first", value: false);
		}
		usingPlayer = null;
		firstUse = false;
	}

	public void OnTerminalAreaEnter()
	{
		if (!StoryGoalCustomEventHandler.main.gunDisabled)
		{
			if (ignorePlayer)
			{
				ignorePlayer = false;
			}
			else
			{
				SetOpen(isOpen: true);
			}
		}
	}

	public void OnTerminalAreaExit()
	{
		if (!StoryGoalCustomEventHandler.main.gunDisabled)
		{
			SetOpen(isOpen: false);
		}
	}

	public string CompileTimeCheck()
	{
		return StoryGoalUtils.CheckStoryGoals(new StoryGoal[2] { disableDeniedGoal, lostRiverHintGoal });
	}
}
