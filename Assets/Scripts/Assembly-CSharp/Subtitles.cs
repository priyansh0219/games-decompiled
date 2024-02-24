using System.Text;
using Gendarme;
using UnityEngine;

public class Subtitles : MonoBehaviour
{
	[AssertLocalization(2)]
	public const string actorFormat = "ActorFormat";

	private const float voiceoverSpeed = 15f;

	public const float defaultSpeed = 15f;

	private const int maxMessageLength = 250;

	private const int maxMessageLengthDefault = 250;

	private const int maxMessageLengthEnlarged = 165;

	private const char wordSeparator = ' ';

	private static readonly char[] sentenceSeparators = new char[3] { '.', '?', '!' };

	private static readonly char[] noSpaceLangSeparators = new char[3] { '。', '、', '，' };

	public static float speed = 15f;

	private static Subtitles _main;

	[AssertNotNull]
	public uGUI_MessageQueue queue;

	private StringBuilder textBuilder = new StringBuilder();

	private readonly StringBuilder partBuilder = new StringBuilder(250);

	private int currentMaxMessageLength = 250;

	public static Subtitles main => _main;

	private void Awake()
	{
		if (_main != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		_main = this;
		DevConsole.RegisterConsoleCommand(this, "subtitles", caseSensitiveArgs: true);
	}

	private void OnEnable()
	{
		uGUI_CanvasScaler.AddUIScaleListener(OnUIScaleChange);
	}

	private void OnDisable()
	{
		uGUI_CanvasScaler.RemoveUIScaleListener(OnUIScaleChange);
	}

	private void Update()
	{
		queue.visible = IsVisible();
	}

	public static bool ShouldSkipCharacter(char c)
	{
		return char.IsWhiteSpace(c);
	}

	public static int CountCharacters(StringBuilder text)
	{
		if (text == null || text.Length == 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < text.Length; i++)
		{
			if (!ShouldSkipCharacter(text[i]))
			{
				num++;
			}
		}
		return num;
	}

	public static float GetSubtitlesDuration(StringBuilder text)
	{
		return (float)CountCharacters(text) / speed;
	}

	public static float GetVoiceoverDuration(StringBuilder text)
	{
		return (float)CountCharacters(text) / 15f;
	}

	public static void Add(string key, object[] args = null)
	{
		if (!(_main == null))
		{
			_main.AddInternal(0, key, 0, args);
		}
	}

	public static void AddFromLine(string key, int lineNumber)
	{
		if (!(_main == null))
		{
			_main.AddInternal(0, key, lineNumber, null);
		}
	}

	public static void AddRawLong(int id, StringBuilder text, float delay, float durationText = -1f, float durationSound = -1f)
	{
		if (!(_main == null))
		{
			_main.AddRawLongInternal(id, text, delay, durationText, durationSound);
		}
	}

	public static bool HasQueued()
	{
		if (_main == null)
		{
			return false;
		}
		return _main.queue.HasQueued();
	}

	private void AddInternal(int id, string key, int startingLineNumber, object[] args)
	{
		if (string.IsNullOrEmpty(key))
		{
			return;
		}
		Language.MetaData metaData = Language.main.GetMetaData(key);
		if (metaData != null)
		{
			float num = 0f;
			for (int i = startingLineNumber; i < metaData.lineCount; i++)
			{
				Language.LineData line = metaData.GetLine(i);
				textBuilder.Length = 0;
				Language.main.AppendFormat(textBuilder, line.text, args);
				if (line.hasDelay)
				{
					num = line.delay;
				}
				float durationText = (line.hasDuration ? line.duration : (-1f));
				AddRawLong(id, textBuilder, num, durationText);
				float num2 = (line.hasDuration ? line.duration : GetVoiceoverDuration(textBuilder));
				num += num2;
			}
		}
		else
		{
			textBuilder.Length = 0;
			Language.main.AppendFormat(textBuilder, key, args);
			AddRawLong(id, textBuilder, 0f);
		}
	}

	private void AddRawLongInternal(int id, StringBuilder text, float delay, float durationText, float durationSound)
	{
		if (!Language.main.showSubtitles)
		{
			return;
		}
		int num = CountCharacters(text);
		if (num == 0)
		{
			return;
		}
		float num2 = num;
		if (durationText < 0f)
		{
			durationText = num2 / speed;
		}
		if (durationSound < 0f)
		{
			durationSound = durationText;
		}
		float num3 = Mathf.Max(durationText, durationSound);
		if (text.Length > currentMaxMessageLength)
		{
			int i = 0;
			while (i < text.Length)
			{
				int num4 = i + currentMaxMessageLength;
				int num5 = text.Length;
				if (num5 > num4)
				{
					num5 = ((!Language.main.DoesLanguageUseSpacesAsSeparators()) ? text.LastIndexOfAny(noSpaceLangSeparators, num4) : text.LastIndexOfAny(sentenceSeparators, num4, ' '));
					if (num5 <= i)
					{
						num5 = text.LastIndexOf(' ', num4);
						if (num5 <= i)
						{
							num5 = num4 - 1;
						}
					}
					num5++;
				}
				for (; i < num5 && char.IsWhiteSpace(text[i]); i++)
				{
				}
				StringBuilderExtensions.Substring(text, i, num5 - i, partBuilder);
				float num6 = Mathf.Clamp01((float)CountCharacters(partBuilder) / num2);
				float duration = num3 * num6;
				queue.Add(id, partBuilder, delay, duration);
				i = num5;
				delay += durationSound * num6;
			}
		}
		else
		{
			queue.Add(id, text, delay, num3);
		}
	}

	private bool IsVisible()
	{
		Player player = Player.main;
		PDA pDA = ((player != null) ? player.GetPDA() : null);
		if (pDA != null && pDA.isInUse && !IntroLifepodDirector.IsActive)
		{
			return false;
		}
		return true;
	}

	private void OnUIScaleChange(float scale)
	{
		currentMaxMessageLength = (((double)scale > 1.2) ? 165 : 250);
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private void OnConsoleCommand_subtitles(NotificationCenter.Notification n)
	{
		if (n.data != null && n.data.Count == 1)
		{
			Add((string)n.data[0]);
		}
		else
		{
			ErrorMessage.AddDebug("usage: subtitles key");
		}
	}
}
