using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class SupplyCrate : HandTarget, IHandTarget
{
	[AssertNotNull]
	public string openClipName = "crate_treasure_chest_open_anim";

	[AssertNotNull]
	public string closeClipName = "crate_treasure_chest_close_anim";

	[AssertLocalization]
	public string openText = "Open_SupplyCrate";

	[AssertLocalization]
	public string closeText = "Close_SupplyCrate";

	[AssertNotNull]
	public string snapOpenOnLoad = "crate_treasure_chest_open_static";

	public GameObject setActiveOnOpen;

	[AssertNotNull]
	public FMODAsset openSound;

	private Sealed sealedComp;

	private Pickupable itemInside;

	[NonSerialized]
	[ProtoMember(1)]
	public bool open;

	[AssertLocalization]
	private const string sealedHandText = "Sealed_SupplyCrate";

	[AssertLocalization]
	private const string sealedInstructionsHandText = "SealedInstructions";

	[AssertLocalization]
	private const string takeItemHandText = "TakeItem_SupplyCrate";

	private void Start()
	{
		sealedComp = GetComponent<Sealed>();
		if (sealedComp != null)
		{
			sealedComp.openedEvent.AddHandler(base.gameObject, OnSealedOpened);
		}
		Animation componentInChildren = GetComponentInChildren<Animation>();
		if (componentInChildren != null && open)
		{
			componentInChildren.Play(snapOpenOnLoad);
		}
	}

	private void FindInsideItemAfterStart()
	{
		itemInside = base.transform.GetComponentInChildren<Pickupable>();
	}

	public void OnHandHover(GUIHand hand)
	{
		FindInsideItemAfterStart();
		bool flag = false;
		if (!open)
		{
			if (sealedComp == null || !sealedComp.IsSealed())
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, openText, translate: true, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			}
			else
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, "Sealed_SupplyCrate", translate: true);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "SealedInstructions", translate: true);
			}
			flag = true;
		}
		else if (itemInside != null)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "TakeItem_SupplyCrate", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			flag = true;
		}
		if (flag)
		{
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnHandClick(GUIHand guiHand)
	{
		FindInsideItemAfterStart();
		if (sealedComp == null || !sealedComp.IsSealed())
		{
			if (!open)
			{
				ToggleOpenState();
			}
			else if (itemInside != null)
			{
				Inventory.main.Pickup(itemInside);
				itemInside = null;
			}
		}
	}

	private void ToggleOpenState()
	{
		Animation componentInChildren = GetComponentInChildren<Animation>();
		if (componentInChildren != null)
		{
			componentInChildren.Play((!open) ? openClipName : closeClipName);
			open = !open;
			if (open && (bool)setActiveOnOpen)
			{
				setActiveOnOpen.SetActive(value: true);
			}
			if (open)
			{
				Utils.PlayFMODAsset(openSound, base.transform);
			}
		}
	}

	private void OnSealedOpened(Sealed sealedComp)
	{
		ToggleOpenState();
	}
}
