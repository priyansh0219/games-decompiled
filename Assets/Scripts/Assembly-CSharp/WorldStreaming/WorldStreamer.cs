using System;
using System.Collections;
using System.IO;
using System.Threading;
using UWE;
using UnityEngine;

namespace WorldStreaming
{
	public sealed class WorldStreamer : MonoBehaviour
	{
		public sealed class Settings
		{
			public string worldPath;

			public Int3 numOctrees;

			public int numOctreesPerBatch;

			public int octreeSize;
		}

		[SerializeField]
		public int numOctreesPerSecond;

		[SerializeField]
		public int numClipsPerSecond;

		[SerializeField]
		public int numCellsPerSecond;

		[SerializeField]
		public int numBuildLayersPerSecond;

		[SerializeField]
		public int numToggleChunksPerSecond;

		[SerializeField]
		public int numDestroyChunksPerSecond;

		[AssertNotNull]
		[SerializeField]
		public Transform chunkRoot;

		public TerrainPoolManager terrainPoolManager;

		[NonSerialized]
		private Settings settings;

		[NonSerialized]
		private UnityThread buildLayersThread;

		[NonSerialized]
		private UnityThread toggleChunksThread;

		[NonSerialized]
		private UnityThread destroyChunksThread;

		[NonSerialized]
		private IThread streamingThread;

		[NonSerialized]
		private int activeSettingsId;

		[NonSerialized]
		private int activeQualityLevel;

		[NonSerialized]
		public bool isLoading;

		[NonSerialized]
		private bool reloading;

		private const float streamingFrequency = 100f;

		private const float streamingDeltaTime = 0.01f;

		private int batchSize;

		private static readonly Task.Function InvokeUnloadDelegate = InvokeUnload;

		[field: NonSerialized]
		public VoxelandBlockType[] blockTypes { get; private set; }

		[field: NonSerialized]
		public BatchOctreesStreamer octreesStreamer { get; private set; }

		[field: NonSerialized]
		public BatchOctreesStreamer lowDetailOctreesStreamer { get; private set; }

		[field: NonSerialized]
		public ClipmapStreamer clipmapStreamer { get; private set; }

		[field: NonSerialized]
		public ClipmapVisibilityUpdater visibilityUpdater { get; private set; }

		[field: NonSerialized]
		public Int3 streamingCenter { get; private set; }

		private void Start()
		{
		}

		private void OnDestroy()
		{
			Stop();
			DestroyStreamers();
		}

		public void Start(string paletteResourcePath, Settings settings)
		{
			VoxelandBlockType[] array = LoadBlockTypes(paletteResourcePath);
			Start(array, settings);
		}

		public void Start(VoxelandBlockType[] blockTypes, Settings settings)
		{
			this.blockTypes = blockTypes;
			this.settings = settings;
			buildLayersThread = new UnityThread("BuildLayers", 128);
			toggleChunksThread = new UnityThread("ToggleChunks", 128);
			destroyChunksThread = new UnityThread("DestroyChunks", 128);
			StartCoroutine(PumpUnityThread(buildLayersThread, () => CalculateNumPerFrame(numBuildLayersPerSecond, isLoading)));
			StartCoroutine(PumpUnityThread(toggleChunksThread, () => CalculateNumPerFrame(numToggleChunksPerSecond, isLoading)));
			StartCoroutine(PumpUnityThread(destroyChunksThread, () => CalculateNumPerFrame(numDestroyChunksPerSecond, isLoading)));
			streamingThread = ThreadUtils.StartThrottledThread("Streaming", "StreamingThread", System.Threading.ThreadPriority.BelowNormal, -2, 128, 100f);
			visibilityUpdater = new ClipmapVisibilityUpdater();
			streamingThread.StartCoroutine(UpdateVisbility(visibilityUpdater));
			int qualityLevel = QualitySettings.GetQualityLevel();
			CreateStreamers(qualityLevel);
		}

		private void CreateStreamers(int qualityLevel)
		{
			activeQualityLevel = qualityLevel;
			batchSize = this.settings.octreeSize * this.settings.numOctreesPerBatch;
			Int3.Bounds octreeBounds = new Int3.Bounds(Int3.zero, this.settings.numOctrees - 1);
			int numOctreesPerBatch = this.settings.numOctreesPerBatch;
			string path = Path.Combine(this.settings.worldPath, "CompiledOctreesCache");
			BatchOctreesAllocator.Initialize();
			LargeWorldStreamer.Settings settings = ParseStreamingSettings(activeQualityLevel);
			lowDetailOctreesStreamer = new BatchOctreesStreamer(streamingThread, octreeBounds, 4, 4, batchSize, numOctreesPerBatch, path, settings.lowDetailOctreesSettings);
			octreesStreamer = new BatchOctreesStreamer(streamingThread, octreeBounds, 0, 3, batchSize, numOctreesPerBatch, path, settings.octreesSettings);
			ClipMapManager.Settings settings2 = ParseClipmapSettings(activeQualityLevel);
			clipmapStreamer = new ClipmapStreamer(this, visibilityUpdater, streamingThread, buildLayersThread, toggleChunksThread, destroyChunksThread, settings2);
			lowDetailOctreesStreamer.RegisterListener(clipmapStreamer);
			octreesStreamer.RegisterListener(clipmapStreamer);
			streamingThread.StartCoroutine(UpdateCenter(octreesStreamer, activeSettingsId, "UpdateOctrees"));
			streamingThread.StartCoroutine(UpdateCenter(lowDetailOctreesStreamer, activeSettingsId, "UpdateLowDetailOctrees"));
			streamingThread.StartCoroutine(UpdateCenter(clipmapStreamer, activeSettingsId, "UpdateClips"));
			streamingThread.StartCoroutine(StreamOctrees(lowDetailOctreesStreamer));
			streamingThread.StartCoroutine(StreamOctrees(octreesStreamer));
			streamingThread.StartCoroutine(StreamClipmap(clipmapStreamer));
		}

