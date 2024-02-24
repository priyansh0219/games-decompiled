using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class UpgradeConsole : HandTarget, IHandTarget, IProtoEventListener, IProtoTreeEventListener
{
	public GameObject module1;

	public GameObject module2;

	public GameObject module3;

	public GameObject module4;

	public GameObject module5;

	public GameObject module6;

	public GameObject modulesRoot;

	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public int version;

	[NonSerialized]
	[ProtoMember(3, OverwriteList = true)]
	public Dictionary<string, string> serializedModuleSlots;

	[AssertLocalization]
	private const string upgradesStorageLabel = "CyclopsUpgradesStorageLabel";

	[AssertLocalization]
	private const string upgradeConsoleHandText = "UpgradeConsole";

	public Equipment modules { get; private set; }

	public override void Awake()
	{
		base.Awake();
		if (modules == null)
		{
			InitializeModules();
		}
	}

	private void InitializeModules()
	{
		modules = new Equipment(base.gameObject, modulesRoot.transform);
		modules.SetLabel("CyclopsUpgradesStorageLabel");
		UpdateVisuals();
		modules.onEquip += OnEquip;
		modules.onUnequip += OnUnequip;
		UnlockDefaultModuleSlots();
	}

	private void UnlockDefaultModuleSlots()
	{
		string[] slots = new string[6] { "Module1", "Module2", "Module3", "Module4", "Module5", "Module6" };
		modules.AddSlots(slots);
	}

	public void OnHandClick(GUIHand guiHand)
	{
		PDA pDA = Player.main.GetPDA();
		Inventory.main.SetUsedStorage(modules);
		pDA.Open(PDATab.Inventory);
	}

	public void OnHandHover(GUIHand guiHand)
	{
		HandReticle main = HandReticle.main;
		main.SetText(HandReticle.TextType.Hand, "UpgradeConsole", translate: true, GameInput.Button.LeftHand);
		main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		main.SetIcon(HandReticle.IconType.Hand);
	}

	private void OnEquip(string slot, InventoryItem item)
	{
		UpdateVisuals();
	}

	private void OnUnequip(string slot, InventoryItem item)
	{
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		SetModuleVisibility("Module1", module1);
		SetModuleVisibility("Module2", module2);
		SetModuleVisibility("Module3", module3);
		SetModuleVisibility("Module4", module4);
		SetModuleVisibility("Module5", module5);
		SetModuleVisibility("Module6", module6);
	}

	private void SetModuleVisibility(string slot, GameObject module)
	{
		if (!(module == null))
		{
			module.SetActive(modules.GetTechTypeInSlot(slot) != TechType.None);
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		version = 2;
		serializedModuleSlots = modules.SaveEquipment();
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (modules == null)
		{
			InitializeModules();
		}
		modules.Clear();
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		if (serializedModuleSlots != null)
		{
			StorageHelper.TransferEquipment(modulesRoot, serializedModuleSlots, modules);
			serializedModuleSlots = null;
		}
		UnlockDefaultModuleSlots();
	}
}
