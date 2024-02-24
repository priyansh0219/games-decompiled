using System;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class PrecursorKeyTerminal : HandTarget, IHandTarget
{
	public enum PrecursorKeyType
	{
		PrecursorKey_Red = 0,
		PrecursorKey_Orange = 1,
		PrecursorKey_Blue = 2,
		PrecursorKey_White = 3,
		PrecursorKey_Purple = 4
	}

	public PrecursorKeyType acceptKeyType;

	public Material[] keyMats = new Material[5];

	[AssertNotNull]
	public Transform keySlotPos;

	[AssertNotNull]
	public MeshRenderer keyFace;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public PlayerCinematicController cinematicController;

	[AssertNotNull]
	public FMODAsset useSound;

	[AssertNotNull]
	public FMODAsset openSound;

	[AssertNotNull]
	public FMODAsset closeSound;

	private const int currentVersion = 1;

	private GameObject keyObject;

	private int restoreQuickSlot = -1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public bool slotted;

	[AssertLocalization]
	private const string insertPrecursorKeyHandText = "Insert_Precursor_Key";

	private void Start()
	{
		keyFace.material = keyMats[(int)acceptKeyType];
	}

	private void OnEnable()
	{
		if (slotted)
		{
			BroadcastDoorsOpen();
		}
	}

	public void OpenDeck()
	{
		if (!slotted)
		{
			Utils.PlayFMODAsset(openSound, base.transform);
			animator.SetBool("Open", value: true);
		}
	}

	public void CloseDeck()
	{
		if (animator.GetBool("Open"))
		{
			animator.SetBool("Open", value: false);
			Utils.PlayFMODAsset(closeSound, base.transform);
		}
	}

	private void DestroyKey()
	{
		if ((bool)keyObject)
		{
			UnityEngine.Object.Destroy(keyObject);
		}
	}

	public void OnPlayerCinematicModeEnd()
	{
		if ((bool)keyObject)
		{
			keyObject.transform.parent = null;
		}
		BroadcastDoorsOpen();
		float num = 0.25f;
		Invoke("CloseDeck", num);
		Invoke("DestroyKey", num + 0.2f);
		if (restoreQuickSlot != -1)
		{
			Inventory.main.quickSlots.Select(restoreQuickSlot);
		}
	}

	private void BroadcastDoorsOpen()
	{
		base.transform.parent.BroadcastMessage("ToggleDoor", true, SendMessageOptions.RequireReceiver);
	}

	public void OnHandHover(GUIHand hand)
	{
		if (!slotted)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Insert_Precursor_Key", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (slotted)
		{
			return;
		}
		TechType techType = ConvertKeyTypeToTechType(acceptKeyType);
		Pickupable pickupable = Inventory.main.container.RemoveItem(techType);
		restoreQuickSlot = -1;
		if (pickupable != null)
		{
			restoreQuickSlot = Inventory.main.quickSlots.activeSlot;
			Inventory.main.ReturnHeld();
			keyObject = pickupable.gameObject;
			keyObject.transform.SetParent(Inventory.main.toolSocket);
			keyObject.transform.localPosition = Vector3.zero;
			keyObject.transform.localRotation = Quaternion.identity;
			keyObject.SetActive(value: true);
			Rigidbody component = keyObject.GetComponent<Rigidbody>();
			if (component != null)
			{
				UWE.Utils.SetIsKinematicAndUpdateInterpolation(component, isKinematic: true);
			}
			cinematicController.StartCinematicMode(Player.main);
			Utils.PlayFMODAsset(useSound, base.transform);
			slotted = true;
		}
	}

	public void ToggleDoor(bool open)
	{
		if (open)
		{
			slotted = true;
			Invoke("CloseDeck", 4f);
		}
	}

	private TechType ConvertKeyTypeToTechType(PrecursorKeyType inputType)
	{
		return (TechType)Enum.Parse(typeof(TechType), inputType.ToString());
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(keyFace.material);
	}
}
