using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonValue : IEnumerable, IEquatable<JsonValue>
{
	public enum Type : byte
	{
		None = 0,
		Bool = 1,
		Int = 2,
		Long = 3,
		Double = 4,
		String = 5,
		Array = 6,
		Object = 7
	}

	private Type propertyType;

	private bool dataBool;

	private double dataDouble;

	private int dataInt;

	private long dataLong;

	private string dataString;

	private List<JsonValue> dataArray;

	private Dictionary<int, JsonValue> dataObject;

	public int Count
	{
		get
		{
			switch (propertyType)
			{
			case Type.Array:
				return dataArray.Count;
			case Type.Object:
				return dataObject.Count;
			default:
				return 0;
			}
		}
	}

	public Dictionary<int, JsonValue>.KeyCollection Keys
	{
		get
		{
			if (!CheckType(Type.Object))
			{
				return null;
			}
			return dataObject.Keys;
		}
	}

	public JsonValue this[int id]
	{
		get
		{
			if (propertyType == Type.Array)
			{
				return dataArray[id];
			}
			if (propertyType == Type.Object)
			{
				return dataObject[id];
			}
			return null;
		}
		set
		{
			if (propertyType == Type.Array)
			{
				dataArray[id] = value;
			}
			else if (propertyType == Type.Object)
			{
				dataObject[id] = value;
			}
		}
	}

	public JsonValue()
	{
		propertyType = Type.None;
	}

	public JsonValue(Type type)
	{
		propertyType = type;
		switch (propertyType)
		{
		case Type.Array:
			dataArray = new List<JsonValue>();
			break;
		case Type.Object:
			dataObject = new Dictionary<int, JsonValue>();
			break;
		default:
			throw new NotImplementedException();
		case Type.None:
		case Type.Bool:
		case Type.Int:
		case Type.Long:
		case Type.Double:
		case Type.String:
			break;
		}
	}

	public JsonValue(bool value)
	{
		propertyType = Type.Bool;
		dataBool = value;
	}

	public JsonValue(int value)
	{
		propertyType = Type.Int;
		dataInt = value;
	}

	public JsonValue(long value)
	{
		propertyType = Type.Long;
		dataLong = value;
	}

	public JsonValue(double value)
	{
		propertyType = Type.Double;
		dataDouble = value;
	}

	public JsonValue(string value)
	{
		propertyType = Type.String;
		dataString = value;
	}

	public bool GetBool(int id, bool defaultValue)
	{
		GetBool(id, out var value, defaultValue);
		return value;
	}

	public int GetInt(int id, int defaultValue)
	{
		GetInt(id, out var value, defaultValue);
		return value;
	}

	public long GetLong(int id, long defaultValue)
	{
		GetLong(id, out var value, defaultValue);
		return value;
	}

	public double GetDouble(int id, double defaultValue)
	{
		GetDouble(id, out var value, defaultValue);
		return value;
	}

	public string GetString(int id, string defaultValue)
	{
		GetString(id, out var value, defaultValue);
		return value;
	}

	public JsonValue GetArray(int id, JsonValue defaultValue)
	{
		GetArray(id, out var value, defaultValue);
		return value;
	}

	public JsonValue GetObject(int id, JsonValue defaultValue)
	{
		GetObject(id, out var value, defaultValue);
		return value;
	}

	public bool TryGetValue(int id, out JsonValue value)
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out value))
		{
			return true;
		}
		value = null;
		return false;
	}

	public bool GetBool(int id, out bool value, bool defaultValue = false)
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out var value2) && value2.GetBool(out value))
		{
			return true;
		}
		value = defaultValue;
		return false;
	}

	public bool GetInt(int id, out int value, int defaultValue = 0)
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out var value2) && value2.GetInt(out value))
		{
			return true;
		}
		value = defaultValue;
		return false;
	}

	public bool GetLong(int id, out long value, long defaultValue = 0L)
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out var value2) && value2.GetLong(out value, 0L))
		{
			return true;
		}
		value = defaultValue;
		return false;
	}

	public bool GetDouble(int id, out double value, double defaultValue = 0.0)
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out var value2) && value2.GetDouble(out value))
		{
			return true;
		}
		value = defaultValue;
		return false;
	}

	public bool GetString(int id, out string value, string defaultValue = "")
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out var value2) && value2.GetString(out value))
		{
			return true;
		}
		value = defaultValue;
		return false;
	}

	public bool GetArray(int id, out JsonValue value, JsonValue defaultValue = null)
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out var value2) && (value2.CheckType(Type.Array) || value2.CheckType(Type.None)))
		{
			value = value2;
			return true;
		}
		value = defaultValue;
		return false;
	}

	public bool GetObject(int id, out JsonValue value, JsonValue defaultValue = null)
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out var value2) && (value2.CheckType(Type.Object) || value2.CheckType(Type.None)))
		{
			value = value2;
			return true;
		}
		value = defaultValue;
		return false;
	}

	public void SetBool(int id, bool value)
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out var value2))
		{
			value2.SetBool(value);
		}
	}

	public void SetInt(int id, int value)
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out var value2))
		{
			value2.SetInt(value);
		}
	}

	public void SetLong(int id, long value)
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out var value2))
		{
			value2.SetLong(value);
		}
	}

	public void SetDouble(int id, double value)
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out var value2))
		{
			value2.SetDouble(value);
		}
	}

	public void SetString(int id, string value)
	{
		if (CheckType(Type.Object) && dataObject.TryGetValue(id, out var value2))
		{
			value2.SetString(value);
		}
	}

	public bool GetBool(bool defaultValue = false)
	{
		if (!CheckType(Type.Bool))
		{
			return defaultValue;
		}
		return dataBool;
	}

	public int GetInt(int defaultValue = 0)
	{
		if (!CheckType(Type.Int))
		{
			return defaultValue;
		}
		return dataInt;
	}

	public long GetLong(long defaultValue = 0L)
	{
		if (!CheckType(Type.Long))
		{
			return defaultValue;
		}
		return dataLong;
	}

	public double GetDouble(double defaultValue = 0.0)
	{
		if (!CheckType(Type.Double))
		{
			return defaultValue;
		}
		return dataDouble;
	}

	public string GetString(string defaultValue = "")
	{
		if (!CheckType(Type.String))
		{
			return defaultValue;
		}
		return dataString;
	}

	public bool GetBool(out bool value, bool defaultValue = false)
	{
		if (CheckType(Type.Bool))
		{
			value = dataBool;
			return true;
		}
		value = defaultValue;
		return false;
	}

	public bool GetInt(out int value, int defaultValue = 0)
	{
		if (CheckType(Type.Int))
		{
			value = dataInt;
			return true;
		}
		value = defaultValue;
		return false;
	}

	public bool GetLong(out long value, long defaultValue = 0L)
	{
		if (CheckType(Type.Long))
		{
			value = dataLong;
			return true;
		}
		value = defaultValue;
		return false;
	}

	public bool GetDouble(out double value, double defaultValue = 0.0)
	{
		if (CheckType(Type.Double))
		{
			value = dataDouble;
			return true;
		}
		value = defaultValue;
		return false;
	}

	public bool GetString(out string value, string defaultValue = "")
	{
		if (CheckType(Type.String))
		{
			value = dataString;
			return true;
		}
		value = defaultValue;
		return false;
	}

	public void SetBool(bool value)
	{
		if (CheckType(Type.Bool))
		{
			dataBool = value;
			return;
		}
		Debug.LogErrorFormat("Attempt to set bool value for JsonValue of type {0}", propertyType.ToString());
	}

	public void SetInt(int value)
	{
		if (CheckType(Type.Int))
		{
			dataInt = value;
			return;
		}
		Debug.LogErrorFormat("Attempt to set int value for JsonValue of type {0}", propertyType.ToString());
	}

	public void SetLong(long value)
	{
		if (CheckType(Type.Long))
		{
			dataLong = value;
			return;
		}
		Debug.LogErrorFormat("Attempt to set long value for JsonValue of type {0}", propertyType.ToString());
	}

	public void SetDouble(double value)
	{
		if (CheckType(Type.Double))
		{
			dataDouble = value;
			return;
		}
		Debug.LogErrorFormat("Attempt to set double value for JsonValue of type {0}", propertyType.ToString());
	}

	public void SetString(string value)
	{
		if (CheckType(Type.String))
		{
			dataString = value;
			return;
		}
		Debug.LogErrorFormat("Attempt to set string value for JsonValue of type {0}", propertyType.ToString());
	}

	public List<JsonValue>.Enumerator GetArrayEnumerator()
	{
		if (!CheckType(Type.Array))
		{
			return default(List<JsonValue>.Enumerator);
		}
		return dataArray.GetEnumerator();
	}

	public Dictionary<int, JsonValue>.Enumerator GetObjectEnumerator()
	{
		if (!CheckType(Type.Object))
		{
			return default(Dictionary<int, JsonValue>.Enumerator);
		}
		return dataObject.GetEnumerator();
	}

	public bool Contains(int id)
	{
		if (CheckType(Type.Object))
		{
			return dataObject.ContainsKey(id);
		}
		return false;
	}

	public bool Add(JsonValue value)
	{
		if (CheckType(Type.None))
		{
			propertyType = Type.Array;
			dataArray = new List<JsonValue>();
		}
		if (!CheckType(Type.Array))
		{
			Debug.LogErrorFormat("Attempt to add new value to JsonValue of type '{0}'. You can only add new values to the None or Array JsonValue's.", propertyType);
			return false;
		}
		dataArray.Add(value);
		return true;
	}

	public bool Add(int id, JsonValue value)
	{
		if (CheckType(Type.None))
		{
			propertyType = Type.Object;
			dataObject = new Dictionary<int, JsonValue>();
		}
		if (!CheckType(Type.Object))
		{
			Debug.LogErrorFormat("Attempt to add new property to JsonValue of type '{0}'. You can only add new properties to the None or Object JsonValue's.", propertyType);
			return false;
		}
		if (dataObject.ContainsKey(id))
		{
			Debug.LogErrorFormat("Attempt to redefine property {0} via Add() method is not allowed.", id);
			return false;
		}
		dataObject.Add(id, value);
		return true;
	}

	public bool Remove(int id)
	{
		if (propertyType == Type.Array)
		{
			if (id >= 0 && id < dataArray.Count)
			{
				dataArray.RemoveAt(id);
				return true;
			}
			Debug.LogErrorFormat("Array index {0} is out of range.", id);
			return false;
		}
		if (propertyType == Type.Object)
		{
			return dataObject.Remove(id);
		}
		Debug.LogErrorFormat("Attempt to remove property {0} from JsonValue which is not Array or Object.", id);
		return false;
	}

	public Type GetValueType()
	{
		return propertyType;
	}

	public bool CheckType(Type type)
	{
		return propertyType == type;
	}

	public JsonValue Copy()
	{
		switch (propertyType)
		{
		case Type.None:
			return new JsonValue();
		case Type.Bool:
			return new JsonValue(dataBool);
		case Type.Int:
			return new JsonValue(dataInt);
		case Type.Long:
			return new JsonValue(dataLong);
		case Type.Double:
			return new JsonValue(dataDouble);
		case Type.String:
			return new JsonValue(dataString);
		case Type.Array:
		{
			JsonValue jsonValue2 = new JsonValue(Type.Array);
			for (int i = 0; i < dataArray.Count; i++)
			{
				jsonValue2.Add(dataArray[i].Copy());
			}
			return jsonValue2;
		}
		case Type.Object:
		{
			JsonValue jsonValue = new JsonValue(Type.Object);
			{
				foreach (KeyValuePair<int, JsonValue> item in dataObject)
				{
					int key = item.Key;
					jsonValue.Add(key, item.Value.Copy());
				}
				return jsonValue;
			}
		}
		default:
			return null;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		switch (propertyType)
		{
		case Type.Array:
			return GetArrayEnumerator();
		case Type.Object:
			return GetObjectEnumerator();
		default:
			return null;
		}
	}

	public bool Equals(JsonValue other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if (propertyType != other.propertyType)
		{
			return false;
		}
		switch (propertyType)
		{
		case Type.None:
			return true;
		case Type.Bool:
			return dataBool == other.dataBool;
		case Type.Int:
			return dataInt == other.dataInt;
		case Type.Long:
			return dataLong == other.dataLong;
		case Type.Double:
			return dataDouble == other.dataDouble;
		case Type.String:
			return string.Equals(dataString, other.dataString);
		case Type.Array:
		{
			int count = dataArray.Count;
			if (count != other.dataArray.Count)
			{
				return false;
			}
			for (int i = 0; i < count; i++)
			{
				if (!Equals(dataArray[i], other.dataArray[i]))
				{
					return false;
				}
			}
			return true;
		}
		case Type.Object:
			if (dataObject.Count != other.dataObject.Count)
			{
				return false;
			}
			foreach (KeyValuePair<int, JsonValue> item in dataObject)
			{
				int key = item.Key;
				JsonValue value = item.Value;
				if (!other.dataObject.TryGetValue(key, out var value2))
				{
					return false;
				}
				if (!value.Equals(value2))
				{
					return false;
				}
			}
			return true;
		default:
			throw new NotImplementedException();
		}
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as JsonValue);
	}

	public override int GetHashCode()
	{
		int num = 47;
		int num2 = 31 * num;
		int num3 = (int)propertyType;
		num = num2 + num3.GetHashCode();
		switch (propertyType)
		{
		case Type.None:
			num = 31 * num;
			break;
		case Type.Bool:
			num = 31 * num + dataBool.GetHashCode();
			break;
		case Type.Int:
			num = 31 * num + dataInt.GetHashCode();
			break;
		case Type.Long:
			num = 31 * num + dataLong.GetHashCode();
			break;
		case Type.Double:
			num = 31 * num + dataDouble.GetHashCode();
			break;
		case Type.String:
			num = 31 * num + dataString.GetHashCode();
			break;
		case Type.Array:
		{
			for (int i = 0; i < dataArray.Count; i++)
			{
				num = 31 * num + dataArray[i].GetHashCode();
			}
			break;
		}
		case Type.Object:
			foreach (KeyValuePair<int, JsonValue> item in dataObject)
			{
				num = 31 * num + item.Key.GetHashCode();
				num = 31 * num + item.Value.GetHashCode();
			}
			break;
		default:
			throw new NotImplementedException();
		}
		return num;
	}

	public static bool Equals(JsonValue a, JsonValue b)
	{
		return a?.Equals(b) ?? ((object)b == null);
	}

	public static bool operator ==(JsonValue a, JsonValue b)
	{
		return Equals(a, b);
	}

	public static bool operator !=(JsonValue a, JsonValue b)
	{
		return !Equals(a, b);
	}
}
