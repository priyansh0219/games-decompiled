using UnityEngine;

public class PrisonCreatureBehaviour : MonoBehaviour
{
	[AssertNotNull]
	public Creature creature;

	public Behaviour[] disableBehaviours;

	public Behaviour[] enableBehaviours;

	public void Activate()
	{
		if (disableBehaviours != null)
		{
			for (int i = 0; i < disableBehaviours.Length; i++)
			{
				disableBehaviours[i].enabled = false;
			}
		}
		if (enableBehaviours != null)
		{
			for (int j = 0; j < enableBehaviours.Length; j++)
			{
				enableBehaviours[j].enabled = true;
			}
		}
		creature.ScanCreatureActions();
	}
}
