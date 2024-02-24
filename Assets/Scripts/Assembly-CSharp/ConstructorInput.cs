using System.Collections;
using ProtoBuf;
using Story;
using UnityEngine;

[ProtoContract]
public class ConstructorInput : Crafter, IHandTarget
{
	[AssertNotNull]
	public Material beamMaterial;

	[AssertNotNull]
	public Constructor constructor;

	[AssertNotNull]
	public ConstructorCinematicController cinematicController;

	[AssertNotNull]
	public Texture2D validCraftPositionMap;

	[AssertNotNull]
	public PDANotification invalidNotification;

	private const float kWorldExtents = 2048f;

	[AssertLocalization]
	private const string useConstructorHandText = "UseConstructor";

	protected override void Craft(TechType techType, float duration)
	{
		Vector3 position = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		GetCraftTransform(techType, ref position, ref rotation);
		if (techType != TechType.Seamoth && techType != TechType.Exosuit && !ReturnValidCraftingPosition(position))
		{
			invalidNotification.Play();
		}
		else if (CrafterLogic.ConsumeResources(techType))
		{
			duration = 3f;
			switch (techType)
			{
			case TechType.RocketBase:
				duration = 25f;
				break;
			case TechType.Cyclops:
				duration = 20f;
				break;
			case TechType.Seamoth:
			case TechType.Exosuit:
				duration = 10f;
				break;
			}
			base.Craft(techType, duration);
		}
	}

	protected override void OnCraftingBegin(TechType techType, float duration)
	{
		StartCoroutine(OnCraftingBeginAsync(techType, duration));
	}

	private IEnumerator OnCraftingBeginAsync(TechType techType, float duration)
	{
		Vector3 craftPosition = Vector3.zero;
		Quaternion craftRotation = Quaternion.identity;
		GetCraftTransform(techType, ref craftPosition, ref craftRotation);
		if (!GameInput.GetButtonHeld(GameInput.Button.Sprint))
		{
			uGUI.main.craftingMenu.Close(this);
			cinematicController.DisengageConstructor();
		}
		GameObject gameObject;
		if (techType == TechType.Cyclops)
		{
			SubConsoleCommand.main.SpawnSub("cyclops", craftPosition, craftRotation);
			FMODUWE.PlayOneShot("event:/tools/constructor/spawn", craftPosition);
			gameObject = SubConsoleCommand.main.GetLastCreatedSub();
		}
		else
		{
			TaskResult<GameObject> result = new TaskResult<GameObject>();
			yield return CraftData.InstantiateFromPrefabAsync(techType, result);
			gameObject = result.Get();
			Transform component = gameObject.GetComponent<Transform>();
			component.position = craftPosition;
			component.rotation = craftRotation;
		}
		CrafterLogic.NotifyCraftEnd(gameObject, techType);
		ItemGoalTracker.OnConstruct(techType);
		VFXConstructing componentInChildren = gameObject.GetComponentInChildren<VFXConstructing>();
		if (componentInChildren != null)
		{
			componentInChildren.timeToConstruct = duration;
			componentInChildren.StartConstruction();
		}
		if (gameObject.GetComponentInChildren<BuildBotPath>() == null)
		{
			new GameObject("ConstructorBeam").AddComponent<TwoPointLine>().Initialize(beamMaterial, base.transform, gameObject.transform, 0.1f, 1f, duration);
		}
		else
		{
			constructor.SendBuildBots(gameObject);
		}
		LargeWorldEntity.Register(gameObject);
	}

	protected override void OnCraftingEnd()
	{
		if (base.logic != null)
		{
			base.logic.ResetCrafter();
		}
	}

	private bool GetPlayerAllowedToUse()
	{
		if (!cinematicController.inUse && !Player.main.IsUnderwater() && Player.main.transform.position.y - base.transform.position.y > 0.3f && constructor.deployed)
		{
			return !constructor.IsDeployAnimationInProgress;
		}
		return false;
	}

	private void GetCraftTransform(TechType techType, ref Vector3 position, ref Quaternion rotation)
	{
		Transform itemSpawnPoint = constructor.GetItemSpawnPoint(techType);
		position = itemSpawnPoint.position;
		rotation = itemSpawnPoint.rotation;
	}

	private bool ReturnValidCraftingPosition(Vector3 pollPosition)
	{
		float num = Mathf.Clamp01((pollPosition.x + 2048f) / 4096f);
		float num2 = Mathf.Clamp01((pollPosition.z + 2048f) / 4096f);
		int x = (int)(num * (float)validCraftPositionMap.width);
		int y = (int)(num2 * (float)validCraftPositionMap.height);
		return validCraftPositionMap.GetPixel(x, y).g > 0.5f;
	}

	public void OnHandHover(GUIHand hand)
	{
		if (GetPlayerAllowedToUse())
		{
			if (CraftTree.HasKnown(CraftTree.Type.Constructor))
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, "UseConstructor", translate: true, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
				HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			}
			else
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, "NoRecipesAvailable", translate: true);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
				HandReticle.main.SetIcon(HandReticle.IconType.None);
			}
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (!(base.logic == null) && !base.logic.inProgress && GetPlayerAllowedToUse() && CraftTree.HasKnown(CraftTree.Type.Constructor))
		{
			cinematicController.EngageConstructor(hand.player);
		}
	}

	public void StartUse()
	{
		uGUI.main.craftingMenu.Open(CraftTree.Type.Constructor, this);
		constructor.usingMenu = true;
	}

	public void EndUse()
	{
		uGUI.main.craftingMenu.Close(this);
		constructor.usingMenu = false;
	}
}
