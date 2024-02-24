using System;
using System.Collections;
using ProtoBuf;
using Story;
using UnityEngine;

[ProtoContract]
[RequireComponent(typeof(GenericHandTarget))]
public class BlueprintHandTarget : MonoBehaviour, ICompileTimeCheckable, ILocalizationCheckable
{
	public delegate void OnUsed();

	public TechType unlockTechType;

	[AssertLocalization(AssertLocalizationAttribute.Options.AllowEmptyString)]
	[SerializeField]
	private string primaryTooltip;

	[AssertLocalization(AssertLocalizationAttribute.Options.AllowEmptyString)]
	[SerializeField]
	private string secondaryTooltip;

	[AssertNotNull]
	public StoryGoal onUseGoal;

	[AssertNotNull]
	public ResourceTracker resourceTracker;

	public Animator animator;

	public string animParam;

	public string viewAnimParam;

	public FMODAsset useSound;

	public GameObject disableGameObject;

	public GameObject inspectPrefab;

	public float disableDelay = 1f;

	public float viewAnimDuration = 2f;

	[NonSerialized]
	[ProtoMember(1)]
	public bool used;

	private GameObject inspectObject;

	private string alreadyUnlockedTooltip;

	[AssertLocalization(2)]
	private const string DataboxTooltipFormat = "DataboxToolipFormat";

	[AssertLocalization(2)]
	private const string DataboxAlreadyUnlockedToolipFormat = "DataboxAlreadyUnlockedToolipFormat";

	public event OnUsed OnUsedEvent;

	private void Start()
	{
		bool flag = KnownTech.Contains(unlockTechType);
		used &= flag;
		if (string.IsNullOrEmpty(primaryTooltip))
		{
			primaryTooltip = unlockTechType.AsString();
		}
		if (string.IsNullOrEmpty(secondaryTooltip))
		{
			string arg = Language.main.Get(unlockTechType);
			string arg2 = Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(unlockTechType));
			secondaryTooltip = Language.main.GetFormat("DataboxToolipFormat", arg, arg2);
			alreadyUnlockedTooltip = Language.main.GetFormat("DataboxAlreadyUnlockedToolipFormat", arg, arg2);
		}
		else
		{
			alreadyUnlockedTooltip = secondaryTooltip;
		}
		if (!string.IsNullOrEmpty(animParam) && animator != null)
		{
			animator.SetBool(animParam, used);
		}
		if (used)
		{
			OnTargetUsed();
		}
	}

	private void OnDestroy()
	{
		if ((bool)inspectObject)
		{
			UnityEngine.Object.Destroy(inspectObject);
			Inventory.main.quickSlots.SetIgnoreHotkeyInput(ignore: false);
			Player.main.GetPDA().SetIgnorePDAInput(ignore: false);
		}
	}

	public void HoverBlueprint()
	{
		if (!used)
		{
			bool flag = KnownTech.Contains(unlockTechType);
			HandReticle.main.SetText(HandReticle.TextType.Hand, primaryTooltip, translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, flag ? alreadyUnlockedTooltip : secondaryTooltip, translate: true);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void UnlockBlueprint()
	{
		if (used)
		{
			return;
		}
		bool flag = false;
		if (!string.IsNullOrEmpty(onUseGoal.key))
		{
			onUseGoal.Trigger();
		}
		if ((bool)useSound)
		{
			Utils.PlayFMODAsset(useSound, base.transform);
		}
		used = true;
		if (!string.IsNullOrEmpty(animParam) && animator != null)
		{
			animator.SetBool(animParam, value: true);
			if ((bool)disableGameObject)
			{
				if (disableDelay > 0f)
				{
					StartDisableGameObject();
				}
				else
				{
					flag = !TryToAddToKnownTech();
					OnTargetUsed();
				}
			}
		}
		else
		{
			flag = !TryToAddToKnownTech();
			OnTargetUsed();
		}
		if (flag)
		{
			CraftData.AddToInventory(TechType.Titanium, 2);
		}
	}

	private bool TryToAddToKnownTech()
	{
		return KnownTech.Add(unlockTechType);
	}

	private void StartDisableGameObject()
	{
		StartCoroutine(DisableGameObjectAsync());
	}

	private IEnumerator DisableGameObjectAsync()
	{
		Player.allowSaving = false;
		bool hasViewAnim = !string.IsNullOrEmpty(viewAnimParam);
		if (hasViewAnim)
		{
			Player.main.armsController.StartHolsterTime(disableDelay + viewAnimDuration);
		}
		yield return new WaitForSeconds(disableDelay);
		bool redundant = !TryToAddToKnownTech();
		OnTargetUsed();
		if (hasViewAnim)
		{
			Player.main.armsController.TriggerAnimParam(viewAnimParam, viewAnimDuration);
			if ((bool)inspectPrefab)
			{
				inspectObject = UnityEngine.Object.Instantiate(inspectPrefab);
				inspectObject.transform.SetParent(Player.main.armsController.leftHandAttach);
				inspectObject.transform.localPosition = Vector3.zero;
				inspectObject.transform.localRotation = Quaternion.identity;
			}
			yield return new WaitForSeconds(viewAnimDuration);
			if ((bool)inspectObject)
			{
				UnityEngine.Object.Destroy(inspectObject);
			}
		}
		if (redundant)
		{
			CraftData.AddToInventory(TechType.Titanium, 2);
		}
		Player.allowSaving = true;
	}

	private void OnTargetUsed()
	{
		if ((bool)disableGameObject)
		{
			disableGameObject.SetActive(value: false);
		}
		if (this.OnUsedEvent != null)
		{
			this.OnUsedEvent();
		}
		resourceTracker.OnBlueprintHandTargetUsed();
	}

	public string CompileTimeCheck()
	{
		if (!string.IsNullOrEmpty(onUseGoal.key))
		{
			return StoryGoalUtils.CheckStoryGoal(onUseGoal);
		}
		return null;
	}

	public string CompileTimeCheck(ILanguage language)
	{
		if (string.IsNullOrEmpty(primaryTooltip))
		{
			return language.CheckTechType(unlockTechType);
		}
		if (string.IsNullOrEmpty(secondaryTooltip))
		{
			return language.CheckTechType(unlockTechType) ?? language.CheckTechTypeTooltip(unlockTechType);
		}
		return null;
	}
}
