using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMenuEnvironment : MonoBehaviour
{
	public string sceneName;

	private void Awake()
	{
		AddressablesUtility.LoadScene(sceneName, LoadSceneMode.Additive);
	}
}
