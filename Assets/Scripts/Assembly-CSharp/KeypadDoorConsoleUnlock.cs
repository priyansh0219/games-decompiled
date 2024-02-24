using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class KeypadDoorConsoleUnlock : MonoBehaviour
{
	public GameObject doorIcon;

	public GameObject unlockIcon;

	public GameObject root;

	public FMOD_CustomEmitter acceptedSound;

	[NonSerialized]
	[ProtoMember(1)]
	public bool unlocked;

	private bool tempDisable;

	private void Start()
	{
		if (!unlocked)
		{
			doorIcon.SetActive(value: true);
			unlockIcon.SetActive(value: false);
		}
		else
		{
			doorIcon.SetActive(value: false);
			unlockIcon.SetActive(value: true);
		}
	}

	private void UnlockDoorButtonPress()
	{
		if ((bool)root)
		{
			root.BroadcastMessage("UnlockDoor");
		}
		else
		{
			BroadcastMessage("UnlockDoor");
		}
		UnlockDoor();
	}

	public void UnlockDoor()
	{
		doorIcon.SetActive(value: false);
		unlockIcon.SetActive(value: true);
		unlocked = true;
	}
}