		private void DestroyStreamers()
		{
			if (lowDetailOctreesStreamer != null)
			{
				lowDetailOctreesStreamer.Reset();
				lowDetailOctreesStreamer = null;
			}
			if (octreesStreamer != null)
			{
				octreesStreamer.Reset();
				octreesStreamer = null;
			}
			if (clipmapStreamer != null)
			{
				clipmapStreamer.Reset();
				clipmapStreamer = null;
			}
			BatchOctreesAllocator.Deinitialize();
			streamingThread = null;
		}

		public void Stop()
		{
			if (clipmapStreamer != null)
			{
				clipmapStreamer.Stop();
			}
			if (octreesStreamer != null)
			{
				octreesStreamer.Stop();
			}
			if (lowDetailOctreesStreamer != null)
			{
				lowDetailOctreesStreamer.Stop();
			}
			if (streamingThread != null)
			{
				streamingThread.Stop();
			}
			if (destroyChunksThread != null)
			{
				destroyChunksThread.Stop();
			}
			if (toggleChunksThread != null)
			{
				toggleChunksThread.Stop();
			}
			if (buildLayersThread != null)
			{
				buildLayersThread.Stop();
			}
		}

		public void UpdateStreamingCenter(Vector3 wsPos)
		{
			Vector3 vector = new Vector3(0f, -8f, 0f);
			streamingCenter = (Int3)base.transform.InverseTransformPoint(wsPos + vector);
		}

		public bool IsIdle()
		{
			if (lowDetailOctreesStreamer.IsIdle() && octreesStreamer.IsIdle() && clipmapStreamer.IsIdle() && visibilityUpdater.IsIdle() && buildLayersThread.IsIdle() && destroyChunksThread.IsIdle())
			{
				return toggleChunksThread.IsIdle();
			}
			return false;
		}

		public void IncreaseFreezeCount()
		{
		}

		public void DecreaseFreezeCount()
		{
		}

		public bool IsFrozen()
		{
			return false;
		}

		public int EstimateBytes()
		{
			return 0;
		}

		public void ReloadSettings(ClipMapManager.IClipMapEventHandler clipmapListener)
		{
			if (!reloading)
			{
				StartCoroutine(ReloadSettingsAsync(clipmapListener));
			}
		}

		private IEnumerator ReloadSettingsAsync(ClipMapManager.IClipMapEventHandler clipmapListener)
		{
			reloading = true;
			Debug.Log("Freezing streamers");
			activeSettingsId++;
			Debug.Log("Unloading streamers");
			streamingThread.Enqueue(InvokeUnloadDelegate, this, clipmapStreamer);
			streamingThread.Enqueue(InvokeUnloadDelegate, this, lowDetailOctreesStreamer);
			streamingThread.Enqueue(InvokeUnloadDelegate, this, octreesStreamer);
			yield return streamingThread.Wait();
			Debug.Log("Waiting for idle");
			while (!IsIdle())
			{
				yield return null;
			}
			Debug.Log("Stopping streamers");
			octreesStreamer.Stop();
			lowDetailOctreesStreamer.Stop();
			clipmapStreamer.Stop();
			Debug.Log("Starting new streamers");
			int qualityLevel = QualitySettings.GetQualityLevel();
			CreateStreamers(qualityLevel);
			clipmapStreamer.RegisterListener(clipmapListener);
			reloading = false;
		}

		private static void InvokeUnload(object owner, object state)
		{
			((IStreamer)state).Unload();
		}

		public int GetActiveQualityLevel()
		{
			return activeQualityLevel;
		}

		public BatchOctreesStreamer GetOctreesStreamer(int lod)
		{
			if (lod > octreesStreamer.maxLod)
			{
				return lowDetailOctreesStreamer;
			}
			return octreesStreamer;
		}

		public IThread GetBuildLayersThread()
		{
			return buildLayersThread;
		}

		public IThread GetDestroyChunksThread()
		{
			return destroyChunksThread;
		}

