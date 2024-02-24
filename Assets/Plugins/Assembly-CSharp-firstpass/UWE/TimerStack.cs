using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace UWE
{
	public class TimerStack
	{
		public class Entry
		{
			public string label;

			public Stopwatch watch = new Stopwatch();

			public long childrenTicks;

			public float minLogMS;

			public void Stop()
			{
				watch.Reset();
			}

			public void Start(string label, float minLogMS)
			{
				this.label = label;
				this.minLogMS = minLogMS;
				childrenTicks = 0L;
				watch.Start();
			}
		}

		public static bool forceMuteAll;

		private List<Entry> entries = new List<Entry>();

		private int topEntry = -1;

		private string prefix = "";

		public bool log = true;

		public TimerStack(string prefix)
		{
			this.prefix = prefix;
		}

		public TimerStack()
		{
		}

		public void Begin(string label)
		{
			Begin(label, 0f);
		}

		public void Begin(string label, float minLogMS)
		{
			topEntry++;
			if (topEntry >= entries.Count)
			{
				entries.Add(new Entry());
			}
			entries[topEntry].Start(label, minLogMS);
		}

		public float End()
		{
			if (topEntry >= 0)
			{
				Entry entry = entries[topEntry--];
				float timeElapsedMS = Utils.GetTimeElapsedMS(entry.watch);
				if (log && !forceMuteAll)
				{
					float num = (float)(entry.watch.ElapsedTicks - entry.childrenTicks) * 1f / 10000f;
					if (timeElapsedMS > entry.minLogMS)
					{
						UnityEngine.Debug.Log(("L" + (topEntry + 1) + ": " + prefix + entry.label + " total ms: " + timeElapsedMS + " self ms: " + num) ?? "");
					}
				}
				if (topEntry >= 0)
				{
					entries[topEntry].childrenTicks += entry.watch.ElapsedTicks;
				}
				entry.Stop();
				return timeElapsedMS;
			}
			UnityEngine.Debug.LogError("unmatched UWE.TimerStack.End() call. prefix = " + prefix);
			return 0f;
		}
	}
}
