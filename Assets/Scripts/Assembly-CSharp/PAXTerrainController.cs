using System;
using System.Collections;
using System.IO;
using UWE;
using UnityEngine;
using WorldStreaming;

public sealed class PAXTerrainController : MonoBehaviour
{
	public static PAXTerrainController main;

	public TerrainDebugGUI debugGUI;

	private static int Version = 1;

	public bool debugAlwaysOverwriteWorld;

	public bool debugNotThreaded;

	public bool debugSkipSlots;

	[HideInInspector]
	public string dataDirPath;

	[AssertNotNull]
	[SerializeField]
	private Voxeland land;

	[AssertNotNull]
	[SerializeField]
	private LargeWorldStreamer streamer;

	[AssertNotNull]
	[SerializeField]
	private WorldStreamer streamerV2;

	private LargeWorld _worldCache;

	public bool loadOnlySafeShallows;

	private LargeWorld world
	{
		get
		{
			if (_worldCache == null)
			{
				_worldCache = GetComponent<LargeWorld>();
			}
			return _worldCache;
		}
	}

	private void Awake()
	{
		main = this;
		land.freeze = true;
		if (File.Exists(SNUtils.BuildNumberFile))
		{
			using (StreamReader streamReader = FileUtils.ReadTextFile(SNUtils.BuildNumberFile))
			{
				Version = int.Parse(streamReader.ReadLine());
			}
		}
	}

	private void Start()
	{
		if (debugGUI != null)
		{
			debugGUI.enabled = false;
		}
		DevConsole.RegisterConsoleCommand(this, "rebuild");
		DevConsole.RegisterConsoleCommand(this, "region");
		DevConsole.RegisterConsoleCommand(this, "batch");
		DevConsole.RegisterConsoleCommand(this, "biome");
		DevConsole.RegisterConsoleCommand(this, "chunk");
		DevConsole.RegisterConsoleCommand(this, "printbiome");
	}

	public IEnumerator Initialize()
	{
		Debug.LogFormat("PAXTerrainController::Initialize called - frame = {0}", Time.frameCount);
		dataDirPath = SNUtils.InsideUnmanaged("Build18");
		yield return LoadAsync();
	}

	private void GotoBatch(Int3 index)
	{
		if (!streamer.IsReady())
		{
			ErrorMessage.AddDebug("world not ready yet..");
			return;
		}
		int x = streamer.blocksPerBatch.x;
		Int3 @int = index * x + new Int3(x / 2, x - 3, x / 2);
		Vector3 position = land.transform.TransformPoint(@int.ToVector3());
		Utils.GetLocalPlayerComp().SetPosition(position);
		Utils.GetLocalPlayerComp().OnPlayerPositionCheat();
	}

	private void OnConsoleCommand_batch(NotificationCenter.Notification n)
	{
		string text = (string)n.data[0];
		string text2 = (string)n.data[1];
		string text3 = (string)n.data[2];
		Debug.Log("batch command args: " + text + ", " + text2 + ", " + text3);
		GotoBatch(new Int3(int.Parse(text), int.Parse(text2), int.Parse(text3)));
	}

	private void OnConsoleCommand_printbiome()
	{
		Player player = Player.main;
		LargeWorld largeWorld = LargeWorld.main;
		Vector3 position = player.transform.position;
		Int3 @int = Int3.Floor(largeWorld.land.transform.InverseTransformPoint(position));
		ErrorMessage.AddDebug(player.GetBiomeString());
		for (int i = -100; i <= 100; i += 10)
		{
			for (int j = -100; j <= 100; j += 10)
			{
				GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
				UnityEngine.Object.Destroy(obj.GetComponent<Collider>());
				obj.transform.position = new Vector3(position.x + (float)i, position.y - 10f, position.z + (float)j);
				obj.transform.localScale = Vector3.one * 0.8f;
				obj.GetComponent<Renderer>().material.color = Color.HSVToRGB((float)largeWorld.DebugBiomeMap(@int.xz + new Int2(i, j)) * 0.003921569f, 0.75f, 0.75f);
				UnityEngine.Object.Destroy(obj, 10f);
			}
		}
	}

	private void OnConsoleCommand_chunk(NotificationCenter.Notification n)
	{
		string text = (string)n.data[0];
		string text2 = (string)n.data[1];
		string text3 = (string)n.data[2];
		if (!streamer.IsReady())
		{
			ErrorMessage.AddDebug("world not ready yet..");
			return;
		}
		Debug.Log("chunk command args: " + text + ", " + text2 + ", " + text3);
		Int3 @int = new Int3(int.Parse(text), int.Parse(text2), int.Parse(text3));
		int chunkSize = land.chunkSize;
		Int3 int2 = @int * chunkSize + new Int3(chunkSize / 2, chunkSize - 3, chunkSize / 2);
		Vector3 position = land.transform.TransformPoint(int2.ToVector3());
		Utils.GetLocalPlayerComp().SetPosition(position);
	}

