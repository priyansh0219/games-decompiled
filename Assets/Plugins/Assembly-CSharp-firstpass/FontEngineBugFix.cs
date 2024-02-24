using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public class FontEngineBugFix
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Fix()
	{
		FieldInfo field = typeof(FontEngine).GetField("s_PairAdjustmentRecords_MarshallingArray", BindingFlags.Static | BindingFlags.NonPublic);
		if (field != null)
		{
			Type fieldType = field.FieldType;
			if (fieldType.IsArray && (!(field.GetValue(null) is Array array) || array.Length < 512))
			{
				Array value = Array.CreateInstance(fieldType.GetElementType(), 512);
				field.SetValue(null, value);
			}
		}
	}
}
