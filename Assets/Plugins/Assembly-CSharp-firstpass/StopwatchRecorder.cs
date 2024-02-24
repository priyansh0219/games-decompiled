using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UWE;
using UnityEngine;

public class StopwatchRecorder
{
	public class StopwatchMetaRecord
	{
		public List<float> totalFrameTimes = new List<float>();

		public List<float> totalFrameCounts = new List<float>();

		public List<float> avgFrameTimes = new List<float>();

		public List<float> avgFrameRatesOrMaxFrameTimes = new List<float>();

		public List<long> totalBytesAllocated = new List<long>();

		public float avgTotalFrameTime;

		public float avgTotalFrameCount;

		public float avgAvgFrameTime;

		public float avgAvgFrameRateOrMaxFrameTime;

		public float avgTotalBytesAllocated;

		public bool printFrameRate;

		private float GetAverage(List<float> listToAverage)
		{
			int count = listToAverage.Count;
			if (count == 0)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				num += listToAverage[i];
			}
			return num / (float)count;
		}

		private float GetMax(List<float> listToMax)
		{
			int count = listToMax.Count;
			if (count == 0)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				num = Mathf.Max(num, listToMax[i]);
			}
			return num;
		}

		public long GetTotalBytesAllocated()
		{
			long num = 0L;
			foreach (long item in totalBytesAllocated)
			{
				num += item;
			}
			return num;
		}

		public void CalculateAverages()
		{
			avgTotalFrameTime = GetAverage(totalFrameTimes);
			avgTotalFrameCount = GetAverage(totalFrameCounts);
			avgAvgFrameTime = GetAverage(avgFrameTimes);
			if (printFrameRate)
			{
				avgAvgFrameRateOrMaxFrameTime = GetAverage(avgFrameRatesOrMaxFrameTimes);
			}
			else
			{
				avgAvgFrameRateOrMaxFrameTime = GetMax(avgFrameRatesOrMaxFrameTimes);
			}
			float num = 0f;
			int count = totalBytesAllocated.Count;
			if (count == 0)
			{
				avgTotalBytesAllocated = 0f;
				return;
			}
			for (int i = 0; i < count; i++)
			{
				num += (float)totalBytesAllocated[i];
			}
			avgTotalBytesAllocated = Utils.SafeDiv(num, count);
		}
	}

	private Stopwatch watch;

	private List<float> frameTimes;

	private List<long> bytesAllocated;

	private List<int> gcCounts;

	private long gcMemorySnapshot;

	private int gcCountSnapshot;

	private float maxFrameTime;

	private long recorderTotalBytes;

	private int recorderTotalGCCount;

	private float recorderTotalTime;

	private int recorderTotalCount;

	public int runNumber;

	private ABTestVariant testVariant;

	private StopwatchMetaRecord metaRecordA = new StopwatchMetaRecord();

	private StopwatchMetaRecord metaRecordB = new StopwatchMetaRecord();

	private bool printFrameRate;

	private const float hitchThreshold = 10f;

	private bool hitchRecordingBlocked;

	public string watchID { get; private set; }

	public static string CSVDataHeader => "Run,Timer,TimeElapsed,NumTicks,Avg,Min,Max,GCAlloc(KB),GCCount";

	public static string CSVSummaryHeader => "Run,Timer,TotalTime,TotalTicks,AvgFrameTime,AvgFPS/MaxTime,GCAlloc(KB),GCCount";

	public StopwatchRecorder(string watch_id, bool print_frame_rate = false)
	{
		watchID = watch_id;
		frameTimes = new List<float>();
		bytesAllocated = new List<long>();
		gcCounts = new List<int>();
		watch = new Stopwatch();
		maxFrameTime = 0f;
		recorderTotalTime = 0f;
		recorderTotalCount = 0;
		recorderTotalBytes = 0L;
		recorderTotalGCCount = 0;
		printFrameRate = print_frame_rate;
		runNumber = 0;
		watch.Reset();
	}

	public long GetTotalBytesAllocated()
	{
		return metaRecordA.GetTotalBytesAllocated() + metaRecordB.GetTotalBytesAllocated();
	}

	public void StartTimer()
	{
		watch.Restart();
		gcMemorySnapshot = GC.GetTotalMemory(forceFullCollection: false);
		gcCountSnapshot = GC.CollectionCount(0);
	}

	public void BlockHitchRecording()
	{
		hitchRecordingBlocked = true;
	}

	public void StopTimer()
	{
		if (watch.IsRunning)
		{
			long num = GC.GetTotalMemory(forceFullCollection: false) - gcMemorySnapshot;
			int num2 = GC.CollectionCount(0) - gcCountSnapshot;
			bytesAllocated.Add((num > 0) ? num : 0);
			float timeElapsedMS = Utils.GetTimeElapsedMS(watch);
			if (!printFrameRate && timeElapsedMS > 10f && !hitchRecordingBlocked)
			{
				StopwatchProfiler.HitchStats stats = default(StopwatchProfiler.HitchStats);
				stats.timestamp = StopwatchProfiler.Instance.FrameTimeStamp;
				stats.timeSpent = timeElapsedMS;
				stats.recorderTag = watchID;
				stats.wasDuringGC = num2 > 0;
				stats.threadId = Thread.CurrentThread.ManagedThreadId;
				StopwatchProfiler.AddHitchStat(stats);
				StopwatchProfiler.Instance.NotifyHitchRecorded();
			}
			hitchRecordingBlocked = false;
			if (num2 > 0 && StopwatchProfiler.ShouldRecordGCCountData(gcCountSnapshot))
			{
				StopwatchProfiler.MarkGCCountRecorded(gcCountSnapshot);
				StopwatchProfiler.GarbageCollectionStats stat = default(StopwatchProfiler.GarbageCollectionStats);
				stat.timestamp = StopwatchProfiler.Instance.FrameTimeStamp;
				stat.timeSpent = timeElapsedMS;
				stat.bytesRecovered = num;
				stat.recorderTag = watchID;
				StopwatchProfiler.AddGCStat(stat);
				frameTimes.Add(0f);
				gcCounts.Add(num2);
			}
			else
			{
				frameTimes.Add(timeElapsedMS);
				gcCounts.Add(0);
			}
			watch.Stop();
		}
	}

	public void SetRunNumber(int run_number, bool ab_testing_enabled)
	{
		runNumber = run_number;
		recorderTotalTime = 0f;
		recorderTotalCount = 0;
		recorderTotalBytes = 0L;
		recorderTotalGCCount = 0;
		maxFrameTime = 0f;
		if (ab_testing_enabled)
		{
			testVariant = ((runNumber % 2 == 0) ? ABTestVariant.B : ABTestVariant.A);
		}
	}

	public float GetLastDeltaTime()
	{
		if (frameTimes.Count == 0)
		{
			return 0f;
		}
		return frameTimes[frameTimes.Count - 1];
	}

	public string GetCSV(float timeElapsed, bool ab_testing_enabled)
	{
		float num = 0f;
		float num2 = float.MaxValue;
		float num3 = 0f;
		int count = frameTimes.Count;
		long num4 = 0L;
		int num5 = 0;
		for (int i = 0; i < count; i++)
		{
			num += frameTimes[i];
			num2 = Mathf.Min(frameTimes[i], num2);
			num3 = Mathf.Max(frameTimes[i], num3);
			num4 += bytesAllocated[i];
			num5 += gcCounts[i];
		}
		float num6 = Utils.SafeDiv(num, count);
		string text = watchID;
		if (ab_testing_enabled)
		{
			text = ((testVariant != 0) ? (text + "_B") : (text + "_A"));
		}
		return $"{runNumber},{text},{timeElapsed},{count},{num6},{num2},{num3},{(float)num4 / 1024f},{num5}";
	}

	public void RecordSummaryData()
	{
		float num = Utils.SafeDiv(recorderTotalTime, recorderTotalCount);
		float item = Utils.SafeDiv(1000f, num);
		if (testVariant == ABTestVariant.A)
		{
			metaRecordA.totalFrameTimes.Add(recorderTotalTime);
			metaRecordA.totalFrameCounts.Add(recorderTotalCount);
			metaRecordA.totalBytesAllocated.Add(recorderTotalBytes);
			metaRecordA.avgFrameTimes.Add(num);
			if (printFrameRate)
			{
				metaRecordA.avgFrameRatesOrMaxFrameTimes.Add(item);
			}
			else
			{
				metaRecordA.avgFrameRatesOrMaxFrameTimes.Add(maxFrameTime);
			}
			metaRecordA.printFrameRate = printFrameRate;
			metaRecordA.CalculateAverages();
		}
		else
		{
			metaRecordB.totalFrameTimes.Add(recorderTotalTime);
			metaRecordB.totalFrameCounts.Add(recorderTotalCount);
			metaRecordB.totalBytesAllocated.Add(recorderTotalBytes);
			metaRecordB.avgFrameTimes.Add(num);
			if (printFrameRate)
			{
				metaRecordB.avgFrameRatesOrMaxFrameTimes.Add(item);
			}
			else
			{
				metaRecordB.avgFrameRatesOrMaxFrameTimes.Add(maxFrameTime);
			}
			metaRecordB.printFrameRate = printFrameRate;
			metaRecordB.CalculateAverages();
		}
	}

	public string GetMetaSummaryCSV(ABTestVariant variant)
	{
		switch (variant)
		{
		case ABTestVariant.NOT_TESTING:
			return $"Average,{watchID},{metaRecordA.avgTotalFrameTime},{metaRecordA.avgTotalFrameCount},{metaRecordA.avgAvgFrameTime},{metaRecordA.avgAvgFrameRateOrMaxFrameTime},{metaRecordA.avgTotalBytesAllocated / 1024f}";
		case ABTestVariant.A:
			return $"Test A,{watchID},{metaRecordA.avgTotalFrameTime},{metaRecordA.avgTotalFrameCount},{metaRecordA.avgAvgFrameTime},{metaRecordA.avgAvgFrameRateOrMaxFrameTime},{metaRecordA.avgTotalBytesAllocated / 1024f}";
		default:
			return $"Test B,{watchID},{metaRecordB.avgTotalFrameTime},{metaRecordB.avgTotalFrameCount},{metaRecordB.avgAvgFrameTime},{metaRecordB.avgAvgFrameRateOrMaxFrameTime},{metaRecordB.avgTotalBytesAllocated / 1024f}";
		}
	}

	public string GetSummaryCSV()
	{
		float num = Utils.SafeDiv(recorderTotalTime, recorderTotalCount);
		float num2 = Utils.SafeDiv(1000f, num);
		float num3 = maxFrameTime;
		if (printFrameRate)
		{
			num3 = num2;
		}
		return $"{runNumber},{watchID},{recorderTotalTime},{recorderTotalCount},{num},{num3},{(float)recorderTotalBytes / 1024f},{recorderTotalGCCount}";
	}

	public void ResetFrameTimes()
	{
		int count = frameTimes.Count;
		recorderTotalCount += count;
		for (int i = 0; i < count; i++)
		{
			recorderTotalBytes += bytesAllocated[i];
			recorderTotalGCCount += gcCounts[i];
			recorderTotalTime += frameTimes[i];
			if (frameTimes[i] > maxFrameTime)
			{
				maxFrameTime = frameTimes[i];
			}
		}
		frameTimes.Clear();
		bytesAllocated.Clear();
		gcCounts.Clear();
	}

	public void ResetAll()
	{
		watch.Stop();
		recorderTotalTime = 0f;
		recorderTotalCount = 0;
		recorderTotalBytes = 0L;
		frameTimes.Clear();
		bytesAllocated.Clear();
		gcCounts.Clear();
	}
}
