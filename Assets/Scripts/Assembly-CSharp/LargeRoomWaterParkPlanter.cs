using UnityEngine;

public class LargeRoomWaterParkPlanter : MonoBehaviour, IHandTarget
{
	[SerializeField]
	[AssertNotNull]
	private Planter leftPlanter;

	[SerializeField]
	[AssertNotNull]
	private Planter rightPlanter;

	[AssertLocalization]
	[SerializeField]
	[AssertNotNull]
	private string hoverText = "UsePlanter";

	void IHandTarget.OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, hoverText, translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	void IHandTarget.OnHandClick(GUIHand hand)
	{
		PDA pDA = Player.main.GetPDA();
		Inventory.main.ClearUsedStorage();
		Inventory.main.SetUsedStorage(leftPlanter.enabled ? leftPlanter.storageContainer.container : null, append: true);
		Inventory.main.SetUsedStorage(rightPlanter.enabled ? rightPlanter.storageContainer.container : null, append: true);
		pDA.Open(PDATab.Inventory, base.transform);
	}

	public void SetMaxPlantsHeight(float height)
	{
		leftPlanter.SetMaxPlantsHeight(height);
		rightPlanter.SetMaxPlantsHeight(height);
	}

	public void SetLeftPlanterActive(bool state)
	{
		SetPlanterActive(leftPlanter, state);
	}

	public void SetRightPlanterActive(bool state)
	{
		SetPlanterActive(rightPlanter, state);
	}

	private void SetPlanterActive(Planter planter, bool state)
	{
		planter.enabled = state;
		planter.gameObject.SetActive(state);
		if (!state)
		{
			planter.ResetStorage();
		}
	}
}