		private IEnumerator UpdateCenter(IStreamer streamer, int id, string profilingLabel)
		{
			while (streamer.IsRunning() && id == activeSettingsId)
			{
				streamer.UpdateCenter(streamingCenter);
				yield return null;
			}
		}

		private IEnumerator StreamOctrees(BatchOctreesStreamer streamer)
		{
			while (streamer.IsRunning())
			{
				int num = CalculateNumPerTick(numOctreesPerSecond, isLoading);
				for (int i = 0; i < num; i++)
				{
					if (!streamer.ProcessQueues())
					{
						break;
					}
				}
				yield return null;
			}
		}

		private IEnumerator StreamClipmap(ClipmapStreamer streamer)
		{
			while (streamer.IsRunning())
			{
				int num = CalculateNumPerTick(numClipsPerSecond, isLoading);
				for (int i = 0; i < num; i++)
				{
					if (!streamer.ProcessQueues())
					{
						break;
					}
				}
				yield return null;
			}
		}

		private IEnumerator UpdateVisbility(ClipmapVisibilityUpdater updater)
		{
			while (true)
			{
				int num = CalculateNumPerTick(numCellsPerSecond, isLoading);
				for (int i = 0; i < num; i++)
				{
					if (!updater.ProcessQueue())
					{
						break;
					}
				}
				yield return null;
			}
		}

		private static IEnumerator PumpUnityThread(UnityThread thread, Func<int> numPerFrame)
		{
			while (true)
			{
				thread.Pump(numPerFrame());
				yield return null;
			}
		}

		private static int CalculateNumPerTick(int numPerSecond, bool isLoading)
		{
			return CalculateNumPerDelta(numPerSecond, 0.01f, isLoading);
		}

		private static int CalculateNumPerFrame(int numPerSecond, bool isLoading)
		{
			return CalculateNumPerDelta(numPerSecond, Time.deltaTime, isLoading);
		}

		private static int CalculateNumPerDelta(int numPerSecond, float deltaTime, bool isLoading)
		{
			if (numPerSecond <= 0)
			{
				return 0;
			}
			if (isLoading)
			{
				return int.MaxValue;
			}
			return Mathf.Max((int)((float)numPerSecond * deltaTime), 1);
		}

		private static VoxelandBlockType[] LoadBlockTypes(string paletteResourcePath)
		{
			VoxelandBlockType[] array = Voxeland.LoadPaletteStatic(paletteResourcePath);
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.RuntimeInit(i);
			}
			return array;
		}

		private static LargeWorldStreamer.Settings ParseStreamingSettings(int qualityLevel)
		{
			return JsonUtility.FromJson<LargeWorldStreamer.Settings>(File.ReadAllText(LargeWorldStreamer.GetStreamingSettingsFileForQualityLevel(qualityLevel)));
		}

		private static ClipMapManager.Settings ParseClipmapSettings(int qualityLevel)
		{
			return JsonUtility.FromJson<ClipMapManager.Settings>(File.ReadAllText(ClipMapManager.GetClipMapSettingsFileForQualityLevel(qualityLevel)));
		}

		public void DebugGUI()
		{
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Streaming", GUILayout.Width(60f));
			GUILayout.TextField(streamingThread.GetQueueLength().ToString(), GUILayout.Width(60f));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Streamer", GUILayout.Width(60f));
			GUILayout.Label("Loading", GUILayout.Width(60f));
			GUILayout.Label("Unloading", GUILayout.Width(60f));
			GUILayout.EndHorizontal();
			if (lowDetailOctreesStreamer != null)
			{
				lowDetailOctreesStreamer.OnGUI();
			}
			if (octreesStreamer != null)
			{
				octreesStreamer.OnGUI();
			}
			if (clipmapStreamer != null)
			{
				clipmapStreamer.OnGUI();
			}
			if (visibilityUpdater != null)
			{
				visibilityUpdater.OnGUI();
			}
			GUILayout.BeginHorizontal();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Create", GUILayout.Width(60f));
			GUILayout.Label("Toggle", GUILayout.Width(60f));
			GUILayout.Label("Destroy", GUILayout.Width(60f));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (buildLayersThread != null)
			{
				GUILayout.TextField(buildLayersThread.GetQueueLength().ToString(), GUILayout.Width(60f));
			}
			if (toggleChunksThread != null)
			{
				GUILayout.TextField(toggleChunksThread.GetQueueLength().ToString(), GUILayout.Width(60f));
			}
			if (destroyChunksThread != null)
			{
				GUILayout.TextField(destroyChunksThread.GetQueueLength().ToString(), GUILayout.Width(60f));
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			if (lowDetailOctreesStreamer != null)
			{
				lowDetailOctreesStreamer.DrawGizmos(1f);
			}
			if (octreesStreamer != null)
			{
				octreesStreamer.DrawGizmos(0.5f);
			}
			if (clipmapStreamer != null)
			{
				clipmapStreamer.DrawGizmos();
			}
		}

		private void OnApplicationQuit()
		{
		}
	}
}
