using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sentry;
using UnityEngine;

public class SentrySdkManager : MonoBehaviour
{
	private struct LogInfo
	{
		public DateTime timestamp;

		public string message;

		public int frameNumber;

		public bool IsValid => message != null;

		public override string ToString()
		{
			return $"[{frameNumber}] {timestamp} {message}";
		}
	}

	[SerializeField]
	[AssertNotNull]
	private SentrySdk sentrySdk;

	[SerializeField]
	private bool sendLogHistoryAsBreadcrumbs = true;

	[SerializeField]
	private int maxLogsHistory = 10;

	[SerializeField]
	private float tooOldLogInSeconds = 30f;

	[SerializeField]
	private bool turnEverythingOffIfTooManyErrors = true;

	[SerializeField]
	private int tooManyErrorCount = 300;

	private const int minMaxLogsHistory = 2;

	private const int skipTopMessages = 1;

	private int currentErrorsCount;

	private LogInfo[] logHistory;

	private int currentLogHistoryPosition;

	private void Awake()
	{
		if (maxLogsHistory < 2)
		{
			maxLogsHistory = 2;
		}
		logHistory = new LogInfo[maxLogsHistory];
		sentrySdk.PostProcessEventHandler += PostProcessEvent;
	}

	private void OnDestroy()
	{
		sentrySdk.PostProcessEventHandler -= PostProcessEvent;
	}

	public void OnEnable()
	{
		Application.logMessageReceived += OnLogMessageReceived;
	}

	public void OnDisable()
	{
		Application.logMessageReceived -= OnLogMessageReceived;
	}

	private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
	{
		if (sendLogHistoryAsBreadcrumbs)
		{
			AddLogHistory(condition);
		}
		_ = turnEverythingOffIfTooManyErrors;
	}

	[Conditional("RELEASE")]
	private void CheckForTooManyErrors(LogType type)
	{
		if (type == LogType.Error || type == LogType.Exception)
		{
			currentErrorsCount++;
		}
		if (currentErrorsCount == tooManyErrorCount)
		{
			Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.None);
			sentrySdk.RetrieveCallstackForError = false;
			sentrySdk.enabled = false;
			base.enabled = false;
			UnityEngine.Debug.Log("Too many errors. Stack trace is turned off for error and exception messages. SentrySdk is shut down.");
		}
	}

	private void AddLogHistory(string message)
	{
		try
		{
			LogInfo logInfo = default(LogInfo);
			logInfo.timestamp = DateTime.UtcNow;
			logInfo.message = message;
			logInfo.frameNumber = Time.frameCount;
			LogInfo logInfo2 = logInfo;
			logHistory[currentLogHistoryPosition] = logInfo2;
			currentLogHistoryPosition = (currentLogHistoryPosition + 1) % maxLogsHistory;
		}
		catch (Exception exception)
		{
			UnityEngine.Object.DestroyImmediate(this);
			UnityEngine.Debug.LogException(exception);
		}
	}

	private void PostProcessEvent(SentryEvent @event)
	{
		try
		{
			AddBreadcrumbs(@event);
			@event.extra.frameNumber = Time.frameCount;
			@event.user.id = SystemInfo.deviceUniqueIdentifier;
			if (Player.main != null)
			{
				@event.extra.playerPosition = Player.main.transform.position;
			}
			@event.extra.isGameLoading = WaitScreen.IsWaiting;
			string playthroughId = AnalyticsController.playthroughId;
			if (!string.IsNullOrEmpty(playthroughId))
			{
				@event.tags.playthroughId = playthroughId;
			}
			if (Telemetry.Instance != null && Telemetry.Instance.IsAnalyzingSession())
			{
				@event.tags.sessionId = Telemetry.Instance.SessionID;
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Object.DestroyImmediate(this);
			UnityEngine.Debug.LogException(exception);
		}
	}

	private void AddBreadcrumbs(SentryEvent @event)
	{
		if (!sendLogHistoryAsBreadcrumbs)
		{
			return;
		}
		if (@event.breadcrumbs == null)
		{
			@event.breadcrumbs = new List<Breadcrumb>();
		}
		int num = 0;
		int count = @event.breadcrumbs.Count;
		DateTime utcNow = DateTime.UtcNow;
		int num2 = (maxLogsHistory + (currentLogHistoryPosition - 1 - 1)) % maxLogsHistory;
		for (int i = 0; i < maxLogsHistory - 1; i++)
		{
			int num3 = (maxLogsHistory + (num2 - i)) % maxLogsHistory;
			LogInfo logInfo = logHistory[num3];
			if (!logInfo.IsValid || !((utcNow - logInfo.timestamp).TotalSeconds <= (double)tooOldLogInSeconds))
			{
				break;
			}
			Breadcrumb item = new Breadcrumb(logInfo.timestamp, $"[{logInfo.frameNumber}] {logInfo.message}");
			@event.breadcrumbs.Add(item);
			num++;
		}
		if (num > 0)
		{
			@event.breadcrumbs.Reverse(count, num);
		}
	}
}
