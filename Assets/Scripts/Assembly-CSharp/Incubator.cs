using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using Story;
using UnityEngine;

[ProtoContract]
public class Incubator : MonoBehaviour, IHandTarget, ICompileTimeCheckable
{
	[AssertNotNull]
	public Transform[] eggSpawns;

	[AssertNotNull]
	public GameObject eggPrefab;

	[AssertNotNull]
	public Renderer[] renderers;

	[AssertNotNull]
	public Light terminalLight;

	[AssertNotNull]
	public FMODAsset powerUpSound;

	[AssertNotNull]
	public FMODAsset hatchingSoundscape;

	[AssertNotNull]
	public FMODAsset hatchingMusic;

	[AssertNotNull]
	public IncubatorComputerTerminal computerTerminal;

	[AssertNotNull]
	public StoryGoal onUseGoal;

	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public bool powered;

	[NonSerialized]
	[ProtoMember(3)]
	public bool hatched;

	private readonly List<IncubatorEgg> eggs = new List<IncubatorEgg>();

	[AssertNotNull]
	public string[] hatchAnimations;

	private GameObject enzymesObject;

	private int restoreQuickSlot = -1;

	private MaterialPropertyBlock block;

	private float emissiveIntensity = 1f;

	[AssertLocalization]
	private const string incubatorHandText = "Incubator";

	[AssertLocalization]
	private const string insertHatchingEnzymesHandText = "Insert_Hatching_Enzymes";

	private bool terminalActive
	{
		get
		{
			if (powered)
			{
				return !hatched;
			}
			return false;
		}
	}

	private void Start()
	{
		for (int i = 0; i < eggSpawns.Length; i++)
		{
			GameObject obj = UnityEngine.Object.Instantiate(eggPrefab, Vector3.zero, Quaternion.identity);
			obj.transform.SetParent(eggSpawns[i], worldPositionStays: false);
			IncubatorEgg component = obj.GetComponent<IncubatorEgg>();
			eggs.Add(component);
			if (hatched)
			{
				component.animationController.SetHatched(hatchAnimations[i]);
			}
		}
		computerTerminal.SetActive(terminalActive);
		emissiveIntensity = (terminalActive ? 1 : 0);
		UpdateMaterialsEmissive();
		terminalLight.intensity = emissiveIntensity * 1.85f;
	}

	public void SetPowered(bool isPowered)
	{
		powered = isPowered;
		computerTerminal.SetActive(terminalActive);
		if (isPowered)
		{
			Utils.PlayFMODAsset(powerUpSound, base.transform);
			emissiveIntensity = 1f;
			UpdateMaterialsEmissive();
			terminalLight.intensity = emissiveIntensity * 1.85f;
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		if (terminalActive)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "Incubator", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "Insert_Hatching_Enzymes", translate: true);
			HandReticle.main.SetIcon(HandReticle.IconType.Interact);
			computerTerminal.OnHover();
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (terminalActive)
		{
			Pickupable pickupable = Inventory.main.container.RemoveItem(TechType.HatchingEnzymes);
			if (pickupable != null)
			{
				restoreQuickSlot = Inventory.main.quickSlots.activeSlot;
				Inventory.main.ReturnHeld();
				enzymesObject = pickupable.gameObject;
				enzymesObject.transform.SetParent(Inventory.main.toolSocket);
				enzymesObject.transform.localPosition = Vector3.zero;
				enzymesObject.transform.localRotation = Quaternion.identity;
				enzymesObject.SetActive(value: true);
				Utils.PlayFMODAsset(hatchingSoundscape, base.transform, 30f);
				Utils.PlayFMODAsset(hatchingMusic);
				hatched = true;
				computerTerminal.OnUse();
				OnHatched();
			}
		}
	}

	public void OnHatched()
	{
		if ((bool)enzymesObject)
		{
			UnityEngine.Object.Destroy(enzymesObject);
		}
		computerTerminal.SetActive(state: false);
		onUseGoal.Trigger();
		if ((bool)SeaEmperor.main)
		{
			SeaEmperor.main.OnBabiesHatched();
		}
		for (int i = 0; i < eggs.Count; i++)
		{
			eggs[i].StartHatch(i, hatchAnimations[i]);
		}
		if (restoreQuickSlot != -1)
		{
			Inventory.main.quickSlots.Select(restoreQuickSlot);
		}
		emissiveIntensity = 0f;
		UpdateMaterialsEmissive();
		terminalLight.intensity = emissiveIntensity * 1.85f;
	}

	public string CompileTimeCheck()
	{
		if (hatchAnimations.Length != 5)
		{
			return "Need exactly 5 hatch animations for the eggs!";
		}
		return StoryGoalUtils.CheckStoryGoal(onUseGoal);
	}

	private void UpdateMaterialsEmissive()
	{
		if (block == null)
		{
			block = new MaterialPropertyBlock();
		}
		IEnumerator enumerator = renderers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Renderer renderer = (Renderer)enumerator.Current;
			if (!(renderer == null))
			{
				block.Clear();
				renderer.GetPropertyBlock(block);
				block.SetFloat(ShaderPropertyID._UwePowerLoss, Mathf.Clamp01(1f - emissiveIntensity));
				renderer.SetPropertyBlock(block);
			}
		}
	}
}
