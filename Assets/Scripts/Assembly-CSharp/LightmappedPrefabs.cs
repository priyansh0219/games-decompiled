using System;
using System.Collections;
using System.Collections.Generic;
using UWE;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class LightmappedPrefabs : MonoBehaviour
{
	[Serializable]
	public class AutoLoadScene
	{
		public string sceneName;

		public bool spawnOnStart;
	}

	public delegate void OnPrefabLoaded(GameObject prefab);

	public static LightmappedPrefabs main = null;

	public static string StandardMainObjectName = "__LIGHTMAPPED_PREFAB__";

	[AssertNotNull]
	public AutoLoadScene[] autoloadScenes;

	private AsyncOperationHandle op;

	private TimerStack timer = new TimerStack();

	private string loadingScene = "";

	private Dictionary<string, GameObject> scene2prefab = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

	private Queue<string> loadQueue = new Queue<string>();

	private Dictionary<string, List<OnPrefabLoaded>> callbacks = new Dictionary<string, List<OnPrefabLoaded>>(StringComparer.OrdinalIgnoreCase);

	private void Awake()
	{
		main = this;
		if (autoloadScenes.Length == 0)
		{
			return;
		}
		AutoLoadScene[] array = autoloadScenes;
		foreach (AutoLoadScene autoLoadScene in array)
		{
			if (autoLoadScene.spawnOnStart)
			{
				QueueScenePrefab(autoLoadScene.sceneName, ActivateLoadedPrefab);
			}
			else
			{
				QueueScenePrefab(autoLoadScene.sceneName, null);
			}
		}
	}

	public bool IsWaitingOnLoads()
	{
		if (loadQueue.Count <= 0)
		{
			return op.IsValid();
		}
		return true;
	}

	private void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "scene");
	}

	private void ActivateLoadedPrefab(GameObject go)
	{
		go.transform.parent = null;
		go.transform.position = Vector3.zero;
		go.SetActive(value: true);
	}

	private void LoadPrefabScene(string scene)
	{
		StartCoroutine(LoadPrefabSceneCoroutine(scene));
	}

	private IEnumerator LoadPrefabSceneCoroutine(string scene)
	{
		if (scene2prefab.ContainsKey(scene))
		{
			Debug.Log("Scene " + scene + " already loaded - doing nothing.");
		}
		else if (GameObject.Find(StandardMainObjectName) != null)
		{
			Debug.LogError("There is already an object named " + StandardMainObjectName + " - this clashes with our standards. Please don't do this.");
		}
		else if (!op.IsValid())
		{
			loadingScene = scene;
			op = AddressablesUtility.LoadSceneAsync(scene, LoadSceneMode.Additive);
			timer.Begin("Loading lightmapped prefab scene " + scene);
			WaitScreen.AsyncOperationItem waitItem = WaitScreen.Add("Scene" + scene, op);
			yield return op;
			WaitScreen.Remove(waitItem);
			timer.End();
		}
		else
		{
			Debug.LogError("PROGRAMMER ERROR: load already in progress");
		}
	}

	private void OnConsoleCommand_scene(NotificationCenter.Notification n)
	{
		if (!op.IsValid())
		{
			string scene = (string)n.data[0];
			LoadPrefabScene(scene);
		}
		else
		{
			Debug.Log("load in progress - wait");
		}
	}

	private bool IsPreActivationDone()
	{
		return op.PercentComplete >= 0.9f;
	}

	private void Update()
	{
		if (op.IsValid() && op.IsDone)
		{
			op = default(AsyncOperationHandle);
			GameObject gameObject = GameObject.Find(StandardMainObjectName);
			if (gameObject != null)
			{
				Debug.LogFormat(gameObject, "Loaded lightmapped prefab {0} frame {1}", loadingScene, Time.frameCount);
				gameObject.name = loadingScene + "-MainPrefab";
				gameObject.SetActive(value: false);
				scene2prefab[loadingScene] = gameObject;
				ScenePrefabDatabase.AddScenePrefab(gameObject);
				if (callbacks.TryGetValue(loadingScene, out var value))
				{
					foreach (OnPrefabLoaded item in value)
					{
						item(gameObject);
					}
					value.Clear();
				}
			}
			else
			{
				Debug.LogErrorFormat(this, "Lightmapped-prefab scene '{0}' did not have an object named '{1}'", loadingScene, StandardMainObjectName);
			}
		}
		if (!op.IsValid() && loadQueue.Count > 0)
		{
			LoadPrefabScene(loadQueue.Dequeue());
		}
	}

	private void OnDestroy()
	{
		LightmapSettings.lightmaps = new LightmapData[0];
	}

	private void QueueScenePrefab(string scene, OnPrefabLoaded cb)
	{
		loadQueue.Enqueue(scene);
		if (cb != null)
		{
			callbacks.GetOrAddNew(scene).Add(cb);
		}
	}

	public void RequestScenePrefab(string scene, OnPrefabLoaded cb)
	{
		if (scene2prefab.TryGetValue(scene, out var value))
		{
			cb(value);
		}
		else
		{
			QueueScenePrefab(scene, cb);
		}
	}

	public GameObject GetScenePrefab(string scene)
	{
		if (scene2prefab.TryGetValue(scene, out var value))
		{
			return value;
		}
		return null;
	}
}
