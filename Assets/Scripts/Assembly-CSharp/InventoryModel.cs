using UnityEngine;

public class InventoryModel : MonoBehaviour
{
	[AssertNotNull]
	public Pickupable pickupAble;

	[AssertNotNull]
	public GameObject worldModel;

	[AssertNotNull]
	public GameObject viewModel;

	private void Start()
	{
		pickupAble.pickedUpEvent.AddHandler(base.gameObject, OnPickedUp);
		pickupAble.droppedEvent.AddHandler(base.gameObject, OnDropped);
		UpdateModel(pickupAble.attached);
	}

	public void UpdateModel(bool isPickedUp)
	{
		worldModel.SetActive(!isPickedUp);
		viewModel.SetActive(isPickedUp);
	}

	private void OnPickedUp(Pickupable p)
	{
		UpdateModel(isPickedUp: true);
	}

	private void OnDropped(Pickupable p)
	{
		UpdateModel(isPickedUp: false);
	}
}
