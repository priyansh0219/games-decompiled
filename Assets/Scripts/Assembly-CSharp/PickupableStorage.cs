using UnityEngine;

public class PickupableStorage : MonoBehaviour, IHandTarget
{
	[AssertNotNull]
	public Pickupable pickupable;

	[AssertNotNull]
	public StorageContainer storageContainer;

	[AssertLocalization]
	public string cantPickupHoverText = "LuggageBagNotEmptyCantPickup";

	[AssertLocalization]
	public string cantPickupClickText = "LuggageBagNotEmptyCantPickup";

	public void OnHandHover(GUIHand hand)
	{
		if (storageContainer.IsEmpty())
		{
			pickupable.OnHandHover(hand);
		}
		else if (!string.IsNullOrEmpty(cantPickupHoverText))
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, cantPickupHoverText, translate: true);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (storageContainer.IsEmpty())
		{
			pickupable.OnHandClick(hand);
		}
		else if (!string.IsNullOrEmpty(cantPickupClickText))
		{
			ErrorMessage.AddError(Language.main.Get(cantPickupClickText));
		}
	}
}
