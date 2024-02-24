using System;
using System.Collections.Generic;
using Story;
using UWE;
using UnityEngine;

public class SeaEmperor : MonoBehaviour, IStoryGoalListener, ICompileTimeCheckable
{
	public static SeaEmperor main;

	[AssertNotNull]
	public StoryGoal onBlowGoal;

	[AssertNotNull]
	public StoryGoal onDialog1Goal;

	[AssertNotNull]
	public StoryGoal onDialog2Goal;

	[AssertNotNull]
	public StoryGoal onDialog3Goal;

	[AssertNotNull]
	public StoryGoal onDialog4Goal;

	[AssertNotNull]
	public StoryGoal onBabiesLeftGoal;

	[AssertNotNull]
	public string listenForIncubaborActiveGoal = "PrecursorPrisonAquariumIncubatorActive";

	[AssertNotNull]
	public string listenForTeleporterActiveGoal = "PrecursorPrisonAquariumFinalTeleporterActive";

	[AssertNotNull]
	public FMOD_CustomEmitter appearSound;

	[AssertNotNull]
	public FMOD_CustomEmitter landSound;

	[AssertNotNull]
	public FMOD_CustomEmitter blowSound;

	[AssertNotNull]
	public FMOD_CustomEmitter babiesInteractSound;

	[AssertNotNull]
	public VFXController fxControl;

	[AssertNotNull]
	public Animator animationController;

	[AssertNotNull]
	public Animator babyAttachAnimationController;

	[AssertNotNull]
	public Transform[] babyAttachPoints;

	[AssertNotNull]
	public string[] babyAnimations;

	public float interpolationRange = 5f;

	public float interpolationDuration = 1f;

	[AssertNotNull]
	public RestoreAnimatorState restoreAnimatorState;

	[AssertNotNull]
	public string finalStateName;

	public float waitForBabiesTimout = 40f;

	private readonly HashSet<SeaEmperorBaby> babies = new HashSet<SeaEmperorBaby>();

	private float timeWaitForBabiesStart;

	public bool waitForBabies { get; private set; }

	public IEnumerable<SeaEmperorBaby> GetBabies()
	{
		return babies;
	}

	public Transform GetBabyAttachPoint(int babyIdentifier)
	{
		if (babyIdentifier < 0 || babyIdentifier >= babyAttachPoints.Length)
		{
			Debug.LogErrorFormat(this, "Failed to find baby attach point {0}", babyIdentifier);
			return base.transform;
		}
		return babyAttachPoints[babyIdentifier];
	}

	public string GetBabyAnimation(int babyIdentifier)
	{
		if (babyIdentifier < 0 || babyIdentifier >= babyAnimations.Length)
		{
			Debug.LogErrorFormat(this, "Failed to find baby attach point {0}", babyIdentifier);
			return "mother";
		}
		return babyAnimations[babyIdentifier];
	}

	public void RegisterBaby(SeaEmperorBaby baby)
	{
		timeWaitForBabiesStart = Time.time;
		waitForBabies = true;
		babies.Add(baby);
	}

	private bool AreBabiesArrived()
	{
		if (Time.time > timeWaitForBabiesStart + waitForBabiesTimout)
		{
			return true;
		}
		HashSet<SeaEmperorBaby>.Enumerator enumerator = babies.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!enumerator.Current.IsAtTargetPosition(interpolationRange))
			{
				return false;
			}
		}
		return true;
	}

	private void Start()
	{
		main = this;
		StoryGoalManager storyGoalManager = StoryGoalManager.main;
		if ((bool)storyGoalManager)
		{
			storyGoalManager.AddListener(this);
		}
	}

	private void Update()
	{
		if (waitForBabies && babies.Count == babyAttachPoints.Length && AreBabiesArrived())
		{
			StartBabyInteraction();
		}
	}

	private void OnDestroy()
	{
		StoryGoalManager storyGoalManager = StoryGoalManager.main;
		if ((bool)storyGoalManager)
		{
			storyGoalManager.RemoveListener(this);
		}
	}

	public void StartBabyInteraction()
	{
		waitForBabies = false;
		HashSet<SeaEmperorBaby>.Enumerator enumerator = babies.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SeaEmperorBaby current = enumerator.Current;
			Transform babyAttachPoint = GetBabyAttachPoint(current.GetId());
			current.transform.SetParent(babyAttachPoint, worldPositionStays: true);
			current.StopAdjustScale();
			current.transform.localScale = Vector3.one;
			current.cinematicController.SetCinematicMode(cinematicOn: true);
			current.cinematicController.StartCinematic(GetBabyAnimation(current.GetId()));
			current.StartCoroutine(UWE.Utils.LerpTransform(current.transform, Vector3.zero, Quaternion.identity, Vector3.one, interpolationDuration));
		}
		animationController.SetTrigger("mother");
		babyAttachAnimationController.SetTrigger("mother");
		babiesInteractSound.Play();
		babies.Clear();
	}

	private void LeviathanAppears()
	{
		appearSound.Play();
	}

	private void LeviathanLands()
	{
		landSound.Play();
		fxControl.Play(3);
	}

	private void LeviathanClawDustLeft()
	{
		fxControl.Play(4);
		fxControl.Play(6);
	}

	private void LeviathanClawDustRight()
	{
		fxControl.Play(5);
		fxControl.Play(6);
	}

	private void LeviathanBlows()
	{
		blowSound.Play();
	}

	private void PlayDialog1()
	{
		onDialog1Goal.Trigger();
	}

	private void PlayDialog2()
	{
		onDialog2Goal.Trigger();
	}

	private void PlayDialog3()
	{
		onDialog3Goal.Trigger();
	}

	private void PlayDialog4()
	{
		onDialog4Goal.Trigger();
	}

	private void BlowAwaySand()
	{
		onBlowGoal.Trigger();
		fxControl.Play(0);
	}

	private void LeviathanSandLeft()
	{
		fxControl.Play(1);
	}

	private void LeviathanSandRight()
	{
		fxControl.Play(2);
	}

	public void OnBabiesHatched()
	{
		onBabiesLeftGoal.Trigger();
		restoreAnimatorState.isCapturing = false;
		restoreAnimatorState.parameterValues.Clear();
		restoreAnimatorState.stateNameHash = Animator.StringToHash(finalStateName);
		restoreAnimatorState.normalizedTime = 0f;
	}

	public void NotifyGoalComplete(string key)
	{
		if (string.Equals(key, listenForIncubaborActiveGoal, StringComparison.OrdinalIgnoreCase))
		{
			PlayDialog2();
		}
		else if (string.Equals(key, listenForTeleporterActiveGoal, StringComparison.OrdinalIgnoreCase))
		{
			PlayDialog3();
		}
	}

	public string CompileTimeCheck()
	{
		return StoryGoalUtils.CheckStoryGoals(new StoryGoal[6] { onDialog1Goal, onDialog2Goal, onDialog3Goal, onDialog4Goal, onBlowGoal, onBabiesLeftGoal });
	}
}
