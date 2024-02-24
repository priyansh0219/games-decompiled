using UnityEngine;

public class PrecursorDoorway : MonoBehaviour
{
	public bool useDoorFunctionality = true;

	[AssertNotNull]
	public GameObject forceField;

	[AssertNotNull]
	public GameObject colliderAndSound;

	[AssertNotNull]
	public VFXLerpColor colorControl;

	public bool isOpen;

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
		ToggleDoor(NoCostConsoleCommand.main.unlockDoors);
	}

	private void Start()
	{
		if (!useDoorFunctionality)
		{
			forceField.SetActive(value: false);
		}
		SetDoorState(isOpen);
	}

	public void ToggleDoor(bool open)
	{
		if (open != isOpen)
		{
			SetDoorState(open);
		}
	}

	private void SetDoorState(bool open)
	{
		if (open)
		{
			DisableField();
		}
		else
		{
			EnableField();
		}
		isOpen = open;
	}

	private void EnableField()
	{
		forceField.SetActive(value: true);
		colliderAndSound.SetActive(value: true);
		colorControl.gameObject.SetActive(value: true);
		colorControl.ResetColor();
		CancelInvoke("DisableVisual");
	}

	private void DisableField()
	{
		colorControl.Play();
		Invoke("DisableVisual", colorControl.duration);
		colliderAndSound.SetActive(value: false);
	}

	private void DisableVisual()
	{
		colorControl.gameObject.SetActive(value: false);
	}
}
