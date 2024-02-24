using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using LitJson;
using UWE;
using UnityEngine;
using UnityEngine.Networking;

public class GameAnalytics
{
	public enum Event
	{
		None = 0,
		NewGameStarted = 1,
		GameLoaded = 2,
		GameSaved = 3,
		FeedbackSent = 4,
		MusicPlayed = 5,
		TechConstructed = 6,
		TechCrafted = 7,
		Death = 8,
		BlueprintUnlocked = 9,
		FirstUnlockedCreate = 10,
		CraftingStats = 11,
		Goal = 12,
		GameQuit = 13,
		PlaythroughUpdate = 14,
		LoadingDurations = 15,
		LegacyFeedback = 16,
		LegacyMusic = 17,
		LegacyConstruct = 18,
		LegacyGoal = 19
	}

	public struct EventInfo
	{
		public string name;

		public TelemetryEventCategory category;

		public EventFlag flags;

		public string endpoint;

		public bool queued;

		public bool HasFlag(EventFlag check)
		{
			return (flags & check) != 0;
		}
	}

	[Flags]
	public enum EventFlag
	{
		None = 0,
		Disabled = 1,
		EditorDisableOnCheat = 2,
		BuildDisableOnCheat = 4,
		Playthrough = 8,
		SingleInstance = 0x10,
		Raw = 0x20,
		DisableOnCheat = 6
	}

	[Serializable]
	public class EventInfoCollection : Dictionary<Event, EventInfo>
	{
		public void Add(Event eventId, string name, TelemetryEventCategory category, EventFlag flags = EventFlag.None, string endpoint = null, bool queued = true)
		{
			Add(eventId, new EventInfo
			{
				name = name,
				category = category,
				flags = flags,
				endpoint = endpoint,
				queued = queued
			});
		}

		public EventInfoCollection()
		{
		}

