using System;
using Gendarme;
using UWE;
using UnityEngine;

[SkipProtoContractCheck]
[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule")]
public class CuteFishHandTarget : CinematicModeTriggerBase
{
	[Serializable]
	[SuppressMessage("Gendarme.Rules.Serialization", "MarkAllNonSerializableFieldsRule")]
	public class CuteFishCinematic
	{
		public PlayerCinematicController cinematicController;

		public GameObject itemPrefab;
	}

	private enum State
	{
		None = 0,
		Rotate = 1,
		Play = 2,
		Command = 3
	}

	[AssertNotNull]
	public Transform rootTransform;

	[AssertNotNull]
	public Rigidbody creatureRigidbody;

	[AssertNotNull]
	public Transform playerCinLoc;

	[AssertNotNull]
	public CuteFish cuteFish;

	[AssertNotNull]
	public LiveMixin liveMixin;

	public FMODAsset followSound;

	public FMODAsset staySound;

	public string followCommandAnimation;

	public string stayCommandAnimation;

	public float intTime = 1f;

	public bool debugGoodbye;

	[AssertNotNull]
	public CuteFishCinematic[] cinematics;

	[AssertNotNull]
	public CuteFishCinematic goodbyeCinematic;

	private CuteFishCinematic currentCinematic;

	private State state;

	private float startTime;

	private Vector3 startPosition;

	private Quaternion startRotation;

	private Quaternion playerStartRotation;

	private Vector3 targetPosition;

	private Quaternion targetRotation;

	private Quaternion playerTargetRotation;

	private GameObject tempCinematicItem;

	private bool commandAnimationStared;

	[AssertLocalization]
	private const string fishStartFollowHandText = "FishStartFollow";

	[AssertLocalization]
	private const string fishStopFollowHandText = "FishStopFollow";

	[AssertLocalization]
	private const string playWithFishHandText = "PlayWithFish";

	[AssertLocalization]
	private const string sayFarewellHandText = "SayFarewell";

	public void Start()
	{
		onCinematicEnd.AddListener(OnCinematicEnd);
	}

	public override void OnHandHover(GUIHand hand)
	{
		if (!AllowedToInteract())
		{
			return;
		}
		if (!cuteFish.goodbyePlayed && GameInput.GetButtonDown(GameInput.Button.RightHand) && state == State.None)
		{
			if (Rocket.IsAnyRocketReady || debugGoodbye)
			{
				PrepareCinematicMode(hand.player, goodbyeCinematic);
				cuteFish.goodbyePlayed = true;
			}
			else
			{
				state = State.Command;
				commandAnimationStared = false;
			}
			return;
		}
		string text = string.Empty;
		if (!cuteFish.goodbyePlayed)
		{
			text = ((!Rocket.IsAnyRocketReady && !debugGoodbye) ? (cuteFish.followingPlayer ? "FishStopFollow" : "FishStartFollow") : "SayFarewell");
			text = LanguageCache.GetButtonFormat(text, GameInput.Button.RightHand);
		}
		HandReticle.main.SetText(HandReticle.TextType.Hand, "PlayWithFish", translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	public override void OnHandClick(GUIHand hand)
	{
		if (AllowedToInteract() && !GameOptions.GetVrAnimationMode() && state == State.None && base.isValidHandTarget)
		{
			PrepareCinematicMode(hand.player, cinematics.GetRandom());
		}
	}

	private bool PlayCommandAnimation()
	{
		quickSlot = Inventory.main.quickSlots.activeSlot;
		if (!Inventory.main.ReturnHeld())
		{
			return false;
		}
		if (state != State.Command)
		{
			return false;
		}
		string value = (cuteFish.followingPlayer ? stayCommandAnimation : followCommandAnimation);
		if (!string.IsNullOrEmpty(value))
		{
			Player.main.playerAnimator.SetBool(value, value: true);
		}
		FMODAsset fMODAsset = (cuteFish.followingPlayer ? staySound : followSound);
		if ((bool)fMODAsset)
		{
			Utils.PlayFMODAsset(fMODAsset, base.transform);
		}
		cuteFish.followingPlayer = !cuteFish.followingPlayer;
		startTime = Time.time;
		return true;
	}

	private bool AllowedToInteract()
	{
		if (state != 0)
		{
			return false;
		}
		if (PlayerCinematicController.cinematicModeCount > 0)
		{
			return false;
		}
		if ((bool)Player.main && Player.main.transform.position.y > 0f)
		{
			return false;
		}
		if (!liveMixin.IsAlive())
		{
			return false;
		}
		return true;
	}

	private void PrepareCinematicMode(Player setPlayer, CuteFishCinematic cinematic)
	{
		state = State.Rotate;
		startTime = Time.time;
		player = setPlayer;
		currentCinematic = cinematic;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(creatureRigidbody, isKinematic: true);
		startPosition = rootTransform.position;
		startRotation = rootTransform.rotation;
		Vector3 position = playerCinLoc.InverseTransformPoint(rootTransform.position);
		targetPosition = MainCameraControl.main.viewModel.TransformPoint(position);
		Vector3 forward = player.transform.position - targetPosition;
		targetRotation = Quaternion.LookRotation(forward);
		player.cinematicModeActive = true;
		playerStartRotation = player.transform.rotation;
		Quaternion quaternion = Quaternion.Inverse(rootTransform.rotation) * playerCinLoc.rotation;
		playerTargetRotation = targetRotation * quaternion;
	}

	private void Update()
	{
		if (state == State.Rotate)
		{
			if (player == null || !player.cinematicModeActive || currentCinematic == null)
			{
				StopPlaying();
			}
			else
			{
				float num = Mathf.InverseLerp(startTime, startTime + intTime, Time.time);
				rootTransform.position = Vector3.Lerp(startPosition, targetPosition, num);
				rootTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, num);
				player.transform.rotation = Quaternion.Slerp(playerStartRotation, playerTargetRotation, num);
				if (num == 1f)
				{
					cinematicController = currentCinematic.cinematicController;
					if (currentCinematic.itemPrefab != null)
					{
						tempCinematicItem = UnityEngine.Object.Instantiate(currentCinematic.itemPrefab, Vector3.zero, Quaternion.identity);
						tempCinematicItem.transform.SetParent(player.armsController.rightHandAttach, worldPositionStays: false);
					}
					state = State.Play;
					if (!StartCinematicMode(player))
					{
						StopPlaying();
					}
				}
			}
		}
		if (state != State.Command)
		{
			return;
		}
		if (!commandAnimationStared)
		{
			if (PlayCommandAnimation())
			{
				commandAnimationStared = true;
			}
			else
			{
				state = State.None;
			}
		}
		else if (Time.time > startTime + 2f)
		{
			if (!string.IsNullOrEmpty(followCommandAnimation))
			{
				Player.main.playerAnimator.SetBool(followCommandAnimation, value: false);
			}
			if (!string.IsNullOrEmpty(stayCommandAnimation))
			{
				Player.main.playerAnimator.SetBool(stayCommandAnimation, value: false);
			}
			if (restoreActiveQuickSlot)
			{
				Inventory.main.quickSlots.Select(quickSlot);
			}
			state = State.None;
		}
	}

	private void StopPlaying()
	{
		if (state == State.Rotate && player != null)
		{
			player.cinematicModeActive = false;
		}
		UnityEngine.Object.Destroy(tempCinematicItem);
		state = State.None;
		player = null;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(creatureRigidbody, isKinematic: false);
	}

	public void OnCinematicEnd(CinematicModeEventData eventData)
	{
		StopPlaying();
	}

	private void OnDisable()
	{
		if (state != 0)
		{
			StopPlaying();
		}
	}
}
