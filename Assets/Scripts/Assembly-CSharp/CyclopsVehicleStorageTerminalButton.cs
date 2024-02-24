using UnityEngine;
using UnityEngine.EventSystems;

public class CyclopsVehicleStorageTerminalButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, IPointerHoverHandler
{
	public CyclopsVehicleStorageTerminalManager.VehicleStorageType vehicleStorageType;

	public int slotID;

	[AssertNotNull]
	public CyclopsVehicleStorageTerminalManager terminalManager;

	[AssertNotNull(AssertNotNullAttribute.Options.IgnorePrefabs)]
	public Animator animator;

	public void OnPointerEnter(PointerEventData data)
	{
		HandReticle.main.SetIcon(HandReticle.IconType.Interact);
		animator.SetBool("Highlighted", value: true);
	}

	public void OnPointerExit(PointerEventData data)
	{
		HandReticle.main.SetIcon(HandReticle.IconType.Default);
		animator.SetBool("Highlighted", value: false);
	}

	public void OnPointerClick(PointerEventData data)
	{
		animator.SetTrigger("Pressed");
		if (vehicleStorageType == CyclopsVehicleStorageTerminalManager.VehicleStorageType.Module)
		{
			terminalManager.ModuleButtonClick();
		}
		else
		{
			terminalManager.StorageButtonClick(vehicleStorageType, slotID);
		}
	}

	public void OnPointerHover(PointerEventData eventData)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, GetHandTextKey(), translate: true, GameInput.button0);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Interact);
	}

	private string GetHandTextKey()
	{
		switch (vehicleStorageType)
		{
		case CyclopsVehicleStorageTerminalManager.VehicleStorageType.Storage:
			return "SeamothStorageOpen";
		case CyclopsVehicleStorageTerminalManager.VehicleStorageType.Torpedo:
			return "SeamothTorpedoStorage";
		case CyclopsVehicleStorageTerminalManager.VehicleStorageType.TorpedoArm:
			return "ExosuitTorpedoStorage";
		case CyclopsVehicleStorageTerminalManager.VehicleStorageType.Module:
			return "UpgradeConsole";
		default:
			Debug.LogError("GetHandTextKey unhandled VehicleStorageType");
			return string.Empty;
		}
	}
}
