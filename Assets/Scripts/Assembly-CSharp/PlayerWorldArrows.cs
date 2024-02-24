using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PlayerWorldArrows : MonoBehaviour
{
	public enum GameCondition
	{
		None = 0,
		LowAir = 1,
		BoardCyclops = 2,
		HasKnife = 3,
		Hungry = 4,
		HasInventory = 5,
		DockSeamoth = 6,
		BleederAttached = 7
	}

	public delegate bool GameConditionDelegate(ref Transform outputTransform);

	[Serializable]
	public class PlayerWorldArrow
	{
		public bool inInventory;

		public bool underwaterOnly;

		public TechType objectTechType;

		public GameCondition gameCondition;

		public Vector3 arrowOffset = new Vector3(0f, 0f, 0f);

		public bool offsetIsLocal;

		public bool pointDown = true;

		public string arrowText;

		public string customGoal;

		public GameConditionDelegate gameConditionDelegate;

		public float priority;

		public GameModeOption notInGameMode;

		public float localScale = 1f;

		public float timeNoticeablyDisplayed;

		public GameInput.Button? button;

		[NonSerialized]
		public EcoRegion.TargetFilter isTargetValidFilter;

		public bool IsTargetValid(IEcoTarget target)
		{
			return CraftData.GetTechType(target.GetGameObject()) == objectTechType;
		}
	}

	public static PlayerWorldArrows main;

	[NonSerialized]
	public bool debug;

	private float arrowUpdateTime = 1f;

	private float kMaxObjectDistance = 25f;

	public List<PlayerWorldArrow> worldArrows = new List<PlayerWorldArrow>();

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public readonly HashSet<string> completedCustomGoals = new HashSet<string>();

	private readonly HashSet<PlayerWorldArrow> ignoreArrows = new HashSet<PlayerWorldArrow>();

	private void Awake()
	{
		main = this;
	}

	private void CreateWorldArrow(bool inInventory, bool underwaterOnly, TechType objectTechType, string arrowText, GameInput.Button? button, string customGoal, float priority, Vector3 offset, bool offsetIsLocal = false, bool pointDown = false, GameModeOption notInGameMode = GameModeOption.None, GameCondition gameCondition = GameCondition.None, float localScale = 1f)
	{
		PlayerWorldArrow playerWorldArrow = new PlayerWorldArrow();
		worldArrows.Add(playerWorldArrow);
		playerWorldArrow.isTargetValidFilter = playerWorldArrow.IsTargetValid;
		playerWorldArrow.inInventory = inInventory;
		playerWorldArrow.underwaterOnly = underwaterOnly;
		playerWorldArrow.objectTechType = objectTechType;
		playerWorldArrow.arrowText = arrowText;
		playerWorldArrow.customGoal = customGoal;
		playerWorldArrow.priority = priority;
		playerWorldArrow.arrowOffset = offset;
		playerWorldArrow.offsetIsLocal = offsetIsLocal;
		playerWorldArrow.pointDown = pointDown;
		playerWorldArrow.notInGameMode = notInGameMode;
		playerWorldArrow.gameCondition = gameCondition;
		playerWorldArrow.localScale = localScale;
		playerWorldArrow.button = button;
		if (!string.IsNullOrEmpty(playerWorldArrow.customGoal))
		{
			GoalManager.main.AddCustomGoal(playerWorldArrow.customGoal, playerWorldArrow.arrowText);
		}
		AddGameCondition(playerWorldArrow);
	}

	private void CreateWorldArrows()
	{
		CreateWorldArrow(inInventory: false, underwaterOnly: true, TechType.Creepvine, "WorldArrow_CutCreepvine", null, "Cut_Creepvine", 0f, new Vector3(0f, 10f, 0f), offsetIsLocal: false, pointDown: true, GameModeOption.NoCost, GameCondition.HasKnife);
		CreateWorldArrow(inInventory: true, underwaterOnly: true, TechType.Constructor, "WorldArrow_ReleaseConstructor", null, "Release_Constructor", 0f, new Vector3(0f, 0.5f, 0.1f), offsetIsLocal: true, pointDown: true, GameModeOption.InitialItems, GameCondition.None, 0.6f);
		CreateWorldArrow(inInventory: false, underwaterOnly: false, TechType.None, "WorldArrow_EatSomething", null, "Eat_Something", 0.3f, new Vector3(0f, 0f, 0f), offsetIsLocal: true, pointDown: true, GameModeOption.NoSurvival, GameCondition.Hungry, 0.7f);
		CreateWorldArrow(inInventory: false, underwaterOnly: true, TechType.ScrapMetal, "WorldArrow_PickupMetal", null, "Pickup_ScrapMetal", 0.1f, new Vector3(0f, 1f, 0f), offsetIsLocal: false, pointDown: true, GameModeOption.NoCost);
		CreateWorldArrow(inInventory: false, underwaterOnly: true, TechType.LimestoneChunk, "WorldArrow_BreakLimestone", null, "Break_Limestone", 0f, new Vector3(0f, 1f, 0f), offsetIsLocal: false, pointDown: true, GameModeOption.NoCost);
		CreateWorldArrow(inInventory: false, underwaterOnly: true, TechType.EscapePod, "WorldArrow_BoardEscapePod", null, "Board_EscapePod_Loot", 0f, new Vector3(1.178418f, -0.748204f, 0f), offsetIsLocal: false, pointDown: false, GameModeOption.NoCost | GameModeOption.InitialItems, GameCondition.HasInventory);
		CreateWorldArrow(inInventory: false, underwaterOnly: false, TechType.Fabricator, "WorldArrow_UseFabricator", null, "Use_Fabricator_Loot", 0.2f, new Vector3(0f, 0f, 0f), offsetIsLocal: false, pointDown: true, GameModeOption.NoCost | GameModeOption.InitialItems, GameCondition.HasInventory);
		CreateWorldArrow(inInventory: false, underwaterOnly: false, TechType.None, "WorldArrow_OpenPDA", GameInput.Button.PDA, "Open_PDA", 0f, new Vector3(0f, 0f, 0f), offsetIsLocal: true, pointDown: true, GameModeOption.NoCost | GameModeOption.InitialItems, GameCondition.HasInventory, 0.7f);
		CreateWorldArrow(inInventory: false, underwaterOnly: true, TechType.Peeper, "WorldArrow_CatchPeeper", null, "Pickup_Peeper", 0f, new Vector3(0f, 0.5f, 0f), offsetIsLocal: false, pointDown: true, GameModeOption.NoSurvival);
		CreateWorldArrow(inInventory: false, underwaterOnly: true, TechType.Seamoth, "WorldArrow_EnterSeamoth", null, "Enter_Seamoth", 0.3f, new Vector3(0f, -1.5f, 1f));
		CreateWorldArrow(inInventory: false, underwaterOnly: true, TechType.None, "WorldArrow_BoardCyclops", null, "Board_Cyclops", 0.5f, new Vector3(0f, 0f, 0f), offsetIsLocal: false, pointDown: false, GameModeOption.None, GameCondition.BoardCyclops, 1.25f);
		CreateWorldArrow(inInventory: false, underwaterOnly: true, TechType.None, "WorldArrow_DockSeamoth", null, "Dock_Seamoth", 0.4f, new Vector3(0f, 0f, 0f), offsetIsLocal: false, pointDown: false, GameModeOption.None, GameCondition.DockSeamoth);
		CreateWorldArrow(inInventory: false, underwaterOnly: true, TechType.Bleeder, "WorldArrow_AttackBleeder", GameInput.Button.RightHand, "AttackBleeder", 1.1f, new Vector3(-0.1f, -0.2f, -0.3f), offsetIsLocal: true, pointDown: true, GameModeOption.None, GameCondition.BleederAttached, 0.15f);
	}

	private void Start()
	{
		CreateWorldArrows();
		GoalManager.main.onCompleteGoalEvent.AddHandler(base.gameObject, OnCompleteGoal);
		InvokeRepeating("ArrowUpdate", arrowUpdateTime, arrowUpdateTime);
	}

	private void AddGameCondition(PlayerWorldArrow arrow)
	{
		if (arrow.gameCondition == GameCondition.LowAir)
		{
			arrow.gameConditionDelegate = delegate
			{
				return Player.main.GetOxygenAvailable() <= 10f;
			};
		}
		else if (arrow.gameCondition == GameCondition.BoardCyclops)
		{
			arrow.gameConditionDelegate = delegate(ref Transform refTransform)
			{
				bool result2 = false;
				if (Player.main.GetMode() == Player.Mode.Normal)
				{
					GameObject[] array2 = GameObject.FindGameObjectsWithTag("Submarine");
					foreach (GameObject obj2 in array2)
					{
						SubRoot subRoot2 = (SubRoot)HasComponentInParent(obj2, "SubRoot");
						if ((bool)subRoot2)
						{
							refTransform = subRoot2.entranceHatch.transform;
							result2 = true;
						}
					}
				}
				return result2;
			};
		}
		else if (arrow.gameCondition == GameCondition.HasKnife)
		{
			arrow.gameConditionDelegate = delegate
			{
				return Inventory.main.GetPickupCount(TechType.Knife) > 0;
			};
		}
		else if (arrow.gameCondition == GameCondition.Hungry)
		{
			arrow.gameConditionDelegate = delegate
			{
				Survival component = Player.main.gameObject.GetComponent<Survival>();
				return (bool)component && component.food < 30.000002f;
			};
		}
		else if (arrow.gameCondition == GameCondition.HasInventory)
		{
			arrow.gameConditionDelegate = delegate
			{
				return Inventory.main.GetTotalItemCount() >= 5;
			};
		}
		else if (arrow.gameCondition == GameCondition.DockSeamoth)
		{
			arrow.gameConditionDelegate = delegate(ref Transform outputTransform)
			{
				bool result = false;
				if (Player.main.GetMode() == Player.Mode.LockedPiloting)
				{
					GameObject[] array = GameObject.FindGameObjectsWithTag("Submarine");
					foreach (GameObject obj in array)
					{
						SubRoot subRoot = (SubRoot)HasComponentInParent(obj, "SubRoot");
						if ((bool)subRoot)
						{
							outputTransform = subRoot.seamothLaunchBay.transform;
							result = true;
						}
					}
				}
				return result;
			};
		}
		else if (arrow.gameCondition == GameCondition.BleederAttached)
		{
			arrow.gameConditionDelegate = delegate
			{
				return Player.main.armsController.bleederAttackTarget.attached;
			};
		}
	}

	private void OnCompleteGoal(Goal goal)
	{
		if (goal.goalType == GoalType.Custom)
		{
			completedCustomGoals.Add(goal.customGoalName);
		}
	}

	private GameObject FindNearestObjectTechType(PlayerWorldArrow arrow, out Transform outputTransform)
	{
		GameObject gameObject = null;
		outputTransform = null;
		IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Tech, Player.main.transform.position, arrow.isTargetValidFilter);
		if (ecoTarget != null && Vector3.Distance(ecoTarget.GetPosition(), Player.main.transform.position) <= kMaxObjectDistance)
		{
			gameObject = ecoTarget.GetGameObject();
			outputTransform = gameObject.transform;
		}
		return gameObject;
	}

	private Component HasComponentInParent(GameObject obj, string compName)
	{
		GameObject gameObject = obj;
		while (gameObject != null)
		{
			if (gameObject.GetComponent(compName) != null)
			{
				return gameObject.GetComponent(compName);
			}
			if (!(gameObject.transform.parent != null))
			{
				break;
			}
			gameObject = gameObject.transform.parent.gameObject;
		}
		return null;
	}

	private bool ProcessArrow(PlayerWorldArrow arrow, ref Transform outAnchorTransform)
	{
		Transform outputTransform = Player.main.playerArrowTransform;
		if (string.IsNullOrEmpty(arrow.arrowText) || string.IsNullOrEmpty(arrow.customGoal))
		{
			if (debug)
			{
				Debug.Log("text blank");
			}
			return false;
		}
		if (GameModeUtils.IsOptionActive(arrow.notInGameMode | GameModeOption.NoHints))
		{
			if (debug)
			{
				Debug.Log("notInGameMode");
			}
			return false;
		}
		if (arrow.objectTechType != 0)
		{
			if (!arrow.inInventory)
			{
				outputTransform = null;
				if (FindNearestObjectTechType(arrow, out outputTransform) == null)
				{
					if (debug)
					{
						Debug.Log("FindNearest is null");
					}
					return false;
				}
			}
			else if (Inventory.main.GetPickupCount(arrow.objectTechType) < 1)
			{
				return false;
			}
		}
		if (arrow.underwaterOnly && !Player.main.IsUnderwater())
		{
			if (debug)
			{
				Debug.Log("Underwater only");
			}
			return false;
		}
		if (arrow.gameConditionDelegate != null && !arrow.gameConditionDelegate(ref outputTransform))
		{
			if (debug)
			{
				Debug.Log("delegate");
			}
			return false;
		}
		outAnchorTransform = outputTransform;
		return true;
	}

	private void ArrowUpdate()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (GUIController.main.GetHidePhase() != 0)
		{
			WorldArrowManager.main.DeactivateArrow();
			return;
		}
		PlayerWorldArrow playerWorldArrow = null;
		Transform outAnchorTransform = null;
		foreach (PlayerWorldArrow worldArrow in worldArrows)
		{
			if (debug)
			{
				Debug.Log("Processing arrow: " + worldArrow.customGoal);
			}
			if (!completedCustomGoals.Contains(worldArrow.customGoal) && !ignoreArrows.Contains(worldArrow))
			{
				if (playerWorldArrow != null && !(worldArrow.priority > playerWorldArrow.priority))
				{
					continue;
				}
				bool flag = ProcessArrow(worldArrow, ref outAnchorTransform);
				if (debug)
				{
					Debug.Log("  rc: " + flag);
				}
				if (flag)
				{
					if (debug)
					{
						Debug.Log("Found arrow " + worldArrow.customGoal);
					}
					playerWorldArrow = worldArrow;
				}
			}
			else if (debug)
			{
				Debug.Log(" false: completedCustomGoals: " + completedCustomGoals.Contains(worldArrow.customGoal) + " ignoreArrows: " + ignoreArrows.Contains(worldArrow));
			}
		}
		if (playerWorldArrow != null)
		{
			WorldArrowManager.main.CreateCustomGoalArrow(outAnchorTransform, playerWorldArrow.arrowOffset, playerWorldArrow.offsetIsLocal, playerWorldArrow.arrowText, playerWorldArrow.button, playerWorldArrow.customGoal, playerWorldArrow.pointDown, playerWorldArrow.localScale);
			ProcessIgnoreArrow(playerWorldArrow, outAnchorTransform, arrowUpdateTime);
		}
	}

	private void ProcessIgnoreArrow(PlayerWorldArrow displayArrow, Transform arrowTransform, float time)
	{
		float num = Utils.Dist(arrowTransform, MainCamera.camera.gameObject.transform);
		if (Vector3.Dot(Vector3.Normalize(arrowTransform.position - Player.main.gameObject.transform.position), MainCamera.camera.gameObject.transform.forward) > 0.5f && num < 5f)
		{
			displayArrow.timeNoticeablyDisplayed += time;
		}
		if (displayArrow.timeNoticeablyDisplayed >= 5f && ignoreArrows.Add(displayArrow))
		{
			WorldArrowManager.main.DeactivateArrow();
		}
	}
}
