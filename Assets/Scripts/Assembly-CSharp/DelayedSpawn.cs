using UnityEngine;

public class DelayedSpawn : MonoBehaviour
{
	public GameObject prefab;

	public bool attachToParent;

	public bool registerToStreamer = true;

	public int numSpawns = 1;

	private void Awake()
	{
	}

	public void Spawn()
	{
		bool activeSelf = prefab.activeSelf;
		prefab.SetActive(value: false);
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < numSpawns; i++)
		{
			GameObject gameObject = Object.Instantiate(prefab);
			if (!attachToParent)
			{
				gameObject.transform.parent = null;
				gameObject.transform.position = base.transform.position + vector;
				gameObject.transform.rotation = base.transform.rotation;
			}
			else
			{
				gameObject.transform.parent = base.transform;
				gameObject.transform.localPosition = vector;
				gameObject.transform.localRotation = Quaternion.identity;
			}
			OnBeforeActivate(gameObject);
			gameObject.SetActive(value: true);
			LargeWorldStreamer main = LargeWorldStreamer.main;
			if (registerToStreamer && (bool)main)
			{
				main.cellManager.RegisterEntity(gameObject);
			}
			vector = Random.insideUnitSphere * 4f;
		}
		prefab.SetActive(activeSelf);
	}

	protected virtual void OnBeforeActivate(GameObject go)
	{
	}
}
