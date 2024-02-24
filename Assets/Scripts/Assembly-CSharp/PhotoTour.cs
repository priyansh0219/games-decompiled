using System;
using System.Collections.Generic;
using System.IO;
using Platform.IO;
using UWE;
using UnityEngine;

public class PhotoTour : MonoBehaviour, IOnQuitBehaviour
{
	[Serializable]
	private class Settings
	{
		public double samplePeriod = 1.0;
	}

	public delegate bool CanTakeShot();

	public struct Sample
	{
		public float time;

		public Vector3 pos;

		public float rotationX;

		public float rotationY;

		public bool screenshot;

		public void Capture(MainCameraControl cam)
		{
			Player localPlayerComp = Utils.GetLocalPlayerComp();
			pos = localPlayerComp.transform.position;
			rotationX = cam.rotationX;
			rotationY = cam.rotationY;
		}

		public void Write(BinaryWriter w)
		{
			w.Write(time);
			w.Write(pos);
			w.Write(rotationX);
			w.Write(rotationY);
			w.Write(screenshot);
		}

		public void Read(BinaryReader r, int version)
		{
			time = r.ReadSingle();
			pos = r.ReadVector3();
			if (version >= 1)
			{
				rotationX = r.ReadSingle();
				rotationY = r.ReadSingle();
			}
			else
			{
				Quaternion quaternion = r.ReadQuaternion();
				rotationX = quaternion.eulerAngles.y;
				rotationY = quaternion.eulerAngles.x;
			}
			screenshot = r.ReadBoolean();
		}
	}

	public delegate void Handler(PhotoTour tour);

	public CanTakeShot canTakeShot;

	private MainCameraControl cam;

	private static PhotoTour instance;

	public float playbackTimeScale = 1f;

	[NonSerialized]
	private Settings settings;

	private const int Version = 0;

	private float lastSampleTime = -1f;

	private int numSamples;

	private string state = "idle";

	private BinaryWriter writer;

	private string recordingPath;

	private readonly List<Sample> playingSamples = new List<Sample>(4096);

	private int nextSample;

	private float playStartTime;

	private string playMode;

	private string sshotDir;

	private long prevTicks = -1L;

	private float recordStartTime;

	private float recordEndTime;

	public bool bScreenShotsAllowed = true;

	private float shotWaitTime;

	public static PhotoTour main
	{
		get
		{
			if (instance == null)
			{
				instance = new GameObject("Photo Tour singleton").AddComponent<PhotoTour>();
			}
			return instance;
		}
		set
		{
			instance = value;
		}
	}

	public static string toursFolder => SNUtils.InsideUnmanaged("phototours");

	public event Handler onPlaybackDone;

	public event Handler onRecordingDone;

	private void LoadSettings()
	{
		string json = Platform.IO.File.ReadAllText(SNUtils.InsideUnmanaged("phototour.json"));
		settings = JsonUtility.FromJson<Settings>(json);
	}

	private void Awake()
	{
		PlatformUtils.RegisterOnQuitBehaviour(this);
		LoadSettings();
	}

	private void RecordingUpdate()
	{
		bool flag = false;
		bool screenshot = false;
		if (lastSampleTime < 0f)
		{
			flag = true;
		}
		else if ((double)(Time.time - lastSampleTime) > settings.samplePeriod)
		{
			flag = true;
		}
		if (Input.GetKeyDown(KeyCode.F10))
		{
			flag = true;
			screenshot = true;
		}
		if (flag)
		{
			lastSampleTime = Time.time;
			Sample sample = default(Sample);
			sample.Capture(cam);
			sample.screenshot = screenshot;
			sample.time = Time.time - recordStartTime;
			sample.Write(writer);
			numSamples++;
		}
		if (Time.time > recordEndTime)
		{
			StopRecording();
		}
	}

	private void SetPlayerView(Vector3 pos, float rotationX, float rotationY)
	{
		Utils.GetLocalPlayerComp().SetPosition(pos);
		cam.rotationX = rotationX;
		cam.rotationY = rotationY;
	}

	private void NormalPlayingUpdate()
	{
		float num = playbackTimeScale * (Time.time - playStartTime);
		while (true)
		{
			if (nextSample >= playingSamples.Count)
			{
				Debug.Log("Done playing photo tour");
				StopPlaying();
				break;
			}
			Sample sample = playingSamples[nextSample];
			if (!(sample.time < num))
			{
				break;
			}
			SetPlayerView(sample.pos, sample.rotationX, sample.rotationY);
			if (sample.screenshot)
			{
				TakeShot();
			}
			nextSample++;
		}
		if (state == "playing" && nextSample < playingSamples.Count && nextSample > 0)
		{
			Sample sample2 = playingSamples[nextSample - 1];
			Sample sample3 = playingSamples[nextSample];
			float t = (num - sample2.time) / (sample3.time - sample2.time);
			Vector3 pos = Vector3.Lerp(sample2.pos, sample3.pos, t);
			float rotationX = Mathf.Lerp(sample2.rotationX, sample3.rotationX, t);
			float rotationY = Mathf.Lerp(sample2.rotationY, sample3.rotationY, t);
			SetPlayerView(pos, rotationX, rotationY);
		}
	}

