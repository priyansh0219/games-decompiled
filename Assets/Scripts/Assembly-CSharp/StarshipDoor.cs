using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class StarshipDoor : HandTarget, IHandTarget
{
	public enum OpenMethodEnum
	{
		Manual = 0,
		Sealed = 1,
		Keycard = 2,
		Powercell = 3
	}

	public OpenMethodEnum doorOpenMethod;

	[AssertLocalization]
	public string openText = "OpenDoor";

	[AssertLocalization]
	public string closeText = "CloseDoor";

	[NonSerialized]
	[ProtoMember(1)]
	public bool doorOpen;

	[NonSerialized]
	[ProtoMember(2)]
	public bool doorLocked = true;

	public bool startDoorOpen;

	public bool requirePlayerInFrontToOpen;

	public GameObject doorObject;

	public FMOD_CustomEmitter openSound;

	private Sealed sealedComponent;

	private Vector3 closedPos;

	private Vector3 openPos;

	[AssertLocalization]
	private const string sealedDoorHandText = "Sealed_Door";

	[AssertLocalization]
	private const string sealedInstructionsHandText = "SealedInstructions";

	[AssertLocalization]
	private const string lockedDoorHandText = "Locked_Door";

	[AssertLocalization]
	private const string doorInstructionsKeycardHandText = "DoorInstructions_Keycard";

	[AssertLocalization]
	private const string doorInstructionsPowercellHandText = "DoorInstructions_Powercell";

	private void OnEnable()
	{
		NoCostConsoleCommand.main.UnlockDoorsEvent += OnUnlockDoorsCheat;
	}

	private void OnDisable()
	{
		NoCostConsoleCommand.main.UnlockDoorsEvent -= OnUnlockDoorsCheat;
	}

	private void OnUnlockDoorsCheat()
	{
		if (NoCostConsoleCommand.main.unlockDoors)
		{
			UnlockDoor();
		}
		else
		{
			LockDoor();
		}
	}

	private void Start()
	{
		if (!doorObject)
		{
			doorObject = base.gameObject;
		}
		sealedComponent = GetComponent<Sealed>();
		if (sealedComponent != null)
		{
			sealedComponent.openedEvent.AddHandler(base.gameObject, OnSealedDoorOpen);
		}
		closedPos = doorObject.transform.position;
		openPos = doorObject.transform.TransformPoint(new Vector3(0f, 1.6f, 0f));
		if (startDoorOpen || doorOpen)
		{
			doorLocked = false;
			doorOpen = true;
			doorObject.transform.position = openPos;
		}
		if (!doorLocked)
		{
			UnlockDoor();
		}
		if (NoCostConsoleCommand.main.unlockDoors)
		{
			UnlockDoor();
		}
	}

	private void Update()
	{
		if (doorOpenMethod == OpenMethodEnum.Manual)
		{
			Vector3 position = doorObject.transform.position;
			position = ((!doorOpen) ? Vector3.Lerp(position, closedPos, Time.deltaTime * 2f) : Vector3.Lerp(position, openPos, Time.deltaTime * 2f));
			doorObject.transform.position = position;
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		switch (doorOpenMethod)
		{
		case OpenMethodEnum.Manual:
			if (!doorLocked)
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, doorOpen ? closeText : openText, translate: true, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
				HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			}
			break;
		case OpenMethodEnum.Sealed:
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Sealed_Door", translate: true);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "SealedInstructions", translate: true);
			HandReticle.main.SetProgress(sealedComponent.GetSealedPercentNormalized());
			HandReticle.main.SetIcon(HandReticle.IconType.Progress);
			break;
		case OpenMethodEnum.Keycard:
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Locked_Door", translate: true);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "DoorInstructions_Keycard", translate: true);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			break;
		case OpenMethodEnum.Powercell:
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Locked_Door", translate: true);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "DoorInstructions_Powercell", translate: true);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			break;
		}
	}

	public void OnHandClick(GUIHand guiHand)
	{
		if ((!requirePlayerInFrontToOpen || Utils.CheckObjectInFront(base.transform, Player.main.transform)) && !doorLocked)
		{
			OnDoorToggle();
		}
	}

	public void UnlockDoor()
	{
		doorLocked = false;
		StarshipDoorLocked component = GetComponent<StarshipDoorLocked>();
		if (component != null)
		{
			component.SetDoorLockState(locked: false);
		}
	}

	public void LockDoor()
	{
		doorLocked = true;
		StarshipDoorLocked component = GetComponent<StarshipDoorLocked>();
		if (component != null)
		{
			component.SetDoorLockState(locked: true);
		}
	}

	private void OnDoorToggle()
	{
		if (doorOpenMethod == OpenMethodEnum.Manual)
		{
			doorOpen = !doorOpen;
		}
		if ((bool)openSound)
		{
			openSound.Play();
		}
	}

	private void OnSealedDoorOpen(Sealed sealedComponent)
	{
		OnDoorToggle();
	}
}
