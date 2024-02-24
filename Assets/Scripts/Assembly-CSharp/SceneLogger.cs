using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLogger : MonoBehaviour
{
	private void Start()
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			Debug.LogFormat("Scene {0}", sceneAt.name);
			GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
			foreach (GameObject gameObject in rootGameObjects)
			{
				Debug.LogFormat("GameObject {0}", gameObject.name);
				Component[] componentsInChildren = gameObject.GetComponentsInChildren<Component>(includeInactive: true);
				foreach (Component component in componentsInChildren)
				{
					Debug.LogFormat("Component {0}", component.GetType());
				}
			}
		}
	}
}
