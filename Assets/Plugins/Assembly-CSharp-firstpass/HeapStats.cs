using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class HeapStats
{
	public struct CategoryData
	{
		public int numCells;

		public float avgProcessTime;

		public float medianProcessTime;

		public float worstProcessTime;
	}

	private static HeapStats _main;

	private readonly Dictionary<string, List<CellProcessingStats>> entries = new Dictionary<string, List<CellProcessingStats>>();

	public static HeapStats main
	{
		get
		{
			if (_main == null)
			{
				_main = new HeapStats();
			}
			return _main;
		}
	}

	public bool IsRecording { get; set; }

	public void Clear()
	{
		entries.Clear();
	}

	public void RecordStats(string statId, CellProcessingStats stats)
	{
		entries.GetOrAddNew(statId).Add(stats);
	}

	private CategoryData CollectCategoryStats(List<CellProcessingStats> stats)
	{
		stats.Sort((CellProcessingStats a, CellProcessingStats b) => a.timeToProcess.CompareTo(b.timeToProcess));
		CategoryData result = default(CategoryData);
		result.numCells = stats.Count;
		if (result.numCells == 0)
		{
			result.avgProcessTime = 0f;
			result.medianProcessTime = 0f;
			result.worstProcessTime = 0f;
		}
		else
		{
			result.avgProcessTime = stats.Average((CellProcessingStats a) => a.timeToProcess);
			result.medianProcessTime = stats[stats.Count / 2].timeToProcess;
			result.worstProcessTime = stats[stats.Count - 1].timeToProcess;
		}
		return result;
	}

	public void WriteStatsToFile(string phototourId)
	{
		foreach (KeyValuePair<string, List<CellProcessingStats>> entry in entries)
		{
			WriteStatsToFile(entry.Value, phototourId, entry.Key);
		}
	}

	private void WriteStatsToFile(List<CellProcessingStats> stats, string phototourId, string statId)
	{
		if (stats.Count == 0)
		{
			return;
		}
		using (StreamWriter streamWriter = FileUtils.CreateTextFile(StopwatchProfiler.Instance.GetOutputFilePath(SNUtils.GetDevTempPath(), phototourId, statId)))
		{
			streamWriter.Write("OrderEnter,OrderExit,TimeEnter,TimeExit,TimeToProcess,PriorityEnter,PriorityExit,PriorityDelta,HeapSize,DistOnCreate,DistOnProcess,DeltaDist,AngleOnCreate,AngleOnProcess,DeltaAngle,NumPriChanges\n");
			stats.Sort((CellProcessingStats a, CellProcessingStats b) => a.inId.CompareTo(b.inId));
			foreach (CellProcessingStats stat in stats)
			{
				streamWriter.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}", stat.inId, stat.outId, stat.inTime, stat.outTime, stat.timeToProcess, stat.inPriority, stat.outPriority, stat.deltaPriority, stat.inQueueLength, stat.inDistance, stat.outDistance, Mathf.Abs(stat.deltaDistance), stat.inAngle, stat.outAngle, Mathf.Abs(stat.deltaAngle), stat.numPriorityChanges);
			}
			List<CellProcessingStats> stats2 = stats.Where((CellProcessingStats a) => a.inDistance < 100f).ToList();
			List<CellProcessingStats> stats3 = stats.Where((CellProcessingStats a) => a.inDistance >= 100f && a.inDistance < 200f).ToList();
			List<CellProcessingStats> stats4 = stats.Where((CellProcessingStats a) => a.inDistance >= 200f && a.inDistance < 300f).ToList();
			List<CellProcessingStats> stats5 = stats.Where((CellProcessingStats a) => a.inDistance >= 300f).ToList();
			CategoryData categoryData = CollectCategoryStats(stats);
			CategoryData categoryData2 = CollectCategoryStats(stats2);
			CategoryData categoryData3 = CollectCategoryStats(stats3);
			CategoryData categoryData4 = CollectCategoryStats(stats4);
			CategoryData categoryData5 = CollectCategoryStats(stats5);
			streamWriter.WriteLine();
			streamWriter.WriteLine("CellDistance,All,< 100m,< 200m,< 300m,> 300m");
			streamWriter.WriteLine("NumCells,{0},{1},{2},{3},{4}", categoryData.numCells, categoryData2.numCells, categoryData3.numCells, categoryData4.numCells, categoryData5.numCells);
			streamWriter.WriteLine("Avg Time To Process,{0},{1},{2},{3},{4}", categoryData.avgProcessTime, categoryData2.avgProcessTime, categoryData3.avgProcessTime, categoryData4.avgProcessTime, categoryData5.avgProcessTime);
			streamWriter.WriteLine("Median Time To Process,{0},{1},{2},{3},{4}", categoryData.medianProcessTime, categoryData2.medianProcessTime, categoryData3.medianProcessTime, categoryData4.medianProcessTime, categoryData5.medianProcessTime);
			streamWriter.WriteLine("Worst Time To Process,{0},{1},{2},{3},{4}", categoryData.worstProcessTime, categoryData2.worstProcessTime, categoryData3.worstProcessTime, categoryData4.worstProcessTime, categoryData5.worstProcessTime);
			float onscreenAngle = 45f;
			List<CellProcessingStats> stats6 = stats.Where((CellProcessingStats a) => a.inAngle < onscreenAngle).ToList();
			List<CellProcessingStats> stats7 = stats.Where((CellProcessingStats a) => a.inAngle < onscreenAngle && a.inDistance < 100f).ToList();
			List<CellProcessingStats> stats8 = stats.Where((CellProcessingStats a) => a.inAngle < onscreenAngle && a.inDistance >= 100f && a.inDistance < 200f).ToList();
			List<CellProcessingStats> stats9 = stats.Where((CellProcessingStats a) => a.inAngle < onscreenAngle && a.inDistance >= 200f).ToList();
			List<CellProcessingStats> stats10 = stats.Where((CellProcessingStats a) => a.inAngle >= onscreenAngle && a.inDistance < 100f).ToList();
			List<CellProcessingStats> stats11 = stats.Where((CellProcessingStats a) => a.inAngle >= onscreenAngle).ToList();
			CategoryData categoryData6 = CollectCategoryStats(stats6);
			CategoryData categoryData7 = CollectCategoryStats(stats7);
			CategoryData categoryData8 = CollectCategoryStats(stats8);
			CategoryData categoryData9 = CollectCategoryStats(stats9);
			CategoryData categoryData10 = CollectCategoryStats(stats10);
			CategoryData categoryData11 = CollectCategoryStats(stats11);
			streamWriter.WriteLine();
			streamWriter.WriteLine("Angle and Distance,on screen,on screen < 100m,on screen < 200m,on screen > 200m,off screen < 100m,off screen");
			streamWriter.WriteLine("NumCells,{0},{1},{2},{3},{4},{5}", categoryData6.numCells, categoryData7.numCells, categoryData8.numCells, categoryData9.numCells, categoryData10.numCells, categoryData11.numCells);
			streamWriter.WriteLine("Avg Time To Process,{0},{1},{2},{3},{4},{5}", categoryData6.avgProcessTime, categoryData7.avgProcessTime, categoryData8.avgProcessTime, categoryData9.avgProcessTime, categoryData10.avgProcessTime, categoryData11.avgProcessTime);
			streamWriter.WriteLine("Median Time To Process,{0},{1},{2},{3},{4},{5}", categoryData6.medianProcessTime, categoryData7.medianProcessTime, categoryData8.medianProcessTime, categoryData9.medianProcessTime, categoryData10.medianProcessTime, categoryData11.medianProcessTime);
			streamWriter.WriteLine("Worst Time To Process,{0},{1},{2},{3},{4},{5}", categoryData6.worstProcessTime, categoryData7.worstProcessTime, categoryData8.worstProcessTime, categoryData9.worstProcessTime, categoryData10.worstProcessTime, categoryData11.worstProcessTime);
			List<CellProcessingStats> stats12 = stats.Where((CellProcessingStats a) => a.inPriority > 0f).ToList();
			List<CellProcessingStats> stats13 = stats.Where((CellProcessingStats a) => a.inPriority < 0f).ToList();
			List<CellProcessingStats> stats14 = stats.Where((CellProcessingStats a) => a.inPriority > 0f && a.deltaPriority > 0f).ToList();
			List<CellProcessingStats> stats15 = stats.Where((CellProcessingStats a) => a.inPriority < 0f && a.deltaPriority < 0f).ToList();
			CategoryData categoryData12 = CollectCategoryStats(stats12);
			CategoryData categoryData13 = CollectCategoryStats(stats13);
			CategoryData categoryData14 = CollectCategoryStats(stats14);
			CategoryData categoryData15 = CollectCategoryStats(stats15);
			streamWriter.WriteLine();
			streamWriter.WriteLine("Before and After,A,B,C,D");
			streamWriter.WriteLine("NumCells,{0},{1},{2},{3}", categoryData12.numCells, categoryData13.numCells, categoryData14.numCells, categoryData15.numCells);
			streamWriter.WriteLine("Avg Time To Process,{0},{1},{2},{3}", categoryData12.avgProcessTime, categoryData13.avgProcessTime, categoryData14.avgProcessTime, categoryData15.avgProcessTime);
			streamWriter.WriteLine("Median Time To Process,{0},{1},{2},{3}", categoryData12.medianProcessTime, categoryData13.medianProcessTime, categoryData14.medianProcessTime, categoryData15.medianProcessTime);
			streamWriter.WriteLine("Worst Time To Process,{0},{1},{2},{3}", categoryData12.worstProcessTime, categoryData13.worstProcessTime, categoryData14.worstProcessTime, categoryData15.worstProcessTime);
			streamWriter.WriteLine();
			streamWriter.WriteLine("HISTOGRAM");
			streamWriter.WriteLine("Process Time,# of Cells");
			for (float lowerBound = 0f; lowerBound < categoryData.worstProcessTime; lowerBound += 1f)
			{
				float upperBound = lowerBound + 1f;
				streamWriter.WriteLine("{0} -> {1}s,{2}", lowerBound, upperBound, stats.Count((CellProcessingStats a) => a.timeToProcess >= lowerBound && a.timeToProcess < upperBound));
			}
		}
	}
}
