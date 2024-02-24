using System;
using System.Collections.Generic;
using System.Text;
using UWE;
using UnityEngine;

public class Dbg : MonoBehaviour
{
	public class Styles
	{
		private readonly Texture2D textureNormal;

		private readonly Texture2D textureHover;

		private readonly Texture2D textureActive;

		public readonly Font font;

		public readonly int fontSize;

		public readonly GUIStyle label;

		public readonly GUIStyle button;

		public readonly GUIStyle bar;

		public readonly GUIStyle barLabel;

		public readonly GUIStyle barRaw;

		public Styles()
		{
			textureNormal = GetTexture(4, 4, new Color32(0, 0, 0, 128));
			textureHover = GetTexture(4, 4, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 192));
			textureActive = GetTexture(4, 4, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 128));
			Dictionary<string, int> obj = new Dictionary<string, int>
			{
				{ "DejaVu Sans Mono", 11 },
				{ "Consolas", 12 },
				{ "Lucida Console", 12 },
				{ "ProggyTinyTT", 16 },
				{ "Courier New", 12 }
			};
			font = null;
			HashSet<string> hashSet = new HashSet<string>(Font.GetOSInstalledFontNames());
			foreach (KeyValuePair<string, int> item in obj)
			{
				if (hashSet.Contains(item.Key))
				{
					font = Font.CreateDynamicFontFromOSFont(item.Key, item.Value);
					fontSize = item.Value;
					break;
				}
			}
			if (font == null)
			{
				font = Resources.GetBuiltinResource<Font>("Arial.ttf");
				fontSize = 12;
			}
			label = new GUIStyle
			{
				padding = new RectOffset(10, 10, 10, 10),
				font = font,
				richText = true,
				normal = new GUIStyleState
				{
					background = Texture2D.whiteTexture,
					textColor = new Color(1f, 1f, 1f, 1f)
				}
			};
			button = new GUIStyle
			{
				font = font,
				padding = new RectOffset(6, 6, 6, 6),
				border = new RectOffset(4, 4, 4, 4),
				margin = new RectOffset(6, 6, 6, 6),
				alignment = TextAnchor.MiddleCenter,
				clipping = TextClipping.Clip,
				normal = new GUIStyleState
				{
					background = textureNormal,
					textColor = new Color(1f, 1f, 1f, 1f)
				},
				hover = new GUIStyleState
				{
					background = textureHover,
					textColor = new Color(1f, 1f, 1f, 1f)
				},
				active = new GUIStyleState
				{
					background = textureActive,
					textColor = new Color(1f, 1f, 1f, 1f)
				}
			};
			bar = new GUIStyle
			{
				padding = new RectOffset(6, 6, 6, 6),
				margin = new RectOffset(6, 6, 6, 6),
				normal = new GUIStyleState
				{
					background = Texture2D.whiteTexture
				}
			};
			barLabel = new GUIStyle
			{
				alignment = TextAnchor.MiddleCenter,
				font = font,
				padding = new RectOffset(6, 6, 6, 6),
				margin = new RectOffset(6, 6, 6, 6),
				normal = new GUIStyleState
				{
					textColor = new Color(1f, 1f, 1f, 1f)
				}
			};
			barRaw = new GUIStyle
			{
				normal = new GUIStyleState
				{
					background = Texture2D.whiteTexture
				}
			};
		}

		private static Texture2D GetTexture(int width, int height, Color32 color)
		{
			Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
			Color32[] array = new Color32[width * height];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = color;
			}
			texture2D.SetPixels32(array);
			texture2D.Apply();
			texture2D.hideFlags = HideFlags.HideAndDontSave;
			return texture2D;
		}
	}

	private static Dbg _main;

	private static int sBarHash = "Bar".GetHashCode();

	private static Styles _styles;

	private StringBuilder _sb = new StringBuilder();

	private GUIContent _content = new GUIContent();

	private static Vector3[] sWorldCornersArray = new Vector3[4];

	private static readonly Dictionary<Type, int> enumValueCount = new Dictionary<Type, int>();

	public static Dbg main
	{
		get
		{
			if (_main == null)
			{
				_main = new GameObject("Dbg")
				{
					hideFlags = HideFlags.HideAndDontSave
				}.AddComponent<Dbg>();
			}
			return _main;
		}
	}

	public static Styles styles
	{
		get
		{
			if (_styles == null)
			{
				_styles = new Styles();
			}
			return _styles;
		}
	}

	private void Awake()
	{
		base.useGUILayout = false;
	}

	private void OnGUI()
	{
		if (Event.current.type == EventType.Repaint && _sb.Length > 0)
		{
			WriteRawInternal(TextAnchor.UpperLeft, new Vector2(10f, 10f), _sb.ToString());
			_sb.Length = 0;
		}
	}

	private void WriteRawInternal(TextAnchor anchor, Vector2 offset, string text)
	{
		_content.text = text;
		Vector2 vector = styles.label.CalcSize(_content);
		Vector2 vector2 = new Vector2(0f, 0f);
		switch (anchor)
		{
		case TextAnchor.UpperLeft:
			vector2.x = offset.x;
			vector2.y = offset.y;
			break;
		case TextAnchor.UpperCenter:
			vector2.x = 0.5f * ((float)Screen.width - vector.x);
			vector2.y = offset.y;
			break;
		case TextAnchor.UpperRight:
			vector2.x = (float)Screen.width - vector.x - offset.x;
			vector2.y = offset.y;
			break;
		case TextAnchor.MiddleLeft:
			vector2.x = offset.x;
			vector2.y = 0.5f * ((float)Screen.height - vector.y);
			break;
		case TextAnchor.MiddleCenter:
			vector2.x = 0.5f * ((float)Screen.width - vector.x);
			vector2.y = 0.5f * ((float)Screen.height - vector.y);
			break;
		case TextAnchor.MiddleRight:
			vector2.x = (float)Screen.width - vector.x - offset.x;
			vector2.y = 0.5f * ((float)Screen.height - vector.y);
			break;
		case TextAnchor.LowerLeft:
			vector2.x = offset.x;
			vector2.y = (float)Screen.height - vector.y - offset.y;
			break;
		case TextAnchor.LowerCenter:
			vector2.x = 0.5f * ((float)Screen.width - vector.x);
			vector2.y = (float)Screen.height - vector.y - offset.y;
			break;
		case TextAnchor.LowerRight:
			vector2.x = (float)Screen.width - vector.x - offset.x;
			vector2.y = (float)Screen.height - vector.y - offset.y;
			break;
		}
		Color backgroundColor = GUI.backgroundColor;
		GUI.backgroundColor = new Color(0f, 0f, 0f, 0.75f);
		GUI.Label(new Rect(vector2.x, vector2.y, vector.x, vector.y), _content, styles.label);
		GUI.backgroundColor = backgroundColor;
	}

	public static void Write(string text)
	{
		if (main._sb.Length > 0)
		{
			main._sb.Append('\n');
		}
		main._sb.Append(text);
	}

	public static void Write(object obj)
	{
		Write((obj != null) ? obj.ToString() : "null");
	}

	public static void Write(string format, params object[] args)
	{
		Write(string.Format(format, args));
	}

	public static void WriteRaw(TextAnchor anchor, Vector2 offset, string text)
	{
		main.WriteRawInternal(anchor, offset, text);
	}

	public static string LogHierarchy(GameObject gameObject)
	{
		if (gameObject == null)
		{
			return LogHierarchy();
		}
		return LogHierarchy(gameObject.GetComponent<Transform>());
	}

	public static string LogHierarchy(Transform transform = null)
	{
		if (transform == null)
		{
			return "null";
		}
		using (ListPool<Transform> listPool = Pool<ListPool<Transform>>.Get())
		{
			using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
			{
				List<Transform> list = listPool.list;
				StringBuilder sb = stringBuilderPool.sb;
				while (transform != null)
				{
					list.Add(transform);
					transform = transform.parent;
				}
				int num = list.Count - 1;
				for (int num2 = num; num2 >= 0; num2--)
				{
					transform = list[num2];
					sb.Append(' ', num - num2);
					sb.AppendLine(transform.name);
				}
				return sb.ToString();
			}
		}
	}

	public static string LogPath(Transform transform = null)
	{
		if (transform == null)
		{
			return "null";
		}
		using (ListPool<Transform> listPool = Pool<ListPool<Transform>>.Get())
		{
			using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
			{
				List<Transform> list = listPool.list;
				StringBuilder sb = stringBuilderPool.sb;
				while (transform != null)
				{
					list.Add(transform);
					transform = transform.parent;
				}
				for (int num = list.Count - 1; num >= 0; num--)
				{
					transform = list[num];
					sb.Append('/');
					sb.Append(transform.name);
				}
				return sb.ToString();
			}
		}
	}

	public static void TraceTarget()
	{
		float maxDist = 100f;
		UWE.Utils.TraceForFPSTarget(Player.main.gameObject, maxDist, 0.15f, out var closestObj, out var _);
		Write(LogHierarchy(closestObj));
	}

	public static string LogTree(TreeNode tree)
	{
		string text = string.Empty;
		using (IEnumerator<TreeNode> enumerator = tree.Traverse())
		{
			while (enumerator.MoveNext())
			{
				TreeNode current = enumerator.Current;
				text += new string('\t', current.depth);
				text += current.id;
				text += "\n";
			}
			return text;
		}
	}

	public static T RandomEnumValue<T>() where T : struct, IConvertible
	{
		int index;
		int length;
		return RandomEnumValue<T>(out index, out length);
	}

	public static T RandomEnumValue<T>(out int index, out int length) where T : struct, IConvertible
	{
		index = -1;
		length = -1;
		Type typeFromHandle = typeof(T);
		if (typeFromHandle.IsEnum)
		{
			Array values = Enum.GetValues(typeFromHandle);
			length = values.Length;
			index = UnityEngine.Random.Range(0, length - 1);
			return (T)values.GetValue(index);
		}
		Debug.LogError("T must be an enumerated type");
		return default(T);
	}

	public static int PlainEnumIndex(Enum instance, out int length)
	{
		length = -1;
		Array values = Enum.GetValues(instance.GetType());
		length = values.Length;
		for (int i = 0; i < length; i++)
		{
			if (values.GetValue(i).Equals(instance))
			{
				return i;
			}
		}
		return -1;
	}

	public static void HighlightRect(RectTransform rectTransform, Color color, float duration)
	{
		if (!(rectTransform == null))
		{
			rectTransform.GetWorldCorners(sWorldCornersArray);
			Debug.DrawLine(sWorldCornersArray[0], sWorldCornersArray[1], color, duration);
			Debug.DrawLine(sWorldCornersArray[1], sWorldCornersArray[2], color, duration);
			Debug.DrawLine(sWorldCornersArray[2], sWorldCornersArray[3], color, duration);
			Debug.DrawLine(sWorldCornersArray[3], sWorldCornersArray[0], color, duration);
		}
	}

	public static float DoBarLayout(string text, float value, Color background, Color foreground, bool horizontal = true, params GUILayoutOption[] options)
	{
		return DoBar(GUILayoutUtility.GetRect(0f, 0f, styles.bar, options), (!string.IsNullOrEmpty(text)) ? new GUIContent(text) : null, value, foreground, background, horizontal, styles.bar, styles.barLabel);
	}

	public static float DoBar(Rect position, GUIContent content, float value, Color foreground, Color background, bool horizontal, GUIStyle styleRect, GUIStyle styleLabel)
	{
		Event current = Event.current;
		int controlID = GUIUtility.GetControlID(sBarHash, FocusType.Passive, position);
		float num = ((!horizontal) ? Mathf.Clamp01((position.height - current.mousePosition.y + position.y) / position.height) : Mathf.Clamp01((current.mousePosition.x - position.x) / position.width));
		switch (current.GetTypeForControl(controlID))
		{
		case EventType.MouseDown:
			if (position.Contains(current.mousePosition) && GUIUtility.hotControl == 0)
			{
				value = num;
				GUI.changed = true;
				GUIUtility.hotControl = controlID;
				current.Use();
			}
			break;
		case EventType.MouseUp:
			if (GUIUtility.hotControl == controlID)
			{
				value = num;
				GUI.changed = true;
				GUIUtility.hotControl = 0;
				current.Use();
			}
			break;
		case EventType.MouseDrag:
			if (GUIUtility.hotControl == controlID)
			{
				value = num;
				GUI.changed = true;
				current.Use();
			}
			break;
		case EventType.Repaint:
		{
			float v = ((GUIUtility.hotControl == controlID) ? num : value);
			DrawBar(position, v, background, foreground, horizontal, styleRect);
			if (content != null && !string.IsNullOrEmpty(content.text))
			{
				styleLabel.Draw(position, content, isHover: false, isActive: false, on: false, hasKeyboardFocus: false);
			}
			break;
		}
		}
		return value;
	}

	public static void DrawBar(Rect position, float v, Color background, Color foreground, bool horizontal, GUIStyle style)
	{
		if (Event.current.type == EventType.Repaint)
		{
			Color backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = background;
			style.Draw(position, isHover: false, isActive: false, on: false, hasKeyboardFocus: false);
			GUI.backgroundColor = foreground;
			Rect position2;
			if (horizontal)
			{
				position2 = position;
				position2.width *= Mathf.Clamp01(v);
			}
			else
			{
				float num = position.height * Mathf.Clamp01(v);
				float num2 = position.height - num;
				position2 = new Rect(position.x, position.y + num2, position.width, num);
			}
			style.Draw(position2, isHover: false, isActive: false, on: false, hasKeyboardFocus: false);
			GUI.backgroundColor = backgroundColor;
		}
	}

	public static Color GetColor(object obj)
	{
		float num;
		if (obj != null)
		{
			int hashCode = obj.GetHashCode();
			int value = int.MaxValue;
			Type type = obj.GetType();
			if (type.IsEnum && !enumValueCount.TryGetValue(type, out value))
			{
				value = Enum.GetNames(type).Length;
				enumValueCount.Add(type, value);
			}
			num = (float)hashCode / (float)value;
		}
		else
		{
			num = 0f;
		}
		return Color.HSVToRGB(num * 0.5f + 0.5f, 0.6f, 0.8f, hdr: false);
	}

	public static string FormatColor(object obj)
	{
		string arg = ColorUtility.ToHtmlStringRGB(GetColor(obj));
		return $"<color=#{arg}>{obj}</color>";
	}

	public static float Slider(string label, float value, float min, float max, params GUILayoutOption[] options)
	{
		GUIStyle label2 = GUI.skin.label;
		GUIStyle horizontalSlider = GUI.skin.horizontalSlider;
		GUIContent content = new GUIContent(label);
		Vector2 vector = label2.CalcSize(content);
		Rect rect = GUILayoutUtility.GetRect(vector.x, vector.x + 5f + 100f, 18f, 18f, horizontalSlider, options);
		GUI.Label(rect, content);
		rect.x += vector.x + 5f;
		rect.width -= vector.x + 5f;
		rect.y += 0.5f * (vector.y - 18f);
		return GUI.HorizontalSlider(rect, value, min, max);
	}
}
