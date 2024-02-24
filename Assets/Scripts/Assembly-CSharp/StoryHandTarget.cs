using Story;
using UnityEngine;

public class StoryHandTarget : HandTarget, IHandTarget, ICompileTimeCheckable
{
	[AssertNotNull]
	public string primaryTooltip;

	public string secondaryTooltip;

	[AssertNotNull]
	public StoryGoal goal;

	public GameObject informGameObject;

	public GameObject destroyGameObject;

	public void OnHandHover(GUIHand hand)
	{
		HandReticle.main.SetText(HandReticle.TextType.Hand, primaryTooltip, translate: true, GameInput.Button.LeftHand);
		HandReticle.main.SetText(HandReticle.TextType.HandSubscript, secondaryTooltip, translate: true);
		HandReticle.main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnHandClick(GUIHand hand)
	{
		goal.Trigger();
		if ((bool)informGameObject)
		{
			informGameObject.SendMessage("OnStoryHandTarget", SendMessageOptions.DontRequireReceiver);
		}
		Object.Destroy(destroyGameObject);
	}

	public string CompileTimeCheck()
	{
		return StoryGoalUtils.CheckStoryGoal(goal);
	}
}
