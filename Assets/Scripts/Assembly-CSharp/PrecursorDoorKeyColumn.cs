using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PrecursorDoorKeyColumn : HandTarget, IHandTarget
{
	public GameObject glowFX;

	public BoxCollider boxCollider;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public bool unlocked;

	[AssertLocalization]
	private const string insertKeyHandText = "Insert_Precursor_Key";

	public void SlotKey(GameObject keyObject)
	{
		if (!unlocked)
		{
			unlocked = true;
			if ((bool)boxCollider)
			{
				boxCollider.enabled = false;
			}
			base.gameObject.BroadcastMessage("ToggleDoor", true, SendMessageOptions.RequireReceiver);
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		if (!unlocked)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Insert_Precursor_Key", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
	}
}
