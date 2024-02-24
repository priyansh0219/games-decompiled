using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

public class LanguageSDFDebug
{
	public HashSet<int> assetIds;

	public Func<TMP_FontAsset, int> getAssetType;

	private static GUIStyle style;

	private static FieldInfo field;

	private float updateInterval;

	private float timeNextUpdate;

	private int updateFrame = -1;

	private Dictionary<TMP_FontAsset, List<string>> allFontAssets = new Dictionary<TMP_FontAsset, List<string>>();

	private List<TMP_FontAsset> fontAssetsSorted = new List<TMP_FontAsset>();

	private StringBuilder sb = new StringBuilder();

	private Vector2 scrollPosition;

	private int depth;

	private Dictionary<Texture2D, long> allTextures = new Dictionary<Texture2D, long>();

	private int mode;

	private bool showUsage = true;

	private bool showTextures = true;

	private float sizeMultiplier = 0.125f;

	private GUIContent[] modeOptions = new GUIContent[2]
	{
		new GUIContent("Summary"),
		new GUIContent("Detailed")
	};

	private GUILayoutOption[] layoutOptionsExpandWidth = new GUILayoutOption[1] { GUILayout.ExpandWidth(expand: true) };

	private GUILayoutOption[] layoutOptionsExpandAll = new GUILayoutOption[2]
	{
		GUILayout.ExpandWidth(expand: true),
		GUILayout.ExpandHeight(expand: true)
	};

	private GUILayoutOption[] layoutOptionsExpandWidthHeight30 = new GUILayoutOption[2]
	{
		GUILayout.ExpandWidth(expand: true),
		GUILayout.Height(30f)
	};

	private static void Initialize()
	{
		if (style == null)
		{
			style = new GUIStyle(Dbg.styles.label)
			{
				padding = new RectOffset(0, 0, 1, 2),
				fontSize = 14,
				richText = true,
				normal = new GUIStyleState
				{
					textColor = new Color(1f, 1f, 1f, 1f)
				}
			};
		}
		if (field == null)
		{
			field = typeof(TMP_FontAsset).GetField("m_CharacterLookupDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
		}
	}

	public void OnGUI()
	{
		Initialize();
		mode = GUILayout.Toolbar(mode, modeOptions, layoutOptionsExpandWidthHeight30);
		int num = mode;
		if (num != 1)
		{
			updateInterval = Dbg.Slider($"Update Interval ({updateInterval:0.0})", updateInterval, 0f, 5f, layoutOptionsExpandWidth);
			showUsage = GUILayout.Toggle(showUsage, "Show Usage");
			showTextures = GUILayout.Toggle(showTextures, "Show Textures");
			if (showTextures)
			{
				sizeMultiplier = Dbg.Slider($"Texture Size ({sizeMultiplier:0.0})", sizeMultiplier, 0f, 1f, layoutOptionsExpandWidth);
			}
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, layoutOptionsExpandAll);
			DrawSummary();
			GUILayout.EndScrollView();
		}
		else
		{
			scrollPosition = GUILayout.BeginScrollView(scrollPosition);
			GUILayout.TextArea(DrawDetailed(), style, layoutOptionsExpandAll);
			GUILayout.EndScrollView();
		}
	}

