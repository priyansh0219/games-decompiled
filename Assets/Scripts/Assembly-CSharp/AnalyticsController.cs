using System;
using System.Collections.Generic;
using Gendarme;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class AnalyticsController : MonoBehaviour
{
	private const int defaultStatisticsPeriod = 1200;

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.LateUpdateAfterInput;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public string _playthroughId;

	[NonSerialized]
	[ProtoMember(3)]
	public readonly HashSet<string> _tags = new HashSet<string>();

	private float sendTime;

	private static AnalyticsController _main;

	public static string playthroughId
	{
		get
		{
			if (_main != null)
			{
				if (string.IsNullOrEmpty(_main._playthroughId))
				{
					_main._playthroughId = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22);
				}
				return _main._playthroughId;
			}
			return string.Empty;
		}
	}

	public static AnalyticsController main => _main;

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void Awake()
	{
		if (_main != null)
		{
			Debug.LogErrorFormat("Multiple {0} in scene!", GetType().ToString());
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			_main = this;
		}
	}

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdateAfterInput, OnUpdate);
		sendTime = Time.realtimeSinceStartup + (float)GetSessionPeriod();
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(OnUpdate);
	}

	private void OnUpdate()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup > sendTime)
		{
			sendTime = realtimeSinceStartup + (float)GetSessionPeriod();
			SendPlaythroughUpdate();
		}
	}

	private void SendPlaythroughUpdate()
	{
		using (GameAnalytics.EventData eventData = GameAnalytics.CustomEvent(GameAnalytics.Event.PlaythroughUpdate))
		{
			PlatformServices services = PlatformUtils.main.GetServices();
			string value = services.GetName() ?? "Null";
			string value2 = services.GetUserId() ?? "Null";
			eventData.Add("product_id", 264710);
			eventData.Add("platform", value);
			eventData.Add("platform_user_id", value2);
			eventData.Add("playthrough_id", playthroughId);
			eventData.Add("session_id", Telemetry.Instance.SessionID);
			eventData.Add("used_cheats", DevConsole.HasUsedConsole());
			eventData.Add("total_game_length", Mathf.RoundToInt(SaveLoadManager.main.timePlayedTotal));
			GameModeUtils.GetGameMode(out var mode, out var _);
			eventData.Add("game_mode", (int)mode);
			eventData.Add("changeset", SNUtils.GetPlasticChangeSetOfBuild());
			eventData.Add("tags", _tags.GetEnumerator());
			CraftingAnalytics.main.AppendCraftingStatistics(eventData);
		}
	}

	public static void AddTag(string name)
	{
		if (_main != null)
		{
			_main._tags.Add(name);
		}
	}

	private int GetSessionPeriod()
	{
		int num = 0;
		Telemetry instance = Telemetry.Instance;
		if (instance != null)
		{
			num = instance.statisticsPeriod;
		}
		if (num <= 0)
		{
			num = 1200;
		}
		return Mathf.Clamp(num, 10, 31536000);
	}
}
