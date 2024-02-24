using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UWE;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public static class AddressablesUtility
{
	private struct HandleReleaseRequest
	{
		public readonly AsyncOperationHandle handle;

		public readonly float releaseTime;

		public HandleReleaseRequest(AsyncOperationHandle handle, float delay)
		{
			this.handle = handle;
			releaseTime = GetTime() + delay;
		}

		public void Release()
		{
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
		}
	}

	private struct Request
	{
		public AsyncOperationHandle<GameObject> handle;

		public bool isLoading;

		public int refCount;

		public float releaseTime;
	}

	private class AddressableScene
	{
		public AssetReference assetReference;

		public AsyncOperationHandle<SceneInstance> handle;

		public AddressableScene(AssetReference reference)
		{
			assetReference = reference;
		}
	}

	private const float releaseDelay = 3f;

	private static readonly ProfilerMarker loadAsyncMarker = new ProfilerMarker("AddressablesUtility.LoadAsync");

	private static readonly ProfilerMarker loadAllAsyncMarker = new ProfilerMarker("AddressablesUtility.LoadAllAsync");

	private static readonly Queue<HandleReleaseRequest> releaseQueue = new Queue<HandleReleaseRequest>();

	private static readonly Dictionary<string, Request> keyToHandle = new Dictionary<string, Request>();

	private const float deferAsyncInstantiationMaxSecs = 3f;

	private static bool shouldDeferAsyncInstantiation;

	private static Dictionary<string, AddressableScene> allScenes = new Dictionary<string, AddressableScene>
	{
		{
			"cleaner",
			new AddressableScene(new AssetReference("Assets/Scenes/Cleaner.unity"))
		},
		{
			"main",
			new AddressableScene(new AssetReference("Assets/Scenes/Main.unity"))
		},
		{
			"escapepod",
			new AddressableScene(new AssetReference("Assets/SubmarineScenes/EscapePod.unity"))
		},
		{
			"aurora",
			new AddressableScene(new AssetReference("Assets/SubmarineScenes/Aurora.unity"))
		},
		{
			"cyclops",
			new AddressableScene(new AssetReference("Assets/SubmarineScenes/Cyclops.unity"))
		},
		{
			"menuenvironment",
			new AddressableScene(new AssetReference("Assets/Scenes/MenuEnvironment.unity"))
		},
		{
			"essentials",
			new AddressableScene(new AssetReference("Assets/Scenes/Essentials.unity"))
		},
		{
			"emptyscene",
			new AddressableScene(new AssetReference("Assets/Scenes/EmptyScene.unity"))
		},
		{
			"rocketspace",
			new AddressableScene(new AssetReference("Assets/Scenes/RocketSpace.unity"))
		},
		{
			"endcredits",
			new AddressableScene(new AssetReference("Assets/Scenes/EndCredits.unity"))
		},
		{
			"endcreditsscenecleaner",
			new AddressableScene(new AssetReference("Assets/Scenes/EndCreditsSceneCleaner.unity"))
		},
		{
			"xmenu",
			new AddressableScene(new AssetReference("Assets/Scenes/XMenu.unity"))
		},
		{
			"startscreen",
			new AddressableScene(new AssetReference("Assets/Scenes/StartScreen.unity"))
		}
	};

	[Conditional("ADDRESSABLES_VERBOSE")]
	public static void VerboseLogFormat(string format, params object[] args)
	{
		UnityEngine.Debug.LogFormat(format, args);
	}

	public static void Reset()
	{
		while (releaseQueue.Count > 0)
		{
			releaseQueue.Dequeue().Release();
		}
		foreach (KeyValuePair<string, Request> item in keyToHandle)
		{
			AsyncOperationHandle<GameObject> handle = item.Value.handle;
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
		}
		keyToHandle.Clear();
	}

	public static void Update()
	{
		bool flag = false;
		float time = GetTime();
		float num = time + 3f;
		using (ListPool<string> listPool = Pool<ListPool<string>>.Get())
		{
			List<string> list = listPool.list;
			list.AddRange(keyToHandle.Keys);
			foreach (string item in list)
			{
				Request value = keyToHandle[item];
				AsyncOperationHandle<GameObject> handle = value.handle;
				if (!handle.IsValid())
				{
					keyToHandle.Remove(item);
				}
				else if (handle.IsDone)
				{
					if (value.isLoading)
					{
						value.isLoading = false;
						keyToHandle[item] = value;
						continue;
					}
					bool flag2 = value.refCount <= 0;
					if (value.releaseTime >= 0f != flag2)
					{
						value.releaseTime = (flag2 ? num : (-1f));
						keyToHandle[item] = value;
					}
					if (value.releaseTime >= 0f && time > value.releaseTime)
					{
						keyToHandle.Remove(item);
						releaseQueue.Enqueue(new HandleReleaseRequest(handle, 0f));
					}
				}
				else
				{
					flag = true;
				}
			}
		}
		bool num2 = releaseQueue.Count > 0;
		shouldDeferAsyncInstantiation = num2 && flag;
		if (num2 && !flag)
		{
			ProcessUnloads();
		}
	}

	private static void ProcessUnloads()
	{
		float time = GetTime();
		while (releaseQueue.Count > 0)
		{
			HandleReleaseRequest handleReleaseRequest = releaseQueue.Peek();
			if (time >= handleReleaseRequest.releaseTime)
			{
				releaseQueue.Dequeue();
				handleReleaseRequest.Release();
				continue;
			}
			break;
		}
	}

	public static bool Exists<T>(object key)
	{
		IList<IResourceLocation> locations;
		return Locate<T>(key, out locations);
	}

	public static AsyncOperationHandle<T> LoadAsync<T>(object key)
	{
		try
		{
			return Addressables.LoadAssetAsync<T>(key);
		}
		finally
		{
		}
	}

	public static CoroutineTask<GameObject> InstantiateAsync(string key, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), bool awake = true)
	{
		TaskResult<GameObject> result = new TaskResult<GameObject>();
		return new CoroutineTask<GameObject>(InstantiateAsync(key, result, parent, position, rotation, awake), result);
	}

	public static IEnumerator InstantiateAsync(string key, IOut<GameObject> result, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), bool awake = true)
	{
		if (key == null)
		{
			UnityEngine.Debug.LogErrorFormat("[AddressablesUtility] key cannot be null");
			result.Set(null);
			yield break;
		}
		bool hasParent = parent != null;
		float deferEndTime = GetTime() + 3f;
		while (shouldDeferAsyncInstantiation && GetTime() < deferEndTime)
		{
			yield return CoroutineUtils.waitForNextFrame;
		}
		if (!keyToHandle.TryGetValue(key, out var value))
		{
			try
			{
				Request request = default(Request);
				request.handle = Addressables.LoadAssetAsync<GameObject>(key);
				request.isLoading = true;
				request.refCount = 0;
				request.releaseTime = -1f;
				value = request;
				keyToHandle.Add(key, value);
			}
			catch (Exception innerException)
			{
				UnityEngine.Debug.LogException(new Exception("Addressables.LoadAssetAsync has failed for key " + key, innerException));
				result.Set(null);
				yield break;
			}
		}
		while (true)
		{
			if (keyToHandle.TryGetValue(key, out value))
			{
				if (!value.handle.IsValid())
				{
					UnityEngine.Debug.LogErrorFormat("[AddressablesUtility] Handle became invalid when loading asset: {0}", key);
					result.Set(null);
					yield break;
				}
				if (value.handle.IsDone)
				{
					break;
				}
				yield return CoroutineUtils.waitForNextFrame;
				continue;
			}
			UnityEngine.Debug.LogErrorFormat("[AddressablesUtility] Asset request was removed while loading: {0}", key);
			result.Set(null);
			yield break;
		}
		if (hasParent && parent == null)
		{
			result.Set(null);
			yield break;
		}
		value.handle.LogExceptionIfFailed(key);
		GameObject result2 = value.handle.Result;
		if (result2 == null)
		{
			UnityEngine.Debug.LogErrorFormat("AddressablesUtility.InstantiateAsync {0} for '{1}' due to {2}!", value.handle.Status, key, value.handle.OperationException.Message);
			result.Set(null);
			yield break;
		}
		GameObject gameObject = EditorModifications.Instantiate(result2, parent, position, rotation, awake);
		PrefabIdentifier component = null;
		if (gameObject.TryGetComponent<PrefabIdentifier>(out component))
		{
			component.SetPrefabKey(key);
		}
		result.Set(gameObject);
	}

	public static AsyncOperationHandle<IList<T>> LoadAllAsync<T>(string key)
	{
		try
		{
			return Addressables.LoadAssetsAsync<T>(key, null);
		}
		finally
		{
		}
	}

	public static void IncreaseRefCount(string key)
	{
		if (keyToHandle.TryGetValue(key, out var value))
		{
			value.refCount++;
			keyToHandle[key] = value;
		}
		else
		{
			UnityEngine.Debug.LogErrorFormat("[AddressablesUtility] Attempting to increase a reference count that doesn't exist: {0}", key);
		}
	}

	public static void DecreaseRefCount(string key)
	{
		if (keyToHandle.TryGetValue(key, out var value))
		{
			value.refCount--;
			keyToHandle[key] = value;
		}
		else
		{
			UnityEngine.Debug.LogErrorFormat("[AddressablesUtility] Attempting to decrease a reference count that doesn't exist: {0}", key);
		}
	}

	public static void QueueRelease<T>(ref AsyncOperationHandle<T> handle)
	{
		if (handle.IsValid())
		{
			releaseQueue.Enqueue(new HandleReleaseRequest(handle, 3f));
			handle = default(AsyncOperationHandle<T>);
		}
	}

	private static float GetTime()
	{
		return Time.unscaledTime;
	}

	[Conditional("ADDRESSABLES_VERBOSE")]
	public static void VerboseLogFormat(UnityEngine.Object obj, string format, params object[] args)
	{
		UnityEngine.Debug.LogFormat(obj, format, args);
	}

	[Conditional("ADDRESSABLES_VERBOSE")]
	public static void PrintRefCount(string key, UnityEngine.Object obj)
	{
		keyToHandle.TryGetValue(key, out var _);
	}

	public static bool IsAddressableScene(string sceneName)
	{
		return allScenes.ContainsKey(sceneName.ToLower());
	}

	public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(string sceneName, LoadSceneMode mode)
	{
		UnityEngine.Debug.Log("AddressablesUtility.LoadSceneAsync loading scene: " + sceneName + " in mode " + mode);
		if (allScenes.TryGetValue(sceneName.ToLower(), out var value))
		{
			value.handle = Addressables.LoadSceneAsync(value.assetReference, mode);
			return value.handle;
		}
		UnityEngine.Debug.LogWarning("AddressablesUtility.LoadSceneAsync could not find scene: " + sceneName.ToString() + ". Falling back to SceneManagerLoading...");
		SceneManager.LoadSceneAsync(sceneName.ToString(), mode);
		return default(AsyncOperationHandle<SceneInstance>);
	}

	public static void LoadScene(string sceneName, LoadSceneMode mode)
	{
		UnityEngine.Debug.Log("AddressablesUtility.LoadScene loading scene: " + sceneName + " in mode " + mode);
		if (allScenes.TryGetValue(sceneName.ToLower(), out var value))
		{
			Addressables.LoadScene(value.assetReference, mode);
			return;
		}
		UnityEngine.Debug.LogWarning("AddressablesUtility.LoadScene could not find scene: " + sceneName.ToString() + ". Falling back to SceneManagerLoading...");
		SceneManager.LoadScene(sceneName.ToString(), mode);
	}

	public static void UnloadSceneAsync(string sceneName)
	{
		UnityEngine.Debug.Log("AddressablesUtility.UnloadSceneAsync unloading scene: " + sceneName);
		if (allScenes.TryGetValue(sceneName.ToLower(), out var value))
		{
			Addressables.UnloadSceneAsync(value.handle);
		}
		else
		{
			UnityEngine.Debug.LogError("AddressablesUtility.UnloadSceneAsync could not find scene: " + sceneName.ToString());
		}
	}

	private static bool Locate<T>(object key, out IList<IResourceLocation> locations)
	{
		Type typeFromHandle = typeof(T);
		foreach (IResourceLocator resourceLocator in Addressables.ResourceLocators)
		{
			if (resourceLocator.Locate(key, typeFromHandle, out locations))
			{
				return true;
			}
		}
		locations = null;
		return false;
	}

	public static void AppendDebug(StringBuilder sb)
	{
		_ = releaseQueue.Count;
		sb.AppendFormat("shouldDeferAsyncInstantiation: {0}", shouldDeferAsyncInstantiation);
		sb.AppendFormat("\nreleaseQueue.Count: {0}", releaseQueue.Count);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		foreach (KeyValuePair<string, Request> item in keyToHandle)
		{
			Request value = item.Value;
			AsyncOperationHandle<GameObject> handle = value.handle;
			if (handle.IsDone)
			{
				num++;
			}
			if (handle.IsValid())
			{
				num2++;
			}
			switch (handle.Status)
			{
			case AsyncOperationStatus.None:
				num3++;
				break;
			case AsyncOperationStatus.Succeeded:
				num4++;
				break;
			case AsyncOperationStatus.Failed:
				num5++;
				break;
			}
			if (value.releaseTime >= 0f)
			{
				num6++;
			}
		}
		sb.AppendFormat("\nkeyToHandle.Count: {0} IsDone: {1} valid: {2}\n    inProgress: {3}\n    succeeded: {4}\n    failed: {5}\n    pendingRelease: {6}", keyToHandle.Count, num, num2, num3, num4, num5, num6);
	}
}
