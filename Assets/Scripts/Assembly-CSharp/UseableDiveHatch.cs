using UnityEngine;

[SkipProtoContractCheck]
public class UseableDiveHatch : HandTarget, IHandTarget, IObstacle
{
	[AssertNotNull]
	public GameObject outsideExit;

	[AssertNotNull]
	public GameObject insideSpawn;

	public GameObject ignoreObject;

	private const float obstacleRadius = 0.2f;

	[Range(1f, 10f)]
	public float exitSearchDistance = 2f;

	[AssertLocalization]
	public string enterCustomText;

	[AssertLocalization]
	public string exitCustomText;

	public string enterCustomGoalText;

	public bool customGoalWithLootOnly;

	public bool enterOnly;

	public bool isForEscapePod;

	public bool isForWaterPark;

	public bool secureInventory = true;

	public PlayerCinematicController enterCinematicController;

	public PlayerCinematicController exitCinematicController;

	private int quickSlot = -1;

	public Vector3 GetDiverSpawnPosition()
	{
		if (!IsInside() || enterOnly)
		{
			return insideSpawn.transform.position;
		}
		return outsideExit.transform.position;
	}

	public Vector3 GetInsideSpawnPosition()
	{
		return insideSpawn.transform.position;
	}

	private bool IsInside()
	{
		if (Player.main.IsInsideWalkable())
		{
			return Player.main.currentWaterPark == null;
		}
		return false;
	}

	private bool GetExitPosition(out Vector3 exitPosition)
	{
		exitPosition = outsideExit.transform.position;
		if (CanExit(exitPosition) || isForWaterPark)
		{
			return true;
		}
		for (int i = 0; i < 10; i++)
		{
			Vector3 position = Random.insideUnitSphere * exitSearchDistance;
			position.z = Mathf.Abs(position.z);
			exitPosition = outsideExit.transform.TransformPoint(position);
			if (CanExit(exitPosition))
			{
				return true;
			}
		}
		Debug.LogWarningFormat(this, "failed to find exit position for hatch {0}", base.name);
		return false;
	}

	private bool CanExit(Vector3 exitPosition)
	{
		if (GetComponentInParent<Base>() != null)
		{
			return Player.main.playerController.WayToPositionClear(exitPosition, insideSpawn.transform.position, PlayerController.ControllerSize.Swim, IgnoreObject);
		}
		GameObject ignoreObj = ((ignoreObject == null) ? base.gameObject : ignoreObject);
		return Player.main.playerController.WayToPositionClear(exitPosition, PlayerController.ControllerSize.Swim, ignoreObj, ignoreLiving: true, insideSpawn.transform.position);
	}

	private bool IgnoreObject(GameObject go)
	{
		if (go.GetComponentInParent<Living>() != null)
		{
			return true;
		}
		BaseCell componentInParent = GetComponentInParent<BaseCell>();
		if ((bool)componentInParent && componentInParent == go.GetComponentInParent<BaseCell>())
		{
			return true;
		}
		Constructable componentInParent2 = go.GetComponentInParent<Constructable>();
		if (componentInParent2 != null && GetComponentInParent<Base>() == componentInParent2.GetComponentInParent<Base>())
		{
			return true;
		}
		return false;
	}

	private bool StartCinematicMode(PlayerCinematicController cinematicController, Player player)
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
		cinematicController.informGameObject = base.gameObject;
		cinematicController.StartCinematicMode(player);
		return true;
	}

	public void OnPlayerCinematicModeEnd(PlayerCinematicController cinematicController)
	{
		if (cinematicController == enterCinematicController)
		{
			EnterExitHelper.Enter(base.gameObject, cinematicController.GetPlayer(), isForEscapePod);
		}
		else if (cinematicController == exitCinematicController)
		{
			EnterExitHelper.Exit(base.transform, cinematicController.GetPlayer(), isForEscapePod, isForWaterPark);
		}
		if (quickSlot >= 0)
		{
			Inventory.main.quickSlots.Select(quickSlot);
			quickSlot = -1;
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		if (base.enabled)
		{
			string text = (IsInside() ? exitCustomText : enterCustomText);
			if (enterOnly)
			{
				text = enterCustomText;
			}
			HandReticle.main.SetText(HandReticle.TextType.Hand, text, translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (!base.enabled)
		{
			return;
		}
		Player component = hand.gameObject.GetComponent<Player>();
		if (IsInside() && !enterOnly)
		{
			Vector3 exitPosition;
			if ((bool)exitCinematicController && CanExit(outsideExit.transform.position))
			{
				StartCinematicMode(exitCinematicController, component);
			}
			else if (GetExitPosition(out exitPosition))
			{
				component.SetPosition(exitPosition);
				EnterExitHelper.Exit(base.transform, component, isForEscapePod, isForWaterPark);
				if ((bool)exitCinematicController && (bool)exitCinematicController.sound)
				{
					Utils.PlayFMODAsset(exitCinematicController.sound, base.transform);
				}
			}
		}
		else
		{
			Vector3 exitPosition2;
			if ((bool)enterCinematicController && CanExit(outsideExit.transform.position))
			{
				StartCinematicMode(enterCinematicController, component);
			}
			else if (GetExitPosition(out exitPosition2))
			{
				Vector3 diverSpawnPosition = GetDiverSpawnPosition();
				component.SetPosition(diverSpawnPosition);
				EnterExitHelper.Enter(base.gameObject, component, isForEscapePod);
				if ((bool)enterCinematicController && (bool)enterCinematicController.sound)
				{
					Utils.PlayFMODAsset(enterCinematicController.sound, base.transform);
				}
			}
			if (enterCustomGoalText != "" && (!customGoalWithLootOnly || Inventory.main.GetTotalItemCount() > 0))
			{
				Debug.Log("OnCustomGoalEvent(" + enterCustomText);
				GoalManager.main.OnCustomGoalEvent(enterCustomGoalText);
			}
		}
		if (secureInventory)
		{
			Inventory.Get().SecureItems(verbose: true);
		}
	}

	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	public bool CanDeconstruct(out string r)
	{
		r = null;
		return true;
	}
}
