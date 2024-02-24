using Story;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_RadioMessageIndicator : MonoBehaviour
{
	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public Image sprite;

	private void OnEnable()
	{
		StoryGoalManager.PendingMessageEvent += NewRadioMessage;
		Radio.CancelIconEvent += DisableSprite;
	}

	private void OnDisable()
	{
		StoryGoalManager.PendingMessageEvent -= NewRadioMessage;
		Radio.CancelIconEvent -= DisableSprite;
	}

	private void NewRadioMessage(bool newMessages)
	{
		if (newMessages && (bool)StoryGoalManager.main && StoryGoalManager.main.IsGoalComplete("OnPlayRadioBounceBack"))
		{
			animator.SetTrigger("Init");
			Invoke("DisableSprite", 50f);
		}
	}

	public void DisableSprite()
	{
		animator.SetTrigger("Idle");
	}
}
