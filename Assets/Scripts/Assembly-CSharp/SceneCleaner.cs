using System.Collections;
using FMODUnity;
using UWE;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCleaner : MonoBehaviour
{
	public string loadScene;

	private static bool _isLoading;

	public static bool isLoading => _isLoading;

	public static void Open()
	{
		_isLoading = true;
		AddressablesUtility.LoadScene("Cleaner", LoadSceneMode.Single);
	}

	private IEnumerator Start()
	{
		_isLoading = false;
		if (!PlatformUtils.main.IsUserLoggedIn())
		{
			SaveLoadManager.main.Deinitialize();
		}
		GameObject[] array = Object.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (!(gameObject.transform.parent != null) && !(gameObject == base.gameObject) && !(gameObject.GetComponent<SceneCleanerPreserve>() != null) && !gameObject.GetComponent<SystemsSpawner>() && !gameObject.GetComponent<RuntimeManager>() && !gameObject.GetComponent<Language>())
			{
				Object.Destroy(gameObject);
			}
		}
		Base.Deinitialize();
		StreamTiming.Deinitialize();
		VoxelandData.OctNode.ClearPool();
		EcoRegionManager.Deinitialize();
		PDA.Deinitialize();
		FreezeTime.Deinitialize();
		uGUI.Deinitialize();
		PDAData.Deinitialize();
		TimeCapsuleContentProvider.Deinitialize();
		PDASounds.Deinitialize();
		AssetBundleManager.Deinitialize();
		PingManager.Deinitialize();
		ItemDragManager.Deinitialize();
		GameInfoIcon.Deinitialize();
		Language.Deinitialize();
		LanguageSDF.ClearDynamicFontAssets();
		SDFCutout.Deinitialize();
		LanguageCache.Deinitialize();
		if (DeferredSpawner.instance != null)
		{
			DeferredSpawner.instance.Reset();
		}
		GameObjectPool.ClearPools();
		AddressablesUtility.Reset();
		yield return null;
		Resources.UnloadUnusedAssets();
		Time.timeScale = 1f;
		yield return AddressablesUtility.LoadSceneAsync(loadScene, LoadSceneMode.Single);
	}
}
