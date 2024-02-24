using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UWE;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class LanguageSDF
{
	public static readonly string[] assetEntryPoints = new string[3] { "Assets/Fonts/AddressableResources/Fonts & Materials/Aller_Rg SDF.asset", "Assets/Fonts/AddressableResources/Fonts & Materials/Aller_W_Bd SDF.asset", "Assets/Fonts/AddressableResources/Sprite Atlases/InputSymbols.asset" };

	private static TMP_Asset[] allAssets;

	private static TMP_FontAsset[] fontAssets;

	private static TMP_SpriteAsset[] spriteAssets;

	private static string cachedLanguage;

	private static readonly Dictionary<TMP_FontAsset, TMP_FontAsset> clones = new Dictionary<TMP_FontAsset, TMP_FontAsset>();

	private static readonly List<TMP_FontAsset> modifiedAssets = new List<TMP_FontAsset>();

	private static IEnumerator loadFontAssetsRoutine;

	private static Coroutine initializeAsyncRoutine;

	private static readonly Dictionary<TMP_FontAsset, Font> sourceFontFiles = new Dictionary<TMP_FontAsset, Font>();

	public static void Initialize(Language language)
	{
		if (allAssets == null)
		{
			if (initializeAsyncRoutine == null)
			{
				initializeAsyncRoutine = CoroutineHost.StartCoroutine(InitializeAsync(language));
			}
		}
		else
		{
			InitializeInternal(language);
		}
	}

	private static IEnumerator InitializeAsync(Language language)
	{
		yield return PreloadFontAssets();
		InitializeInternal(language);
		initializeAsyncRoutine = null;
	}

	public static IEnumerator PreloadFontAssets()
	{
		if (loadFontAssetsRoutine == null)
		{
			loadFontAssetsRoutine = LoadFontAssets();
		}
		yield return loadFontAssetsRoutine;
		loadFontAssetsRoutine = null;
		List<TMP_FontAsset> list = new List<TMP_FontAsset>();
		List<TMP_SpriteAsset> list2 = new List<TMP_SpriteAsset>();
		for (int i = 0; i < allAssets.Length; i++)
		{
			TMP_Asset tMP_Asset = allAssets[i];
			TMP_FontAsset tMP_FontAsset = tMP_Asset as TMP_FontAsset;
			if (tMP_FontAsset != null)
			{
				if (list.Count == 0)
				{
					SetDefaultFontAsset(tMP_FontAsset);
				}
				list.Add(tMP_FontAsset);
				continue;
			}
			TMP_SpriteAsset tMP_SpriteAsset = tMP_Asset as TMP_SpriteAsset;
			if (!(tMP_SpriteAsset != null))
			{
				continue;
			}
			if (list2.Count == 0)
			{
				SetDefaultSpriteAsset(tMP_SpriteAsset);
			}
			else
			{
				TMP_SpriteAsset defaultSpriteAsset = TMP_Settings.defaultSpriteAsset;
				if (defaultSpriteAsset != null)
				{
					defaultSpriteAsset.fallbackSpriteAssets.Add(tMP_SpriteAsset);
				}
				else
				{
					Debug.LogErrorFormat("TMP_Settings.defaultSpriteAsset was not set. Can't add fallback sprite asset!");
				}
			}
			list2.Add(tMP_SpriteAsset);
		}
		fontAssets = list.ToArray();
		spriteAssets = list2.ToArray();
	}

	private static IEnumerator LoadFontAssets()
	{
		AsyncOperationHandle<TMP_Asset>[] operations = new AsyncOperationHandle<TMP_Asset>[assetEntryPoints.Length];
		for (int j = 0; j < assetEntryPoints.Length; j++)
		{
			operations[j] = AddressablesUtility.LoadAsync<TMP_Asset>(assetEntryPoints[j]);
		}
		for (int i = 0; i < operations.Length; i++)
		{
			yield return operations[i];
		}
		allAssets = new TMP_Asset[assetEntryPoints.Length];
		for (int k = 0; k < operations.Length; k++)
		{
			AsyncOperationHandle<TMP_Asset> asyncOperationHandle = operations[k];
			if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
			{
				TMP_Asset result = asyncOperationHandle.Result;
				allAssets[k] = result;
				continue;
			}
			Debug.LogErrorFormat("Unable to load addressable asset at path '{0}'. Status: {1}. Exception: {2}", assetEntryPoints[k], asyncOperationHandle.Status, asyncOperationHandle.OperationException);
		}
	}

	private static void SetDefaultFontAsset(TMP_FontAsset fontAsset)
	{
		if (fontAsset == null)
		{
			Debug.LogError("Font asset cannot be null!");
			return;
		}
		TMP_Settings instance = TMP_Settings.instance;
		FieldInfo field = typeof(TMP_Settings).GetField("m_defaultFontAsset", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field == null)
		{
			Debug.LogErrorFormat("m_defaultFontAsset field is not found in TMP_Settings! TextMeshPro will unnecessarily allocate textures in persistent assets!");
		}
		else
		{
			field.SetValue(instance, fontAsset);
		}
	}

	private static void SetDefaultSpriteAsset(TMP_SpriteAsset spriteAsset)
	{
		if (spriteAsset == null)
		{
			Debug.LogError("Sprite asset cannot be null!");
			return;
		}
		TMP_Settings instance = TMP_Settings.instance;
		FieldInfo field = typeof(TMP_Settings).GetField("m_defaultSpriteAsset", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field == null)
		{
			Debug.LogErrorFormat("m_defaultSpriteAsset field is not found in TMP_Settings!");
		}
		else
		{
			field.SetValue(instance, spriteAsset);
		}
	}

	private static void InitializeInternal(Language language)
	{
		string currentLanguage = language.GetCurrentLanguage();
		if (cachedLanguage == currentLanguage)
		{
			return;
		}
		if (cachedLanguage != null)
		{
			Deinitialize();
		}
		cachedLanguage = currentLanguage;
		string characters = new string(ToArray(language.GetAllUsedChars()));
		HashSet<TMP_FontAsset> cleared = new HashSet<TMP_FontAsset>();
		for (int i = 0; i < fontAssets.Length; i++)
		{
			ClearFontAsset(fontAssets[i], cleared);
		}
		for (int j = 0; j < fontAssets.Length; j++)
		{
			TMP_FontAsset tMP_FontAsset = fontAssets[j];
			if (!tMP_FontAsset.TryAddCharacters(characters, out var missingCharacters))
			{
				List<TMP_FontAsset> fallbackFontAssetTable = tMP_FontAsset.fallbackFontAssetTable;
				for (int k = 0; k < fallbackFontAssetTable.Count && !fallbackFontAssetTable[k].TryAddCharacters(missingCharacters, out missingCharacters); k++)
				{
				}
			}
			CloneFontAsset(tMP_FontAsset);
		}
		foreach (KeyValuePair<TMP_FontAsset, TMP_FontAsset> clone in clones)
		{
			TMP_FontAsset key = clone.Key;
			_ = key.sourceFontFile;
			key.atlasPopulationMode = AtlasPopulationMode.Static;
		}
		language.NotifyLanguageChanged();
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfTypeAll(typeof(TextMeshProUGUI));
		for (int l = 0; l < array.Length; l++)
		{
			TextMeshProUGUI textMeshProUGUI = array[l] as TextMeshProUGUI;
			if (textMeshProUGUI != null)
			{
				textMeshProUGUI.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
			}
		}
		for (int m = 0; m < fontAssets.Length; m++)
		{
			TMP_FontAsset tMP_FontAsset2 = fontAssets[m];
			TMP_FontAsset item = clones[tMP_FontAsset2];
			tMP_FontAsset2.fallbackFontAssetTable.Add(item);
			modifiedAssets.Add(tMP_FontAsset2);
		}
	}

	public static void Deinitialize()
	{
		if (cachedLanguage == null)
		{
			return;
		}
		cachedLanguage = null;
		foreach (KeyValuePair<TMP_FontAsset, TMP_FontAsset> clone in clones)
		{
			TMP_FontAsset key = clone.Key;
			_ = key.sourceFontFile;
			TMP_FontAsset value = clone.Value;
			key.atlasPopulationMode = AtlasPopulationMode.Dynamic;
			key.ClearFontAssetData(setAtlasSizeToZero: true);
			value.ClearFontAssetData(setAtlasSizeToZero: true);
		}
		for (int i = 0; i < modifiedAssets.Count; i++)
		{
			List<TMP_FontAsset> fallbackFontAssetTable = modifiedAssets[i].fallbackFontAssetTable;
			fallbackFontAssetTable.RemoveAt(fallbackFontAssetTable.Count - 1);
		}
		modifiedAssets.Clear();
	}

	public static void ClearDynamicFontAssets()
	{
		foreach (KeyValuePair<TMP_FontAsset, TMP_FontAsset> clone in clones)
		{
			clone.Value.ClearFontAssetData(setAtlasSizeToZero: true);
		}
	}

	private static TMP_FontAsset CloneFontAsset(TMP_FontAsset src)
	{
		if (!sourceFontFiles.TryGetValue(src, out var value))
		{
			value = src.sourceFontFile;
			sourceFontFiles[src] = value;
		}
		if (!clones.TryGetValue(src, out var value2))
		{
			int atlasWidth = System.Math.Min(1024, src.atlasWidth);
			int atlasHeight = System.Math.Min(1024, src.atlasHeight);
			value2 = TMP_FontAsset.CreateFontAsset(value, src.faceInfo.pointSize, src.atlasPadding, src.atlasRenderMode, atlasWidth, atlasHeight);
			value2.name = $"{src.name} (Dynamic Clone)";
			clones.Add(src, value2);
			value2.fallbackFontAssetTable = new List<TMP_FontAsset>(src.fallbackFontAssetTable.Count);
			for (int i = 0; i < src.fallbackFontAssetTable.Count; i++)
			{
				value2.fallbackFontAssetTable.Add(CloneFontAsset(src.fallbackFontAssetTable[i]));
			}
		}
		return value2;
	}

	private static void ClearFontAsset(TMP_FontAsset fontAsset, HashSet<TMP_FontAsset> cleared)
	{
		if (!cleared.Contains(fontAsset))
		{
			cleared.Add(fontAsset);
			fontAsset.ClearFontAssetData(setAtlasSizeToZero: true);
		}
		for (int i = 0; i < fontAsset.fallbackFontAssetTable.Count; i++)
		{
			ClearFontAsset(fontAsset.fallbackFontAssetTable[i], cleared);
		}
	}

	private static char[] ToArray(HashSet<char> chars)
	{
		int count = chars.Count;
		char[] array = new char[count];
		chars.CopyTo(array, 0, count);
		Array.Sort(array);
		return array;
	}

	private static void ClearUnknownAssets()
	{
		TMP_FontAsset[] array = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
		foreach (TMP_FontAsset tMP_FontAsset in array)
		{
			if (tMP_FontAsset != null && !clones.ContainsKey(tMP_FontAsset) && !clones.ContainsValue(tMP_FontAsset))
			{
				tMP_FontAsset.ClearFontAssetData(setAtlasSizeToZero: true);
			}
		}
	}
}
