using System;
using System.Collections.Generic;
using Gendarme;
using UWE;
using UnityEngine;

public class GUIHand : MonoBehaviour
{
	[Flags]
	private enum InputState : uint
	{
		None = 0u,
		Down = 1u,
		Held = 2u,
		Up = 4u
	}

	public enum Mode
	{
		Free = 0,
		Tool = 1,
		Placing = 2
	}

	public enum GrabMode
	{
		None = 0,
		World = 1,
		Screen = 2
	}

	public const float kUseDistance = 2f;

	[AssertNotNull]
	public Player player;

	private GameObject activeTarget;

	private float activeHitDistance;

	private bool suppressTooltip;

	private bool usedToolThisFrame;

	private bool usedAltAttackThisFrame;

	private GrabMode grabMode;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.UpdateGUIHand;

	private int cachedTextEnergyScalar = -1;

	private string cachedEnergyHudText = "";

	private Dictionary<GameInput.Button, InputState> inputState = new Dictionary<GameInput.Button, InputState>();

	private float timeOfLastToolUseAnim = -1f;

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateGUIHand, OnUpdate);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdateGUIHand, OnUpdate);
	}

	private void UpdateActiveTarget()
	{
		PlayerTool tool = GetTool();
		if (tool != null && tool.GetComponent<PropulsionCannon>() != null && tool.GetComponent<PropulsionCannon>().IsGrabbingObject())
		{
			activeTarget = tool.GetComponent<PropulsionCannon>().GetInteractableGrabbedObject();
			suppressTooltip = true;
		}
		else if (tool != null && tool.DoesOverrideHand())
		{
			activeTarget = null;
			activeHitDistance = 0f;
		}
		else
		{
			DebugTargetConsoleCommand.RecordNext();
			if (Targeting.GetTarget(Player.main.gameObject, 2f, out activeTarget, out activeHitDistance))
			{
				IHandTarget handTarget = null;
				Transform parent = activeTarget.transform;
				while (parent != null)
				{
					handTarget = parent.GetComponent<IHandTarget>();
					if (handTarget == null)
					{
						parent = parent.parent;
						continue;
					}
					activeTarget = parent.gameObject;
					break;
				}
				if (handTarget == null)
				{
					switch (TechData.GetHarvestType(CraftData.GetTechType(activeTarget)))
					{
					case HarvestType.Pick:
						if (Utils.FindAncestorWithComponent<Pickupable>(activeTarget) == null)
						{
							LargeWorldEntity largeWorldEntity = Utils.FindAncestorWithComponent<LargeWorldEntity>(activeTarget);
							largeWorldEntity.gameObject.AddComponent<Pickupable>();
							largeWorldEntity.gameObject.AddComponent<WorldForces>().useRigidbody = largeWorldEntity.GetComponent<Rigidbody>();
						}
						break;
					case HarvestType.None:
						activeTarget = null;
						break;
					}
				}
			}
			else
			{
				activeTarget = null;
				activeHitDistance = 0f;
			}
		}
		if (uGUI.isIntro || IntroLifepodDirector.IsActive)
		{
			activeTarget = FilterIntroTarget(activeTarget);
		}
	}

	public void DestroyActiveTarget()
	{
		if (activeTarget != null)
		{
			UnityEngine.Object.Destroy(activeTarget);
		}
	}

	private void UpdateInput(GameInput.Button button)
	{
		InputState inputState = InputState.None;
		if (GameInput.GetButtonDown(button))
		{
			inputState |= InputState.Down;
		}
		if (GameInput.GetButtonHeld(button))
		{
			inputState |= InputState.Held;
		}
		if (GameInput.GetButtonUp(button))
		{
			inputState |= InputState.Up;
		}
		this.inputState[button] = inputState;
	}

	private void UseInput(GameInput.Button button, InputState mask)
	{
		if (inputState.TryGetValue(button, out var value))
		{
			inputState[button] = value & ~mask;
		}
	}

	private bool GetInput(GameInput.Button button, InputState flag)
	{
		if (!inputState.TryGetValue(button, out var value))
		{
			return false;
		}
		return (value & flag) != 0;
	}

	private bool IsPDAInUse()
	{
		if (player != null && player.GetPDA() != null)
		{
			return player.GetPDA().isInUse;
		}
		return false;
	}

	[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidComplexMethodsRule")]
	private void OnUpdate()
	{
		usedToolThisFrame = false;
		usedAltAttackThisFrame = false;
		suppressTooltip = false;
		GameInput.Button button = GameInput.Button.LeftHand;
		GameInput.Button button2 = GameInput.Button.RightHand;
		GameInput.Button button3 = GameInput.Button.Reload;
		GameInput.Button button4 = GameInput.Button.Exit;
		GameInput.Button button5 = GameInput.Button.AltTool;
		GameInput.Button button6 = GameInput.Button.AutoMove;
		GameInput.Button button7 = GameInput.Button.PDA;
		UpdateInput(button);
		UpdateInput(button2);
		UpdateInput(button3);
		UpdateInput(button4);
		UpdateInput(button5);
		UpdateInput(button6);
		UpdateInput(button7);
		if (player.IsFreeToInteract() && (AvatarInputHandler.main.IsEnabled() || Builder.inputHandlerActive))
		{
			string text = string.Empty;
			InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
			Pickupable pickupable = heldItem?.item;
			PlayerTool playerTool = ((pickupable != null) ? pickupable.GetComponent<PlayerTool>() : null);
			bool flag = playerTool != null && playerTool is DropTool;
			EnergyMixin energyMixin = null;
			if (playerTool != null)
			{
				text = playerTool.GetCustomUseText();
				energyMixin = playerTool.GetComponent<EnergyMixin>();
			}
			ItemAction itemAction = ItemAction.None;
			if ((playerTool == null || flag) && heldItem != null)
			{
				ItemAction allItemActions = Inventory.main.GetAllItemActions(heldItem);
				if ((allItemActions & ItemAction.Eat) != 0)
				{
					itemAction = ItemAction.Eat;
				}
				else if ((allItemActions & ItemAction.Use) != 0)
				{
					itemAction = ItemAction.Use;
				}
				if (itemAction == ItemAction.Eat)
				{
					Plantable component = pickupable.GetComponent<Plantable>();
					LiveMixin component2 = pickupable.GetComponent<LiveMixin>();
					if (component == null && component2 != null)
					{
						itemAction = ItemAction.None;
					}
				}
				if (itemAction == ItemAction.None && (allItemActions & ItemAction.Drop) != 0)
				{
					itemAction = ItemAction.Drop;
				}
				if (itemAction != 0)
				{
					HandReticle.main.SetText(HandReticle.TextType.Use, GetActionString(itemAction, pickupable), translate: true, GameInput.Button.RightHand);
				}
			}
			if (energyMixin != null && energyMixin.allowBatteryReplacement)
			{
				int num = Mathf.FloorToInt(energyMixin.GetEnergyScalar() * 100f);
				if (cachedTextEnergyScalar != num)
				{
					if (num <= 0)
					{
						cachedEnergyHudText = LanguageCache.GetButtonFormat("ExchangePowerSource", GameInput.Button.Reload);
					}
					else
					{
						cachedEnergyHudText = Language.main.GetFormat("PowerPercent", energyMixin.GetEnergyScalar());
					}
					cachedTextEnergyScalar = num;
				}
				HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
				HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, cachedEnergyHudText);
			}
			else if (!string.IsNullOrEmpty(text))
			{
				HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
			}
			if (AvatarInputHandler.main.IsEnabled() && !IsPDAInUse())
			{
				if (grabMode == GrabMode.None)
				{
					UpdateActiveTarget();
				}
				HandReticle.main.SetTargetDistance(activeHitDistance);
				if (activeTarget != null && !suppressTooltip)
				{
					TechType techType = CraftData.GetTechType(activeTarget);
					if (techType != 0)
					{
						HandReticle.main.SetText(HandReticle.TextType.Hand, techType.AsString(), translate: true);
					}
					Send(activeTarget, HandTargetEventType.Hover, this);
				}
				if (Inventory.main.container.Contains(TechType.Scanner))
				{
					PDAScanner.UpdateTarget(8f, GetInput(button5, InputState.Down | InputState.Held));
					PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;
					if (scanTarget.isValid && PDAScanner.CanScan(scanTarget) == PDAScanner.Result.Scan && !PDAScanner.scanTarget.isPlayer)
					{
						uGUI_ScannerIcon.main.Show();
					}
				}
				if (playerTool != null && (!flag || itemAction == ItemAction.Drop || itemAction == ItemAction.None))
				{
					if (GetInput(button2, InputState.Down))
					{
						if (playerTool.OnRightHandDown())
						{
							UseInput(button2, InputState.Down | InputState.Held | InputState.Up);
							usedToolThisFrame = true;
							playerTool.OnToolActionStart();
						}
					}
					else if (GetInput(button2, InputState.Held))
					{
						if (playerTool.OnRightHandHeld())
						{
							UseInput(button2, InputState.Down | InputState.Held);
						}
					}
					else if (GetInput(button2, InputState.Up) && playerTool.OnRightHandUp())
					{
						UseInput(button2, InputState.Up);
					}
					if (GetInput(button, InputState.Down))
					{
						if (playerTool.OnLeftHandDown())
						{
							UseInput(button, InputState.Down | InputState.Held | InputState.Up);
							playerTool.OnToolActionStart();
						}
					}
					else if (GetInput(button, InputState.Held))
					{
						if (playerTool.OnLeftHandHeld())
						{
							UseInput(button, InputState.Down | InputState.Held);
						}
					}
					else if (GetInput(button, InputState.Up) && playerTool.OnLeftHandUp())
					{
						UseInput(button, InputState.Up);
					}
					if (GetInput(button5, InputState.Down))
					{
						if (playerTool.OnAltDown())
						{
							UseInput(button5, InputState.Down | InputState.Held | InputState.Up);
							usedAltAttackThisFrame = true;
							playerTool.OnToolActionStart();
						}
					}
					else if (GetInput(button5, InputState.Held))
					{
						if (playerTool.OnAltHeld())
						{
							UseInput(button5, InputState.Down | InputState.Held);
						}
					}
					else if (GetInput(button5, InputState.Up) && playerTool.OnAltUp())
					{
						UseInput(button5, InputState.Up);
					}
					if (GetInput(button3, InputState.Down) && playerTool.OnReloadDown())
					{
						UseInput(button3, InputState.Down);
					}
					if (GetInput(button4, InputState.Down) && playerTool.OnExitDown())
					{
						UseInput(button4, InputState.Down);
					}
				}
				if (itemAction != 0 && GetInput(button2, InputState.Down))
				{
					if (itemAction == ItemAction.Drop)
					{
						UseInput(button2, InputState.Down | InputState.Held);
						Inventory.main.DropHeldItem(applyForce: true);
					}
					else
					{
						UseInput(button2, InputState.Down | InputState.Held);
						Inventory.main.ExecuteItemAction(itemAction, heldItem);
					}
				}
				if (player.IsFreeToInteract() && !usedToolThisFrame && activeTarget != null && GetInput(button, InputState.Down))
				{
					UseInput(button, InputState.Down | InputState.Held);
					Send(activeTarget, HandTargetEventType.Click, this);
				}
			}
		}
		if (AvatarInputHandler.main.IsEnabled() && GetInput(button6, InputState.Down))
		{
			UseInput(button6, InputState.Down | InputState.Held | InputState.Up);
			GameInput.AutoMove = !GameInput.AutoMove;
		}
		if (AvatarInputHandler.main.IsEnabled() && !uGUI.isIntro && !IntroLifepodDirector.IsActive && GetInput(button7, InputState.Down))
		{
			UseInput(button7, InputState.Down | InputState.Held | InputState.Up);
			player.GetPDA().Open();
		}
	}

	private static string GetActionString(ItemAction action, Pickupable pickupable)
	{
		TechType techType = pickupable.GetTechType();
		LiveMixin component = pickupable.GetComponent<LiveMixin>();
		Eatable component2 = pickupable.GetComponent<Eatable>();
		Plantable component3 = pickupable.GetComponent<Plantable>();
		string result = string.Empty;
		switch (action)
		{
		case ItemAction.Use:
			result = "ItemActionUse";
			break;
		case ItemAction.Eat:
			if (component2 != null)
			{
				result = ((!(component != null) && !(component3 != null) && !(component2.foodValue > 0f)) ? ((!(component2.waterValue > 0f) && techType != TechType.Coffee) ? "ItemActionEat" : "ItemActionDrink") : "ItemActionEat");
			}
			break;
		case ItemAction.Drop:
			result = ((component3 == null && component != null && component.IsAlive()) ? "ItemActionRelease" : "ItemActionDrop");
			break;
		default:
			result = $"ItemAction{action.ToString()}";
			break;
		}
		return result;
	}

	public static void Send(GameObject target, HandTargetEventType e, GUIHand hand)
	{
		if (target == null || !target.activeInHierarchy || e == HandTargetEventType.None)
		{
			return;
		}
		IHandTarget component = target.GetComponent<IHandTarget>();
		if (component == null)
		{
			return;
		}
		try
		{
			switch (e)
			{
			case HandTargetEventType.Hover:
				component.OnHandHover(hand);
				break;
			case HandTargetEventType.Click:
				component.OnHandClick(hand);
				break;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private GameObject FilterIntroTarget(GameObject target)
	{
		if (target == null)
		{
			return null;
		}
		if (target.GetComponent<UnusableInLifepodIntro>() != null)
		{
			return null;
		}
		if (target.GetComponent<Fabricator>() != null)
		{
			return null;
		}
		if (target.GetComponent<Radio>() != null)
		{
			return null;
		}
		if (target.GetComponent<MedicalCabinet>() != null)
		{
			return null;
		}
		return target;
	}

	public PlayerTool GetTool()
	{
		return Inventory.main.GetHeldTool();
	}

	public T GetToolOfType<T>() where T : Behaviour
	{
		T result = null;
		PlayerTool heldTool = Inventory.main.GetHeldTool();
		if (heldTool != null)
		{
			return heldTool.gameObject.GetComponent<T>();
		}
		return result;
	}

	public bool GetUsingTool()
	{
		if (AvatarInputHandler.main.IsEnabled() && GetTool() != null)
		{
			if (!usedToolThisFrame)
			{
				return GetTool().GetUsedToolThisFrame();
			}
			return true;
		}
		return false;
	}

	public bool GetAltAttacking()
	{
		if (AvatarInputHandler.main.IsEnabled() && GetTool() != null)
		{
			return GetTool().GetAltUsedToolThisFrame();
		}
		return false;
	}

	public void OnToolBleederHitAnim()
	{
		if (GetTool() != null)
		{
			GetTool().OnToolBleederHitAnim(this);
		}
	}

	public void OnToolUseAnim()
	{
		if (timeOfLastToolUseAnim == -1f || Time.time > timeOfLastToolUseAnim + 0.5f)
		{
			if (GetTool() != null)
			{
				GetTool().OnToolUseAnim(this);
			}
			timeOfLastToolUseAnim = Time.time;
		}
	}

	public void OnToolAnimHolster()
	{
		PlayerTool tool = GetTool();
		if (tool != null)
		{
			tool.OnToolAnimHolster();
		}
	}

	public void OnToolAnimDraw()
	{
		PlayerTool tool = GetTool();
		if (tool != null)
		{
			tool.OnToolAnimDraw();
		}
	}

	public void BashHit()
	{
		if (GetTool() != null)
		{
			GetTool().SendMessage("BashHit", this, SendMessageOptions.DontRequireReceiver);
		}
		if (activeTarget != null)
		{
			activeTarget.SendMessage("BashHit", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void FireExSpray()
	{
		if (GetTool() != null)
		{
			GetTool().SendMessage("FireExSpray", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	public Mode GetMode()
	{
		if (GetTool() != null)
		{
			return Mode.Tool;
		}
		return Mode.Free;
	}

	public bool IsTool()
	{
		return GetMode() == Mode.Tool;
	}

	public bool IsFreeToInteract()
	{
		if (GetMode() != 0)
		{
			return GetMode() == Mode.Tool;
		}
		return true;
	}

	public void SetGrabMode(GrabMode val)
	{
		this.grabMode = val;
		GrabMode grabMode = this.grabMode;
		if ((uint)grabMode > 1u && grabMode == GrabMode.Screen)
		{
			GetComponent<MouseLook>().SetEnabled(val: false);
		}
		else
		{
			GetComponent<MouseLook>().SetEnabled(val: true);
		}
	}

	public Camera GetPlayerCamera()
	{
		return MainCamera.camera;
	}

	public Vector3 GetGrabbingHandPosition()
	{
		Camera playerCamera = GetPlayerCamera();
		return playerCamera.transform.position + playerCamera.transform.forward * activeHitDistance;
	}

	public Vector3 GetActiveHitPosition()
	{
		return GetGrabbingHandPosition();
	}

	public Facing GetFacingInSub()
	{
		Vector3 vector = player.GetCurrentSub().transform.InverseTransformDirection(player.transform.forward);
		float x = vector.x;
		float z = vector.z;
		if (Mathf.Abs(x) > Mathf.Abs(z))
		{
			if (!(x > 0f))
			{
				return Facing.West;
			}
			return Facing.East;
		}
		if (!(z > 0f))
		{
			return Facing.South;
		}
		return Facing.North;
	}

	public Vector3 GetPlayerEyePos()
	{
		return GetPlayerCamera().transform.position;
	}

	public GameObject GetActiveTarget()
	{
		return activeTarget;
	}
}
