using System;
using System.Collections;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class MedicalCabinet : HandTarget, IHandTarget
{
	[AssertNotNull]
	public FMOD_CustomEmitter openSFX;

	[AssertNotNull]
	public FMOD_CustomEmitter closeSFX;

	public Constructable constructable;

	public FMOD_CustomEmitter playSound;

	public GameObject medKitModel;

	public GameObject door;

	public Transform doorOpenTransform;

	public Renderer doorRenderer;

	private Material doorMat;

	private bool doorOpen;

	private bool changeDoorState;

	private Quaternion doorOpenQuat;

	private Quaternion doorCloseQuat;

	[NonSerialized]
	[ProtoMember(1)]
	public bool hasMedKit;

	public float medKitSpawnInterval = 600f;

	public bool startWithMedKit;

	[NonSerialized]
	[ProtoMember(2)]
	public float timeSpawnMedKit = -1f;

	[AssertLocalization]
	private const string handHoverText = "MedicalCabinet";

	[AssertLocalization]
	private const string pickupMedKitHandText = "MedicalCabinet_PickupMedKit";

	[AssertLocalization]
	private const string doorOpenHandText = "MedicalCabinet_DoorOpen";

	[AssertLocalization]
	private const string doorCloseHandText = "MedicalCabinet_DoorClose";

	private void Start()
	{
		doorMat = doorRenderer.material;
		doorMat.SetFloat(ShaderPropertyID._GlowStrength, 0f);
		doorMat.SetFloat(ShaderPropertyID._GlowStrengthNight, 0f);
		DayNightCycle main = DayNightCycle.main;
		if (timeSpawnMedKit < 0f && (bool)main)
		{
			hasMedKit = false;
			timeSpawnMedKit = (float)main.timePassed + (startWithMedKit ? 0f : medKitSpawnInterval);
		}
		doorOpenQuat = doorOpenTransform.localRotation;
		doorCloseQuat = door.transform.localRotation;
		medKitModel.SetActive(hasMedKit);
	}

	private void Update()
	{
		DayNightCycle main = DayNightCycle.main;
		if (!hasMedKit && (bool)main && main.timePassed > (double)timeSpawnMedKit)
		{
			hasMedKit = true;
			InvokeRepeating("BlinkRepeat", 0f, 1f);
			playSound.Play();
		}
		else if (!hasMedKit)
		{
			CancelInvoke("BlinkRepeat");
			playSound.Stop();
		}
		if (changeDoorState)
		{
			Quaternion b = (doorOpen ? doorOpenQuat : doorCloseQuat);
			door.transform.localRotation = Quaternion.Slerp(door.transform.localRotation, b, Time.deltaTime * 5f);
		}
		medKitModel.SetActive(hasMedKit);
	}

	private void BlinkRepeat()
	{
		StartCoroutine("BlinkCoroutine");
	}

	private IEnumerator BlinkCoroutine()
	{
		SetEmission(1f);
		yield return new WaitForSeconds(0.05f);
		SetEmission(0f);
		yield return new WaitForSeconds(0.2f);
		SetEmission(1f);
		yield return new WaitForSeconds(0.05f);
		SetEmission(0f);
	}

	private void SetEmission(float emission)
	{
		doorMat.SetFloat(ShaderPropertyID._GlowStrength, emission);
		doorMat.SetFloat(ShaderPropertyID._GlowStrengthNight, emission);
	}

	public void OnHandHover(GUIHand hand)
	{
		if (!(constructable == null) && !constructable.constructed)
		{
			return;
		}
		bool flag = Player.main.HasInventoryRoom(1, 1);
		if (doorOpen && hasMedKit)
		{
			if (flag)
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, "MedicalCabinet", translate: true, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "MedicalCabinet_PickupMedKit", translate: true);
				HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			}
			else
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, "MedicalCabinet", translate: true, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "InventoryFull", translate: true);
			}
		}
		else if (hasMedKit)
		{
			string text = (doorOpen ? "MedicalCabinet_DoorClose" : "MedicalCabinet_DoorOpen");
			HandReticle.main.SetText(HandReticle.TextType.Hand, "MedicalCabinet", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text, translate: true);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
		else
		{
			float num = timeSpawnMedKit - medKitSpawnInterval;
			float progress = Mathf.InverseLerp(num, num + medKitSpawnInterval, DayNightCycle.main.timePassedAsFloat);
			HandReticle.main.SetProgress(progress);
			HandReticle.main.SetIcon(HandReticle.IconType.Progress);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (constructable.constructed)
		{
			bool flag = Player.main.HasInventoryRoom(1, 1);
			if (doorOpen && hasMedKit && flag)
			{
				CraftData.AddToInventory(TechType.FirstAidKit);
				hasMedKit = false;
				timeSpawnMedKit = DayNightCycle.main.timePassedAsFloat + medKitSpawnInterval;
				Invoke("ToggleDoorState", 2f);
			}
			else if (hasMedKit)
			{
				ToggleDoorState();
			}
		}
	}

	private void ToggleDoorState()
	{
		changeDoorState = true;
		doorOpen = !doorOpen;
		(doorOpen ? openSFX : closeSFX).Play();
		CancelInvoke("DoorInactive");
		Invoke("DoorInactive", 4f);
	}

	private void DoorInactive()
	{
		changeDoorState = false;
	}

	public void ForceSpawnMedKit()
	{
		startWithMedKit = true;
		hasMedKit = true;
		InvokeRepeating("BlinkRepeat", 0f, 1f);
		playSound.Play();
	}
}
