using System;
using ProtoBuf;
using TMPro;
using UnityEngine;

[ProtoContract]
public class KeypadDoorConsole : MonoBehaviour
{
	public string accessCode;

	public GameObject keypadUI;

	public GameObject unlockIcon;

	public GameObject buttonHolder;

	public TextMeshProUGUI numberField;

	public GameObject root;

	public FMOD_CustomEmitter keyInputSound;

	public FMOD_CustomEmitter acceptedSound;

	public FMOD_CustomEmitter rejectedSound;

	[NonSerialized]
	[ProtoMember(1)]
	public bool unlocked;

	private bool tempDisable;

	private void Start()
	{
		if (!unlocked)
		{
			keypadUI.SetActive(value: true);
			unlockIcon.SetActive(value: false);
		}
		else
		{
			keypadUI.SetActive(value: false);
			unlockIcon.SetActive(value: true);
		}
		int num = 1;
		foreach (Transform item in buttonHolder.transform)
		{
			item.GetComponent<KeypadDoorConsoleButton>().index = num;
			item.GetChild(0).GetComponent<TextMeshProUGUI>().text = num.ToString();
			num++;
		}
	}

	public void NumberButtonPress(int index)
	{
		if (numberField.text.Length <= 3)
		{
			if ((bool)keyInputSound)
			{
				keyInputSound.Play();
			}
			numberField.text += index;
		}
		if (numberField.text.Length != 4)
		{
			return;
		}
		if (numberField.text == accessCode)
		{
			unlocked = true;
			tempDisable = true;
			numberField.color = Color.green;
			Invoke("AcceptNumberField", 2f);
			if ((bool)acceptedSound)
			{
				acceptedSound.Play();
			}
		}
		else
		{
			tempDisable = true;
			numberField.color = Color.red;
			Invoke("ResetNumberField", 2f);
			if ((bool)rejectedSound)
			{
				rejectedSound.Play();
			}
		}
	}

	private void AcceptNumberField()
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
		keypadUI.SetActive(value: false);
		unlockIcon.SetActive(value: true);
	}

	private void ResetNumberField()
	{
		numberField.color = Color.white;
		numberField.text = "";
		tempDisable = false;
	}

	public void BackspaceButtonPress()
	{
		if (!tempDisable && numberField.text.Length > 0)
		{
			numberField.text = numberField.text.Remove(numberField.text.Length - 1);
		}
	}
}
