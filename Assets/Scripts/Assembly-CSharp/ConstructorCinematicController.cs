using UnityEngine;

public class ConstructorCinematicController : MonoBehaviour
{
	private const ManagedUpdate.Queue queueUpdate = ManagedUpdate.Queue.Update;

	private const ManagedUpdate.Queue queueLateUpdate = ManagedUpdate.Queue.LateUpdate;

	[AssertNotNull]
	public PlayerCinematicController engageCinematicController;

	[AssertNotNull]
	public PlayerCinematicController disengageCinematicController;

	[AssertNotNull]
	public ConstructorInput constructorInput;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public Transform playerAttach;

	private Player usingPlayer;

	private int quickSlot = -1;

	public bool inUse => usingPlayer != null;

	private void Subscribe()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.Update, OnUpdate);
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdate, OnLateUpdate);
	}

	private void Unsubscribe()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.Update, OnUpdate);
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdate, OnLateUpdate);
	}

	private void OnUpdate()
	{
		if (inUse)
		{
			bool flag = !usingPlayer.liveMixin.IsAlive();
			if (flag || uGUI.main.craftingMenu.client != constructorInput)
			{
				DisengageConstructor(usingPlayer, flag);
			}
		}
	}

	private void OnLateUpdate()
	{
		if (inUse)
		{
			usingPlayer.SetPosition(playerAttach.position, playerAttach.rotation);
			MainCameraControl.main.transform.position = usingPlayer.camAnchor.position;
		}
	}

	public void OnPlayerCinematicModeEnd(PlayerCinematicController cinematicController)
	{
		if (cinematicController == engageCinematicController)
		{
			Player player = (usingPlayer = cinematicController.GetPlayer());
			Subscribe();
			if (GameOptions.GetVrAnimationMode())
			{
				ForcedSetAnimParams(engageParamValue: true, disengageParamValue: false, null);
			}
			player.playerController.SetEnabled(enabled: false);
			MainCameraControl.main.cinematicMode = true;
			MainCameraControl.main.lookAroundMode = true;
			MainCameraControl.main.viewModel.localRotation = Quaternion.identity;
			animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			constructorInput.StartUse();
		}
		else if (quickSlot != -1)
		{
			Inventory.main.quickSlots.Select(quickSlot);
		}
	}

	public void EngageConstructor(Player player)
	{
		quickSlot = Inventory.main.quickSlots.activeSlot;
		if (Inventory.main.ReturnHeld())
		{
			ResetAnimParams(player.playerAnimator);
			engageCinematicController.StartCinematicMode(player);
		}
	}

	public void DisengageConstructor()
	{
		if (inUse)
		{
			DisengageConstructor(usingPlayer);
		}
	}

	public void DisengageConstructor(Player player, bool skipCinematics = false)
	{
		if (usingPlayer != null && player == usingPlayer)
		{
			if (!skipCinematics)
			{
				ResetAnimParams(player.playerAnimator);
				disengageCinematicController.StartCinematicMode(player);
			}
			else
			{
				animator.SetTrigger("reset");
				player.cinematicModeActive = false;
				ForcedSetAnimParams(engageParamValue: false, disengageParamValue: false, player.playerAnimator);
			}
			if (GameOptions.GetVrAnimationMode())
			{
				ForcedSetAnimParams(engageParamValue: false, disengageParamValue: true, null);
			}
		}
		usingPlayer = null;
		Unsubscribe();
		MainCameraControl.main.lookAroundMode = false;
		constructorInput.EndUse();
	}

	private void ResetAnimParams(Animator playerAnimator)
	{
		bool vrAnimationMode = GameOptions.GetVrAnimationMode();
		SafeAnimator.SetBool(animator, "cinematics", !vrAnimationMode);
		ForcedSetAnimParams(engageParamValue: false, disengageParamValue: false, vrAnimationMode ? null : playerAnimator);
	}

	private void ForcedSetAnimParams(bool engageParamValue, bool disengageParamValue, Animator playerAnimator)
	{
		SafeAnimator.SetBool(animator, engageCinematicController.animParam, engageParamValue);
		SafeAnimator.SetBool(animator, disengageCinematicController.animParam, disengageParamValue);
		if (playerAnimator != null)
		{
			SafeAnimator.SetBool(playerAnimator, engageCinematicController.playerViewAnimationName, engageParamValue);
			SafeAnimator.SetBool(playerAnimator, disengageCinematicController.playerViewAnimationName, disengageParamValue);
		}
	}
}
