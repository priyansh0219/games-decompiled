using UnityEngine;

[SkipProtoContractCheck]
public class SeamothStorageInput : HandTarget, IHandTarget
{
	public SeaMoth seamoth;

	public int slotID = -1;

	public GameObject model;

	public Transform flap;

	public Collider collider;

	public float timeOpen = 0.5f;

	public float timeClose = 0.25f;

	public Vector3 anglesClosed = new Vector3(0f, 0f, 0f);

	public Vector3 anglesOpened = new Vector3(0f, 0f, -60f);

	public FMODAsset openSound;

	public FMODAsset closeSound;

	private Transform tr;

	private Sequence sequence;

	private Vehicle.DockType dockType;

	private bool state;

	public override void Awake()
	{
		base.Awake();
		tr = GetComponent<Transform>();
		sequence = new Sequence();
		UpdateColliderState();
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

	private void OnDisable()
	{
		sequence.Reset();
		flap.localRotation = Quaternion.Euler(anglesClosed);
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
		ItemsContainer storageInSlot = seamoth.GetStorageInSlot(slotID, TechType.VehicleStorageModule);
		if (storageInSlot != null)
		{
			PDA pDA = Player.main.GetPDA();
			Inventory.main.SetUsedStorage(storageInSlot);
			if (!pDA.Open(PDATab.Inventory, tr, OnClosePDA))
			{
				OnClosePDA(pDA);
			}
		}
		else
		{
			OnClosePDA(null);
		}
	}

	private void OnClosePDA(PDA pda)
	{
		sequence.Set(timeClose, target: false);
		Utils.PlayFMODAsset(closeSound, base.transform, 1f);
	}

	private void UpdateColliderState()
	{
		if (collider != null)
		{
			collider.enabled = state && dockType != Vehicle.DockType.Cyclops;
		}
	}

	public void SetEnabled(bool state)
	{
		if (this.state != state)
		{
			this.state = state;
			UpdateColliderState();
			if (model != null)
			{
				model.SetActive(state);
			}
		}
	}

	public void OpenFromExternal()
	{
		ItemsContainer storageInSlot = seamoth.GetStorageInSlot(slotID, TechType.VehicleStorageModule);
		if (storageInSlot != null)
		{
			PDA pDA = Player.main.GetPDA();
			Inventory.main.SetUsedStorage(storageInSlot);
			pDA.Open(PDATab.Inventory);
		}
	}

	public void SetDocked(Vehicle.DockType dockType)
	{
		this.dockType = dockType;
		UpdateColliderState();
	}

	public void OnHandHover(GUIHand hand)
	{
		HandReticle main = HandReticle.main;
		main.SetText(HandReticle.TextType.Hand, "SeamothStorageOpen", translate: true, GameInput.Button.LeftHand);
		main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnHandClick(GUIHand hand)
	{
		if (seamoth.GetStorageInSlot(slotID, TechType.VehicleStorageModule) != null)
		{
			ChangeFlapState(open: true, pda: true);
		}
	}
}
