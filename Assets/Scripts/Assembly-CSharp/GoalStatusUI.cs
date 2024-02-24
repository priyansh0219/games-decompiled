using System.Collections.Generic;
using UnityEngine;

public class GoalStatusUI : MonoBehaviour
{
	public GameObject templateGUITextObject;

	public float yTopInset = 50f;

	public float ySpacing = 10f;

	public AudioClip goalAddedClip;

	public AudioClip goalCompletedClip;

	public float punchAmount = 0.2f;

	public float punchTime = 3f;

	private List<GUIText> goalTextList = new List<GUIText>();

	private void Start()
	{
		GoalManager.main.onNewGoalEvent.AddHandler(base.gameObject, OnNewGoal);
		GoalManager.main.onCompleteGoalEvent.AddHandler(base.gameObject, OnCompleteGoal);
		iTween.FadeTo(base.gameObject, iTween.Hash("a", 0, "time", 0f));
	}

	public void OnNewGoal(Goal goal)
	{
		GameObject obj = Object.Instantiate(templateGUITextObject, templateGUITextObject.transform.position, Quaternion.identity);
		GUIText component = obj.GetComponent<GUIText>();
		component.text = goal.GetGoalDescription().ToUpper();
		obj.SetActive(value: true);
		goalTextList.Add(component);
		goal.SetIsDisplayed(newDisplayState: true);
		if ((bool)goalAddedClip)
		{
			AudioSource.PlayClipAtPoint(goalAddedClip, Utils.GetLocalPlayer().transform.position);
		}
		obj.transform.parent = base.gameObject.transform;
		SetYs();
		iTween.PunchScale(base.gameObject, iTween.Hash("amount", new Vector3(punchAmount, punchAmount, 0f), "time", punchTime));
	}

	private void SetYs()
	{
		for (int i = 0; i < goalTextList.Count; i++)
		{
			GUIText gUIText = goalTextList[i];
			float y = 1f - (yTopInset + (float)i * ySpacing) / (float)Screen.height;
			gUIText.transform.position = new Vector3(gUIText.transform.position.x, y, gUIText.transform.position.z);
		}
	}

	public void OnCompleteGoal(Goal goal)
	{
		for (int i = 0; i < goalTextList.Count; i++)
		{
			GUIText gUIText = goalTextList[i];
			if (gUIText.text == goal.GetGoalDescription().ToUpper())
			{
				if ((bool)goalCompletedClip)
				{
					AudioSource.PlayClipAtPoint(goalCompletedClip, Utils.GetLocalPlayer().transform.position);
				}
				gUIText.text += " (complete)";
				iTween.FadeTo(gUIText.gameObject, iTween.Hash("alpha", 0, "time", 0.6f, "delay", 5f, "oncomplete", "DeleteGoalText", "oncompletetarget", base.gameObject, "oncompleteparams", gUIText));
				goal.SetIsDisplayed(newDisplayState: false);
				break;
			}
		}
	}

	private void DeleteGoalText(GUIText gt)
	{
		Object.Destroy(gt.gameObject);
		SetYs();
	}

	public void ShipComputerActiveStateChanged(bool newState)
	{
		if (newState)
		{
			iTween.FadeTo(base.gameObject, iTween.Hash("a", 1, "time", 0.75f));
		}
		else
		{
			iTween.FadeTo(base.gameObject, iTween.Hash("a", 0, "time", 0.2f));
		}
	}
}
