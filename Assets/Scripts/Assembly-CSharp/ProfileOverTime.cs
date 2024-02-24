using System.Diagnostics;
using UWE;
using UnityEngine;

public class ProfileOverTime
{
	private int m_FrameWindow;

	private float[] m_FrameTimes;

	private int m_FrameIndex;

	private string m_TimerID;

	private Stopwatch watch;

	public ProfileOverTime(string timerID, int frameWindow)
	{
		m_FrameWindow = frameWindow;
		m_FrameTimes = new float[m_FrameWindow];
		m_FrameIndex = 0;
		m_TimerID = timerID;
	}

	public void StartTimer()
	{
		if (watch == null)
		{
			watch = new Stopwatch();
			watch.Reset();
		}
		watch.Restart();
	}

	public void StopTimer()
	{
		if (watch.IsRunning)
		{
			watch.Stop();
			float timeElapsedMS = UWE.Utils.GetTimeElapsedMS(watch);
			m_FrameTimes[m_FrameIndex] = timeElapsedMS;
			m_FrameIndex++;
			if (m_FrameIndex == m_FrameWindow)
			{
				CalculateAverageTime();
				m_FrameIndex = 0;
			}
		}
	}

	private void CalculateAverageTime()
	{
		float num = 0f;
		float num2 = float.MaxValue;
		float num3 = 0f;
		for (int i = 0; i < m_FrameWindow; i++)
		{
			num += m_FrameTimes[i];
			num2 = Mathf.Min(m_FrameTimes[i], num2);
			num3 = Mathf.Max(m_FrameTimes[i], num3);
		}
		float num4 = num / (float)m_FrameWindow;
		UnityEngine.Debug.LogFormat("TIMER_{0},{1},{2},{3},{4}", m_TimerID, m_FrameWindow, num4, num2, num3);
	}
}
