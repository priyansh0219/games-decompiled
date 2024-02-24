using System.Diagnostics;
using System.IO;
using UWE;
using UnityEngine;

public class FrameTimeRecorder : MonoBehaviour
{
	private static FrameTimeRecorder instance;

	private StreamWriter writer;

	private Stopwatch watch = new Stopwatch();

	private float startRecordTime;

	public static FrameTimeRecorder main
	{
		get
		{
			if (instance == null)
			{
				instance = MainCamera.camera.gameObject.AddComponent<FrameTimeRecorder>();
			}
			return instance;
		}
		set
		{
			instance = value;
		}
	}

	public bool IsRecording()
	{
		return writer != null;
	}

	public void Record(string path, float delaySecs = 0f)
	{
		writer = FileUtils.CreateTextFile(path);
		watch.Reset();
		startRecordTime = Time.time + delaySecs;
	}

	public void Stop()
	{
		if (writer != null)
		{
			writer.Close();
		}
	}

	private void OnPostRender()
	{
		if (writer != null)
		{
			if (watch.IsRunning && Time.time > startRecordTime)
			{
				float timeElapsedMS = Utils.GetTimeElapsedMS(watch);
				writer.WriteLine(string.Concat(timeElapsedMS));
			}
			watch.Restart();
		}
	}

	private void OnDestroy()
	{
		Stop();
	}

	private void OnApplicationQuit()
	{
		Stop();
	}
}
