using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CyclopsVehicleStorageTerminalManager : MonoBehaviour
{
	public enum VehicleStorageType
	{
		Storage = 0,
		Torpedo = 1,
		TorpedoArm = 2,
		Module = 3
	}

	private enum DockedVehicleType
	{
		None = 0,
		Exo = 1,
		Seamoth = 2
	}

	[AssertNotNull]
	public GameObject noVehicleDockedScreen;

	[AssertNotNull]
	public GameObject seamothVehicleScreen;

	[AssertNotNull]
	public GameObject exoVehicleScreen;

	[AssertNotNull]
	public GameObject textStatus;

	[AssertNotNull]
	public TextMeshProUGUI healthText;

	[AssertNotNull]
	public TextMeshProUGUI powerText;

	[AssertNotNull]
	public Transform seamothModulesUIHolder;

	[AssertNotNull]
	public Transform exoModulesUIHolder;

	[AssertNotNull]
	public Sprite storageIcon;

	[AssertNotNull]
	public Sprite torpedoIcon;

	[AssertNotNull(AssertNotNullAttribute.Options.IgnorePrefabs)]
	public VehicleDockingBay dockingBay;

	private Transform usingModulesUIHolder;

	private VehicleUpgradeConsoleInput vehicleUpgradeConsole;

	private GameObject currentScreen;

	private DockedVehicleType dockedVehicleType;

	private int maxUpgrades;

	private Vehicle currentVehicle;

	private EnergyMixin currentVehicleEnergyMixin;

	private int lastHealthPercent = -1;

	private int lastEnergyPercent = -1;

	private void OnEnable()
	{
		VehicleDockingBay vehicleDockingBay = dockingBay;
		vehicleDockingBay.onDockedChanged = (VehicleDockingBay.OnDockedChanged)Delegate.Combine(vehicleDockingBay.onDockedChanged, new VehicleDockingBay.OnDockedChanged(OnDockedChanged));
	}

	private void OnDisable()
	{
		VehicleDockingBay vehicleDockingBay = dockingBay;
		vehicleDockingBay.onDockedChanged = (VehicleDockingBay.OnDockedChanged)Delegate.Remove(vehicleDockingBay.onDockedChanged, new VehicleDockingBay.OnDockedChanged(OnDockedChanged));
		CancelInvoke();
	}

	private void OnDockedChanged()
	{
		Vehicle dockedVehicle = dockingBay.GetDockedVehicle();
		if (dockedVehicle != null)
		{
			VehicleDocked(dockedVehicle);
		}
		else
		{
			VehicleUndocked();
		}
	}

	public bool GetTerminalInteractable()
	{
		if (dockedVehicleType == DockedVehicleType.None)
		{
			return false;
		}
		return true;
	}

	public void VehicleUndocked()
	{
		if ((bool)currentScreen)
		{
			currentScreen.SetActive(value: false);
		}
		noVehicleDockedScreen.SetActive(value: true);
		textStatus.SetActive(value: false);
		currentScreen = null;
		if ((bool)vehicleUpgradeConsole && vehicleUpgradeConsole.equipment != null)
		{
			vehicleUpgradeConsole.equipment.onEquip -= OnEquip;
			vehicleUpgradeConsole.equipment.onUnequip -= OnUneqip;
		}
		dockedVehicleType = DockedVehicleType.None;
		currentVehicle = null;
		currentVehicleEnergyMixin = null;
		CancelInvoke();
	}

	public void VehicleDocked(Vehicle vehicle)
	{
		noVehicleDockedScreen.SetActive(value: false);
		textStatus.SetActive(value: true);
		currentVehicle = vehicle;
		currentVehicleEnergyMixin = currentVehicle.GetComponent<EnergyMixin>();
		if (vehicle is Exosuit)
		{
			dockedVehicleType = DockedVehicleType.Exo;
			usingModulesUIHolder = exoModulesUIHolder;
			currentScreen = exoVehicleScreen;
		}
		else if (vehicle is SeaMoth)
		{
			dockedVehicleType = DockedVehicleType.Seamoth;
			usingModulesUIHolder = seamothModulesUIHolder;
			currentScreen = seamothVehicleScreen;
		}
		InvokeRepeating("UpdateText", UnityEngine.Random.value, 1f);
		currentScreen.SetActive(value: true);
		vehicleUpgradeConsole = currentVehicle.GetComponentInChildren<VehicleUpgradeConsoleInput>();
		if ((bool)vehicleUpgradeConsole && vehicleUpgradeConsole.equipment != null)
		{
			vehicleUpgradeConsole.equipment.onEquip += OnEquip;
			vehicleUpgradeConsole.equipment.onUnequip += OnUneqip;
		}
		RebuildVehicleScreen();
		TextMeshProUGUI componentInChildren = currentScreen.GetComponentInChildren<TextMeshProUGUI>();
		if ((bool)componentInChildren)
		{
			componentInChildren.text = currentVehicle.GetName();
		}
	}

	private void UpdateText()
	{
		if (currentVehicle == null)
		{
			return;
		}
		float healthFraction = currentVehicle.liveMixin.GetHealthFraction();
		int num = (int)(healthFraction * 100f);
		if (lastHealthPercent != num)
		{
			lastHealthPercent = num;
			healthText.text = Language.main.GetFormat("CyclopsUpgradesHealthPercentFormat", healthFraction);
		}
		if (currentVehicleEnergyMixin != null)
		{
			float energyScalar = currentVehicleEnergyMixin.GetEnergyScalar();
			int num2 = (int)(energyScalar * 100f);
			if (num2 != lastEnergyPercent)
			{
				lastEnergyPercent = num2;
				powerText.text = Language.main.GetFormat("CyclopsUpgradesEnergyPercentFormat", energyScalar);
			}
		}
	}

	private void RebuildVehicleScreen()
	{
		if (!currentScreen || !currentVehicle || !vehicleUpgradeConsole || vehicleUpgradeConsole.equipment == null)
		{
			return;
		}
		maxUpgrades = vehicleUpgradeConsole.slots.Length;
		for (int i = 0; i < maxUpgrades; i++)
		{
			if (dockedVehicleType == DockedVehicleType.Seamoth)
			{
				TechType techTypeInSlot = vehicleUpgradeConsole.equipment.GetTechTypeInSlot(vehicleUpgradeConsole.slots[i].id);
				if (i >= usingModulesUIHolder.childCount)
				{
					break;
				}
				bool flag = techTypeInSlot == TechType.VehicleStorageModule || techTypeInSlot == TechType.SeamothTorpedoModule;
				usingModulesUIHolder.GetChild(i).gameObject.SetActive(flag);
				Sprite sprite = null;
				CyclopsVehicleStorageTerminalButton component = usingModulesUIHolder.GetChild(i).GetComponent<CyclopsVehicleStorageTerminalButton>();
				if (flag)
				{
					switch (techTypeInSlot)
					{
					case TechType.SeamothTorpedoModule:
						if ((bool)component)
						{
							component.vehicleStorageType = VehicleStorageType.Torpedo;
						}
						sprite = torpedoIcon;
						break;
					case TechType.VehicleStorageModule:
						if ((bool)component)
						{
							component.vehicleStorageType = VehicleStorageType.Storage;
						}
						sprite = storageIcon;
						break;
					}
				}
				if ((bool)sprite)
				{
					usingModulesUIHolder.GetChild(i).GetChild(0).GetComponent<Image>()
						.sprite = sprite;
				}
			}
			else if (dockedVehicleType == DockedVehicleType.Exo)
			{
				bool active = vehicleUpgradeConsole.equipment.GetTechTypeInSlot("ExosuitArmLeft") == TechType.ExosuitTorpedoArmModule;
				bool active2 = vehicleUpgradeConsole.equipment.GetTechTypeInSlot("ExosuitArmRight") == TechType.ExosuitTorpedoArmModule;
				usingModulesUIHolder.GetChild(0).gameObject.SetActive(active);
				usingModulesUIHolder.GetChild(1).gameObject.SetActive(active2);
			}
		}
	}

	private void OnEquip(string slot, InventoryItem item)
	{
		RebuildVehicleScreen();
	}

	private void OnUneqip(string slot, InventoryItem item)
	{
		RebuildVehicleScreen();
	}

	public void StorageButtonClick(VehicleStorageType type, int slotID)
	{
		if (currentVehicle == null)
		{
			return;
		}
		switch (type)
		{
		case VehicleStorageType.Storage:
			if (dockedVehicleType == DockedVehicleType.Seamoth)
			{
				SeamothStorageInput[] allComponentsInChildren = currentVehicle.GetAllComponentsInChildren<SeamothStorageInput>();
				foreach (SeamothStorageInput seamothStorageInput in allComponentsInChildren)
				{
					if (seamothStorageInput.slotID == slotID)
					{
						seamothStorageInput.OpenFromExternal();
						break;
					}
				}
			}
			else if (dockedVehicleType == DockedVehicleType.Exo)
			{
				StorageContainer componentInChildren = currentVehicle.GetComponentInChildren<StorageContainer>();
				if ((bool)componentInChildren)
				{
					componentInChildren.Open(base.transform);
				}
			}
			break;
		case VehicleStorageType.Torpedo:
			if (dockedVehicleType == DockedVehicleType.Seamoth)
			{
				SeaMoth component = currentVehicle.GetComponent<SeaMoth>();
				if ((bool)component)
				{
					component.OpenTorpedoStorage(base.transform);
				}
			}
			break;
		case VehicleStorageType.TorpedoArm:
		{
			if (dockedVehicleType != DockedVehicleType.Exo)
			{
				break;
			}
			Exosuit exosuit = currentVehicle as Exosuit;
			if ((bool)exosuit)
			{
				ExosuitTorpedoArm exosuitTorpedoArm = exosuit.GetArm(slotID) as ExosuitTorpedoArm;
				if (exosuitTorpedoArm != null)
				{
					exosuitTorpedoArm.OpenTorpedoStorageExternal(base.transform);
				}
			}
			break;
		}
		}
	}

	public void ModuleButtonClick()
	{
		if (!(currentVehicle == null))
		{
			VehicleUpgradeConsoleInput componentInChildren = currentVehicle.GetComponentInChildren<VehicleUpgradeConsoleInput>();
			if ((bool)componentInChildren)
			{
				componentInChildren.OpenFromExternal();
			}
		}
	}
}
