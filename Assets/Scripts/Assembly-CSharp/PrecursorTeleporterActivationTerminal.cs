using System;
using ProtoBuf;
using Story;
using UWE;
using UnityEngine;

[ProtoContract]
public class PrecursorTeleporterActivationTerminal : HandTarget, ICompileTimeCheckable
{
	[AssertNotNull]
	public PrecursorTeleporterActivationTerminalProxy proxy;

	[AssertNotNull]
	public PlayerCinematicController cinematicController;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public FMODAsset useSound;

	[AssertNotNull]
	public FMODAsset openSound;

	[AssertNotNull]
	public FMODAsset closeSound;

	[AssertNotNull]
	public StoryGoal onUseGoal;

	public GameObject root;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public bool unlocked;

	private GameObject crystalObject;

	private int restoreQuickSlot = -1;

	[AssertLocalization]
	private const string insertCrystalHandText = "Insert_Precursor_Crystal";

	private void Start()
	{
		base.isValidHandTarget = false;
	}

	public void OpenDeck()
	{
		if (!unlocked)
		{
			animator.SetBool("Open", value: true);
			Utils.PlayFMODAsset(openSound, base.transform);
		}
	}

	public void CloseDeck()
	{
		if (animator.GetBool("Open"))
		{
			animator.SetBool("Open", value: false);
			Utils.PlayFMODAsset(closeSound, base.transform);
		}
	}

	public void OnProxyHandHover(GUIHand hand)
	{
		if (!unlocked)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Insert_Precursor_Crystal", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnProxyHandClick(GUIHand hand)
	{
		if (unlocked)
		{
			return;
		}
		Pickupable pickupable = Inventory.main.container.RemoveItem(TechType.PrecursorIonCrystal);
		if (pickupable != null)
		{
			restoreQuickSlot = Inventory.main.quickSlots.activeSlot;
			Inventory.main.ReturnHeld();
			crystalObject = pickupable.gameObject;
			crystalObject.transform.SetParent(Inventory.main.toolSocket);
			crystalObject.transform.localPosition = Vector3.zero;
			crystalObject.transform.localRotation = Quaternion.identity;
			crystalObject.SetActive(value: true);
			Rigidbody component = crystalObject.GetComponent<Rigidbody>();
			if (component != null)
			{
				UWE.Utils.SetIsKinematicAndUpdateInterpolation(component, isKinematic: true);
			}
			cinematicController.StartCinematicMode(Player.main);
			Utils.PlayFMODAsset(useSound, base.transform);
			unlocked = true;
		}
	}

	public void OnPlayerCinematicModeEnd(PlayerCinematicController controller)
	{
		if ((bool)crystalObject)
		{
			UnityEngine.Object.Destroy(crystalObject);
		}
		if ((bool)root)
		{
			root.BroadcastMessage("ToggleDoor", true, SendMessageOptions.RequireReceiver);
		}
		else
		{
			BroadcastMessage("ToggleDoor", true, SendMessageOptions.RequireReceiver);
		}
		CloseDeck();
		onUseGoal.Trigger();
		if (restoreQuickSlot != -1)
		{
			Inventory.main.quickSlots.Select(restoreQuickSlot);
		}
	}

	public string CompileTimeCheck()
	{
		return StoryGoalUtils.CheckStoryGoal(onUseGoal);
	}
}
