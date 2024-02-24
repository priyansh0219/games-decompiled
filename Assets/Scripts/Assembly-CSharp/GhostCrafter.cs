using ProtoBuf;
using UnityEngine;

[ProtoInclude(5100, typeof(Fabricator))]
[ProtoInclude(5200, typeof(Workbench))]
[ProtoContract]
public class GhostCrafter : Crafter, IHandTarget, IConstructable, IObstacle
{
	protected const float craftingTimeMessageThreshold = 20f;

	[AssertLocalization]
	protected const string unpoweredSecondaryText = "unpowered";

	[AssertLocalization(2)]
	protected const string craftingBeginFormat = "CraftingBegin";

	[AssertLocalization(1)]
	protected const string craftingEndFormat = "CraftingEnd";

	public CrafterGhostModel ghost;

	public CraftTree.Type craftTree = CraftTree.Type.Fabricator;

	public float closeDistance = 3f;

	public string handOverText = "UseFabricator";

	public bool pickupOutOfRange;

	public bool isDeconstructionObstacle = true;

	protected float spawnAnimationDelay = 1f;

	protected float spawnAnimationDuration = 1.7f;

	protected PowerRelay powerRelay;

	private bool _opened;

	private bool _progressChanged;

	private bool _itemChanged;

	private float _progressDelayScalar;

	private bool opened
	{
		get
		{
			return _opened;
		}
		set
		{
			if (_opened != value)
			{
				_opened = value;
				OnOpenedChanged(_opened);
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		powerRelay = base.gameObject.GetComponentInParent<PowerRelay>();
	}

	protected virtual void LateUpdate()
	{
		if (opened)
		{
			if (!HasEnoughPower() || (!FPSInputModule.current.lockMovement && !PlayerIsInRange(closeDistance)))
			{
				Close();
			}
		}
		else if (HasCraftedItem())
		{
			opened = true;
		}
		if (_itemChanged)
		{
			_itemChanged = false;
			if (ghost != null)
			{
				ghost.UpdateModel((base.logic != null) ? base.logic.currentTechType : TechType.None);
			}
		}
		if (_progressChanged)
		{
			_progressChanged = false;
			if (ghost != null && base.logic != null)
			{
				ghost.UpdateProgress(Mathf.Clamp01((base.logic.progress - _progressDelayScalar) / (1f - _progressDelayScalar)));
			}
		}
	}

	protected override void Initialize()
	{
		if (!_initialized)
		{
			base.Initialize();
			if (base.logic != null && ghost != null)
			{
				ghost.UpdateModel(base.logic.currentTechType);
				ghost.UpdateProgress(base.logic.progress);
			}
		}
	}

	protected override void Deinitialize()
	{
		if (_initialized)
		{
			base.Deinitialize();
			if (base.logic != null && ghost != null)
			{
				ghost.UpdateModel(TechType.None);
				ghost.UpdateProgress(0f);
			}
		}
	}

	protected override void Craft(TechType techType, float duration)
	{
		if (CrafterLogic.ConsumeEnergy(powerRelay, 5f) && CrafterLogic.ConsumeResources(techType))
		{
			duration = ((!TechData.GetCraftTime(techType, out duration)) ? (spawnAnimationDelay + spawnAnimationDuration) : Mathf.Max(spawnAnimationDelay + spawnAnimationDuration, duration));
			base.Craft(techType, duration);
		}
	}

	protected override void OnCraftingBegin(TechType techType, float duration)
	{
		if (duration > 20f)
		{
			ErrorMessage.AddMessage(Language.main.GetFormat("CraftingBegin", Language.main.Get(techType.AsString()), duration));
		}
		_progressDelayScalar = Mathf.Clamp(spawnAnimationDelay / duration, 0f, 0.9f);
		base.OnCraftingBegin(techType, duration);
	}

	protected override void OnCraftingEnd()
	{
		if (!(base.logic == null))
		{
			if (PlayerIsInRange(closeDistance) || pickupOutOfRange)
			{
				base.logic.TryPickup();
			}
			else
			{
				ErrorMessage.AddMessage(Language.main.GetFormat("CraftingEnd", Language.main.Get(base.logic.craftingTechType.AsString())));
			}
		}
	}

	protected override void OnCraftedItemPickup(GameObject item)
	{
		PlayerTool component = item.GetComponent<PlayerTool>();
		if (component != null && component.ShouldPlayFirstUseAnimation() && Inventory.main.quickSlots.SelectSlotByGameObject(item))
		{
			Close();
		}
	}

	protected override void OnItemChanged(TechType techType)
	{
		_itemChanged = true;
		_progressChanged = true;
	}

	protected override void OnProgress(float progress)
	{
		_progressChanged = true;
	}

	protected virtual void OnOpenedChanged(bool opened)
	{
		if (!opened)
		{
			uGUI.main.craftingMenu.Close(this);
		}
	}

	private void Close()
	{
		if (HasCraftedItem())
		{
			uGUI.main.craftingMenu.Close(this);
		}
		else
		{
			opened = false;
		}
	}

	private bool PlayerIsInRange(float distance)
	{
		return (Player.main.transform.position - base.transform.position).sqrMagnitude < distance * distance;
	}

	private bool HasEnoughPower()
	{
		if (GameModeUtils.RequiresPower())
		{
			if (powerRelay != null)
			{
				return powerRelay.GetPower() >= 5f;
			}
			return false;
		}
		return true;
	}

	public void OnHandHover(GUIHand hand)
	{
		if (!base.enabled || base.logic == null)
		{
			return;
		}
		string text = handOverText;
		string text2 = string.Empty;
		if (base.logic.inProgress)
		{
			text = base.logic.craftingTechType.AsString();
			HandReticle.main.SetProgress(base.logic.progress);
			HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1.5f);
		}
		else if (HasCraftedItem())
		{
			text = base.logic.currentTechType.AsString();
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
		else
		{
			if (!HasEnoughPower())
			{
				text2 = "unpowered";
			}
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
		HandReticle.main.SetText(HandReticle.TextType.Hand, text, translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, translate: true);
	}

	public void OnHandClick(GUIHand hand)
	{
		if (base.enabled && !(base.logic == null) && !base.logic.inProgress)
		{
			if (HasCraftedItem())
			{
				base.logic.TryPickup();
			}
			else if (HasEnoughPower() && base.isValidHandTarget)
			{
				opened = true;
				uGUI.main.craftingMenu.Open(craftTree, this);
			}
		}
	}

	public bool IsDeconstructionObstacle()
	{
		return isDeconstructionObstacle;
	}

	public bool CanDeconstruct(out string reason)
	{
		reason = null;
		if (base.logic == null || !base.logic.inProgress)
		{
			return !HasCraftedItem();
		}
		return false;
	}

	public void OnConstructedChanged(bool constructed)
	{
		if (!constructed)
		{
			Close();
		}
	}
}
