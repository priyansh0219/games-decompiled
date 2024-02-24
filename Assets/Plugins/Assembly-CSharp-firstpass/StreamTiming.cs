using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class StreamTiming
{
	public struct Block : IDisposable
	{
		private string label;

		public Block(string label)
		{
			this.label = label;
			main.Begin(label);
		}

		public void Dispose()
		{
			main.End(label);
		}
	}

	private static StreamTiming _main;

	private Dictionary<string, long> label2totalTicks = new Dictionary<string, long>();

	private Dictionary<string, long> label2beginTick = new Dictionary<string, long>();

	private Dictionary<string, int> label2calls = new Dictionary<string, int>();

	private Stopwatch sw = new Stopwatch();

	public static StreamTiming main
	{
		get
		{
			if (_main == null)
			{
				_main = new StreamTiming();
			}
			return _main;
		}
	}

	public static void Deinitialize()
	{
		_main = null;
	}

	public StreamTiming()
	{
		sw.Restart();
	}

	public void Begin(string label)
	{
		if (label2beginTick.ContainsKey(label))
		{
			label2calls[label] += 1;
		}
		else
		{
			label2calls[label] = 0;
		}
		label2beginTick[label] = sw.ElapsedTicks;
	}

	public void End(string label)
	{
		if (!label2totalTicks.ContainsKey(label))
		{
			label2totalTicks[label] = 0L;
		}
		label2totalTicks[label] += sw.ElapsedTicks - label2beginTick[label];
		label2beginTick[label] = -1L;
	}

	public float GetTotalSecs(string label)
	{
		return (float)label2totalTicks[label] * 1f / (float)Stopwatch.Frequency;
	}

	public void LogAll()
	{
		UnityEngine.Debug.Log("-- BEGIN STREAM TIMING LOG --");
		foreach (KeyValuePair<string, long> label2totalTick in label2totalTicks)
		{
			string key = label2totalTick.Key;
			float num = (float)label2totalTick.Value * 1f / (float)Stopwatch.Frequency * 1000f;
			UnityEngine.Debug.Log(key + " --> " + num + " ms (" + label2calls[key] + " calls)");
		}
		UnityEngine.Debug.Log("-- END STREAM TIMING LOG --");
	}

	public void Reset()
	{
		label2totalTicks.Clear();
		label2beginTick.Clear();
	}
}
