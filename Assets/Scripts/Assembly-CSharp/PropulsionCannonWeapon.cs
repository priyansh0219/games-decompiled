using System.Collections.Generic;
using UnityEngine;

public class PropulsionCannonWeapon : PlayerTool, IEquippable
{
	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public PropulsionCannon propulsionCannon;

	private string cachedPrimaryUseText = string.Empty;

	private string cachedAltUseText = string.Empty;

	private string cachedCustomUseText = string.Empty;

	private bool firstUse;

	[AssertLocalization(1)]
	private const string shootControlFormat = "PropulsionCannonToShoot";

	[AssertLocalization(1)]
	private const string releaseControlFormat = "PropulsionCannonToRelease";

	[AssertLocalization(1)]
	private const string grabControlFormat = "PropulsionCannonToGrab";

	[AssertLocalization(1)]
	private const string loadControlFormat = "PropulsionCannonToLoad";

	[AssertLocalization]
	private const string noItemsMessage = "PropulsionCannonNoItems";

	public override string GetCustomUseText()
	{
		bool flag = propulsionCannon.IsGrabbingObject();
		bool flag2 = propulsionCannon.HasChargeForShot();
		if (usingPlayer == null || usingPlayer.IsInSub() || !(flag || flag2))
		{
			return base.GetCustomUseText();
		}
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (flag)
		{
			empty = LanguageCache.GetButtonFormat("PropulsionCannonToShoot", GameInput.Button.RightHand);
			empty2 = LanguageCache.GetButtonFormat("PropulsionCannonToRelease", GameInput.Button.AltTool);
		}
		else
		{
			empty = LanguageCache.GetButtonFormat("PropulsionCannonToGrab", GameInput.Button.RightHand);
			empty2 = LanguageCache.GetButtonFormat("PropulsionCannonToLoad", GameInput.Button.AltTool);
		}
		if (empty != cachedPrimaryUseText || empty2 != cachedAltUseText)
		{
			cachedCustomUseText = $"{empty}, {empty2}";
			cachedPrimaryUseText = empty;
			cachedAltUseText = empty2;
		}
		return cachedCustomUseText;
	}

	public override void OnDraw(Player p)
	{
		TechType techType = pickupable.GetTechType();
		firstUse = !p.IsToolUsed(techType) || PlayerToolConsoleCommands.debugFirstUse;
		base.OnDraw(p);
	}

	public override void OnHolster()
	{
		base.OnHolster();
		propulsionCannon.ReleaseGrabbedObject();
	}

	public override void OnToolBleederHitAnim(GUIHand guiHand)
	{
		if (usingPlayer != null)
		{
			Bleeder bleeder = usingPlayer.GetComponentInChildren<BleederAttachTarget>().bleeder;
			if (bleeder != null)
			{
				bleeder.attachAndSuck.SetDetached();
				propulsionCannon.ReleaseGrabbedObject();
				propulsionCannon.GrabObject(bleeder.gameObject);
			}
		}
	}

	public override FMODAsset GetBleederHitSound(FMODAsset defaultSound)
	{
		return null;
	}

	public override bool OnExitDown()
	{
		if (usingPlayer != null && !usingPlayer.IsInSub())
		{
			propulsionCannon.ReleaseGrabbedObject();
			return true;
		}
		return false;
	}

	public override bool OnAltDown()
	{
		if (usingPlayer != null && usingPlayer.IsInSub())
		{
			return false;
		}
		if (firstUseAnimationStarted)
		{
			OnFirstUseAnimationStop();
		}
		if (propulsionCannon.IsGrabbingObject())
		{
			propulsionCannon.ReleaseGrabbedObject();
		}
		else if (propulsionCannon.HasChargeForShot() && !propulsionCannon.OnReload(new List<IItemsContainer> { Inventory.main.container }))
		{
			ErrorMessage.AddMessage(Language.main.Get("PropulsionCannonNoItems"));
		}
		return true;
	}

	public override bool OnRightHandDown()
	{
		if (usingPlayer != null && usingPlayer.IsInSub())
		{
			return false;
		}
		if (firstUseAnimationStarted)
		{
			OnFirstUseAnimationStop();
		}
		return propulsionCannon.OnShoot();
	}

	public override void OnToolReloadBeginAnim(GUIHand guiHand)
	{
		base.OnToolReloadBeginAnim(guiHand);
		propulsionCannon.ReleaseGrabbedObject();
	}

	protected override void OnFirstUseAnimationStop()
	{
		base.OnFirstUseAnimationStop();
		propulsionCannon.StopFirstUseFxAndSound();
	}

	public void OnEquip(GameObject sender, string slot)
	{
		if (base.isDrawn && firstUse)
		{
			propulsionCannon.PlayFirstUseFxAndSound();
			animator.SetBool("using_tool_first", value: true);
		}
	}

	public void OnUnequip(GameObject sender, string slot)
	{
		if (firstUse)
		{
			propulsionCannon.StopFirstUseFxAndSound();
			animator.SetBool("using_tool_first", value: false);
		}
	}

	public void UpdateEquipped(GameObject sender, string slot)
	{
		if (usingPlayer != null && !usingPlayer.IsInSub())
		{
			propulsionCannon.usingCannon = GameInput.GetButtonHeld(GameInput.Button.RightHand);
			propulsionCannon.UpdateActive();
			SafeAnimator.SetBool(Player.main.armsController.GetComponent<Animator>(), "cangrab_propulsioncannon", propulsionCannon.canGrab || propulsionCannon.grabbedObject != null);
		}
	}
}
