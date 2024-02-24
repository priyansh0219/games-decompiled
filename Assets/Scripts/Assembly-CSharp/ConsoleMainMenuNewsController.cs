using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ConsoleMainMenuNewsController : MonoBehaviour
{
	[SerializeField]
	[AssertNotNull]
	private LayoutElement newsContainer;

	[SerializeField]
	[AssertNotNull]
	private ConsoleMainMenuNews newsPrefab;

	private GameObject newsGameObject;

	private Coroutine loadCoroutine;

	private void DisableNews()
	{
		if (loadCoroutine != null)
		{
			StopCoroutine(loadCoroutine);
			loadCoroutine = null;
		}
		if (newsGameObject != null)
		{
			UnityEngine.Object.Destroy(newsGameObject);
			newsGameObject = null;
		}
		newsContainer.enabled = false;
	}

	private string GetDefaultUrl()
	{
		return PlatformUtils.main.GetServices().GetDefaultNewsUrl();
	}

	private void OnEnable()
	{
		if (newsGameObject == null && MiscSettings.newsEnabled)
		{
			if (loadCoroutine == null)
			{
				loadCoroutine = StartCoroutine(Load(GetDefaultUrl()));
			}
		}
		else if (newsGameObject != null && !MiscSettings.newsEnabled)
		{
			DisableNews();
		}
	}

	private IEnumerator Load(string url)
	{
		if (!MiscSettings.newsEnabled)
		{
			DisableNews();
			yield break;
		}
		bool hasNews = false;
		PlatformServices platformServices = null;
		while (platformServices == null)
		{
			platformServices = PlatformUtils.main.GetServices();
			yield return null;
		}
		yield return platformServices.TryEnsureServerAccessAsync();
		if (platformServices.CanAccessServers())
		{
			UnityWebRequest webRequest = UnityWebRequest.Get(url);
			yield return webRequest.SendWebRequest();
			if (webRequest.isNetworkError || webRequest.isHttpError)
			{
				DisplayErrorAndDisableNews("Failed to load news. UnityWebRequest error - " + webRequest.error);
			}
			else
			{
				try
				{
					JsonData jsonData = JsonMapper.ToObject(webRequest.downloadHandler.text);
					if (!jsonData.IsArray)
					{
						DisplayNewsEntry(jsonData);
						hasNews = true;
					}
					else if (jsonData.Count > 0)
					{
						DisplayNewsEntry(jsonData[0]);
						hasNews = true;
					}
				}
				catch (Exception ex)
				{
					DisplayErrorAndDisableNews(ex.Message);
				}
			}
		}
		newsContainer.enabled = hasNews;
		loadCoroutine = null;
	}

	private void DisplayNewsEntry(JsonData newsEntryData)
	{
		ConsoleMainMenuNews consoleMainMenuNews = UnityEngine.Object.Instantiate(newsPrefab);
		consoleMainMenuNews.transform.SetParent(base.transform, worldPositionStays: false);
		newsGameObject = consoleMainMenuNews.gameObject;
		consoleMainMenuNews.Initialize(ExtractTagData(newsEntryData, "header"), ExtractTagData(newsEntryData, "text"), ExtractTagData(newsEntryData, "created_at"), ExtractTagData(newsEntryData, "read_more_url"), ExtractTagData(newsEntryData, "image_url"));
	}

	private string ExtractTagData(JsonData data, string tag)
	{
		JsonData jsonData = null;
		string text = "";
		try
		{
			jsonData = data[tag];
		}
		catch (KeyNotFoundException ex)
		{
			jsonData = null;
			DisplayErrorWithoutDisablingNews("Tag: " + tag + " was not found in the Json: " + ex.Message);
		}
		try
		{
			text = jsonData.ToString();
		}
		catch (NullReferenceException ex2)
		{
			text = "";
			DisplayErrorWithoutDisablingNews("The value for tag: " + tag + " was null in the Json: " + ex2.Message);
		}
		return text;
	}

	private void DisplayErrorWithoutDisablingNews(string message)
	{
		message += "\nNo need to disable news from this";
		Debug.LogWarning(message);
	}

	private void DisplayErrorAndDisableNews(string message)
	{
		DisableNews();
		Debug.LogError(message);
	}
}
