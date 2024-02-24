using System.Collections;
using Story;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SeaEmperorBabiesSpawner : MonoBehaviour, ICompileTimeCheckable
{
	[AssertNotNull]
	public string listenForBabiesLeftPrisonGoal = "SeaEmperorBabiesLeftPrisonAquarium";

	[AssertNotNull]
	public StoryGoal babiesSpawnedGoal;

	[AssertNotNull]
	public AssetReferenceGameObject babyEmporerPrefabReference;

	[AssertNotNull]
	public Transform[] spawnPoints;

	public float babyScale = 2.02f;

	private IEnumerator Start()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if (!main || !main.IsGoalComplete(listenForBabiesLeftPrisonGoal))
		{
			yield break;
		}
		for (int i = 0; i < spawnPoints.Length; i++)
		{
			CoroutineTask<GameObject> task = AddressablesUtility.InstantiateAsync(babyEmporerPrefabReference.RuntimeKey as string, null, spawnPoints[i].position, spawnPoints[i].rotation);
			yield return task;
			GameObject babyGO = task.GetResult();
			if (babyGO != null)
			{
				SeaEmperorBaby component = babyGO.GetComponent<SeaEmperorBaby>();
				if ((bool)component)
				{
					component.SetScale(babyScale);
					component.dropEnzymes.enabled = true;
					component.dropEnzymes.spawnLimit = 5;
					Vector3 position = component.transform.position - component.transform.forward * 3f;
					CoroutineTask<GameObject> cureBallTask = AddressablesUtility.InstantiateAsync(component.dropEnzymes.enzymePrefabReference.RuntimeKey as string, null, position, Quaternion.identity);
					yield return cureBallTask;
					GameObject result = cureBallTask.GetResult();
					LargeWorldEntity.Register(babyGO);
					LargeWorldEntity.Register(result);
				}
			}
		}
		babiesSpawnedGoal.Trigger();
		Object.Destroy(base.gameObject);
	}

	public string CompileTimeCheck()
	{
		return StoryGoalUtils.CheckStoryGoal(babiesSpawnedGoal);
	}
}
