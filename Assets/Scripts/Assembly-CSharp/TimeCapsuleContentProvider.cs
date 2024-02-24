using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Gendarme;
using LitJson;
using UnityEngine;

public class TimeCapsuleContentProvider
{
	public const string tokenTimeCapsules = "time_capsules";

	private const string tokenId = "_id";

	private const string tokenIsActive = "is_active";

	private const string tokenUpdatedAt = "modified_at";

	public const string tokenTitle = "title";

	public const string tokenText = "text";

	public const string tokenImage = "image";

	public const string tokenItems = "items";

	private const string tokenCopiesFound = "copies_found";

	private const int currentItemDataVersion = 1;

	private const string tokenVersion = "version";

	private const string tokenTechType = "techType";

	private const string tokenBatteryType = "batteryType";

	private const string tokenBatteryCharge = "batteryCharge";

	private static bool initialized = false;

	private static Dictionary<string, TimeCapsuleContent> data = new Dictionary<string, TimeCapsuleContent>();

	public static void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
		}
	}

	public static void Deinitialize()
	{
		initialized = false;
		data.Clear();
	}

	public static Dictionary<string, TimeCapsuleContent> Serialize()
	{
		return data;
	}

	public static void Deserialize(Dictionary<string, TimeCapsuleContent> data)
	{
		if (data != null)
		{
			TimeCapsuleContentProvider.data = data;
			SyncContent();
		}
	}

	public static void Set(string id, TimeCapsuleContent content)
	{
		data[id] = content;
	}

	public static TimeCapsuleContent DeserializeContent(JsonData timeCapsule, out string id, out string error)
	{
		TimeCapsuleContent timeCapsuleContent = new TimeCapsuleContent();
		try
		{
			ICollection<string> keys = timeCapsule.Keys;
			id = ((IJsonWrapper)timeCapsule["_id"]).GetString();
			timeCapsuleContent.title = ((IJsonWrapper)timeCapsule["title"]).GetString();
			if (keys.Contains("modified_at"))
			{
				timeCapsuleContent.updatedAt = ((IJsonWrapper)timeCapsule["modified_at"]).GetString();
			}
			else
			{
				timeCapsuleContent.updatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			}
			if (keys.Contains("is_active"))
			{
				timeCapsuleContent.isActive = ((IJsonWrapper)timeCapsule["is_active"]).GetBoolean();
			}
			if (keys.Contains("text"))
			{
				timeCapsuleContent.text = ((IJsonWrapper)timeCapsule["text"]).GetString();
			}
			if (keys.Contains("image"))
			{
				timeCapsuleContent.imageUrl = ((IJsonWrapper)timeCapsule["image"]).GetString();
			}
			if (keys.Contains("items"))
			{
				string @string = ((IJsonWrapper)timeCapsule["items"]).GetString();
				if (timeCapsuleContent.items != null)
				{
					timeCapsuleContent.items.Clear();
				}
				else
				{
					timeCapsuleContent.items = new List<TimeCapsuleItem>();
				}
				if (!DeserializeItems(@string, timeCapsuleContent.items, out var error2))
				{
					throw new ArgumentException($"Failed to deserialize json as items data. Error: {error2}\nJson value: '{@string}'");
				}
			}
			if (keys.Contains("copies_found"))
			{
				timeCapsuleContent.copiesFound = ((IJsonWrapper)timeCapsule["copies_found"]).GetInt();
			}
			else
			{
				timeCapsuleContent.copiesFound = 0;
			}
			error = null;
		}
		catch (Exception ex)
		{
			id = string.Empty;
			error = $"Exception deserializing time capsule JsonData: '{ex.ToString()}'";
		}
		return timeCapsuleContent;
	}

	public static bool GetData(string id, out string title, out string text, out string imageUrl)
	{
		if (data.TryGetValue(id, out var value))
		{
			title = value.title;
			text = value.text;
			imageUrl = value.imageUrl;
			return true;
		}
		title = string.Empty;
		text = string.Empty;
		imageUrl = string.Empty;
		return false;
	}

	public static string GetTitle(string id)
	{
		if (data.TryGetValue(id, out var value))
		{
			return value.title;
		}
		return null;
	}

	public static string GetText(string id)
	{
		if (data.TryGetValue(id, out var value))
		{
			return value.text;
		}
		return null;
	}

	public static string GetImageUrl(string id)
	{
		if (data.TryGetValue(id, out var value))
		{
			return value.imageUrl;
		}
		return null;
	}

	public static List<TimeCapsuleItem> GetItems(string id)
	{
		if (data.TryGetValue(id, out var value))
		{
			return value.items;
		}
		return null;
	}

	public static bool GetIsActive(string id)
	{
		if (data.TryGetValue(id, out var value))
		{
			return value.isActive;
		}
		return false;
	}

	public static int GetCopiesFound(string id)
	{
		if (data.TryGetValue(id, out var value))
		{
			return value.copiesFound;
		}
		return 0;
	}

	public static string GetAbsoluteImageUrl(string imageUrl)
	{
		return ScreenshotManager.Combine("https://s3.amazonaws.com/subnautica-unknownworlds-com/time-capsule-images/", imageUrl);
	}

	public static void SyncContent()
	{
		if (data == null || data.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		JsonWriter jsonWriter = new JsonWriter(stringBuilder);
		jsonWriter.WriteArrayStart();
		foreach (KeyValuePair<string, TimeCapsuleContent> datum in data)
		{
			string key = datum.Key;
			TimeCapsuleContent value = datum.Value;
			if (!string.IsNullOrEmpty(key) && value != null)
			{
				string updatedAt = value.updatedAt;
				jsonWriter.WriteObjectStart();
				jsonWriter.WritePropertyName("_id");
				jsonWriter.Write(key);
				jsonWriter.WritePropertyName("modified_at");
				jsonWriter.Write(updatedAt);
				jsonWriter.WriteObjectEnd();
			}
		}
		jsonWriter.WriteArrayEnd();
		PlayerTimeCapsule.main.SyncContent(stringBuilder.ToString());
	}

	public static string SerializeItems(IItemsContainer container)
	{
		StringBuilder stringBuilder = new StringBuilder();
		JsonWriter jsonWriter = new JsonWriter(stringBuilder);
		jsonWriter.WriteObjectStart();
		jsonWriter.WritePropertyName("version");
		jsonWriter.Write(1);
		jsonWriter.WritePropertyName("items");
		jsonWriter.WriteArrayStart();
		IEnumerator<InventoryItem> enumerator = container.GetEnumerator();
		while (enumerator.MoveNext())
		{
			InventoryItem current = enumerator.Current;
			if (current == null)
			{
				continue;
			}
			Pickupable item = current.item;
			if (item == null)
			{
				continue;
			}
			TechType techType = item.GetTechType();
			jsonWriter.WriteObjectStart();
			jsonWriter.WritePropertyName("techType");
			jsonWriter.Write((int)techType);
			EnergyMixin component = item.GetComponent<EnergyMixin>();
			if (component != null)
			{
				TechType number = TechType.None;
				float num = 0f;
				GameObject batteryGameObject = component.GetBatteryGameObject();
				if (batteryGameObject != null)
				{
					IBattery component2 = batteryGameObject.GetComponent<IBattery>();
					if (component2 != null)
					{
						number = CraftData.GetTechType(batteryGameObject);
						num = component2.charge / component2.capacity;
					}
				}
				jsonWriter.WritePropertyName("batteryType");
				jsonWriter.Write((int)number);
				jsonWriter.WritePropertyName("batteryCharge");
				jsonWriter.Write(num);
			}
			jsonWriter.WriteObjectEnd();
		}
		jsonWriter.WriteArrayEnd();
		jsonWriter.WriteObjectEnd();
		return stringBuilder.ToString();
	}

	public static bool DeserializeItems(string json, List<TimeCapsuleItem> items, out string error)
	{
		if (items != null)
		{
			items.Clear();
		}
		else
		{
			items = new List<TimeCapsuleItem>();
		}
		error = string.Empty;
		if (string.IsNullOrEmpty(json))
		{
			error = "json string is null or empty.";
			return false;
		}
		try
		{
			JsonData jsonData = JsonMapper.ToObject(json);
			if (jsonData.IsObject)
			{
				int num = -1;
				if (((IDictionary)jsonData).Contains((object)"version"))
				{
					JsonData jsonData2 = jsonData["version"];
					if (jsonData2.IsInt)
					{
						num = ((IJsonWrapper)jsonData2).GetInt();
					}
				}
				if (num == 1)
				{
					return DeserializeItemsV1(jsonData, items, out error);
				}
				error = $"Unknown json items data version '{num.ToString()}'";
			}
		}
		catch (Exception ex)
		{
			error = $"Exception parsing sever response json: '{ex.ToString()}'";
		}
		return false;
	}

	[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidUnnecessarySpecializationRule")]
	private static bool DeserializeItemsV1(JsonData json, List<TimeCapsuleItem> items, out string error)
	{
		error = string.Empty;
		if (!((IDictionary)json).Contains((object)"items"))
		{
			error = string.Format("json contains no {0} field", "items");
			return false;
		}
		JsonData jsonData = json["items"];
		if (!jsonData.IsArray)
		{
			error = string.Format("{0} json field is not an array", "items");
			return false;
		}
		for (int i = 0; i < jsonData.Count; i++)
		{
			JsonData jsonData2 = jsonData[i];
			if (jsonData2.IsObject)
			{
				bool flag = true;
				TimeCapsuleItem timeCapsuleItem = new TimeCapsuleItem();
				foreach (string key in jsonData2.Keys)
				{
					IJsonWrapper jsonWrapper = jsonData2[key];
					switch (key)
					{
					case "techType":
						if (jsonWrapper.IsInt)
						{
							timeCapsuleItem.techType = (TechType)jsonWrapper.GetInt();
							break;
						}
						flag = false;
						Debug.LogErrorFormat("{0} json field is of type {1} but expected int", "techType", jsonWrapper.GetJsonType().ToString());
						break;
					case "batteryType":
						if (jsonWrapper.IsInt)
						{
							timeCapsuleItem.hasBattery = true;
							timeCapsuleItem.batteryType = (TechType)jsonWrapper.GetInt();
							break;
						}
						flag = false;
						Debug.LogErrorFormat("{0} json field is of type {1} but expected int", "batteryType", jsonWrapper.GetJsonType().ToString());
						break;
					case "batteryCharge":
						if (jsonWrapper.IsDouble)
						{
							timeCapsuleItem.batteryCharge = (float)jsonWrapper.GetDouble();
							break;
						}
						if (jsonWrapper.IsInt)
						{
							timeCapsuleItem.batteryCharge = jsonWrapper.GetInt();
							break;
						}
						flag = false;
						Debug.LogErrorFormat("{0} json field is of type {1} but expected double", "batteryCharge", jsonWrapper.GetJsonType().ToString());
						break;
					}
					if (!flag)
					{
						break;
					}
				}
				if (flag && timeCapsuleItem.IsValid())
				{
					items.Add(timeCapsuleItem);
				}
			}
			else
			{
				Debug.LogErrorFormat("Item at index {0} in json data is of type {1}, but expected JsonType.Object", i.ToString(), jsonData2.GetJsonType().ToString());
			}
		}
		return true;
	}
}