		protected EventInfoCollection(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	public class EventData : IDisposable
	{
		private static readonly EventData singleton = new EventData();

		private Event eventId;

		private StringBuilder sb;

		private JsonWriter writer;

		public bool synchronous;

		private EventData()
		{
			sb = new StringBuilder(255);
			writer = new JsonWriter(sb);
		}

		public static EventData Initialize(Event eventId)
		{
			singleton.Reset();
			singleton.eventId = eventId;
			singleton.writer.WriteObjectStart();
			return singleton;
		}

		public void Add(string name, bool value)
		{
			writer.WritePropertyName(name);
			writer.Write(value);
		}

		public void Add(string name, decimal value)
		{
			writer.WritePropertyName(name);
			writer.Write(value);
		}

		public void Add(string name, double value)
		{
			writer.WritePropertyName(name);
			writer.Write(value);
		}

		public void Add(string name, int value)
		{
			writer.WritePropertyName(name);
			writer.Write(value);
		}

		public void Add(string name, long value)
		{
			writer.WritePropertyName(name);
			writer.Write(value);
		}

		public void Add(string name, string value)
		{
			writer.WritePropertyName(name);
			writer.Write(value);
		}

		public void Add(string name, ulong value)
		{
			writer.WritePropertyName(name);
			writer.Write(value);
		}

		public void AddPosition(Vector3 position)
		{
			Add("x", position.x);
			Add("y", position.y);
			Add("z", position.z);
		}

		public void AddVector(string name, Vector3 position)
		{
			writer.WritePropertyName(name);
			writer.WriteObjectStart();
			AddPosition(position);
			writer.WriteObjectEnd();
		}

		public void Add(string name, IEnumerator<int> e)
		{
			writer.WritePropertyName(name);
			writer.WriteArrayStart();
			while (e.MoveNext())
			{
				writer.Write(e.Current);
			}
			writer.WriteArrayEnd();
		}

		public void Add(string name, IEnumerator<float> e)
		{
			writer.WritePropertyName(name);
			writer.WriteArrayStart();
			while (e.MoveNext())
			{
				writer.Write(e.Current);
			}
			writer.WriteArrayEnd();
		}

		public void Add(string name, IEnumerator<string> e)
		{
			writer.WritePropertyName(name);
			writer.WriteArrayStart();
			while (e.MoveNext())
			{
				writer.Write(e.Current);
			}
			writer.WriteArrayEnd();
		}

		public void WriteObjectStart(string name)
		{
			writer.WritePropertyName(name);
			writer.WriteObjectStart();
		}

		public void WriteObjectEnd()
		{
			writer.WriteObjectEnd();
		}

		private void Reset()
		{
			synchronous = false;
			eventId = Event.None;
			writer.Reset();
			sb.Length = 0;
		}

		public void Dispose()
		{
			try
			{
				writer.WriteObjectEnd();
				Send(eventId, synchronous, sb.ToString());
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			finally
			{
				Reset();
			}
		}
	}

	private static readonly EventInfoCollection definitions = new EventInfoCollection
	{
		{
			Event.NewGameStarted,
			"NewGameStarted",
			TelemetryEventCategory.Sessions
		},
		{
			Event.GameLoaded,
			"GameLoaded",
			TelemetryEventCategory.Sessions
		},
		{
			Event.GameSaved,
			"GameSaved",
			TelemetryEventCategory.Sessions
		},
		{
			Event.FeedbackSent,
			"FeedbackSent",
			TelemetryEventCategory.Other
		},
		{
			Event.MusicPlayed,
			"MusicPlayed",
			TelemetryEventCategory.Music,
			EventFlag.DisableOnCheat
		},
		{
			Event.TechConstructed,
			"TechConstructed",
			TelemetryEventCategory.Tech,
			EventFlag.DisableOnCheat
		},
		{
			Event.TechCrafted,
			"TechCrafted",
			TelemetryEventCategory.Tech,
			EventFlag.DisableOnCheat
		},
		{
			Event.Death,
			"Death",
			TelemetryEventCategory.Other,
			EventFlag.DisableOnCheat
		},
		{
			Event.BlueprintUnlocked,
			"BlueprintUnlocked",
			TelemetryEventCategory.Tech,
			EventFlag.DisableOnCheat
		},
		{
			Event.FirstUnlockedCreate,
			"FirstUnlockedCreate",
			TelemetryEventCategory.Tech,
			EventFlag.DisableOnCheat
		},
		{
			Event.Goal,
			"Goal",
			TelemetryEventCategory.Story,
			EventFlag.DisableOnCheat | EventFlag.Playthrough
		},
		{
			Event.GameQuit,
			"GameQuit",
			TelemetryEventCategory.Sessions,
			EventFlag.DisableOnCheat
		},
		{
			Event.PlaythroughUpdate,
			string.Empty,
			TelemetryEventCategory.Other,
			EventFlag.Raw,
			string.Format("{0}/playthrough-update", "https://analytics.unknownworlds.com/api")
		},
		{
			Event.LoadingDurations,
			"LoadingDurations",
			TelemetryEventCategory.Sessions,
			EventFlag.SingleInstance
		},
		{
			Event.CraftingStats,
			"CraftingStats",
			TelemetryEventCategory.Tech,
			EventFlag.DisableOnCheat | EventFlag.Disabled | EventFlag.Playthrough | EventFlag.SingleInstance
		},
		{
			Event.LegacyFeedback,
			"Feedback_{0}",
			TelemetryEventCategory.Other,
			EventFlag.DisableOnCheat | EventFlag.Disabled
		},
		{
			Event.LegacyMusic,
			"MusicPlayed_{0}",
			TelemetryEventCategory.Music,
			EventFlag.DisableOnCheat | EventFlag.Disabled
		},
		{
			Event.LegacyConstruct,
			"Tech_{0}_3_Constructed",
			TelemetryEventCategory.Tech,
			EventFlag.DisableOnCheat | EventFlag.Disabled
		},
		{
			Event.LegacyGoal,
			"StoryGoal_{0}",
			TelemetryEventCategory.Story,
			EventFlag.DisableOnCheat | EventFlag.Disabled
		}
	};

	public static int timeNow
	{
		get
		{
			DayNightCycle main = DayNightCycle.main;
			if (!main)
			{
				return 0;
			}
			return (int)System.Math.Round(main.timePassedSinceOrigin);
		}
	}

	public static EventData CustomEvent(Event eventId)
	{
		return EventData.Initialize(eventId);
	}

	public static void SimpleEvent(Event eventId)
	{
		Send(eventId);
	}

	private static void Send(Event eventId, bool synchronous = false, string data = null)
	{
		if (definitions.TryGetValue(eventId, out var value))
		{
			Send(value, synchronous, data);
			return;
		}
		Debug.LogErrorFormat("No event definition found for analytics event '{0}' - it should be defined in GameAnalytics.definitions!", eventId);
	}

	private static void Send(EventInfo eventInfo, bool synchronous, string data)
	{
		bool num = eventInfo.HasFlag(EventFlag.Disabled);
		bool flag = eventInfo.HasFlag(EventFlag.BuildDisableOnCheat);
		bool flag2 = Player.main != null && Player.main.hasUsedConsole;
		bool num2 = num || (flag && flag2);
		if (data == null)
		{
			data = "null";
		}
		if (num2)
		{
			return;
		}
		if (eventInfo.HasFlag(EventFlag.Raw))
		{
			if (!string.IsNullOrEmpty(eventInfo.endpoint))
			{
				CoroutineHost.StartCoroutine(SendCoroutine(eventInfo, data));
			}
			else
			{
				Debug.LogError("Event marked with EventFlag.Raw, but endpoint is not set!");
			}
			return;
		}
		try
		{
			Telemetry.Instance.SendAnalyticsEvent(eventInfo.category, eventInfo.name, data, synchronous, eventInfo.HasFlag(EventFlag.Playthrough), eventInfo.HasFlag(EventFlag.SingleInstance), eventInfo.queued);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private static IEnumerator SendCoroutine(EventInfo eventInfo, string data)
	{
		PlatformServices services = PlatformUtils.main.GetServices();
		yield return services.TryEnsureServerAccessAsync();
		if (services.CanAccessServers())
		{
			UnityWebRequest unityWebRequest = UnityWebRequest.Post(eventInfo.endpoint, data);
			yield return unityWebRequest.SendWebRequest();
		}
	}

	public static void LegacyEvent(Event eventId, string data)
	{
		if (definitions.TryGetValue(eventId, out var value))
		{
			value.name = string.Format(value.name, data);
			Send(value, synchronous: false, "null");
		}
		else
		{
			Debug.LogErrorFormat("No event definition found for analytics event '{0}' - it should be defined in GameAnalytics.definitions!", eventId);
		}
	}
}
