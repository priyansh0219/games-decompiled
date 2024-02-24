using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

public class UIConsoleCommand
{
	private class DebugSpriteAsset
	{
		private static DebugSpriteAsset singleton;

		private bool enabled;

		private FieldInfo fieldMaterialPostfixes;

		private FieldInfo fieldMaterialMatches;

		public DebugSpriteAsset()
		{
			Type typeFromHandle = typeof(TMP_SpriteAsset);
			fieldMaterialPostfixes = typeFromHandle.GetField("materialPostfixes", BindingFlags.Instance | BindingFlags.NonPublic);
			fieldMaterialMatches = typeFromHandle.GetField("materialMatches", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public static void OnConsoleCommand(NotificationCenter.Notification n)
		{
			if (singleton == null)
			{
				singleton = new DebugSpriteAsset();
			}
			singleton.Toggle();
		}

		public void Toggle()
		{
			enabled = !enabled;
			if (enabled)
			{
				ManagedUpdate.Subscribe(ManagedUpdate.Queue.Update, Update);
			}
			else
			{
				ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.Update, Update);
			}
		}

		private void Update()
		{
			TMP_SpriteAsset defaultSpriteAsset = TMP_Settings.defaultSpriteAsset;
			if (defaultSpriteAsset == null)
			{
				Dbg.Write("defaultSpriteAsset: [null]");
				return;
			}
			using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
			{
				StringBuilder sb = stringBuilderPool.sb;
				sb.AppendFormat("defaultSpriteAsset: {0}", defaultSpriteAsset.name);
				string text = ((defaultSpriteAsset.material != null) ? defaultSpriteAsset.material.name : "[null]");
				sb.AppendFormat("\n  material:{0}", text);
				string[] array = fieldMaterialPostfixes.GetValue(defaultSpriteAsset) as string[];
				List<Material> materials = defaultSpriteAsset.materials;
				if (materials != null || materials.Count == 0)
				{
					sb.AppendFormat("  materials ({0} total):", materials.Count);
					for (int i = 0; i < materials.Count; i++)
					{
						Material material = materials[i];
						sb.AppendFormat("\n    {0} material:{1}", i, material.name);
						if (array != null && array.Length == materials.Count)
						{
							sb.Append(" postfix:");
							string text2 = array[i];
							if (text2 == null)
							{
								sb.Append("[null]");
							}
							else if (text2.Length == 0)
							{
								sb.Append("[empty]");
							}
							else
							{
								sb.Append('"').Append(text2).Append('"');
							}
						}
					}
				}
				Dictionary<int, int> dictionary = fieldMaterialMatches.GetValue(defaultSpriteAsset) as Dictionary<int, int>;
				sb.AppendFormat("\n  materialMatches ({0} total):", dictionary.Count);
				foreach (KeyValuePair<int, int> item in dictionary)
				{
					int value = item.Value;
					string arg = string.Empty;
					if (value < 0)
					{
						arg = text;
					}
					else if (materials != null && value < materials.Count)
					{
						arg = materials[value].name;
					}
					sb.AppendFormat("\n    {0} -> {1}", item.Key, arg);
				}
				Dbg.Write(sb);
			}
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void RegisterCommands()
	{
		DevConsole.RegisterConsoleCommand("debugspriteasset", DebugSpriteAsset.OnConsoleCommand);
	}
}
