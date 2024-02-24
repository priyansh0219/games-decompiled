using System.Collections;
using UnityEngine;

public class SpawnRandomChild : MonoBehaviour
{
	public GameObject[] prefabs;

	public bool keepScale;

	public bool gridTestSpawn;

	private void Start()
	{
		if (!gridTestSpawn)
		{
			Utils.SpawnZeroedAt(prefabs.GetRandom(), base.transform, keepScale);
		}
	}

	private IEnumerator SpawnGrid()
	{
		Vector3 localPos = Vector3.zero;
		int count = 0;
		GameObject[] array = prefabs;
		for (int i = 0; i < array.Length; i++)
		{
			Utils.SpawnZeroedAt(array[i], base.transform).transform.localPosition = localPos;
			localPos.z += 10f;
			if (localPos.z > 100f)
			{
				localPos.z = 0f;
				localPos.x += 10f;
			}
			count++;
			Debug.Log(count + "/" + prefabs.Length);
			yield return new WaitForSeconds(0.5f);
		}
	}

	private void Update()
	{
		if (gridTestSpawn && Input.GetKeyDown("p"))
		{
			StartCoroutine(SpawnGrid());
		}
	}
}