	private void TakeShot()
	{
		if (bScreenShotsAllowed)
		{
			Int3 @int = Int3.Floor(MainCamera.camera.transform.position);
			PlatformUtils.main.CaptureScreenshot(Platform.IO.Path.Combine(sshotDir, "phototour-shot-" + @int.x + "-" + @int.y + "-" + @int.z + ".png"));
		}
	}

	private void ShotsOnlyUpdate()
	{
		Sample sample;
		while (true)
		{
			if (nextSample >= playingSamples.Count)
			{
				Debug.Log("Done playing photo tour");
				if (this.onPlaybackDone != null)
				{
					this.onPlaybackDone(this);
				}
				state = "idle";
				return;
			}
			sample = playingSamples[nextSample];
			if (sample.screenshot)
			{
				break;
			}
			nextSample++;
			shotWaitTime = 0f;
		}
		SetPlayerView(sample.pos, sample.rotationX, sample.rotationY);
		shotWaitTime += Time.deltaTime;
		if (shotWaitTime > 5f && (canTakeShot == null || canTakeShot()))
		{
			TakeShot();
			nextSample++;
			shotWaitTime = 0f;
		}
	}

	private void Update()
	{
		if (state == "recording")
		{
			RecordingUpdate();
		}
		else if (state == "playing")
		{
			if (playMode == "shotsonly")
			{
				ShotsOnlyUpdate();
			}
			else
			{
				NormalPlayingUpdate();
			}
		}
	}

	private void OnDestroy()
	{
		PlatformUtils.DeregisterOnQuitBehaviour(this);
		if (writer != null)
		{
			writer.Close();
		}
	}

	private void OnApplicationQuit()
	{
		OnQuit();
	}

	public void OnQuit()
	{
		if (writer != null)
		{
			writer.Close();
		}
	}

	public static string[] GetTourFiles()
	{
		return Platform.IO.Directory.GetFiles(toursFolder, "*.tour");
	}

	public void StartRecording(string tourName, int timeSeconds)
	{
		FindCamera();
		if (string.IsNullOrEmpty(tourName))
		{
			recordingPath = UWE.Utils.GenerateNumberedFileName(Platform.IO.Path.Combine(toursFolder, "phototour-"), ".tour");
		}
		else
		{
			recordingPath = Platform.IO.Path.Combine(toursFolder, $"{tourName}.tour");
		}
		writer = new BinaryWriter(FileUtils.CreateFile(recordingPath));
		writer.Write('P');
		int val = 1;
		writer.WriteInt32(val);
		lastSampleTime = Time.time;
		state = "recording";
		recordStartTime = Time.time;
		if (timeSeconds == 0)
		{
			recordEndTime = float.MaxValue;
		}
		else
		{
			recordEndTime = Time.time + (float)timeSeconds;
		}
	}

	public void StopRecording()
	{
		writer.Close();
		state = "idle";
		if (this.onRecordingDone != null)
		{
			this.onRecordingDone(this);
		}
	}

	public bool LayoutGUI()
	{
		bool result = true;
		if (state == "idle")
		{
			if (GUILayout.Button("Start Recording"))
			{
				StartRecording(string.Empty, 0);
			}
			string[] tourFiles = GetTourFiles();
			foreach (string text in tourFiles)
			{
				if (GUILayout.Button("Play " + text))
				{
					string mode = (Event.current.alt ? "shotsonly" : "normal");
					PlayFile(text, mode, ".");
					result = false;
				}
			}
		}
		else if (state == "recording")
		{
			GUILayout.Label("Recorded " + numSamples + " samples into " + recordingPath);
			if (GUILayout.Button("Stop Recording"))
			{
				StopRecording();
			}
		}
		return result;
	}

	private bool TourFileHasVersionInfo(string path)
	{
		bool flag = false;
		using (BinaryReader binaryReader = new BinaryReader(FileUtils.ReadFile(path)))
		{
			return binaryReader.ReadChar() == 'P';
		}
	}

	public void PlayFile(string path, string mode, string sshotDir)
	{
		if (!(state == "idle"))
		{
			return;
		}
		FindCamera();
		playingSamples.Clear();
		this.sshotDir = sshotDir;
		playMode = mode;
		bool flag = TourFileHasVersionInfo(path);
		using (BinaryReader binaryReader = new BinaryReader(FileUtils.ReadFile(path)))
		{
			int version = 0;
			if (flag)
			{
				binaryReader.ReadChar();
				version = binaryReader.ReadInt32();
			}
			while (true)
			{
				try
				{
					Sample item = default(Sample);
					item.Read(binaryReader, version);
					playingSamples.Add(item);
				}
				catch (EndOfStreamException)
				{
					break;
				}
			}
		}
		Debug.Log("Read " + playingSamples.Count + " samples from " + path);
		StartPlaying();
	}

	private void StartPlaying()
	{
		nextSample = 0;
		playStartTime = Time.time;
		state = "playing";
	}

	public void StopPlaying()
	{
		state = "idle";
		if (this.onPlaybackDone != null)
		{
			this.onPlaybackDone(this);
		}
	}

	private void FindCamera()
	{
		MainCameraControl[] array = UnityEngine.Object.FindObjectsOfType<MainCameraControl>();
		int num = 0;
		if (num < array.Length)
		{
			MainCameraControl mainCameraControl = array[num];
			cam = mainCameraControl;
		}
	}
}