	private void DrawSummary()
	{
		UpdateData();
		GUILayout.Label($"Update Frame: {updateFrame}", style);
		int num = 0;
		int num2 = 0;
		foreach (TMP_FontAsset item in fontAssetsSorted)
		{
			num++;
			AtlasPopulationMode atlasPopulationMode = item.atlasPopulationMode;
			List<string> list = allFontAssets[item];
			if (item != null)
			{
				int num3 = ((getAssetType != null) ? getAssetType(item) : 0);
				int instanceID = item.GetInstanceID();
				bool flag = assetIds != null && assetIds.Contains(instanceID);
				long num4 = 0L;
				Texture2D[] atlasTextures = item.atlasTextures;
				foreach (Texture2D texture2D in atlasTextures)
				{
					if (texture2D != null)
					{
						long runtimeMemorySizeLong = Profiler.GetRuntimeMemorySizeLong(texture2D);
						num4 += runtimeMemorySizeLong;
					}
				}
				int num5 = 43;
				num5 = 31 * num5 + num3.GetHashCode();
				num5 = 31 * num5 + atlasPopulationMode.GetHashCode();
				if (num > 1 && num5 != num2)
				{
					GUILayout.Label(new string('-', 60), style);
				}
				num2 = num5;
				sb.Length = 0;
				sb.AppendFormat("{0,2} ", num);
				sb.AppendFormat("<color={0}>{1,11}</color>", flag ? "cyan" : "white", instanceID);
				sb.Append(" ").Append(item.name);
				sb.Append(" ").Append(Dbg.FormatColor(atlasPopulationMode));
				if (num4 > 1024)
				{
					float t = (float)Math.Pow((double)num4 / 16777216.0, 0.4);
					Color.Lerp(new Color(0.58f, 0.87f, 0f), new Color(0.875f, 0.251f, 0.188f), t);
					sb.Append(" (");
					AppendSize(num4, sb, 16777216L, 1024L);
					sb.Append(")");
				}
				GUILayout.Label(sb.ToString(), style);
				if (showTextures)
				{
					Texture2D[] atlasTextures2 = item.atlasTextures;
					if (atlasTextures2 != null)
					{
						bool flag2 = true;
						Rect position = default(Rect);
						foreach (Texture2D texture2D2 in atlasTextures2)
						{
							if (!(texture2D2 == null) && texture2D2.width != 0 && texture2D2.height != 0)
							{
								if (flag2)
								{
									flag2 = false;
									position = GUILayoutUtility.GetRect(10f, (float)texture2D2.height * sizeMultiplier);
								}
								position.width = (float)texture2D2.width * sizeMultiplier;
								GUI.DrawTexture(position, Texture2D.whiteTexture);
								GUI.DrawTexture(position, texture2D2);
								position.x += position.width + 10f;
							}
						}
					}
				}
				if (!showUsage)
				{
					continue;
				}
				sb.Length = 0;
				for (int k = 0; k < list.Count; k++)
				{
					if (k > 0)
					{
						sb.Append('\n');
					}
					sb.AppendFormat("  {0} {1}", k, list[k]);
					if (k >= 10)
					{
						sb.Append("\n  ...");
						break;
					}
				}
				if (sb.Length > 0)
				{
					GUILayout.Label(sb.ToString(), style);
				}
			}
			else
			{
				GUILayout.Label($"\n{num,2} [null]", style);
			}
		}
	}

	public string DrawDetailed()
	{
		UpdateData();
		depth = 0;
		sb.Length = 0;
		sb.AppendFormat("{0}:", Language.main.GetCurrentLanguage());
		foreach (TMP_FontAsset item in fontAssetsSorted)
		{
			AppendFontAssetInfo(item, sb);
		}
		AppendNewLine(sb);
		sb.Append("\nIndividual textures:");
		int num = 0;
		long num2 = 0L;
		foreach (KeyValuePair<Texture2D, long> allTexture in allTextures)
		{
			Texture2D key = allTexture.Key;
			if (key.width != 0)
			{
				AppendNewLine(sb);
				sb.AppendFormat("{0} ", num);
				AppendName(key.name, "cyan", sb);
				sb.AppendFormat(" {0}x{1} ", key.width, key.height);
				long runtimeMemorySizeLong = Profiler.GetRuntimeMemorySizeLong(key);
				num2 += runtimeMemorySizeLong;
				AppendSize(runtimeMemorySizeLong, sb, 8388608L, 1024L);
				num++;
			}
		}
		AppendNewLine(sb);
		sb.Append("Total: ");
		AppendSize(num2, sb, 16777216L, 1024L);
		return sb.ToString();
	}

