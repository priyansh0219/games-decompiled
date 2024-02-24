using UnityEngine;

public class SubConsoleCommand : MonoBehaviour
{
	public static SubConsoleCommand main;

	private Vector3 spawnPosition;

	private Quaternion spawnRotation;

	private GameObject lastCreatedSub;

	private void Awake()
	{
		main = this;
		DevConsole.RegisterConsoleCommand(this, "sub");
	}

	private void OnSubPrefabLoaded(GameObject prefab)
	{
		GameObject gameObject = Utils.SpawnPrefabAt(prefab, null, spawnPosition);
		gameObject.transform.rotation = spawnRotation;
		gameObject.SetActive(value: true);
		gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
		LargeWorldEntity.Register(gameObject);
		CrafterLogic.NotifyCraftEnd(gameObject, CraftData.GetTechType(gameObject));
		lastCreatedSub = gameObject;
	}

	public GameObject GetLastCreatedSub()
	{
		return lastCreatedSub;
	}

	private void OnConsoleCommand_sub(NotificationCenter.Notification n)
	{
		string text = (string)n.data[0];
		if (text != null && text != "")
		{
			Transform transform = MainCamera.camera.transform;
			spawnPosition = transform.position + 20f * transform.forward;
			spawnRotation = Quaternion.LookRotation(MainCamera.camera.transform.right);
			LightmappedPrefabs.main.RequestScenePrefab(text, OnSubPrefabLoaded);
		}
		else
		{
			ErrorMessage.AddDebug("Must specify sub name (beetle, cyclops).");
		}
	}

	public void SpawnSub(string subSceneName, Vector3 position, Quaternion rotation)
	{
		spawnPosition = position;
		spawnRotation = rotation;
		LightmappedPrefabs.main.RequestScenePrefab(subSceneName, OnSubPrefabLoaded);
	}
}
