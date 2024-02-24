using UnityEngine;

public class CustomizedSpawn : DelayedSpawn
{
	public string[] disableBehaviours;

	private void Start()
	{
		Spawn();
		Object.Destroy(base.gameObject);
	}

	protected override void OnBeforeActivate(GameObject go)
	{
		for (int i = 0; i < disableBehaviours.Length; i++)
		{
			string text = disableBehaviours[i];
			MonoBehaviour monoBehaviour = go.GetComponent(text) as MonoBehaviour;
			if (!monoBehaviour)
			{
				Debug.LogWarningFormat(this, "could not find component '{0}' on game object '{1}'", text, go);
			}
			else
			{
				monoBehaviour.enabled = false;
			}
		}
	}
}
