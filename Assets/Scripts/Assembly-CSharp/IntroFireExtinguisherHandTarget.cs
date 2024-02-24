using UnityEngine;

[SkipProtoContractCheck]
public class IntroFireExtinguisherHandTarget : HandTarget, IHandTarget
{
	public GameObject extinguisherModel;

	[AssertLocalization]
	private const string pickupHandText = "Pickup_FireExtinguisher";

	private void Start()
	{
		if (Utils.GetContinueMode())
		{
			extinguisherModel.SetActive(value: false);
			Object.Destroy(base.gameObject);
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, "Pickup_FireExtinguisher", translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnHandClick(GUIHand hand)
	{
		UseVolume();
	}

	private void UseVolume()
	{
		if ((bool)extinguisherModel)
		{
			extinguisherModel.SetActive(value: false);
		}
		CraftData.AddToInventory(TechType.FireExtinguisher);
		Inventory.main.SecureItems(verbose: false);
		Object.Destroy(base.gameObject);
	}
}
