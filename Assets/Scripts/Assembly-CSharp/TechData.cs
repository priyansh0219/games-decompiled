using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Gendarme;
using LitJson;
using UnityEngine;

[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
[SuppressMessage("Gendarme.Rules.Performance", "PreferLiteralOverInitOnlyFieldsRule")]
public static class TechData
{
	private class Reader : StringReader
	{
		private int _line = 1;

		private int _column;

		private const int LF = 10;

		public int line => _line;

		public int column => _column;

		public Reader(string text)
			: base(text)
		{
		}

		public override int Read()
		{
			int num = base.Read();
			if (num >= 0)
			{
				AdvancePosition((char)num);
			}
			return num;
		}

		private void AdvancePosition(char c)
		{
			if (c == '\n')
			{
				_line++;
				_column = 0;
			}
			else
			{
				_column++;
			}
		}
	}

	public const string dataPath = "Balance/TechData";

	private const bool prettyPrint = true;

	private const bool comments = true;

	private static Dictionary<string, int> propertyToID = new Dictionary<string, int>();

	private static Dictionary<int, string> idToProperty = new Dictionary<int, string>();

	private static int propertyIndex = 1;

	private static Dictionary<TechType, JsonValue> entries = new Dictionary<TechType, JsonValue>();

	public static int propertyTechType = PropertyToID("techType");

	public static int propertyItemSize = PropertyToID("itemSize");

	public static int propertyX = PropertyToID("x");

	public static int propertyY = PropertyToID("y");

	public static int propertyBackgroundType = PropertyToID("backgroundType");

	public static int propertyEquipmentType = PropertyToID("equipmentType");

	public static int propertySlotType = PropertyToID("slotType");

	public static int propertyCraftTime = PropertyToID("craftTime");

	public static int propertyCraftAmount = PropertyToID("craftAmount");

	public static int propertyIngredients = PropertyToID("ingredients");

	public static int propertyAmount = PropertyToID("amount");

	public static int propertyLinkedItems = PropertyToID("linkedItems");

	public static int propertyProcessed = PropertyToID("processed");

	public static int propertyBuildable = PropertyToID("buildable");

	public static int propertySoundPickup = PropertyToID("soundPickup");

	public static int propertySoundDrop = PropertyToID("soundDrop");

	public static int propertySoundUse = PropertyToID("soundUse");

	public static int propertyHarvestType = PropertyToID("harvestType");

	public static int propertyHarvestOutput = PropertyToID("harvestOutput");

	public static int propertyHarvestFinalCutBonus = PropertyToID("harvestFinalCutBonus");

	public static int propertyMaxCharge = PropertyToID("maxCharge");

	public static int propertyEnergyCost = PropertyToID("energyCost");

	public static int propertyPoweredPrefab = PropertyToID("poweredPrefab");

	public static int propertyEntries = PropertyToID("entries");

	public static int propertyIsExpanded = PropertyToID("isExpanded");

	public static int propertyArrayItem = PropertyToID("arrayItem");

	private static readonly Vector2int defaultItemSize = new Vector2int(1, 1);

	private static readonly CraftData.BackgroundType defaultBackgroundType = CraftData.BackgroundType.Normal;

	private static readonly EquipmentType defaultEquipmentType = EquipmentType.None;

	private static readonly QuickSlotType defaultSlotType = QuickSlotType.None;

	private static readonly float defaultCraftTime = 0f;

	private static readonly int defaultCraftAmount = 1;

	private static readonly TechType defaultProcessed = TechType.None;

	private static readonly bool defaultBuildable = false;

	private static readonly string defaultSoundPickup = "event:/loot/pickup_default";

	private static readonly string defaultSoundDrop = "event:/tools/pda/drop_item";

	private static readonly string defaultSoundUse = "event:/player/eat";

	private static readonly HarvestType defaultHarvestType = HarvestType.None;

	private static readonly TechType defaultHarvestOutput = TechType.None;

	private static readonly int defaultHarvestFinalCutBonus = 0;

	private static readonly float defaultMaxCharge = -1f;

	private static readonly float defaultEnergyCost = 0f;

	private static readonly string defaultPoweredPrefab = string.Empty;

	public static JsonValue defaults = new JsonValue
	{
		{
			propertyTechType,
			new JsonValue(0)
		},
		{
			propertyItemSize,
			new JsonValue
			{
				{
					propertyX,
					new JsonValue(defaultItemSize.x)
				},
				{
					propertyY,
					new JsonValue(defaultItemSize.y)
				}
			}
		},
		{
			propertyBackgroundType,
			new JsonValue((int)defaultBackgroundType)
		},
		{
			propertyEquipmentType,
			new JsonValue((int)defaultEquipmentType)
		},
		{
			propertySlotType,
			new JsonValue((int)defaultSlotType)
		},
		{
			propertyCraftTime,
			new JsonValue(defaultCraftTime)
		},
		{
			propertyCraftAmount,
			new JsonValue(defaultCraftAmount)
		},
		{
			propertyIngredients,
			new JsonValue(JsonValue.Type.Array)
		},
		{
			propertyLinkedItems,
			new JsonValue(JsonValue.Type.Array)
		},
		{
			propertyProcessed,
			new JsonValue((int)defaultProcessed)
		},
		{
			propertyBuildable,
			new JsonValue(defaultBuildable)
		},
		{
			propertySoundPickup,
			new JsonValue(defaultSoundPickup)
		},
		{
			propertySoundDrop,
			new JsonValue(defaultSoundDrop)
		},
		{
			propertySoundUse,
			new JsonValue(defaultSoundUse)
		},
		{
			propertyHarvestType,
			new JsonValue((int)defaultHarvestType)
		},
		{
			propertyHarvestOutput,
			new JsonValue((int)defaultHarvestOutput)
		},
		{
			propertyHarvestFinalCutBonus,
			new JsonValue(defaultHarvestFinalCutBonus)
		},
		{
			propertyMaxCharge,
			new JsonValue(defaultMaxCharge)
		},
		{
			propertyEnergyCost,
			new JsonValue(defaultEnergyCost)
		},
		{
			propertyPoweredPrefab,
			new JsonValue(defaultPoweredPrefab)
		}
	};

	public static List<int> defaultProperties = new List<int>(defaults.Keys);

	private static JsonWriter writer;

	private static List<int> propertyPath = new List<int>();

	private static Dictionary<TechType, ReadOnlyCollection<Ingredient>> cachedIngredients = new Dictionary<TechType, ReadOnlyCollection<Ingredient>>();

	private static Dictionary<TechType, ReadOnlyCollection<TechType>> cachedLinkedItems = new Dictionary<TechType, ReadOnlyCollection<TechType>>();

	public static void Initialize()
	{
		Clear();
		TextAsset textAsset = Resources.Load<TextAsset>("Balance/TechData");
		if (textAsset != null)
		{
			string text = textAsset.text;
			if (!string.IsNullOrEmpty(text))
			{
				Deserialize(text);
			}
			else
			{
				Debug.LogError(string.Format("TechData : Failed to load data! Contents of TextAsset file at path '{0}' is null or empty!", "Balance/TechData"));
			}
		}
		else
		{
			Debug.LogError(string.Format("TechData : Failed to load data! TextAsset at path '{0}' is not found!", "Balance/TechData"));
		}
		Cache();
	}

	public static void Uninitialize()
	{
		Clear();
	}

	public static string Save()
	{
		TrimDefaults(trimUnknown: false);
		return Serialize();
	}

	public static void Clear()
	{
		entries.Clear();
		ClearCache();
	}

	private static void ClearCache()
	{
		cachedIngredients.Clear();
		cachedLinkedItems.Clear();
	}

	public static void Add(TechType techType, JsonValue entry)
	{
		entries.Add(techType, entry);
	}

	public static bool TryGetDefaultValue(List<int> path, out JsonValue value)
	{
		value = defaults;
		if (path != null)
		{
			for (int i = 0; i < path.Count; i++)
			{
				if (!value.TryGetValue(path[i], out value))
				{
					value = null;
					return false;
				}
			}
		}
		return true;
	}

	public static bool TryGetDefaultValue(int id, out JsonValue value)
	{
		return defaults.TryGetValue(id, out value);
	}

	public static bool TryGetValue(TechType techType, out JsonValue value)
	{
		return entries.TryGetValue(techType, out value);
	}

	public static bool Contains(TechType techType)
	{
		return entries.ContainsKey(techType);
	}

	public static Dictionary<TechType, JsonValue>.Enumerator GetEnumerator()
	{
		return entries.GetEnumerator();
	}

	private static string Serialize()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("// Use TechData Editor (Window > TechData Editor) instead of modifying this file directly");
		writer = new JsonWriter(stringBuilder);
		writer.PrettyPrint = true;
		writer.WriteObjectStart();
		writer.WritePropertyName(IDToProperty(propertyEntries));
		writer.WriteArrayStart();
		List<TechType> list = new List<TechType>(entries.Keys);
		for (int i = 0; i < TechTypeExtensions.sTechTypes.Count; i++)
		{
			TechType techType = TechTypeExtensions.sTechTypes[i];
			if (entries.TryGetValue(techType, out var value))
			{
				list.Remove(techType);
				Write(value);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			TechType key = list[j];
			Write(entries[key]);
		}
		writer.WriteArrayEnd();
		writer.WriteObjectEnd();
		return stringBuilder.ToString();
	}

	private static void Write(JsonValue json)
	{
		switch (json.GetValueType())
		{
		case JsonValue.Type.None:
			WriteComment(GetValueComment(json), delegate
			{
				writer.Write(null);
			});
			break;
		case JsonValue.Type.Bool:
			WriteComment(GetValueComment(json), delegate
			{
				writer.Write(json.GetBool());
			});
			break;
		case JsonValue.Type.Int:
			WriteComment(GetValueComment(json), delegate
			{
				writer.Write(json.GetInt());
			});
			break;
		case JsonValue.Type.Long:
			WriteComment(GetValueComment(json), delegate
			{
				writer.Write(json.GetLong(0L));
			});
			break;
		case JsonValue.Type.Double:
			WriteComment(GetValueComment(json), delegate
			{
				writer.Write(json.GetDouble());
			});
			break;
		case JsonValue.Type.String:
			WriteComment(GetValueComment(json), delegate
			{
				writer.Write(json.GetString());
			});
			break;
		case JsonValue.Type.Array:
			WriteArray(json);
			break;
		case JsonValue.Type.Object:
			WriteObject(json);
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private static void WriteComment(string comment, Action callback)
	{
		if (!string.IsNullOrEmpty(comment) && writer.PrettyPrint)
		{
			writer.WriteComment(comment, callback);
		}
		else
		{
			callback();
		}
	}

	private static void WriteProperty(int id, JsonValue property)
	{
		PathPush(id);
		WriteComment(GetPropertyComment(property), delegate
		{
			writer.WritePropertyName(IDToProperty(id));
		});
		Write(property);
		PathPop();
	}

	private static void WriteArray(JsonValue array)
	{
		PathPush(propertyArrayItem);
		writer.WriteArrayStart();
		using (List<JsonValue>.Enumerator enumerator = array.GetArrayEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Write(enumerator.Current);
			}
		}
		writer.WriteArrayEnd();
		PathPop();
	}

	private static void WriteObject(JsonValue obj)
	{
		WriteComment(GetObjectComment(obj), delegate
		{
			writer.WriteObjectStart();
		});
		List<int> list = new List<int>(obj.Count);
		using (Dictionary<int, JsonValue>.Enumerator enumerator = obj.GetObjectEnumerator())
		{
			while (enumerator.MoveNext())
			{
				list.Add(enumerator.Current.Key);
			}
		}
		List<int> list2 = null;
		if (TryGetDefaultValue(propertyPath, out var value))
		{
			list2 = new List<int>();
			using (Dictionary<int, JsonValue>.Enumerator enumerator2 = value.GetObjectEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					int key = enumerator2.Current.Key;
					list2.Add(key);
				}
			}
		}
		if (list2 != null)
		{
			for (int i = 0; i < list2.Count; i++)
			{
				int num = list2[i];
				if (obj.TryGetValue(num, out var value2))
				{
					list.Remove(num);
					WriteProperty(num, value2);
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			int id = list[j];
			JsonValue property = obj[id];
			WriteProperty(id, property);
		}
		writer.WriteObjectEnd();
	}

	private static string GetObjectComment(JsonValue json)
	{
		switch (propertyPath.Count)
		{
		case 0:
		{
			if (json.GetInt(propertyTechType, out var value2))
			{
				return ((TechType)value2).AsString();
			}
			break;
		}
		case 2:
		{
			int num = propertyPath[0];
			int num2 = propertyPath[1];
			if (num == propertyIngredients && num2 == propertyArrayItem && json.GetInt(propertyTechType, out var value))
			{
				return ((TechType)value).AsString();
			}
			break;
		}
		}
		return null;
	}

	private static string GetPropertyComment(JsonValue json)
	{
		if (propertyPath.Count == 1)
		{
			int num = propertyPath[0];
			if (num == propertyBackgroundType && json.GetInt(out var value))
			{
				return ((CraftData.BackgroundType)value).ToString();
			}
			if (num == propertyEquipmentType && json.GetInt(out var value2))
			{
				EquipmentType equipmentType = (EquipmentType)value2;
				return equipmentType.ToString();
			}
			if (num == propertySlotType && json.GetInt(out var value3))
			{
				QuickSlotType quickSlotType = (QuickSlotType)value3;
				return quickSlotType.ToString();
			}
			if (num == propertyProcessed && json.GetInt(out var value4))
			{
				return ((TechType)value4).AsString();
			}
			if (num == propertyHarvestType && json.GetInt(out var value5))
			{
				HarvestType harvestType = (HarvestType)value5;
				return harvestType.ToString();
			}
			if (num == propertyHarvestOutput && json.GetInt(out var value6))
			{
				return ((TechType)value6).AsString();
			}
		}
		return null;
	}

	private static string GetValueComment(JsonValue json)
	{
		if (propertyPath.Count == 2)
		{
			int num = propertyPath[0];
			int num2 = propertyPath[1];
			if (num == propertyLinkedItems && num2 == propertyArrayItem && json.GetInt(out var value))
			{
				return ((TechType)value).AsString();
			}
		}
		return null;
	}

	private static void PathPush(int id)
	{
		propertyPath.Add(id);
	}

	private static void PathPop()
	{
		propertyPath.RemoveAt(propertyPath.Count - 1);
	}

	private static void PathClear()
	{
		propertyPath.Clear();
	}

	private static void TrimDefaults(bool trimUnknown)
	{
		List<TechType> list = new List<TechType>();
		List<int> list2 = new List<int>();
		foreach (KeyValuePair<TechType, JsonValue> entry in entries)
		{
			TechType key = entry.Key;
			JsonValue value = entry.Value;
			list2.Clear();
			using (Dictionary<int, JsonValue>.Enumerator enumerator2 = value.GetObjectEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					KeyValuePair<int, JsonValue> current2 = enumerator2.Current;
					int key2 = current2.Key;
					if (key2 == propertyTechType)
					{
						continue;
					}
					JsonValue value2 = current2.Value;
					if (defaults.TryGetValue(key2, out var value3))
					{
						if (value2.Equals(value3))
						{
							list2.Add(key2);
						}
					}
					else if (trimUnknown)
					{
						list2.Add(key2);
					}
				}
			}
			for (int i = 0; i < list2.Count; i++)
			{
				int id = list2[i];
				value.Remove(id);
			}
			int count = value.Count;
			if (count == 0 || (count == 1 && value.Contains(propertyTechType)))
			{
				list.Add(key);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			TechType techType = list[j];
			entries.Remove(techType);
			Debug.LogFormat("{0} TechData trimmed (all fields have default values)", techType.AsString());
		}
	}

	private static void Deserialize(string json)
	{
		using (Reader reader = new Reader(json))
		{
			JsonReader reader2 = new JsonReader(reader);
			JsonValue jsonValue = null;
			try
			{
				jsonValue = ReadValue(reader2);
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("CRITICAL EXCEPTION deserializing '{0}' (line {1}, column {2}):\n{3}", "Balance/TechData", reader.line, reader.column, ex.ToString());
				return;
			}
			if (jsonValue.GetArray(propertyEntries, out var value))
			{
				using (List<JsonValue>.Enumerator enumerator = value.GetArrayEnumerator())
				{
					while (enumerator.MoveNext())
					{
						JsonValue current = enumerator.Current;
						if (current.GetInt(propertyTechType, out var value2))
						{
							entries.Add((TechType)value2, current);
						}
					}
					return;
				}
			}
			Debug.LogErrorFormat("CRITICAL {0} ({1}) property is not found in '{2}'", IDToProperty(propertyEntries), propertyEntries, "Balance/TechData");
		}
	}

	private static JsonValue ReadValue(JsonReader reader)
	{
		reader.Read();
		if (reader.Token == JsonToken.ArrayEnd || reader.Token == JsonToken.Null)
		{
			return null;
		}
		if (reader.Token == JsonToken.String)
		{
			return new JsonValue((string)reader.Value);
		}
		if (reader.Token == JsonToken.Double)
		{
			return new JsonValue((double)reader.Value);
		}
		if (reader.Token == JsonToken.Int)
		{
			return new JsonValue((int)reader.Value);
		}
		if (reader.Token == JsonToken.Long)
		{
			return new JsonValue((long)reader.Value);
		}
		if (reader.Token == JsonToken.Boolean)
		{
			return new JsonValue((bool)reader.Value);
		}
		if (reader.Token == JsonToken.ArrayStart)
		{
			JsonValue jsonValue = new JsonValue(JsonValue.Type.Array);
			while (true)
			{
				JsonValue jsonValue2 = ReadValue(reader);
				if (jsonValue2 == null && reader.Token == JsonToken.ArrayEnd)
				{
					break;
				}
				jsonValue.Add(jsonValue2);
			}
			return jsonValue;
		}
		if (reader.Token == JsonToken.ObjectStart)
		{
			JsonValue jsonValue3 = new JsonValue(JsonValue.Type.Object);
			while (true)
			{
				reader.Read();
				if (reader.Token == JsonToken.ObjectEnd)
				{
					break;
				}
				int num = PropertyToID((string)reader.Value);
				JsonValue value = ReadValue(reader);
				if (num != 0)
				{
					jsonValue3.Add(num, value);
				}
			}
			return jsonValue3;
		}
		return null;
	}

	public static int PropertyToID(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return 0;
		}
		if (!propertyToID.TryGetValue(name, out var value))
		{
			value = propertyIndex;
			propertyIndex++;
			propertyToID.Add(name, value);
			idToProperty.Add(value, name);
		}
		return value;
	}

	public static string IDToProperty(int id)
	{
		if (id == 0)
		{
			return string.Empty;
		}
		if (!idToProperty.TryGetValue(id, out var value))
		{
			return string.Empty;
		}
		return value;
	}

	private static void Cache()
	{
		ClearCache();
		foreach (KeyValuePair<TechType, JsonValue> entry in entries)
		{
			TechType key = entry.Key;
			JsonValue value = entry.Value;
			if (value.GetArray(propertyIngredients, out var value2))
			{
				List<Ingredient> list = null;
				for (int i = 0; i < value2.Count; i++)
				{
					JsonValue jsonValue = value2[i];
					TechType @int = (TechType)jsonValue.GetInt(propertyTechType, 0);
					int int2 = jsonValue.GetInt(propertyAmount, 0);
					if (@int != 0 && int2 > 0)
					{
						if (list == null)
						{
							list = new List<Ingredient>();
							cachedIngredients.Add(key, list.AsReadOnly());
						}
						list.Add(new Ingredient(@int, int2));
					}
				}
			}
			if (!value.GetArray(propertyLinkedItems, out var value3))
			{
				continue;
			}
			List<TechType> list2 = null;
			for (int j = 0; j < value3.Count; j++)
			{
				TechType int3 = (TechType)value3[j].GetInt();
				if (list2 == null)
				{
					list2 = new List<TechType>();
					cachedLinkedItems.Add(key, list2.AsReadOnly());
				}
				list2.Add(int3);
			}
		}
	}

	public static ReadOnlyCollection<Ingredient> GetIngredients(TechType techType)
	{
		ReadOnlyCollection<Ingredient> value = null;
		cachedIngredients.TryGetValue(techType, out value);
		return value;
	}

	public static ReadOnlyCollection<TechType> GetLinkedItems(TechType techType)
	{
		ReadOnlyCollection<TechType> value = null;
		cachedLinkedItems.TryGetValue(techType, out value);
		return value;
	}

	public static Vector2int GetItemSize(TechType techType)
	{
		Vector2int result = defaultItemSize;
		if (TryGetValue(techType, out var value) && value.GetObject(propertyItemSize, out var value2))
		{
			result.x = value2.GetInt(propertyX, result.x);
			result.y = value2.GetInt(propertyY, result.y);
		}
		return result;
	}

	public static CraftData.BackgroundType GetBackgroundType(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetInt(propertyBackgroundType, out var value2))
		{
			return (CraftData.BackgroundType)value2;
		}
		return defaultBackgroundType;
	}

	public static EquipmentType GetEquipmentType(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetInt(propertyEquipmentType, out var value2))
		{
			return (EquipmentType)value2;
		}
		return defaultEquipmentType;
	}

	public static QuickSlotType GetSlotType(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetInt(propertySlotType, out var value2))
		{
			return (QuickSlotType)value2;
		}
		return defaultSlotType;
	}

	public static bool GetCraftTime(TechType techType, out float result)
	{
		if (TryGetValue(techType, out var value) && value.GetDouble(propertyCraftTime, out var value2))
		{
			result = (float)value2;
			return true;
		}
		result = defaultCraftTime;
		return false;
	}

	public static int GetCraftAmount(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetInt(propertyCraftAmount, out var value2))
		{
			return value2;
		}
		return defaultCraftAmount;
	}

	public static TechType GetProcessed(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetInt(propertyProcessed, out var value2))
		{
			return (TechType)value2;
		}
		return defaultProcessed;
	}

	public static bool GetBuildable(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetBool(propertyBuildable, out var value2))
		{
			return value2;
		}
		return defaultBuildable;
	}

	public static string GetSoundPickup(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetString(propertySoundPickup, out var value2))
		{
			return value2;
		}
		return defaultSoundPickup;
	}

	public static string GetSoundDrop(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetString(propertySoundDrop, out var value2))
		{
			return value2;
		}
		return defaultSoundDrop;
	}

	public static string GetSoundUse(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetString(propertySoundUse, out var value2))
		{
			return value2;
		}
		return defaultSoundUse;
	}

	public static HarvestType GetHarvestType(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetInt(propertyHarvestType, out var value2))
		{
			return (HarvestType)value2;
		}
		return defaultHarvestType;
	}

	public static TechType GetHarvestOutput(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetInt(propertyHarvestOutput, out var value2))
		{
			return (TechType)value2;
		}
		return defaultHarvestOutput;
	}

	public static int GetHarvestFinalCutBonus(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetInt(propertyHarvestFinalCutBonus, out var value2))
		{
			return value2;
		}
		return defaultHarvestFinalCutBonus;
	}

	public static float GetMaxCharge(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetDouble(propertyMaxCharge, out var value2))
		{
			return (float)value2;
		}
		return defaultMaxCharge;
	}

	public static bool GetEnergyCost(TechType techType, out float result)
	{
		if (TryGetValue(techType, out var value) && value.GetDouble(propertyEnergyCost, out var value2))
		{
			result = (float)value2;
			return true;
		}
		result = defaultEnergyCost;
		return false;
	}

	public static string GetPoweredPrefabName(TechType techType)
	{
		if (TryGetValue(techType, out var value) && value.GetString(propertyPoweredPrefab, out var value2))
		{
			return value2;
		}
		return defaultPoweredPrefab;
	}
}
