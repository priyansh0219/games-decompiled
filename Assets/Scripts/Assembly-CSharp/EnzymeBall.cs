using System;
using Story;
using UWE;
using UnityEngine;

public class EnzymeBall : MonoBehaviour, IStoryGoalListener, IAnimParamReceiver, ICompileTimeCheckable
{
	[AssertNotNull]
	public FMODAsset treatSound;

	[AssertNotNull]
	public FMODAsset curePlayerSound;

	[AssertNotNull]
	public PlayerCinematicController playerCinematicController;

	[AssertNotNull]
	public Transform playerAttachPoint;

	[AssertNotNull]
	public GameObject leftGlovePrefab;

	[AssertNotNull]
	public GameObject rightGlovePrefab;

	[AssertNotNull]
	public StoryGoal onPlayerCuredGoal;

	[AssertNotNull]
	public VFXCureBall fxCureball;

	private GameObject leftGlove;

	private GameObject rightGlove;

	private int restoreQuickSlot = -1;

	private bool playerCured;

	private void Start()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			playerCured = main.IsGoalComplete(onPlayerCuredGoal.key);
			if (!playerCured)
			{
				main.AddListener(this);
			}
		}
	}

	public void NotifyGoalComplete(string key)
	{
		if (string.Equals(key, onPlayerCuredGoal.key, StringComparison.OrdinalIgnoreCase))
		{
			playerCured = true;
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (UWE.Utils.GetComponentInHierarchy<Creature>(col.gameObject) != null)
		{
			InfectedMixin componentInHierarchy = UWE.Utils.GetComponentInHierarchy<InfectedMixin>(col.gameObject);
			if (componentInHierarchy != null && componentInHierarchy.IsInfected())
			{
				componentInHierarchy.RemoveInfection();
				Utils.PlayFMODAsset(treatSound, base.transform);
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public void OnHandHover()
	{
		if (!playerCured)
		{
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			HandReticle.main.SetText(HandReticle.TextType.Hand, "UseEnzymeCureBall", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}

	public void OnHandClick()
	{
		if (!playerCured)
		{
			restoreQuickSlot = Inventory.main.quickSlots.activeSlot;
			Inventory.main.ReturnHeld();
			Player main = Player.main;
			Vector3 forward = main.transform.position - base.transform.position;
			forward.y = 0f;
			base.transform.rotation = Quaternion.LookRotation(forward);
			leftGlove = UnityEngine.Object.Instantiate(leftGlovePrefab, Vector3.zero, Quaternion.identity);
			leftGlove.transform.SetParent(main.armsController.leftHand, worldPositionStays: false);
			rightGlove = UnityEngine.Object.Instantiate(rightGlovePrefab, Vector3.zero, Quaternion.identity);
			rightGlove.transform.SetParent(main.armsController.rightHand, worldPositionStays: false);
			playerCinematicController.StartCinematicMode(main);
			Player.main.StartPlayerInfectionCure();
			fxCureball.StartPlayerCureSequence();
			VFXCureBall component = rightGlove.GetComponent<VFXCureBall>();
			VFXCureBall component2 = leftGlove.GetComponent<VFXCureBall>();
			component.StartPlayerCureSequence();
			component2.StartPlayerCureSequence();
			Utils.PlayFMODAsset(curePlayerSound, base.transform);
		}
	}

	private void OnPlayerCinematicModeEnd(PlayerCinematicController cinematicController)
	{
		playerCured = true;
		onPlayerCuredGoal.Trigger();
		if (restoreQuickSlot != -1)
		{
			Inventory.main.quickSlots.Select(restoreQuickSlot);
		}
		UnityEngine.Object.Destroy(leftGlove);
		UnityEngine.Object.Destroy(rightGlove);
	}

	private void OnDisable()
	{
		UnityEngine.Object.Destroy(leftGlove);
		UnityEngine.Object.Destroy(rightGlove);
	}

	private void OnDestroy()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			main.RemoveListener(this);
		}
	}

	void IAnimParamReceiver.ForwardAnimationParameterBool(string parameterName, bool value)
	{
		if (leftGlove != null)
		{
			Animator component = leftGlove.GetComponent<Animator>();
			if (component != null)
			{
				component.SetBool(parameterName, value);
			}
		}
		if (rightGlove != null)
		{
			Animator component2 = rightGlove.GetComponent<Animator>();
			if (component2 != null)
			{
				component2.SetBool(parameterName, value);
			}
		}
	}

	public string CompileTimeCheck()
	{
		return StoryGoalUtils.CheckStoryGoal(onPlayerCuredGoal);
	}
}
