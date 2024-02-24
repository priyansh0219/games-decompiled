using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Gendarme;
using UWE;
using UnityEngine;

public static class ProfilingUtils
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Sample : IDisposable
	{
		public Sample(string name)
		{
		}

		public Sample(string name, UnityEngine.Object context)
		{
		}

		public void Dispose()
		{
		}
	}

	private static int mainThreadId = 1;

	private const string endSample = "---";

	[ThreadStatic]
	private static int sampleFrame = 0;

	[ThreadStatic]
	private static List<string> sampleLog;

	[ThreadStatic]
	private static Stack<string> sampleStack;

	public static void SetMainThreadId(int managedThreadId)
	{
		mainThreadId = managedThreadId;
	}

	public static int GetMainThreadId()
	{
		return mainThreadId;
	}

	[Conditional("PROFILER_MARKERS")]
	public static void BeginSample(string name)
	{
	}

	[Conditional("PROFILER_MARKERS")]
	public static void BeginSample(string name, UnityEngine.Object context)
	{
	}

	[Conditional("PROFILER_MARKERS")]
	public static void EndSample(string name = null)
	{
	}

	[Conditional("DEBUG_UNITY_EDITOR")]
	private static void LogBeginSample(string name)
	{
		if (Thread.CurrentThread.ManagedThreadId != mainThreadId || Application.isPlaying)
		{
			CheckLog();
			if (sampleStack.Count == 0)
			{
				sampleLog.Clear();
			}
			sampleStack.Push(name);
			sampleLog.Add(name);
		}
	}

	[Conditional("DEBUG_UNITY_EDITOR")]
	[SuppressMessage("Subnautica.Rules", "AvoidDebugLogErrorRule")]
	private static void LogEndSample(string name)
	{
		if (Thread.CurrentThread.ManagedThreadId == mainThreadId && !Application.isPlaying)
		{
			return;
		}
		CheckLog();
		if (sampleStack.Count == 0)
		{
			UnityEngine.Debug.LogErrorFormat("EndSample called without prior call to BeginSample.\nLog:\n{0}", FormatSampleLog());
			return;
		}
		string text = sampleStack.Pop();
		sampleLog.Add("---");
		StackFrame stackFrame = new StackFrame(3, fNeedFileInfo: true);
		sampleLog.Add(stackFrame.ToString());
		if (!string.IsNullOrEmpty(name) && name != text)
		{
			UnityEngine.Debug.LogErrorFormat("Mismatched EndSample call '{0}', expected '{1}'.\nStack: {2}.\nLog:\n{3}", name, text, string.Join(", ", sampleStack.ToArray()), FormatSampleLog());
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidDebugLogErrorRule")]
	private static void CheckLog()
	{
		if (sampleStack == null)
		{
			sampleStack = new Stack<string>();
			sampleLog = new List<string>();
		}
		int num = ((Thread.CurrentThread.ManagedThreadId == mainThreadId) ? Time.frameCount : WorkerThread.executionCounter);
		if (num != sampleFrame)
		{
			if (sampleStack.Count > 1)
			{
				UnityEngine.Debug.LogErrorFormat("Missing EndSample call after prior call to BeginSample.\nStack: {0}.\nLog:\n{1}", string.Join(", ", sampleStack.ToArray()), FormatSampleLog());
			}
			sampleFrame = num;
			sampleStack.Clear();
			sampleLog.Clear();
		}
	}

	private static string FormatSampleLog()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		for (int i = 0; i < sampleLog.Count; i++)
		{
			string text = sampleLog[i];
			if (text == "---")
			{
				num = Mathf.Max(0, num - 1);
			}
			stringBuilder.Append(' ', num * 4);
			stringBuilder.Append(text);
			if (text == "---")
			{
				stringBuilder.Append(' ');
				i++;
				stringBuilder.Append(sampleLog[i]);
			}
			stringBuilder.AppendLine();
			if (text != "---")
			{
				num++;
			}
		}
		return stringBuilder.ToString();
	}
}