	private IEnumerator LoadAsync()
	{
		UWE.Utils.EnterPhysicsSyncSection();
		yield return PreinitBase();
		yield return uGUI_BuilderMenu.EnsureCreatedAsync();
		streamerV2.isLoading = true;
		TaskResult<Result> mountResult = new TaskResult<Result>();
		yield return MountWorld(mountResult);
		if (mountResult.Get().success)
		{
			Debug.Log("Loading world tiles");
			yield return LoadWorldTiles();
			Debug.Log("Loading cells");
			yield return Await("Octrees", streamerV2.lowDetailOctreesStreamer.IsIdle, streamerV2.lowDetailOctreesStreamer.GetQueueLength, 1000);
			yield return Await("Terrain", streamerV2.octreesStreamer.IsIdle, streamerV2.octreesStreamer.GetQueueLength, 100);
			yield return Await("Clipmap", streamerV2.clipmapStreamer.IsIdle, streamerV2.clipmapStreamer.GetQueueLength, 1000);
			yield return Await("UpdatingVisibility", streamerV2.visibilityUpdater.IsIdle, streamerV2.visibilityUpdater.GetQueueLength, 1000);
			streamer.frozen = false;
			yield return Await("EntityCells", streamer.IsWorldSettled, streamer.cellManager.GetQueueLength, 1000);
			Debug.Log("LoadAsync Done");
			UWE.Utils.ExitPhysicsSyncSection();
			Debug.Log("LOADING FINISHED");
			streamerV2.isLoading = false;
		}
	}

	private IEnumerator PreinitBase()
	{
		WaitScreen.ManualWaitItem waitItem = WaitScreen.Add("Builder");
		waitItem.SetProgress(0f);
		yield return Builder.InitializeAsync();
		yield return Base.InitializeAsync();
		yield return WaterPark.InitializeAsync();
		waitItem.SetProgress(1f);
		WaitScreen.Remove(waitItem);
	}

	private IEnumerator MountWorld(IOut<Result> result)
	{
		Debug.LogFormat(this, "PAXTerrainController::MountWorld frame {0}", Time.frameCount);
		WaitScreen.ManualWaitItem waitItem = WaitScreen.Add("WorldMount");
		yield return CoroutineUtils.waitForNextFrame;
		Debug.Log("builders ready, building world..");
		Debug.LogFormat(this, "loading static world at '{0}'.", dataDirPath);
		yield return CoroutineUtils.waitForNextFrame;
		waitItem.SetProgress(1, 10);
		CoroutineTask<Result> task = world.MountWorldAsync(dataDirPath, streamer, streamerV2, land);
		yield return task;
		Result result2 = task.GetResult();
		waitItem.SetProgress(10, 10);
		if (result2.success)
		{
			Debug.LogFormat(this, "OK world mounted: {0}", dataDirPath);
			streamer.frozen = true;
			WaitScreen.Remove(waitItem);
		}
		else
		{
			Debug.LogErrorFormat(this, "Fatal! Could not mount world, dir = '{0}'", dataDirPath);
			string text = result2.error;
			Language language = Language.main;
			if ((bool)language)
			{
				text = language.Get(text);
			}
			WaitScreen.Add(text);
		}
		result.Set(result2);
	}

	private IEnumerator LoadWorldTiles()
	{
		WaitScreen.ManualWaitItem waitItem = WaitScreen.Add("WorldTiles");
		yield return CoroutineUtils.waitForNextFrame;
		Vector3 vector = new Vector3(0f, -8f, 0f);
		Vector3 wsPos = MainCamera.camera.transform.position + vector;
		Int3 containingBatch = streamer.GetContainingBatch(wsPos);
		Int3.Bounds bounds = new Int3.Bounds(containingBatch - 1, containingBatch + 1);
		int numBatches = bounds.size.Product();
		Debug.LogFormat(this, "cam batch {0}, loading bounds {1}, total {2}", containingBatch, bounds, numBatches);
		int i = 0;
		Debug.LogFormat(this, "loading {0} batches", numBatches);
		foreach (Int3 bid in bounds)
		{
			waitItem.SetProgress(i++, numBatches);
			if (streamer.CheckBatch(bid))
			{
				BatchCells batchCells = streamer.cellManager.InitializeBatchCells(bid);
				yield return streamer.LoadBatchTaskedAsync(batchCells, _editMode: false);
				yield return streamer.FinalizeLoadBatchAsync(bid, _editMode: false);
			}
		}
		WaitScreen.Remove(waitItem);
	}

	private static IEnumerator Await(string stage, Func<bool> isIdle, Func<int> getQueueLength, int upperBound)
	{
		WaitScreen.ManualWaitItem waitItem = WaitScreen.Add(stage);
		while (!isIdle())
		{
			int num = getQueueLength();
			waitItem.SetProgress(upperBound, upperBound + num);
			yield return CoroutineUtils.waitForNextFrame;
		}
		WaitScreen.Remove(waitItem);
	}

	public void LayoutDebugGUI()
	{
		Vector3 position = MainCamera.camera.transform.position;
		Int3 @int = Int3.Floor(land.transform.InverseTransformPoint(position));
		GUILayout.Label(string.Concat("Camera in voxel ", @int, "\nworld pos: ", position));
		if (streamer != null && streamer.IsReady())
		{
			GUILayout.Label("Batch " + @int / streamer.blocksPerBatch);
		}
		if (GetComponent<LargeWorld>() != null)
		{
			GUILayout.Label("Biome " + GetComponent<LargeWorld>().GetBiome(@int));
		}
		GUILayout.Label("World version number: " + Version);
		GUILayout.Label("Active world source: " + dataDirPath);
	}
}
