using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Gendarme;
using UWE;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

public class MainGameController : MonoBehaviour
{
	[SerializeField]
	[AssertLocalization]
	private string[] additionalScenes;

	private static MainGameController instance;

	private float lastLookMoveTime;

	private float lastGarbageCollectionTime;

	private int lastGarbageCollectionFrame;

	private int lastFrameGCCount;

	private float pdaOpenTimer;

	private const float timeBeforeCheckingForGarbageCollection1 = 180f;

	private const float timeBeforeCheckingForGarbageCollection2 = 360f;

	private const float timeBeforeForcedGarbageCollection = 600f;

	private const ulong gcTimeSlice = 10000000uL;

	private Stopwatch collectionTimer = new Stopwatch();

	private bool detailedMemoryLog;

	private HashSet<MonoBehaviour> highPrecisionFixedTimestepBehaviors = new HashSet<MonoBehaviour>();

	public static MainGameController Instance => instance;

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void OnDestroy()
	{
		VRUtil.OnRecenter -= ResetOrientation;
		instance = null;
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private IEnumerator Start()
	{
		instance = this;
		DevConsole.RegisterConsoleCommand(this, "togglefixedtimestep");
		return CoroutineUtils.PumpCoroutine(StartGame(), "StartGame", 30f);
	}

	private IEnumerator StartGame()
	{
		_ = Language.main;
		WaitScreen.ManualWaitItem waitItem = WaitScreen.Add("Root");
		waitItem.SetProgress(0f);
		yield return Utils.EnsureLootCubeCreated();
		Physics.autoSyncTransforms = false;
		Physics2D.autoSimulation = false;
		detailedMemoryLog = Environment.GetEnvironmentVariable("SN_DETAILED_MEMLOG") == "1";
		if (detailedMemoryLog && !UnityEngine.Debug.isDebugBuild)
		{
			UnityEngine.Debug.LogWarning("SN_DETAILED_MEMLOG was set, but this is not a debug/dev build. So the detailed mem readings will all be 0.");
		}
		float repeatRate = 60f;
		string environmentVariable = Environment.GetEnvironmentVariable("SN_HEARTBEAT_PERIOD_S");
		if (!string.IsNullOrEmpty(environmentVariable))
		{
			repeatRate = float.Parse(environmentVariable);
		}
		InvokeRepeating("DoHeartbeat", 0f, repeatRate);
		for (int i = 0; i < additionalScenes.Length; i++)
		{
			string text = additionalScenes[i];
			AsyncOperationHandle<SceneInstance> asyncOperationHandle = AddressablesUtility.LoadSceneAsync(text, LoadSceneMode.Additive);
			WaitScreen.AsyncOperationItem sceneWaitItem = WaitScreen.Add("Scene" + text, asyncOperationHandle);
			yield return asyncOperationHandle;
			WaitScreen.Remove(sceneWaitItem);
		}
		while (LightmappedPrefabs.main.IsWaitingOnLoads())
		{
			yield return CoroutineUtils.waitForNextFrame;
		}
		PAXTerrainController main = PAXTerrainController.main;
		if (main != null)
		{
			yield return main.Initialize();
		}
		while (!LargeWorldStreamer.main || !LargeWorldStreamer.main.IsWorldSettled())
		{
			yield return CoroutineUtils.waitForNextFrame;
		}
		WaterBiomeManager.main.Rebuild();
		PerformGarbageAndAssetCollection();
		yield return LoadInitialInventoryAsync();
		Application.backgroundLoadingPriority = ThreadPriority.Normal;
		UpdateFixedTimestep();
		DevConsole.RegisterConsoleCommand(this, "collect");
		DevConsole.RegisterConsoleCommand(this, "endsession");
		VRUtil.OnRecenter += ResetOrientation;
		_ = Player.main;
		bool playIntro = ShouldPlayIntro();
		if (!playIntro)
		{
			WaitScreen.ManualWaitItem waitWorldSettle = WaitScreen.Add("WorldSettle");
			waitWorldSettle.SetProgress(0.5f);
			while (!LargeWorldStreamer.main || !LargeWorldStreamer.main.IsReady() || !LargeWorldStreamer.main.IsWorldSettled())
			{
				yield return new WaitForSecondsRealtime(1f);
			}
			OnIntroDone();
			MainMenuMusic.Stop();
			VRLoadingOverlay.Hide();
			WaitScreen.Remove(waitWorldSettle);
			if (!Utils.GetContinueMode())
			{
				EscapePod.main.StopIntroCinematic(isInterrupted: true);
			}
		}
		waitItem.SetProgress(1f);
		WaitScreen.Remove(waitItem);
		if (playIntro)
		{
			uGUI.main.intro.Play(OnIntroDone);
		}
	}

	public static bool ShouldPlayIntro()
	{
		if (GameModeUtils.SpawnsInitialItems())
		{
			return false;
		}
		if (SNUtils.IsSmokeTesting())
		{
			return false;
		}
		if (Utils.GetContinueMode())
		{
			return false;
		}
		return true;
	}

	private void OnIntroDone()
	{
		if (GameModeUtils.SpawnsInitialItems())
		{
			Player.main.SetupCreativeMode();
		}
	}

	private void OnConsoleCommand_endsession()
	{
		UnityEngine.Debug.Log("endsession cmd");
	}

	public bool CanPerformAutoGarbageCollection()
	{
		if (WaitScreen.IsWaiting)
		{
			return false;
		}
		return Time.time > lastGarbageCollectionTime + 180f;
	}

	private void UpdateAutoGarbageCollection()
	{
		if (!CanPerformAutoGarbageCollection())
		{
			return;
		}
		float time = Time.time;
		if (GameInput.GetLookDelta().sqrMagnitude > 0.1f)
		{
			lastLookMoveTime = time;
		}
		if (time > lastGarbageCollectionTime + 600f)
		{
			PerformIncrementalGarbageCollection();
		}
		else if (time > lastGarbageCollectionTime + 360f)
		{
			if (time > lastLookMoveTime + 0.5f)
			{
				PerformIncrementalGarbageCollection();
			}
		}
		else if (time > lastGarbageCollectionTime + 180f && Player.main.GetPDA().isOpen)
		{
			pdaOpenTimer += PDA.deltaTime;
			if (pdaOpenTimer > 0.5f)
			{
				PerformIncrementalGarbageCollection();
				pdaOpenTimer = 0f;
			}
		}
	}

	public void PerformGarbageAndAssetCollection()
	{
		StartCoroutine(PerformGarbageAndAssetCollectionAsync());
	}

	public void PerformIncrementalGarbageCollection()
	{
		StartCoroutine(PerformIncrementalGarbageCollectionAsync());
	}

	public IEnumerator PerformGarbageAndAssetCollectionAsync()
	{
		UnityEngine.Debug.LogFormat("PerformGarbageAndAssetCollection, Time.time={0}, Time.frameCount={1}, DateTime.Now={2}", Time.time, Time.frameCount, DateTime.Now.ToString(CultureInfo.InvariantCulture));
		return PerformGarbageAndAssetCollectionAsyncInternal();
	}

	private IEnumerator PerformGarbageAndAssetCollectionAsyncInternal()
	{
		collectionTimer.Restart();
		yield return PerformIncrementalGarbageCollectionAsync();
		collectionTimer.Stop();
		float gcTime = UWE.Utils.GetTimeElapsedMS(collectionTimer);
		yield return CoroutineUtils.waitForNextFrame;
		collectionTimer.Restart();
		yield return PrefabDatabase.UnloadUnusedAssets();
		collectionTimer.Stop();
		float timeElapsedMS = UWE.Utils.GetTimeElapsedMS(collectionTimer);
		UnityEngine.Debug.LogFormat("--- PerformGarbageAndAssetCollectionAsync: GC Time {0} Asset GC Time {1}", gcTime.ToString(), timeElapsedMS.ToString());
	}

	private IEnumerator PerformIncrementalGarbageCollectionAsync()
	{
		Timer.Begin("PerformGarbageCollection -> GC.Collect");
		while (GarbageCollector.CollectIncremental(10000000uL))
		{
			NotifyGarbageCollected();
			yield return CoroutineUtils.waitForNextFrame;
		}
		Timer.End();
		NotifyGarbageCollected();
	}

	public void NotifyGarbageCollected()
	{
		lastGarbageCollectionTime = Time.time;
		lastGarbageCollectionFrame = Time.frameCount;
	}

	public bool HasGarbageCollectedThisFrame()
	{
		return lastGarbageCollectionFrame == Time.frameCount;
	}

	private IEnumerator LoadInitialInventoryAsync()
	{
		if (GameModeUtils.SpawnsInitialItems())
		{
			WaitScreen.ManualWaitItem waitItem = WaitScreen.Add("Equipment");
			int numTotal = Player.creativeEquipment.Length;
			int i = 0;
			Player.InitialEquipment[] creativeEquipment = Player.creativeEquipment;
			for (int j = 0; j < creativeEquipment.Length; j++)
			{
				Player.InitialEquipment initialEquipment = creativeEquipment[j];
				waitItem.SetProgress(i++, numTotal);
				yield return CraftData.GetPrefabForTechTypeAsync(initialEquipment.techType);
			}
			WaitScreen.Remove(waitItem);
		}
	}

	private void OnConsoleCommand_collect()
	{
		PerformGarbageAndAssetCollection();
	}

	private void Update()
	{
		if (uGUI.main == null || GameApplication.isQuitting)
		{
			return;
		}
		if (GC.CollectionCount(0) != lastFrameGCCount)
		{
			NotifyGarbageCollected();
		}
		UpdateAutoGarbageCollection();
		AddressablesUtility.Update();
		lastFrameGCCount = GC.CollectionCount(0);
		if (UnityEngine.Debug.isDebugBuild && Input.GetKeyDown(KeyCode.F5) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
		{
			if (!Profiler.enabled)
			{
				Profiler.logFile = "profiling-" + Time.frameCount + ".log";
				Profiler.enableBinaryLog = true;
				Profiler.enabled = true;
				UnityEngine.Debug.Log("Started profiling, writing to " + Profiler.logFile);
			}
			else
			{
				Profiler.enabled = false;
				Profiler.enableBinaryLog = false;
				Profiler.logFile = null;
				UnityEngine.Debug.Log("Stopped profiling");
			}
		}
		if (Input.GetKeyDown(KeyCode.F1) || (GameInput.GetButtonHeld(GameInput.Button.PDA) && GameInput.GetButtonDown(GameInput.Button.Reload)))
		{
			TerrainDebugGUI[] array = UnityEngine.Object.FindObjectsOfType<TerrainDebugGUI>();
			foreach (TerrainDebugGUI obj in array)
			{
				obj.enabled = !obj.enabled;
			}
		}
		if (Input.GetKeyDown(KeyCode.F3))
		{
			GraphicsDebugGUI[] array2 = UnityEngine.Object.FindObjectsOfType<GraphicsDebugGUI>();
			foreach (GraphicsDebugGUI graphicsDebugGUI in array2)
			{
				if (graphicsDebugGUI != null)
				{
					graphicsDebugGUI.enabled = !graphicsDebugGUI.enabled;
				}
			}
		}
		if (!Cursor.visible && Cursor.lockState == CursorLockMode.None)
		{
			Cursor.visible = true;
		}
		MiscSettings.Update();
	}

	private long CountTotalBytesUsedByResource<T>() where T : UnityEngine.Object
	{
		long num = 0L;
		T[] array = Resources.FindObjectsOfTypeAll<T>();
		foreach (T o in array)
		{
			num += Profiler.GetRuntimeMemorySize(o);
		}
		return num;
	}

	private void DoHeartbeat()
	{
		CellManager cellManager = LargeWorldStreamer.main.cellManager;
		Vector3 vector = Vector3.zero;
		if ((bool)Player.main)
		{
			vector = Player.main.transform.position;
		}
		string text = "";
		if (detailedMemoryLog)
		{
			text = ", totalMeshMBs," + (float)CountTotalBytesUsedByResource<Mesh>() / 1024f / 1024f + ", totalTextureMBs," + (float)CountTotalBytesUsedByResource<Texture>() / 1024f / 1024f;
		}
		UnityEngine.Debug.Log("Heartbeat CSV, time s," + Time.time + ", GC.GetTotalMemory MB," + (float)GC.GetTotalMemory(forceFullCollection: false) / 1024f / 1024f + ", OctNodes MB," + (float)VoxelandData.OctNode.GetPoolBytesTotal() / 1024f / 1024f + ", CompactOctrees MB," + (float)LargeWorldStreamer.main.EstimateCompactOctreeBytes() / 1024f / 1024f + ", CellManager MB," + (float)(cellManager?.EstimateBytes() ?? 0) / 1024f / 1024f + ", ClipMapManager MB," + (float)LargeWorldStreamer.main.EstimateClipMapManagerBytes() / 1024f / 1024f + ", GCCount," + GC.CollectionCount(0) + ", PlayerPos," + vector.x + "," + vector.y + "," + vector.z + text);
	}

	public void ResetOrientation()
	{
		MainCameraControl.main.rotationY = 0f;
	}

	public void RegisterHighFixedTimestepBehavior(MonoBehaviour behaviour)
	{
		if (highPrecisionFixedTimestepBehaviors.Add(behaviour))
		{
			UpdateFixedTimestep();
		}
	}

	public void DeregisterHighFixedTimestepBehavior(MonoBehaviour behaviour)
	{
		if (highPrecisionFixedTimestepBehaviors.Remove(behaviour))
		{
			UpdateFixedTimestep();
		}
	}

	private void UpdateFixedTimestep()
	{
		if (highPrecisionFixedTimestepBehaviors.Count == 0)
		{
			Time.fixedDeltaTime = 0.1f;
		}
		else
		{
			Time.fixedDeltaTime = 0.02f;
		}
	}

	private void OnConsoleCommand_togglefixedtimestep()
	{
		if (highPrecisionFixedTimestepBehaviors.Contains(this))
		{
			DeregisterHighFixedTimestepBehavior(this);
		}
		else
		{
			RegisterHighFixedTimestepBehavior(this);
		}
		ErrorMessage.AddMessage($"Updated fixed timestep behaviors. Count = {highPrecisionFixedTimestepBehaviors.Count}.");
	}
}
