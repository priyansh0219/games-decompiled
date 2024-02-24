using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using LumenWorks.Framework.IO.Csv;
using UnityEngine;

namespace UWE
{
	public static class CSVUtils
	{
		public delegate T ParseFunction<T>(string str);

		public static List<T> Read<T>(StreamReader reader) where T : new()
		{
			HashSet<string> hashSet = new HashSet<string>();
			Dictionary<string, FieldInfo> dictionary = new Dictionary<string, FieldInfo>();
			FieldInfo[] fields = typeof(T).GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				dictionary[fieldInfo.Name] = fieldInfo;
			}
			List<T> list = new List<T>();
			using (CsvReader csvReader = new CsvReader(reader, hasHeaders: true))
			{
				csvReader.SkipEmptyLines = true;
				int fieldCount = csvReader.FieldCount;
				string[] fieldHeaders = csvReader.GetFieldHeaders();
				while (csvReader.ReadNextRecord())
				{
					bool flag = true;
					for (int j = 0; j < fieldCount; j++)
					{
						if (!string.IsNullOrEmpty(csvReader[j]))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
					T val = new T();
					try
					{
						for (int k = 0; k < fieldCount; k++)
						{
							if (!dictionary.ContainsKey(fieldHeaders[k]))
							{
								hashSet.Add(fieldHeaders[k]);
								continue;
							}
							FieldInfo fieldInfo2 = dictionary[fieldHeaders[k]];
							if (csvReader[k] == "")
							{
								Debug.Log(string.Concat("Row ", list.Count + 2, " column ", k, " is blank. Expected a '", fieldHeaders[k], "' value of type ", fieldInfo2.FieldType, ". Raw row: ", csvReader.RowAsString()));
							}
							else if (fieldInfo2.FieldType == typeof(string))
							{
								fieldInfo2.SetValue(val, csvReader[k]);
							}
							else if (fieldInfo2.FieldType == typeof(float))
							{
								fieldInfo2.SetValue(val, float.Parse(csvReader[k], NumberStyles.Float, CultureInfo.InvariantCulture));
							}
							else if (fieldInfo2.FieldType == typeof(int))
							{
								fieldInfo2.SetValue(val, int.Parse(csvReader[k]));
							}
							else if (fieldInfo2.FieldType == typeof(Int3))
							{
								fieldInfo2.SetValue(val, Int3.Parse(csvReader[k]));
							}
							else
							{
								Debug.LogError("Unsupported field type: " + fieldInfo2.FieldType);
							}
						}
						if (csvReader[0] != "")
						{
							list.Add(val);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						Debug.LogErrorFormat("Error while reading row {0}: {1}", list.Count, csvReader.RowAsString());
					}
				}
			}
			if (hashSet.Count > 0)
			{
				Debug.LogFormat("Found {0} unexpected field name(s) while reading CSV: {1}", hashSet.Count, string.Join(", ", hashSet.ToArray()));
			}
			return list;
		}

		public static void Write<T>(StreamWriter writer, IEnumerable<T> insts)
		{
			Dictionary<string, FieldInfo> dictionary = typeof(T).GetFields().ToDictionary((FieldInfo p) => p.Name);
			string text = dictionary.Keys.Last();
			foreach (KeyValuePair<string, FieldInfo> item in dictionary)
			{
				writer.Write(item.Key);
				if (text != item.Key)
				{
					writer.Write(',');
				}
			}
			writer.WriteLine();
			foreach (T inst in insts)
			{
				foreach (KeyValuePair<string, FieldInfo> item2 in dictionary)
				{
					writer.Write(Filter(item2.Value.GetValue(inst)));
					if (text != item2.Key)
					{
						writer.Write(',');
					}
				}
				writer.WriteLine();
			}
		}

		private static object Filter(object value)
		{
			if (value is Int3 @int)
			{
				return @int.ToCsvString();
			}
			return value;
		}

		public static string RowAsString(this CsvReader csv)
		{
			string text = "";
			for (int i = 0; i < csv.FieldCount; i++)
			{
				text = text + csv[i] + ", ";
			}
			return text;
		}

		public static List<T> Read<T>(string csvText) where T : new()
		{
			return Read<T>(Encoding.ASCII.GetBytes(csvText));
		}

		public static List<T> Read<T>(byte[] csvBytes) where T : new()
		{
			using (MemoryStream csvStream = new MemoryStream(csvBytes, writable: false))
			{
				return Read<T>(csvStream);
			}
		}

		public static List<T> Read<T>(Stream csvStream) where T : new()
		{
			using (StreamReader reader = new StreamReader(csvStream))
			{
				return Read<T>(reader);
			}
		}

		public static List<T> Load<T>(string csvFile) where T : new()
		{
			using (StreamReader reader = new StreamReader(csvFile))
			{
				return Read<T>(reader);
			}
		}

		public static void Save<T>(string csvFile, IEnumerable<T> entries)
		{
			using (StreamWriter writer = new StreamWriter(csvFile))
			{
				Write(writer, entries);
			}
		}

		public static T[,] ReadAsGrid<T>(StreamReader reader, ParseFunction<T> parser)
		{
			List<T[]> list = new List<T[]>();
			int num = -1;
			using (CsvReader csvReader = new CsvReader(reader, hasHeaders: true))
			{
				num = csvReader.FieldCount;
				while (csvReader.ReadNextRecord())
				{
					T[] array = new T[num];
					for (int i = 0; i < num; i++)
					{
						array[i] = parser(csvReader[i]);
					}
					list.Add(array);
				}
			}
			int count = list.Count;
			Debug.Log(count + " rows, " + num + " cols");
			T[,] array2 = new T[count, num];
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < count; k++)
				{
					array2[k, j] = list[k][j];
				}
			}
			return array2;
		}

		public static T[,] LoadAsGrid<T>(string csvFile, ParseFunction<T> parser)
		{
			using (StreamReader reader = new StreamReader(csvFile))
			{
				return ReadAsGrid(reader, parser);
			}
		}
	}
}
