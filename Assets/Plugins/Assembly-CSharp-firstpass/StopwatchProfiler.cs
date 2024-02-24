using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Gendarme;
using UnityEngine;

public class StopwatchProfiler : MonoBehaviour
{
	public struct GarbageCollectionStats
	{
		public string recorderTag;

		public long bytesRecovered;

		public float timestamp;

		public float timeSpent;
	}

	public struct HitchStats
	{
		public string recorderTag;

		public float timestamp;

		public float timeSpent;

		public bool wasDuringGC;

		public int threadId;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct TimerBlock : IDisposable
	{
		public TimerBlock(string timerTag)
		{
		}

		public void Dispose()
		{
		}
	}

	public class HitchSummaryInfo
	{
		public int hitchCount;

		public float totalTime;

		public float avgTime;
	}

	private object lockProfiler = new object();

	private static StopwatchProfiler instance = null;

	private static List<GarbageCollectionStats> gcStats = new List<GarbageCollectionStats>();

	private static List<HitchStats> hitchStats = new List<HitchStats>();

	private static int lastRecordedGCCount = -1;

	private static Dictionary<string, Dictionary<string, string>> profilerTagsPerPrefix = new Dictionary<string, Dictionary<string, string>>();

	private static Dictionary<Type, string> typeNameCache = new Dictionary<Type, string>();

	private TimerCategory activeCategories = TimerCategory.Always | TimerCategory.Detailed;

	private StopwatchRecorder watchFrame;

	private StopwatchRecorder watchUpdate;

	private StopwatchRecorder watchLateUpdate;

	private StopwatchRecorder watchFixedUpdate;

	private readonly Dictionary<string, StopwatchRecorder> allRecorders = new Dictionary<string, StopwatchRecorder>();

	private TimerCategory recordingCategories;

	private bool wantsDataFile;

	private TextWriter fileWriter;

	private TextWriter summaryWriter;

	private float frameTimeStamp;

	private float recordingTimer;

	private float recordingTimeElapsed;

	private float reportFrequency = 1f;

	private readonly Dictionary<int, Stack<string>> activeTimerTagsPerThread = new Dictionary<int, Stack<string>>();

	private string summaryFilePath = string.Empty;

	private string hitchFilePath = string.Empty;

	private string gcFilePath = string.Empty;

	private string saveFilename = string.Empty;

	private int runNumber;

	private string profileId = "";

	private bool profilerActive;

	private string settingsReport = string.Empty;

