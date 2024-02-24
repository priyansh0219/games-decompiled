using UWE;
using UnityEngine;
using UnityEngine.EventSystems;

[SkipProtoContractCheck]
public abstract class CinematicModeTriggerBase : HandTarget, IHandTarget
{
	public enum TriggerType
	{
		HandTarget = 0,
		Volume = 1
	}

	public enum VolumeTriggerType
	{
		OnEnter = 0,
		OnExit = 1
	}

	public TriggerType triggerType;

	public VolumeTriggerType volumeTriggerType;

	public bool showIconOnHandHover = true;

	[AssertNotNull]
	public PlayerCinematicController cinematicController;

	private float timeUsingStarted;

	protected Player player;

	public bool secureInventory;

	public bool restoreActiveQuickSlot;

	protected int quickSlot = -1;

	[AssertNotNull]
	public CinematicModeEvent onCinematicStart;

	[AssertNotNull]
	public CinematicModeEvent onCinematicEnd;

	public bool debug;

	protected virtual void OnStartCinematicMode()
	{
	}

	protected virtual void OnTriggerEnter(Collider collider)
	{
		if (triggerType != TriggerType.Volume || volumeTriggerType != 0)
		{
			return;
		}
		Player componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Player>(collider.gameObject);
		if (componentInHierarchy != null)
		{
			if (debug)
			{
				Debug.Log(base.gameObject.name + ".OnTriggerEnter");
			}
			StartCinematicMode(componentInHierarchy);
		}
	}

	protected virtual void OnTriggerExit(Collider collider)
	{
		if (triggerType != TriggerType.Volume || volumeTriggerType != VolumeTriggerType.OnExit)
		{
			return;
		}
		Player componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Player>(collider.gameObject);
		if (componentInHierarchy != null)
		{
			if (debug)
			{
				Debug.Log(base.gameObject.name + ".OnTriggerExit");
			}
			StartCinematicMode(componentInHierarchy);
		}
	}

	protected bool StartCinematicMode(Player player)
	{
		if (PlayerCinematicController.cinematicModeCount > 0)
		{
			return false;
		}
		quickSlot = Inventory.main.quickSlots.activeSlot;
		if (!Inventory.main.ReturnHeld())
		{
			return false;
		}
		if (debug)
		{
			Debug.Log(base.gameObject.name + ".StartCinematicMode");
		}
		timeUsingStarted = Time.time;
		cinematicController.StartCinematicMode(player);
		OnStartCinematicMode();
		if (secureInventory)
		{
			Inventory.main.SecureItems(verbose: true);
		}
		if (onCinematicStart != null)
		{
			CinematicModeEventData cinematicModeEventData = new CinematicModeEventData(EventSystem.current);
			cinematicModeEventData.player = player;
			onCinematicStart.Invoke(cinematicModeEventData);
		}
		cinematicController.informGameObject = base.gameObject;
		return true;
	}

	public void OnPlayerCinematicModeEnd(PlayerCinematicController cinematicController)
	{
		if ((bool)cinematicController)
		{
			cinematicController.informGameObject = null;
			if (onCinematicEnd != null)
			{
				CinematicModeEventData cinematicModeEventData = new CinematicModeEventData(EventSystem.current);
				cinematicModeEventData.player = Player.main;
				cinematicModeEventData.cinematicController = cinematicController;
				onCinematicEnd.Invoke(cinematicModeEventData);
			}
			if (restoreActiveQuickSlot)
			{
				Inventory.main.quickSlots.Select(quickSlot);
			}
			if (debug)
			{
				Debug.Log(base.gameObject.name + ".OnPlayerCinematicModeEnd");
			}
		}
	}

	public abstract void OnHandHover(GUIHand hand);

	public virtual void OnHandClick(GUIHand hand)
	{
		if (triggerType != 0)
		{
			return;
		}
		base.gameObject.SendMessage("HandDown", null, SendMessageOptions.DontRequireReceiver);
		if (base.isValidHandTarget)
		{
			if (debug)
			{
				Debug.Log(base.gameObject.name + ".OnHandClick");
			}
			StartCinematicMode(hand.GetComponent<Player>());
		}
	}
}
