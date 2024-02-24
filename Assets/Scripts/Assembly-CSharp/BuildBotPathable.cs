using UnityEngine;

public class BuildBotPathable : MonoBehaviour
{
	public BuildBotPath[] paths;

	private bool assigned;

	private void Start()
	{
	}

	private void Update()
	{
		if (!assigned)
		{
			AssignBotsToPath();
			assigned = true;
		}
	}

	public void AssignBotsToPath()
	{
		int count = ConstructorBuildBot.buildbots.Count;
		for (int i = 0; i < paths.Length; i++)
		{
			if (i < count)
			{
				ConstructorBuildBot.buildbots[i].SetPath(paths[i], base.gameObject);
			}
		}
	}
}
