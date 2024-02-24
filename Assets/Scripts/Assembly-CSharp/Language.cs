using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Gendarme;
using LitJson;
using TMPro;
using UWE;
using UnityEngine;

public class Language : MonoBehaviour, ILanguage
{
	public class LineData
	{
		public string text { get; private set; }

		public bool hasDelay { get; private set; }

		public bool hasDuration { get; private set; }

		public float delay { get; private set; }

		public float duration { get; private set; }

		public LineData(string text, float? delay, float? duration)
		{
			this.text = text;
			hasDelay = delay.HasValue;
			hasDuration = duration.HasValue;
			this.delay = delay ?? 0f;
			this.duration = duration ?? 0f;
		}
	}

	public class MetaData
	{
		private string _text;

		private List<LineData> _lines;

		public string text => _text;

		public int lineCount => _lines.Count;

		public MetaData(string text, List<LineData> lines)
		{
			_text = text;
			_lines = lines;
		}

		public LineData GetLine(int index)
		{
			if (index < 0 || index >= _lines.Count)
			{
				return null;
			}
			return _lines[index];
		}
	}

	private static readonly Dictionary<string, string> cultureToLanguage = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		{ "bg-BG", "Bulgarian" },
		{ "zh-CN", "Chinese (Simplified)" },
		{ "zh-Hant", "Chinese (Traditional)" },
		{ "hr-HR", "Croatian" },
		{ "cs-CZ", "Czech" },
		{ "da-DK", "Danish" },
		{ "nl-BE", "Dutch" },
		{ "en-US", "English" },
		{ "et-EE", "Estonian" },
		{ "fi-FI", "Finnish" },
		{ "fr-FR", "French" },
		{ "de-DE", "German" },
		{ "el-GR", "Greek" },
		{ "hu-HU", "Hungarian" },
		{ "ga-IE", "Irish" },
		{ "it-IT", "Italian" },
		{ "ja-JP", "Japanese" },
		{ "ko-KR", "Korean" },
		{ "lv-LV", "Latvian" },
		{ "lt-LT", "Lithuanian" },
		{ "nb-NO", "Norwegian" },
		{ "pl-PL", "Polish" },
		{ "pt-BR", "Portuguese (Brazil)" },
		{ "pt-PT", "Portuguese" },
		{ "ro-RO", "Romanian" },
		{ "ru-RU", "Russian" },
		{ "sr-Cyrl", "Serbian" },
		{ "sk-SK", "Slovak" },
		{ "es-ES", "Spanish" },
		{ "es-MX", "Spanish (Latin America)" },
		{ "es-419", "Spanish (Latin America)" },
		{ "sv-SE", "Swedish" },
		{ "th-TH", "Thai" },
		{ "tr-TR", "Turkish" },
		{ "uk-UA", "Ukrainian" },
		{ "vi-VN", "Vietnamese" }
	};

	public const string defaultLanguage = "English";

	private static Language _main = null;

	private static bool isQuitting = false;

	public bool debug;

	private readonly Dictionary<string, string> strings = new Dictionary<string, string>();

	private Dictionary<string, MetaData> metadata = new Dictionary<string, MetaData>();

	private static StringBuilder sb = new StringBuilder();

	private bool _showSubtitles = true;

	private string currentLanguage;

	private CultureInfo currentCultureInfo;

	private static HashSet<string> missingKeys = new HashSet<string>();

	private static LanguageParser parser = new LanguageParser();

	private Action onLanguageChanged;

	private object[] args = new object[5];

	private string allUsedCharsLanguage;

	private HashSet<char> allUsedChars = new HashSet<char>();

	public static bool isNotQuitting
	{
		get
		{
			if (!(_main != null))
			{
				return !isQuitting;
			}
			return true;
		}
	}

	public static Language main
	{
		get
		{
			if (_main == null)
			{
				if (isQuitting)
				{
					throw new InvalidOperationException("Attempted access Language.main while application is quitting. Use Language.main only if Language.isNotQuitting is true. ");
				}
				GameObject gameObject = new GameObject("Language");
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				_main = gameObject.AddComponent<Language>();
				_main.Initialize();
			}
			return _main;
		}
	}

	public bool showSubtitles
	{
		get
		{
			return _showSubtitles;
		}
		set
		{
			_showSubtitles = value;
		}
	}

	public static event Action OnLanguageChanged
	{
		add
		{
			if (!isQuitting)
			{
				Language language = main;
				language.onLanguageChanged = (Action)Delegate.Combine(language.onLanguageChanged, value);
			}
		}
		remove
		{
			if (!isQuitting)
			{
				Language language = main;
				language.onLanguageChanged = (Action)Delegate.Remove(language.onLanguageChanged, value);
			}
		}
	}

	public static void Deinitialize()
	{
		missingKeys.Clear();
	}

	private void Awake()
	{
		if (_main != null)
		{
			UWE.Utils.DestroyWrap(this);
		}
	}

	private void OnDisable()
	{
		if (Application.isEditor || !isQuitting)
		{
			LanguageSDF.Deinitialize();
		}
	}

	private void OnDestroy()
	{
		if (Application.isEditor || !isQuitting)
		{
			LanguageSDF.Deinitialize();
		}
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void OnApplicationQuit()
	{
		isQuitting = true;
	}

	public void Initialize()
	{
		string @string = PlayerPrefs.GetString("Language", GetDefaultLanguage());
		Initialize(@string);
	}

	public void Initialize(string language)
	{
		SetCurrentLanguage(language);
	}

	private void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "language", caseSensitiveArgs: true, combineArgs: true);
		DevConsole.RegisterConsoleCommand(this, "translationkey", caseSensitiveArgs: false, combineArgs: true);
	}

	private void OnConsoleCommand_language(NotificationCenter.Notification n)
	{
		if (n.data != null && n.data.Count == 1)
		{
			string text = (string)n.data[0];
			SetCurrentLanguage(text);
		}
		else
		{
			ErrorMessage.AddDebug("usage: language <language>");
		}
	}

	private static string GetDefaultLanguage()
	{
		SystemLanguage systemLanguage = Application.systemLanguage;
		switch (systemLanguage)
		{
		case SystemLanguage.Chinese:
		case SystemLanguage.ChineseSimplified:
			return "Chinese (Simplified)";
		case SystemLanguage.ChineseTraditional:
			return "Chinese (Traditional)";
		default:
			return systemLanguage.ToString();
		}
	}

	public void SetCurrentLanguage(string language)
	{
		if (!(currentLanguage == language) && !LoadLanguageFile(language))
		{
			LoadLanguageFile("English");
		}
	}

	public void NotifyLanguageChanged()
	{
		if (onLanguageChanged == null)
		{
			return;
		}
		Delegate[] invocationList = onLanguageChanged.GetInvocationList();
		foreach (Delegate @delegate in invocationList)
		{
			try
			{
				@delegate.DynamicInvoke(null);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public string GetCurrentLanguage()
	{
		return currentLanguage;
	}

	public string[] GetLanguages()
	{
		string[] files = Directory.GetFiles(SNUtils.InsideUnmanaged("LanguageFiles"), "*.json");
		string[] array = new string[files.Length];
		for (int i = 0; i < files.Length; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[i]);
			array[i] = fileNameWithoutExtension;
		}
		return array;
	}

	public string GetLanguageTag(string languageName)
	{
		foreach (KeyValuePair<string, string> item in cultureToLanguage)
		{
			if (cultureToLanguage.Comparer.Equals(item.Value, languageName))
			{
				return item.Key;
			}
		}
		return "en-US";
	}

	public string GetLanguageFromCultureTag(string cultureTag)
	{
		if (cultureToLanguage.TryGetValue(cultureTag, out var value))
		{
			return value;
		}
		return "English";
	}

	public bool DoesLanguageUseSpacesAsSeparators()
	{
		if (currentLanguage == "Chinese (Simplified)" || currentLanguage == "Chinese (Traditional)" || currentLanguage == "Japanese")
		{
			return false;
		}
		return true;
	}

	[SuppressMessage("Gendarme.Rules.Portability", "DoNotHardcodePathsRule")]
	private bool LoadLanguageFile(string language)
	{
		string path = SNUtils.InsideUnmanaged($"LanguageFiles/{language}.json");
		if (!File.Exists(path))
		{
			return false;
		}
		JsonData jsonData;
		using (StreamReader reader = new StreamReader(path))
		{
			try
			{
				jsonData = JsonMapper.ToObject(reader);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return false;
			}
		}
		if (currentLanguage != language)
		{
			missingKeys.Clear();
		}
		PlayerPrefs.SetString("Language", language);
		currentLanguage = language;
		CultureInfo.GetCultures(CultureTypes.AllCultures);
		try
		{
			string text = GetLanguageTag(language);
			if (text == "es-419")
			{
				text = "es-MX";
			}
			currentCultureInfo = new CultureInfo(text);
			string percentSymbol = currentCultureInfo.NumberFormat.PercentSymbol;
			if (percentSymbol.Contains('٪'))
			{
				currentCultureInfo.NumberFormat.PercentSymbol = percentSymbol.Replace('٪', '%');
			}
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
			currentCultureInfo = new CultureInfo("en-US");
		}
		strings.Clear();
		foreach (string key in jsonData.Keys)
		{
			strings[key] = (string)jsonData[key];
		}
		if (Application.platform == RuntimePlatform.PS4)
		{
			Remap(jsonData, ".PS4");
		}
		else if (Application.platform == RuntimePlatform.XboxOne)
		{
			Remap(jsonData, ".XB1");
		}
		else if (Application.platform == RuntimePlatform.PS5)
		{
			Remap(jsonData, ".PS5");
		}
		else if (Application.platform == RuntimePlatform.GameCoreScarlett)
		{
			Remap(jsonData, ".XBX");
		}
		else if (Application.platform == RuntimePlatform.Switch)
		{
			Remap(jsonData, ".Switch");
		}
		ParseMetaData();
		LanguageCache.OnLanguageChanged();
		LanguageSDF.Initialize(this);
		return true;
	}

	private void Remap(JsonData json, string platformSuffix)
	{
		foreach (string item in json.Keys.Where((string p) => p.EndsWith(platformSuffix, StringComparison.Ordinal)))
		{
			string key = item.Substring(0, item.Length - platformSuffix.Length);
			strings[key] = (string)json[item];
		}
	}

	public Dictionary<string, string>.Enumerator GetEnumerator()
	{
		return strings.GetEnumerator();
	}

	public bool TryGet(string key, out string result)
	{
		if (string.IsNullOrEmpty(key))
		{
			result = string.Empty;
			return false;
		}
		if (strings.TryGetValue(key, out result))
		{
			return true;
		}
		if (missingKeys.Add(key))
		{
			_ = $"'{key}' is missing in {currentLanguage}.json!";
		}
		return false;
	}

	public string Get(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return string.Empty;
		}
		if (!TryGet(key, out var result))
		{
			if (debug)
			{
				Debug.LogWarningFormat(this, "no translation for key: '{0}'", key);
			}
			return key;
		}
		return result;
	}

	public string GetOrFallback(string key, string fallbackKey)
	{
		if (TryGet(key, out var result) && !string.IsNullOrEmpty(result))
		{
			return result;
		}
		return Get(fallbackKey);
	}

	public string GetFormat(string key)
	{
		return Get(key);
	}

	public string GetFormat(string key, params object[] args)
	{
		string text = Get(key);
		try
		{
			return FormatString(text, args);
		}
		catch (FormatException innerException)
		{
			Debug.LogException(new FormatException("Language FormatException was caught for key [" + key + "] in [" + currentLanguage + "] language.", innerException));
			return text;
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public string GetFormat<Arg0>(string key, Arg0 arg0)
	{
		return GetFormatImpl(key, arg0, null, null, null, null);
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public string GetFormat<Arg0, Arg1>(string key, Arg0 arg0, Arg1 arg1)
	{
		return GetFormatImpl(key, arg0, arg1, null, null, null);
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public string GetFormat<Arg0, Arg1, Arg2>(string key, Arg0 arg0, Arg1 arg1, Arg2 arg2)
	{
		return GetFormatImpl(key, arg0, arg1, arg2, null, null);
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public string GetFormat<Arg0, Arg1, Arg2, Arg3>(string key, Arg0 arg0, Arg1 arg1, Arg2 arg2, Arg3 arg3)
	{
		return GetFormatImpl(key, arg0, arg1, arg2, arg3, null);
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public string GetFormat<Arg0, Arg1, Arg2, Arg3, Arg4>(string key, Arg0 arg0, Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4)
	{
		return GetFormatImpl(key, arg0, arg1, arg2, arg3, arg4);
	}

	public void AppendFormat(StringBuilder sb, string key, params object[] args)
	{
		try
		{
			FormatString(Get(key), args, sb);
		}
		catch (FormatException innerException)
		{
			Debug.LogException(new FormatException("Language FormatException was caught for key [" + key + "] in [" + currentLanguage + "] language.", innerException));
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public void AppendFormat<Arg0>(StringBuilder sb, string key, Arg0 arg0)
	{
		AppendFormatImpl(sb, key, arg0, null, null, null, null);
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public void AppendFormat<Arg0, Arg1>(StringBuilder sb, string key, Arg0 arg0, Arg1 arg1)
	{
		AppendFormatImpl(sb, key, arg0, arg1, null, null, null);
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public void AppendFormat<Arg0, Arg1, Arg2>(StringBuilder sb, string key, Arg0 arg0, Arg1 arg1, Arg2 arg2)
	{
		AppendFormatImpl(sb, key, arg0, arg1, arg2, null, null);
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public void AppendFormat<Arg0, Arg1, Arg2, Arg3>(StringBuilder sb, string key, Arg0 arg0, Arg1 arg1, Arg2 arg2, Arg3 arg3)
	{
		AppendFormatImpl(sb, key, arg0, arg1, arg2, arg3, null);
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public void AppendFormat<Arg0, Arg1, Arg2, Arg3, Arg4>(StringBuilder sb, string key, Arg0 arg0, Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4)
	{
		AppendFormatImpl(sb, key, arg0, arg1, arg2, arg3, arg4);
	}

	private string GetFormatImpl(string key, object arg0, object arg1, object arg2, object arg3, object arg4)
	{
		args[0] = arg0;
		args[1] = arg1;
		args[2] = arg2;
		args[3] = arg3;
		args[4] = arg4;
		string text = Get(key);
		try
		{
			return FormatString(text, args);
		}
		catch (FormatException innerException)
		{
			Debug.LogException(new FormatException("Language FormatException was caught for key [" + key + "] in [" + currentLanguage + "] language.", innerException));
			return text;
		}
	}

	private void AppendFormatImpl(StringBuilder sb, string key, object arg0, object arg1, object arg2, object arg3, object arg4)
	{
		args[0] = arg0;
		args[1] = arg1;
		args[2] = arg2;
		args[3] = arg3;
		args[4] = arg4;
		try
		{
			FormatString(Get(key), args, sb);
		}
		catch (FormatException innerException)
		{
			Debug.LogException(new FormatException("Language FormatException was caught for key [" + key + "] in [" + currentLanguage + "] language.", innerException));
		}
	}

	private string FormatString(string format, object[] args)
	{
		if (args != null && args.Length != 0)
		{
			try
			{
				return string.Format(currentCultureInfo, format, args);
			}
			catch (FormatException exception)
			{
				RethrowFormatException(format, args, exception);
			}
		}
		return format;
	}

	private void FormatString(string format, object[] args, StringBuilder sb)
	{
		if (args != null && args.Length != 0)
		{
			try
			{
				sb.AppendFormat(currentCultureInfo, format, args);
				return;
			}
			catch (FormatException exception)
			{
				RethrowFormatException(format, args, exception);
			}
		}
		sb.Append(format);
	}

	private void RethrowFormatException(string format, object[] args, FormatException exception)
	{
		string text = string.Join(", ", args);
		throw new FormatException("Language FormatException was caught for format [" + format + "] and args: [" + text + "].", exception);
	}

	private string StringToLiteral(string input)
	{
		return input;
	}

	public bool Contains(string key)
	{
		return strings.ContainsKey(key);
	}

	public bool ContainsEncy(string key)
	{
		if (strings.ContainsKey("Ency_" + key))
		{
			return strings.ContainsKey("EncyDesc_" + key);
		}
		return false;
	}

	public List<string> GetKeysFor(string translated, StringComparison comparison)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(translated))
		{
			return list;
		}
		foreach (KeyValuePair<string, string> @string in strings)
		{
			if (string.Equals(@string.Value, translated, comparison))
			{
				list.Add(@string.Key);
			}
		}
		return list;
	}

	public List<KeyValuePair<string, string>> GetEncyPairs()
	{
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		foreach (KeyValuePair<string, string> @string in strings)
		{
			if (@string.Key.StartsWith("Ency_", StringComparison.OrdinalIgnoreCase))
			{
				list.Add(@string);
			}
		}
		return list;
	}

	private void OnConsoleCommand_translationkey(NotificationCenter.Notification n)
	{
		if (n != null && n.data != null && n.data.Count > 0)
		{
			string text = (string)n.data[0];
			List<string> keysFor = GetKeysFor(text, StringComparison.OrdinalIgnoreCase);
			if (keysFor.Count > 0)
			{
				ErrorMessage.AddDebug(string.Format("Translation key(s) for '{0}' string: '{1}'", text, string.Join("', '", keysFor.ToArray())));
			}
			else
			{
				ErrorMessage.AddDebug($"Translation key for '{text}' string is not defined");
			}
		}
		else
		{
			ErrorMessage.AddDebug("Usage: translationkey translated_string");
		}
	}

	private void ParseMetaData()
	{
		metadata = new Dictionary<string, MetaData>();
		foreach (KeyValuePair<string, string> @string in strings)
		{
			string key = @string.Key;
			string value = @string.Value;
			string[] array = value.Split(LanguageParser.messageSeparators, StringSplitOptions.None);
			if (array.Length > 1)
			{
				List<LineData> list = new List<LineData>();
				foreach (string input in array)
				{
					parser.Parse(input);
					LineData item = new LineData(parser.text, parser.delay, parser.duration);
					list.Add(item);
				}
				sb.Length = 0;
				for (int j = 0; j < list.Count; j++)
				{
					LineData lineData = list[j];
					if (sb.Length > 0)
					{
						sb.Append('\n');
					}
					sb.Append(lineData.text);
				}
				MetaData value2 = new MetaData(sb.ToString(), list);
				metadata.Add(key, value2);
			}
			else
			{
				parser.Parse(value);
				if (parser.delay.HasValue || parser.duration.HasValue)
				{
					List<LineData> list2 = new List<LineData>();
					LineData lineData2 = new LineData(parser.text, parser.delay, parser.duration);
					list2.Add(lineData2);
					MetaData value3 = new MetaData(lineData2.text, list2);
					metadata.Add(key, value3);
				}
			}
		}
		foreach (KeyValuePair<string, MetaData> metadatum in metadata)
		{
			string key2 = metadatum.Key;
			MetaData value4 = metadatum.Value;
			strings[key2] = value4.text;
		}
	}

	public MetaData GetMetaData(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			return metadata.GetOrDefault(key, null);
		}
		return null;
	}

	public static string GetLineText(string key, int line)
	{
		if (string.IsNullOrEmpty(key))
		{
			return key;
		}
		string result = string.Empty;
		MetaData metaData = main.GetMetaData(key);
		if (metaData != null)
		{
			LineData line2 = metaData.GetLine(line);
			if (line2 != null)
			{
				result = line2.text;
			}
		}
		else
		{
			result = main.Get(key);
		}
		return result;
	}

	public HashSet<char> GetAllUsedChars()
	{
		string text = GetCurrentLanguage();
		if (allUsedCharsLanguage != text)
		{
			allUsedCharsLanguage = text;
			allUsedChars.Clear();
			for (int i = 32; i < 127; i++)
			{
				allUsedChars.Add((char)i);
			}
			int year = 2021;
			int day = 27;
			string format = Get("DateFormat");
			using (StringBuilderPool stringBuilderPool = Pool<StringBuilderPool>.Get())
			{
				StringBuilder stringBuilder = stringBuilderPool.sb;
				for (int j = 1; j <= 12; j++)
				{
					stringBuilder.Length = 0;
					int hour = ((j % 2 == 0) ? 15 : 2);
					stringBuilder.AppendFormat(arg0: new DateTime(year, j, day, hour, 10, 0), provider: currentCultureInfo, format: format);
					AddChars(allUsedChars, stringBuilder);
				}
			}
			allUsedChars.Add('…');
			allUsedChars.Add('_');
			using (Dictionary<string, string>.Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					AddChars(allUsedChars, enumerator.Current.Value);
				}
			}
		}
		return allUsedChars;
	}

	public StringBuilder FilterGlyphs(string src, StringBuilder dst)
	{
		char value = (char)TMP_Settings.missingGlyphCharacter;
		dst.Length = 0;
		if (src != null)
		{
			HashSet<char> hashSet = GetAllUsedChars();
			foreach (char c in src)
			{
				if (hashSet.Contains(c))
				{
					dst.Append(c);
				}
				else
				{
					dst.Append(value);
				}
			}
		}
		return dst;
	}

	private static void AddChars(HashSet<char> chars, string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			foreach (char item in text)
			{
				chars.Add(item);
			}
		}
	}

	private static void AddChars(HashSet<char> chars, StringBuilder sb)
	{
		if (sb != null && sb.Length != 0)
		{
			for (int i = 0; i < sb.Length; i++)
			{
				char item = sb[i];
				chars.Add(item);
			}
		}
	}

	public static void SetupDefaultSettings()
	{
		if (main != null)
		{
			main.SetCurrentLanguage(GetDefaultLanguage());
			main.showSubtitles = false;
		}
		Subtitles.speed = 15f;
	}
}
