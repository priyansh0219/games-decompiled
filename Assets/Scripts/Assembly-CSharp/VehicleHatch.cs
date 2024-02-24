using UnityEngine;

public class VehicleHatch : HandTarget, IHandTarget
{
	public GameObject vehicleModel;

	[AssertLocalization]
	private const string driveVehicleHandText = "DriveVehicle";

	public Vector3 GetDiverSpawnPosition()
	{
		return base.transform.position + new Vector3(0f, 2f, 0f);
	}

	public void OnVehicleReturned()
	{
		vehicleModel.SetActive(value: true);
	}

	public void OnVehicleLaunched()
	{
		vehicleModel.SetActive(value: false);
	}

	public void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, "DriveVehicle", translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnHandClick(GUIHand hand)
	{
	}
}
