using System.Collections;
using UnityEngine;

public class MenuLogo : MonoBehaviour
{
	private const string logoAssetBundleName = "logos";

	private GameObject logoObject;

	private string logoPrefabName;

	private IAssetBundleWrapper logoAssetBundle;

	private IEnumerator Start()
	{
		IAssetBundleWrapperCreateRequest loadRequest = AssetBundleManager.LoadBundleAsync("logos");
		yield return loadRequest;
		logoAssetBundle = loadRequest.assetBundle;
		string currentLanguage = Language.main.GetCurrentLanguage();
		string prefabNameForLanguage = GetPrefabNameForLanguage(currentLanguage);
		yield return SetLogoPrefabAsync(prefabNameForLanguage);
		Language.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		Language.OnLanguageChanged -= OnLanguageChanged;
		if (logoAssetBundle != null)
		{
			AssetBundleManager.UnloadBundle("logos");
		}
	}

	private string GetPrefabNameForLanguage(string language)
	{
		if (language == "Chinese (Simplified)")
		{
			return "Assets/Logos/subnautica_logo_chinese.prefab";
		}
		return "Assets/Logos/subnautica_logo.prefab";
	}

	private void OnLanguageChanged()
	{
		StartSyncLogoObjectWithLanguage();
	}

	private void StartSyncLogoObjectWithLanguage()
	{
		if (logoAssetBundle != null)
		{
			string currentLanguage = Language.main.GetCurrentLanguage();
			string prefabNameForLanguage = GetPrefabNameForLanguage(currentLanguage);
			if (prefabNameForLanguage != logoPrefabName)
			{
				StartCoroutine(SetLogoPrefabAsync(prefabNameForLanguage));
			}
		}
	}

	private IEnumerator SetLogoPrefabAsync(string prefabName)
	{
		Object.DestroyObject(logoObject);
		IAssetBundleWrapperRequest assetLoadRequest = logoAssetBundle.LoadAssetAsync<GameObject>(prefabName);
		yield return assetLoadRequest;
		GameObject gameObject = assetLoadRequest.asset as GameObject;
		if (gameObject != null)
		{
			logoObject = Object.Instantiate(gameObject);
			if (logoObject != null)
			{
				logoObject.transform.SetParent(base.transform, worldPositionStays: false);
			}
		}
		logoPrefabName = prefabName;
	}
}