	private void UpdateData()
	{
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime < timeNextUpdate)
		{
			return;
		}
		timeNextUpdate = unscaledTime + updateInterval;
		int frameCount = Time.frameCount;
		if (updateFrame == frameCount)
		{
			return;
		}
		updateFrame = frameCount;
		allFontAssets.Clear();
		fontAssetsSorted.Clear();
		allTextures.Clear();
		TMP_FontAsset[] array = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
		foreach (TMP_FontAsset key in array)
		{
			allFontAssets.Add(key, new List<string>());
		}
		fontAssetsSorted.AddRange(allFontAssets.Keys);
		fontAssetsSorted.Sort(SortByTypeAndName);
		TextMeshProUGUI[] array2 = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>();
		foreach (TextMeshProUGUI textMeshProUGUI in array2)
		{
			List<string> list = null;
			int instanceID = textMeshProUGUI.font.GetInstanceID();
			foreach (KeyValuePair<TMP_FontAsset, List<string>> allFontAsset in allFontAssets)
			{
				if (allFontAsset.Key.GetInstanceID() == instanceID)
				{
					list = allFontAsset.Value;
					break;
				}
			}
			list.Add(Dbg.LogPath(textMeshProUGUI.transform));
		}
	}

	private int SortByTypeAndName(TMP_FontAsset a, TMP_FontAsset b)
	{
		if (a == null != (b == null))
		{
			if (!(a == null))
			{
				return 1;
			}
			return -1;
		}
		if (a == null)
		{
			return 0;
		}
		int num = a.atlasPopulationMode.CompareTo(b.atlasPopulationMode);
		if (num != 0)
		{
			return num;
		}
		if (getAssetType != null)
		{
			num = getAssetType(a).CompareTo(getAssetType(b));
			if (num != 0)
			{
				return num;
			}
		}
		return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
	}

	private void AppendFontAssetInfo(TMP_FontAsset fontAsset, StringBuilder sb)
	{
		if (fontAsset != null)
		{
			AppendNewLine(sb);
			sb.Append(fontAsset.GetInstanceID());
			sb.Append(' ');
			AppendName(fontAsset.name, "yellow", sb);
			sb.Append(' ').Append(Dbg.FormatColor(fontAsset.atlasPopulationMode));
			sb.Append(':');
			Dictionary<uint, TMP_Character> dictionary = field.GetValue(fontAsset) as Dictionary<uint, TMP_Character>;
			AppendNewLine(sb);
			sb.AppendFormat("m_CharacterLookupDictionary: {0}", (dictionary != null) ? $"<color=cyan>{dictionary.Count}</color>" : "<color=red>null</color>");
			depth++;
			AppendNewLine(sb);
			sb.Append("Textures:");
			depth++;
			Texture2D[] atlasTextures = fontAsset.atlasTextures;
			for (int i = 0; i < atlasTextures.Length; i++)
			{
				Texture2D texture2D = atlasTextures[i];
				if (texture2D != null)
				{
					long runtimeMemorySizeLong = Profiler.GetRuntimeMemorySizeLong(texture2D);
					allTextures[texture2D] = runtimeMemorySizeLong;
					AppendNewLine(sb);
					AppendName(texture2D.name, "pink", sb);
					sb.AppendFormat(" {0}x{1} ", texture2D.width, texture2D.height);
					AppendSize(runtimeMemorySizeLong, sb, 8388608L, 1024L);
				}
				else
				{
					AppendNewLine(sb);
					sb.AppendFormat("{0} <color=red>null</color>", i);
				}
			}
			depth--;
			depth--;
		}
		else
		{
			AppendNewLine(sb);
			sb.Append("null");
		}
	}

	private static void AppendName(string name, string color, StringBuilder sb)
	{
		sb.AppendFormat("<color={0}>", color);
		if (name == null)
		{
			sb.Append("[null]");
		}
		else if (name.Length == 0)
		{
			sb.Append("[empty]");
		}
		else
		{
			sb.Append(name);
		}
		sb.Append("</color>");
	}

	private void AppendNewLine(StringBuilder sb)
	{
		if (sb.Length > 0)
		{
			sb.Append('\n');
		}
		for (int i = 0; i < depth; i++)
		{
			sb.Append("    ");
		}
	}

	private static void AppendSize(long value, StringBuilder sb, long maxSize, long threshold)
	{
		if (value >= threshold)
		{
			AppendSize(value, sb, maxSize);
		}
	}

	private static void AppendSize(long value, StringBuilder sb, long maxSize = 0L)
	{
		MathExtensions.GetSizeRank(value, out var divisor, out var metric);
		if (maxSize > 0)
		{
			float t = (float)Math.Pow((double)value / (double)maxSize, 0.4);
			Color color = Color.Lerp(new Color(0.58f, 0.87f, 0f), new Color(0.875f, 0.251f, 0.188f), t);
			sb.AppendFormat("<color=#{0}>{1:0.0}</color> {2}", ColorUtility.ToHtmlStringRGBA(color), (double)value / divisor, metric);
		}
		else
		{
			sb.AppendFormat("{0:0.0} {1}", (double)value / divisor, metric);
		}
	}
}
