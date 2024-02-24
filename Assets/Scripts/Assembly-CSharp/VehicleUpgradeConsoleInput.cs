using System;
using UnityEngine;

[SkipProtoContractCheck]
public class VehicleUpgradeConsoleInput : HandTarget, IHandTarget
{
	[Serializable]
	public struct Slot
	{
		public string id;

		public GameObject model;
	}

	public FMODAsset openSound;

	public FMODAsset closeSound;

	private Equipment _equipment;

	private Vehicle.DockType dockType;

	[AssertNotNull]
	public Transform flap;

	[AssertNotNull]
	public Collider collider;

	public float timeOpen = 0.5f;

	public float timeClose = 0.25f;

	public Vector3 anglesClosed = new Vector3(0f, 0f, 0f);

	public Vector3 anglesOpened = new Vector3(0f, 0f, -60f);

	[AssertLocalization]
	public string interactText = "UpgradeConsole";

	public Slot[] slots;

	private readonly Sequence sequence = new Sequence();

	public Equipment equipment
	{
		get
		{
			return _equipment;
		}
		set
		{
			if (_equipment != null)
			{
				_equipment.onEquip -= OnEquip;
				_equipment.onUnequip -= OnUnequip;
			}
			_equipment = value;
			if (_equipment != null)
			{
				_equipment.onEquip += OnEquip;
				_equipment.onUnequip += OnUnequip;
			}
			UpdateVisuals();
		}
	}

	private bool docked => dockType != Vehicle.DockType.None;

	public override void Awake()
	{
		base.Awake();
		UpdateVisuals();
	}

	private void Update()
	{
		sequence.Update();
		if (sequence.active)
		{
			Quaternion a = Quaternion.Euler(anglesClosed);
			Quaternion b = Quaternion.Euler(anglesOpened);
			flap.localRotation = Quaternion.Lerp(a, b, sequence.t);
		}
	}

	private void OnDestroy()
	{
		if (equipment != null)
		{
			equipment.onEquip -= OnEquip;
			equipment.onUnequip -= OnUnequip;
		}
	}

	public void SetDocked(Vehicle.DockType dockType)
	{
		if (this.dockType != dockType)
		{
			this.dockType = dockType;
			switch (dockType)
			{
			case Vehicle.DockType.None:
				collider.enabled = true;
				ChangeFlapState(open: false);
				break;
			case Vehicle.DockType.Base:
				collider.enabled = true;
				ChangeFlapState(open: true);
				break;
			case Vehicle.DockType.Cyclops:
				collider.enabled = false;
				ChangeFlapState(open: false);
				break;
			}
		}
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
		for (int i = 0; i < slots.Length; i++)
		{
			Slot slot = slots[i];
			GameObject model = slot.model;
			if (model != null)
			{
				bool active = equipment != null && equipment.GetTechTypeInSlot(slot.id) != TechType.None;
				model.SetActive(active);
			}
		}
	}

	private void ChangeFlapState(bool open, bool pda = false)
	{
		float time = (open ? timeOpen : timeClose);
		if (pda)
		{
			sequence.Set(time, open, OpenPDA);
		}
		else
		{
			sequence.Set(time, open);
		}
		Utils.PlayFMODAsset(open ? openSound : closeSound, base.transform, 1f);
	}

	private void OpenPDA()
	{
		if (equipment != null)
		{
			PDA pDA = Player.main.GetPDA();
			Inventory.main.SetUsedStorage(equipment);
			if (!pDA.Open(PDATab.Inventory, base.transform, OnClosePDA))
			{
				OnClosePDA(pDA);
			}
		}
	}

	private void OnClosePDA(PDA pda)
	{
		if (!docked)
		{
			sequence.Set(timeClose, target: false);
			Utils.PlayFMODAsset(closeSound, base.transform, 1f);
		}
	}

	public void OpenFromExternal()
	{
		PDA pDA = Player.main.GetPDA();
		Inventory.main.SetUsedStorage(equipment);
		pDA.Open(PDATab.Inventory);
	}

	public void OnHandClick(GUIHand guiHand)
	{
		if (docked)
		{
			OpenPDA();
		}
		else
		{
			ChangeFlapState(open: true, pda: true);
		}
	}

	public void OnHandHover(GUIHand guiHand)
	{
		if (equipment != null)
		{
			HandReticle main = HandReticle.main;
			main.SetText(HandReticle.TextType.Hand, interactText, translate: true, GameInput.Button.LeftHand);
			main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			main.SetIcon(HandReticle.IconType.Hand);
		}
	}
}
