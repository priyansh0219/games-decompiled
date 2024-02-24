using System.Collections;
using Story;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class JuvenileEmperorSpawner : MonoBehaviour
{
	[AssertNotNull]
	public string listenForBabiesSpawnedOutsideGoal = "SeaEmperorBabiesSpawnedOutsideOfPrisonAquarium";

	[AssertNotNull]
	public AssetReferenceGameObject juvenileEmporerPrefabReference;

	public float expectedLeashDistance = 500f;

	public Vector3 expectedDirectionDistanceMultiplier = Vector3.one;

	private IEnumerator Start()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main && main.IsGoalComplete(listenForBabiesSpawnedOutsideGoal))
		{
			CoroutineTask<GameObject> task = AddressablesUtility.InstantiateAsync(juvenileEmporerPrefabReference.RuntimeKey as string, null, base.transform.position, base.transform.rotation);
			yield return task;
			task.GetResult();
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDrawGizmos()
	{
		Vector3 vector = expectedDirectionDistanceMultiplier;
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z));
		Gizmos.color = Color.red.ToAlpha(0.5f);
		Gizmos.DrawSphere(Vector3.zero, expectedLeashDistance);
		Gizmos.DrawWireSphere(Vector3.zero, expectedLeashDistance);
	}
}
