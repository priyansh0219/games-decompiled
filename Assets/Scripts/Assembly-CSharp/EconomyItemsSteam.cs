using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;

public class EconomyItemsSteam : IEconomyItems
{
	private const string assetServerUrl = "https://economy.unknownworlds.com/";

	private readonly Dictionary<string, TechType> itemMap = new Dictionary<string, TechType>
	{
		{
			"1",
			TechType.SpecialHullPlate
		},
		{
			"1388",
			TechType.DevTestItem
		},
		{
			"1389",
			TechType.BikemanHullPlate
		},
		{
			"1390",
			TechType.EatMyDictionHullPlate
		},
		{
			"1391",
			TechType.DioramaHullPlate
		},
		{
			"1392",
			TechType.MarkiplierHullPlate
		},
		{
			"1393",
			TechType.MuyskermHullPlate
		},
		{
			"1394",
			TechType.LordMinionHullPlate
		},
		{
			"1395",
			TechType.JackSepticEyeHullPlate
		},
		{
			"1397",
			TechType.IGPHullPlate
		},
		{
			"1493",
			TechType.GilathissHullPlate
		},
		{
			"1494",
			TechType.Marki1
		},
		{
			"1495",
			TechType.Marki2
		},
		{
			"1496",
			TechType.JackSepticEye
		},
		{
			"1497",
			TechType.EatMyDiction
		}
	};

	private string steamId;

	private Dictionary<TechType, Dictionary<string, string>> items = new Dictionary<TechType, Dictionary<string, string>>();

	public bool IsReady { get; private set; }

	public EconomyItemsSteam(string steamId)
	{
		this.steamId = steamId;
	}

	public IEnumerator InitializeAsync()
	{
		items.Clear();
		IsReady = false;
		string contextId = null;
		string uri = "https://economy.unknownworlds.com/api/GetContexts/v0001?appid=264710&steamid=" + steamId + "&parent=0";
		using (UnityWebRequest webRequest2 = UnityWebRequest.Get(uri))
		{
			webRequest2.timeout = 10;
			yield return webRequest2.SendWebRequest();
			if (webRequest2.isNetworkError || webRequest2.isHttpError)
			{
				Debug.LogError(webRequest2.error);
			}
			else
			{
				try
				{
					JsonData jsonData = JsonMapper.ToObject(webRequest2.downloadHandler.text)["result"]["contexts"];
					if (jsonData.Count > 0)
					{
						JsonData jsonData2 = jsonData[0];
						if (jsonData2 != null)
						{
							contextId = ((long)jsonData2["id"]).ToString();
						}
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
		if (!string.IsNullOrEmpty(contextId))
		{
			string uri2 = "https://economy.unknownworlds.com/api/GetContextContents/v0001?appid=264710&steamid=" + steamId + "&contextid=" + contextId + "&include_dates=true";
			using (UnityWebRequest webRequest2 = UnityWebRequest.Get(uri2))
			{
				webRequest2.timeout = 10;
				yield return webRequest2.SendWebRequest();
				if (webRequest2.isNetworkError || webRequest2.isHttpError)
				{
					Debug.LogError(webRequest2.error);
				}
				else
				{
					try
					{
						foreach (JsonData item in (IEnumerable)JsonMapper.ToObject(webRequest2.downloadHandler.text)["result"]["assets"])
						{
							string text = null;
							Dictionary<string, string> dictionary = null;
							foreach (JsonData item2 in (IEnumerable)item["class"])
							{
								string text2 = (string)item2["name"];
								string text3 = (string)item2["value"];
								if (text2 == "base_class")
								{
									text = text3;
								}
								else if (text2 == "class_id" && text == null)
								{
									text = text3;
								}
								if (dictionary == null)
								{
									dictionary = new Dictionary<string, string>();
								}
								dictionary.Add(text2, text3);
							}
							if (!string.IsNullOrEmpty(text) && itemMap.TryGetValue(text, out var value) && !items.ContainsKey(value))
							{
								items.Add(value, dictionary);
							}
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
					}
				}
			}
		}
		IsReady = true;
	}

	public bool HasItem(TechType techType)
	{
		return items.ContainsKey(techType);
	}

	public string GetItemProperty(TechType techType, string key)
	{
		if (items.TryGetValue(techType, out var value) && value != null && value.TryGetValue(key, out var value2))
		{
			return value2;
		}
		return string.Empty;
	}
}
