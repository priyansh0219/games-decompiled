using UnityEngine;

public class WorldArrowManager : MonoBehaviour
{
	public enum PointDirection
	{
		None = 0,
		Up = 1,
		Down = 2
	}

	public static WorldArrowManager main;

	[AssertNotNull]
	public GameObject worldArrowPrefab;

	private WorldArrow worldArrow;

	private Animator worldArrowAnimator;

	private string currentGoalText;

	private bool initialized;

	private void Awake()
	{
		main = this;
		base.gameObject.AddComponent<HideForScreenshots>();
	}

	private void HideForScreenshots()
	{
		DeactivateArrow();
	}

	public void InternalCreateWorldArrow()
	{
		worldArrowPrefab.SetActive(value: false);
		GameObject gameObject = Object.Instantiate(worldArrowPrefab);
		worldArrow = gameObject.GetComponent<WorldArrow>();
		worldArrowAnimator = gameObject.GetComponent<Animator>();
	}

	private void Start()
	{
		GoalManager.main.onCompleteGoalEvent.AddHandler(base.gameObject, OnCompleteGoal);
	}

	public void DeactivateArrow()
	{
		if (worldArrow != null)
		{
			worldArrowAnimator.SetTrigger("FadeOut");
			Invoke("InternalDeactivateArrow", 1f);
		}
	}

	private void InternalDeactivateArrow()
	{
		if (!(worldArrow == null))
		{
			worldArrow.transform.parent = null;
			worldArrow.gameObject.SetActive(value: false);
			initialized = false;
		}
	}

	public void OnCompleteGoal(Goal goal)
	{
		if (worldArrow != null && goal.goalType == GoalType.Custom && goal.customGoalName == currentGoalText)
		{
			DeactivateArrow();
		}
	}

	public void CreateArrow(Transform parentTransform, Vector3 offset, bool offsetIsLocal, string arrowText, GameInput.Button? button, PointDirection pointDirection, float scaleFactor = 1f)
	{
		if (worldArrow == null)
		{
			InternalCreateWorldArrow();
		}
		string text = Language.main.Get(arrowText);
		if (button.HasValue)
		{
			text = $"{text} ({GameInput.FormatButton(button.Value)})";
		}
		worldArrow.SetText(text);
		if (offsetIsLocal)
		{
			worldArrow.transform.parent = parentTransform;
			worldArrow.gameObject.transform.localPosition = offset;
			worldArrow.gameObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
		}
		else
		{
			worldArrow.gameObject.transform.position = parentTransform.position + offset;
			worldArrow.transform.parent = parentTransform;
			worldArrow.gameObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
		}
		worldArrow.gameObject.SetActive(value: true);
		if (!initialized)
		{
			worldArrowAnimator.SetTrigger("FadeIn");
			initialized = true;
		}
		switch (pointDirection)
		{
		case PointDirection.None:
			worldArrow.PointNone();
			break;
		case PointDirection.Up:
			worldArrow.PointUp();
			break;
		case PointDirection.Down:
			worldArrow.PointDown();
			break;
		}
	}

	public bool IsArrowNearby()
	{
		if (worldArrow != null && worldArrow.gameObject.activeInHierarchy)
		{
			return (worldArrow.gameObject.transform.position - Player.main.transform.position).magnitude < 15f;
		}
		return false;
	}

	public void CreateCustomGoalArrow(Transform parentTransform, Vector3 offset, bool offsetIsLocal, string arrowText, GameInput.Button? button, string customGoalName, bool pointDown = true, float localScale = 1f)
	{
		currentGoalText = customGoalName;
		CreateArrow(parentTransform, offset, offsetIsLocal, arrowText, button, (!pointDown) ? PointDirection.Up : PointDirection.Down, localScale);
	}
}
