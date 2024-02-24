using UnityEngine;

public class FPModel : MonoBehaviour, IEquippable
{
	public GameObject propModel;

	public GameObject viewModel;

	public void OnEquip(GameObject sender, string slot)
	{
		SetState(state: true);
	}

	public void OnUnequip(GameObject sender, string slot)
	{
		SetState(state: false);
	}

	public void UpdateEquipped(GameObject sender, string slot)
	{
	}

	private void SetState(bool state)
	{
		if (propModel != null)
		{
			propModel.SetActive(!state);
		}
		if (viewModel != null)
		{
			viewModel.SetActive(state);
		}
	}
}