	public static StopwatchProfiler Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindObjectOfType<StopwatchProfiler>();
				if (instance == null)
				{
					instance = new GameObject("StopwatchProfiler").AddComponent<StopwatchProfiler>();
				}
				instance.Init();
			}
			return instance;
		}
	}

	public bool IsRecording => recordingCategories != (TimerCategory)0;

	public bool ABTestingEnabled { get; set; }

	public ABTestVariant currentTestVariant { get; set; }

	public float FrameTimeStamp => frameTimeStamp;

	public static bool ShouldRecordGCCountData(int countIndex)
	{
		return countIndex != lastRecordedGCCount;
	}

	public static void MarkGCCountRecorded(int countIndex)
	{
		lastRecordedGCCount = countIndex;
	}

	public static void AddGCStat(GarbageCollectionStats stat)
	{
		gcStats.Add(stat);
	}

	public static void AddHitchStat(HitchStats stats)
	{
		hitchStats.Add(stats);
	}

	public static string GetCachedProfilerTag(string profilerPrefix, string typeName)
	{
		if (!Instance.IsRecording)
		{
			return profilerPrefix;
		}
		Dictionary<string, string> orAddNew = profilerTagsPerPrefix.GetOrAddNew(profilerPrefix);
		if (orAddNew.TryGetValue(typeName, out var value))
		{
			return value;
		}
		value = profilerPrefix + typeName;
		orAddNew.Add(typeName, value);
		return value;
	}

	public static string GetCachedProfilerTag(string profilerPrefix, Type type)
	{
		if (!Instance.IsRecording)
		{
			return profilerPrefix;
		}
		if (!typeNameCache.TryGetValue(type, out var value))
		{
			value = type.ToString();
			typeNameCache.Add(type, value);
		}
		return GetCachedProfilerTag(profilerPrefix, value);
	}

	private void Init()
	{
		base.gameObject.EnsureComponent<ProfileMarkerFirst>();
		base.gameObject.EnsureComponent<ProfileMarkerLast>();
		watchFrame = new StopwatchRecorder("TotalFrameTime", print_frame_rate: true);
		watchUpdate = new StopwatchRecorder("BehaviourUpdate");
		watchLateUpdate = new StopwatchRecorder("BehaviourLateUpdate");
		watchFixedUpdate = new StopwatchRecorder("BehaviourFixedUpdate");
		allRecorders.Add(watchFrame.watchID, watchFrame);
		allRecorders.Add(watchUpdate.watchID, watchUpdate);
		allRecorders.Add(watchLateUpdate.watchID, watchLateUpdate);
		allRecorders.Add(watchFixedUpdate.watchID, watchFixedUpdate);
	}

	private void Update()
	{
		if (IsRecording)
		{
			frameTimeStamp = Time.time;
			UpdateFrameTimer();
			recordingTimer += Time.deltaTime;
			recordingTimeElapsed += Time.deltaTime;
			if (recordingTimer > reportFrequency)
			{
				WriteAllReportsToFile();
				recordingTimer -= reportFrequency;
			}
		}
	}

	private void WriteAllReportsToFile()
	{
		if (!IsRecording)
		{
			return;
		}
		foreach (KeyValuePair<string, StopwatchRecorder> allRecorder in allRecorders)
		{
			WriteReportToFile(allRecorder.Value);
		}
		if (fileWriter != null)
		{
			fileWriter.Flush();
		}
	}

	private void UpdateFrameTimer()
	{
		if (IsRecording)
		{
			watchFrame.StopTimer();
			watchFrame.StartTimer();
		}
	}

	private void WriteReportToFile(StopwatchRecorder watch)
	{
		if (wantsDataFile)
		{
			string cSV = watch.GetCSV(recordingTimeElapsed, ABTestingEnabled);
			fileWriter.WriteLine(cSV);
		}
		watch.ResetFrameTimes();
	}

	public void NotifyFirstUpdate()
	{
		if (IsRecording)
		{
			watchUpdate.StartTimer();
		}
	}

	public void NotifyLastUpdate()
	{
		if (IsRecording)
		{
			watchUpdate.StopTimer();
		}
	}

	public void NotifyFirstLateUpdate()
	{
		if (IsRecording)
		{
			watchLateUpdate.StartTimer();
		}
	}

	public void NotifyLastLateUpdate()
	{
		if (IsRecording)
		{
			watchLateUpdate.StopTimer();
		}
	}

	public void NotifyFirstFixedUpdate()
	{
		if (IsRecording)
		{
			watchFixedUpdate.StartTimer();
		}
	}

	public void NotifyLastFixedUpdate()
	{
		if (IsRecording)
		{
			watchFixedUpdate.StopTimer();
		}
	}

	public void SetCategoryMinimal()
	{
		activeCategories = TimerCategory.Always;
	}

	public void SetCategoryDetailed()
	{
		activeCategories = TimerCategory.Always | TimerCategory.Detailed;
	}

	public bool IsMinimalOnly()
	{
		return activeCategories == TimerCategory.Always;
	}

	private StopwatchRecorder GetRecorderForTag(string timerTag)
	{
		if (allRecorders.TryGetValue(timerTag, out var value))
		{
			return value;
		}
		value = new StopwatchRecorder(timerTag);
		value.SetRunNumber(runNumber, ABTestingEnabled);
		allRecorders.Add(timerTag, value);
		return value;
	}

	private Stack<string> GetTimerTagStackForCurrentThread()
	{
		int managedThreadId = Thread.CurrentThread.ManagedThreadId;
		return activeTimerTagsPerThread.GetOrAddNew(managedThreadId);
	}

	[Conditional("USE_STOPWATCH_PROFILER")]
	public void StartTimer(string timerTag, TimerCategory category = TimerCategory.Detailed)
	{
		if ((category & recordingCategories) == 0)
		{
			return;
		}
		lock (lockProfiler)
		{
			GetTimerTagStackForCurrentThread().Push(timerTag);
			GetRecorderForTag(timerTag).StartTimer();
		}
	}

	[Conditional("USE_STOPWATCH_PROFILER")]
	public void StopTimer(TimerCategory category = TimerCategory.Detailed)
	{
		if ((category & recordingCategories) == 0)
		{
			return;
		}
		lock (lockProfiler)
		{
			GetTimerTagStackForCurrentThread();
		}
	}

	[Conditional("USE_STOPWATCH_PROFILER")]
	private void StopTimer(string timerTag)
	{
		GetRecorderForTag(timerTag).StopTimer();
	}

	public void NotifyHitchRecorded()
	{
		foreach (string item in GetTimerTagStackForCurrentThread())
		{
			GetRecorderForTag(item).BlockHitchRecording();
		}
	}

	public float GetMostRecentTimeFor(string timerTag)
	{
		return GetRecorderForTag(timerTag).GetLastDeltaTime();
	}

	public string GetOutputFilePath(string file_path, string profile_id, string filename_suffix)
	{
		string text = profile_id;
		text = text + "_" + filename_suffix;
		string text2 = DateTime.Now.ToString("MMddyy_HHmm");
		text = text + "_" + text2;
		text += ".csv";
		return Path.Combine(file_path, text);
	}

	public void StartRecording(string file_path = "", string profile_id = "Stopwatch", float delayTime = 0f, string savefile = "")
	{
		if (IsRecording)
		{
			UnityEngine.Debug.LogWarning("StopwatchProfiler.StartRecording called even though already recording?");
			return;
		}
		profilerActive = true;
		saveFilename = savefile;
		bool flag = false;
		if (profile_id != profileId)
		{
			if (!string.IsNullOrEmpty(profileId))
			{
				StopRecordingAndCloseSession();
			}
			flag = true;
			profileId = profile_id;
		}
		else
		{
			recordingTimer = 0f;
			recordingTimeElapsed = 0f;
		}
		runNumber++;
		foreach (KeyValuePair<string, StopwatchRecorder> allRecorder in allRecorders)
		{
			allRecorder.Value.SetRunNumber(runNumber, ABTestingEnabled);
		}
		if (ABTestingEnabled)
		{
			currentTestVariant = ((runNumber % 2 == 0) ? ABTestVariant.B : ABTestVariant.A);
		}
		if (flag)
		{
			summaryFilePath = GetOutputFilePath(file_path, profile_id, "Summary");
			hitchFilePath = GetOutputFilePath(file_path, profile_id, "Hitches");
			gcFilePath = GetOutputFilePath(file_path, profile_id, "GC");
			summaryWriter = FileUtils.CreateTextFile(summaryFilePath);
			summaryWriter.WriteLine(StopwatchRecorder.CSVSummaryHeader);
			if (wantsDataFile)
			{
				string outputFilePath = GetOutputFilePath(file_path, profile_id, "Data");
				fileWriter = FileUtils.CreateTextFile(outputFilePath);
				fileWriter.WriteLine(StopwatchRecorder.CSVDataHeader);
			}
		}
		StartCoroutine(StartRecordingAfterDelay(delayTime));
	}

	public void SetSettingsReportString(string report)
	{
		settingsReport = report;
	}

	private IEnumerator StartRecordingAfterDelay(float delayTime)
	{
		recordingCategories = activeCategories;
		yield return new WaitForSeconds(delayTime);
		Dictionary<string, StopwatchRecorder>.Enumerator enumerator = allRecorders.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.ResetAll();
		}
	}

	public void FlipTestVariant()
	{
		currentTestVariant = ((currentTestVariant == ABTestVariant.A) ? ABTestVariant.B : ABTestVariant.A);
	}

	public void StopRecording()
	{
		if (IsRecording)
		{
			WriteAllReportsToFile();
			WriteSummaryReportToFile();
			recordingCategories = (TimerCategory)0;
			recordingTimeElapsed = 0f;
			activeTimerTagsPerThread.Clear();
			Dictionary<string, StopwatchRecorder>.Enumerator enumerator = allRecorders.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Value.ResetAll();
			}
		}
	}

	private void CloseOpenFiles()
	{
		runNumber = 0;
		if (summaryWriter != null)
		{
			summaryWriter.Close();
			summaryWriter = null;
		}
		if (fileWriter != null)
		{
			fileWriter.Close();
			fileWriter = null;
		}
	}

	private void OnDisable()
	{
		StopRecordingAndCloseSession();
	}

	public void StopRecordingAndCloseSession()
	{
		StopRecording();
		WriteMetaSummaryToFile();
		WriteGCStatsToFile();
		WriteHitchStatsToFile();
		CloseOpenFiles();
		profilerActive = false;
		profileId = string.Empty;
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private void WriteHitchStatsToFile()
	{
		if (!profilerActive)
		{
			return;
		}
		hitchStats.Sort((HitchStats s1, HitchStats s2) => s2.timeSpent.CompareTo(s1.timeSpent));
		using (StreamWriter streamWriter = FileUtils.CreateTextFile(hitchFilePath))
		{
			int num = 0;
			Dictionary<string, HitchSummaryInfo> dictionary = new Dictionary<string, HitchSummaryInfo>();
			streamWriter.WriteLine("TimerTag,Timestamp,TimeSpent(msec),ThreadID,GC?");
			if (hitchStats.Count > 0)
			{
				foreach (HitchStats hitchStat in hitchStats)
				{
					streamWriter.WriteLine("{0},{1},{2},{3},{4}", hitchStat.recorderTag, hitchStat.timestamp, hitchStat.timeSpent, hitchStat.threadId, hitchStat.wasDuringGC ? "1" : "0");
					if (!hitchStat.wasDuringGC && hitchStat.threadId == ProfilingUtils.GetMainThreadId())
					{
						if (!dictionary.TryGetValue(hitchStat.recorderTag, out var value))
						{
							value = new HitchSummaryInfo();
							dictionary.Add(hitchStat.recorderTag, value);
						}
						value.hitchCount++;
						value.totalTime += hitchStat.timeSpent;
						value.avgTime = value.totalTime / (float)value.hitchCount;
						num++;
					}
				}
			}
			streamWriter.WriteLine();
			streamWriter.WriteLine("HitchSummary");
			streamWriter.WriteLine("[main thread non-GC only]");
			streamWriter.WriteLine("TimerTag,HitchCount,TotalTime,AvgTime");
			foreach (KeyValuePair<string, HitchSummaryInfo> item in dictionary.OrderByDescending((KeyValuePair<string, HitchSummaryInfo> x) => x.Value.hitchCount))
			{
				streamWriter.WriteLine("{0},{1},{2},{3}", item.Key, item.Value.hitchCount, item.Value.totalTime, item.Value.avgTime);
			}
			streamWriter.WriteLine();
			streamWriter.WriteLine("TotalHitchCount,{0}", num);
		}
		hitchStats.Clear();
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private void WriteGCStatsToFile()
	{
		if (!profilerActive)
		{
			return;
		}
		using (StreamWriter streamWriter = FileUtils.CreateTextFile(gcFilePath))
		{
			streamWriter.WriteLine("GC #,Timer Tag,Timestamp,Time Spent (msec),Mem Impact (kb)");
			if (gcStats.Count > 0)
			{
				StopwatchRecorder value = null;
				allRecorders.TryGetValue("TotalFrameTime", out value);
				long num = 0L;
				if (value != null)
				{
					num = value.GetTotalBytesAllocated();
				}
				float num2 = 0f;
				long num3 = 0L;
				int num4 = 0;
				foreach (GarbageCollectionStats gcStat in gcStats)
				{
					num4++;
					streamWriter.WriteLine("{0},{1},{2},{3},{4}", num4, gcStat.recorderTag, gcStat.timestamp, gcStat.timeSpent, (double)gcStat.bytesRecovered / 1024.0);
					num2 += gcStat.timeSpent;
					num3 += gcStat.bytesRecovered;
				}
				float num5 = num2 / (float)num4;
				long num6 = num3 / num4;
				streamWriter.WriteLine("Summary,TotalGarbageAllocated,,,{0}", (double)num / 1024.0);
				streamWriter.WriteLine("Summary,TotalGarbageCollected,,{0},{1}", num2, (double)num3 / 1024.0);
				streamWriter.WriteLine("Summary,AvgGarbageCollected,,{0},{1}", num5, (double)num6 / 1024.0);
				if (gcStats.Count > 1)
				{
					List<GarbageCollectionStats> list = new List<GarbageCollectionStats>();
					foreach (GarbageCollectionStats gcStat2 in gcStats)
					{
						if (gcStat2.timestamp > 0f)
						{
							list.Add(gcStat2);
						}
					}
					float num7 = 0f;
					for (int i = 1; i < list.Count; i++)
					{
						GarbageCollectionStats garbageCollectionStats = list[i];
						GarbageCollectionStats garbageCollectionStats2 = list[i - 1];
						float num8 = garbageCollectionStats.timestamp - garbageCollectionStats2.timestamp;
						num7 += num8;
					}
					float num9 = num7 / (float)(list.Count - 1);
					streamWriter.WriteLine("Summary,AvgTimeBetweenGCs,,{0}", num9);
				}
			}
			else
			{
				streamWriter.WriteLine("No GCs");
			}
		}
		gcStats.Clear();
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private void WriteMetaSummaryToFile()
	{
		if (!profilerActive)
		{
			return;
		}
		summaryWriter.WriteLine();
		if (ABTestingEnabled)
		{
			summaryWriter.WriteLine("Test A");
			Dictionary<string, StopwatchRecorder>.Enumerator enumerator = allRecorders.GetEnumerator();
			while (enumerator.MoveNext())
			{
				StopwatchRecorder value = enumerator.Current.Value;
				summaryWriter.WriteLine(value.GetMetaSummaryCSV(ABTestVariant.A));
			}
			summaryWriter.WriteLine();
			summaryWriter.WriteLine("Test B");
			Dictionary<string, StopwatchRecorder>.Enumerator enumerator2 = allRecorders.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				StopwatchRecorder value2 = enumerator2.Current.Value;
				summaryWriter.WriteLine(value2.GetMetaSummaryCSV(ABTestVariant.B));
			}
		}
		else
		{
			Dictionary<string, StopwatchRecorder>.Enumerator enumerator3 = allRecorders.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				StopwatchRecorder value3 = enumerator3.Current.Value;
				summaryWriter.WriteLine(value3.GetMetaSummaryCSV(ABTestVariant.NOT_TESTING));
			}
		}
	}

	private void WriteSummaryReportToFile()
	{
		Dictionary<string, StopwatchRecorder>.Enumerator enumerator = allRecorders.GetEnumerator();
		while (enumerator.MoveNext())
		{
			StopwatchRecorder value = enumerator.Current.Value;
			value.RecordSummaryData();
			summaryWriter.WriteLine(value.GetSummaryCSV());
		}
		summaryWriter.Flush();
	}
}
