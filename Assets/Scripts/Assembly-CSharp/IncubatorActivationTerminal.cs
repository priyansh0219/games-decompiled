using Story;
using UWE;
using UnityEngine;

public class IncubatorActivationTerminal : MonoBehaviour, IHandTarget, ICompileTimeCheckable
{
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
	public Incubator incubator;

	[AssertNotNull]
	public StoryGoal onUseGoal;

	private GameObject crystalObject;

	private int restoreQuickSlot = -1;

	[AssertLocalization]
	private const string insertPrecursorCrystalHandText = "Insert_Precursor_Crystal";

	public void OpenDeck()
	{
		if (!incubator.powered)
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

	public void OnHandHover(GUIHand hand)
	{
		if (!incubator.powered)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Insert_Precursor_Crystal", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (incubator.powered)
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
		}
	}

	public void OnPlayerCinematicModeEnd(PlayerCinematicController controller)
	{
		if ((bool)crystalObject)
		{
			Object.Destroy(crystalObject);
		}
		incubator.SetPowered(isPowered: true);
		onUseGoal.Trigger();
		CloseDeck();
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
