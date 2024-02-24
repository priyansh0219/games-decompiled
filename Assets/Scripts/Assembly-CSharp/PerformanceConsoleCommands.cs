using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Gendarme;
using UWE;
using UnityEngine;
using UnityEngine.Profiling;
using UnityStandardAssets.ImageEffects;

public class PerformanceConsoleCommands : MonoBehaviour
{
	public class Stats
	{
		public string label;

		public int numDrawCalls;

		public int numEntInsts;

		public int numRigidbodies;

		public int numKMRigidbodies;

		public Stats(string label)
		{
			this.label = label;
		}

		public string ToCSVLine(string name)
		{
			return name + "," + numEntInsts + "," + numDrawCalls + "," + numRigidbodies + "," + numKMRigidbodies + "," + (float)numDrawCalls * 1f / (float)numEntInsts + ",";
		}

		public static string GetCSVHeader()
		{
			return "objectName,numEntInsts,num draw calls,num RBs, num Kinematic RBs,DCs per ent";
		}

		public static int CompareByDrawCallsDesc(Stats a, Stats b)
		{
			if (a.numDrawCalls < b.numDrawCalls)
			{
				return 1;
			}
			if (a.numDrawCalls == b.numDrawCalls)
			{
				return 0;
			}
			return -1;
		}
	}

	public GameObject sizeRefPrefab;

	public GameObject seaGlidePrefab;

	public bool includeRandomVariantsInLootGrid;

	private static int runCount;

	private static string currentPhototourId;

	private static string currentPhototourFilename;

	private static bool isProfiling;

	private static bool recordingPhototour;

	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "perf");
		DevConsole.RegisterConsoleCommand(this, "noshadows");
		DevConsole.RegisterConsoleCommand(this, "nobloom");
		DevConsole.RegisterConsoleCommand(this, "tsram");
		DevConsole.RegisterConsoleCommand(this, "tsgui");
		DevConsole.RegisterConsoleCommand(this, "sizeref");
		DevConsole.RegisterConsoleCommand(this, "seaglide");
		DevConsole.RegisterConsoleCommand(this, "lootgrid");
		DevConsole.RegisterConsoleCommand(this, "osgui");
		DevConsole.RegisterConsoleCommand(this, "vsync");
		DevConsole.RegisterConsoleCommand(this, "targetframerate");
		DevConsole.RegisterConsoleCommand(this, "gcc");
		DevConsole.RegisterConsoleCommand(this, "limitshadows");
		DevConsole.RegisterConsoleCommand(this, "animculloff");
		DevConsole.RegisterConsoleCommand(this, "animstat");
		DevConsole.RegisterConsoleCommand(this, "octrees");
		DevConsole.RegisterConsoleCommand(this, "rigids");
		DevConsole.RegisterConsoleCommand(this, "trace");
		DevConsole.RegisterConsoleCommand(this, "viewmodels");
		DevConsole.RegisterConsoleCommand(this, "dcstats");
		DevConsole.RegisterConsoleCommand(this, "rbdist");
		DevConsole.RegisterConsoleCommand(this, "killent");
		DevConsole.RegisterConsoleCommand(this, "shaderkw");
		DevConsole.RegisterConsoleCommand(this, "spawnperf");
		DevConsole.RegisterConsoleCommand(this, "fog");
		DevConsole.RegisterConsoleCommand(this, "ecostats");
		DevConsole.RegisterConsoleCommand(this, "drones");
		DevConsole.RegisterConsoleCommand(this, "spark");
		DevConsole.RegisterConsoleCommand(this, "resstats");
		DevConsole.RegisterConsoleCommand(this, "dblit");
		DevConsole.RegisterConsoleCommand(this, "testexception");
		DevConsole.RegisterConsoleCommand(this, "pcannon");
		DevConsole.RegisterConsoleCommand(this, "phototour");
		DevConsole.RegisterConsoleCommand(this, "stopphototour");
		DevConsole.RegisterConsoleCommand(this, "photoprofile");
		DevConsole.RegisterConsoleCommand(this, "stopwatch");
		DevConsole.RegisterConsoleCommand(this, "abflip");
		DevConsole.RegisterConsoleCommand(this, "recordtour");
		DevConsole.RegisterConsoleCommand(this, "stoprecording");
		DevConsole.RegisterConsoleCommand(this, "disablepoolPurge");
		DevConsole.RegisterConsoleCommand(this, "enablepoolpurge");
		DevConsole.RegisterConsoleCommand(this, "dumpgameobjectpool");
	}

	private void OnConsoleCommand_viewmodels()
	{
		UnityEngine.Debug.Log("-------- Listing all viewmodel layer objects with renderers -----");
		int num = LayerMask.NameToLayer("Viewmodel");
		GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (gameObject.layer == num && gameObject.GetComponent<Renderer>() != null)
			{
				UnityEngine.Debug.Log(gameObject.GetFullHierarchyPath());
			}
		}
	}

	private void OnConsoleCommand_trace()
	{
		int num = UWE.Utils.RaycastIntoSharedBuffer(MainCamera.camera.transform.position, MainCamera.camera.transform.forward);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
			UnityEngine.Debug.Log(raycastHit.collider.gameObject.GetFullHierarchyPath());
		}
	}

	private void OnConsoleCommand_rigids()
	{
		int num = 0;
		int num2 = 0;
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(Rigidbody));
		for (int i = 0; i < array.Length; i++)
		{
			Rigidbody obj = (Rigidbody)array[i];
			num++;
			if (!obj.isKinematic)
			{
				num2++;
			}
		}
		UnityEngine.Debug.Log("Total RBs: " + num + ", non kinematic: " + num2);
	}

	private void OnConsoleCommand_octrees()
	{
		int num = 0;
		int num2 = 0;
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(VoxelandData));
		for (int i = 0; i < array.Length; i++)
		{
			VoxelandData voxelandData = (VoxelandData)array[i];
			num++;
			num2 += voxelandData.EstimateOctreeBytes();
		}
		UnityEngine.Debug.Log(num + " VoxelandData's, total size = " + (float)num2 / 1024f / 1024f + " MB");
	}

	public static int ReportNumberOf<T>() where T : Behaviour
	{
		T[] array = UnityEngine.Object.FindObjectsOfType(typeof(T)) as T[];
		UnityEngine.Debug.Log("Scene contains " + array.Length + " " + typeof(T).Name + " instances");
		if (array.Length != 0)
		{
			UnityEngine.Debug.Log("First instance: " + array[0].gameObject.name);
		}
		return array.Length;
	}

	private void OnConsoleCommand_animstat()
	{
		HashSet<string> hashSet = new HashSet<string>();
		HashSet<string> hashSet2 = new HashSet<string>();
		Animator[] array = UnityEngine.Object.FindObjectsOfType<Animator>();
		foreach (Animator animator in array)
		{
			if (animator.cullingMode == AnimatorCullingMode.AlwaysAnimate)
			{
				hashSet.Add(animator.gameObject.name);
			}
			if (animator.runtimeAnimatorController == null)
			{
				hashSet2.Add(animator.gameObject.name);
			}
		}
		UnityEngine.Debug.Log("-- AlwaysAnimate set --");
		foreach (string item in hashSet)
		{
			UnityEngine.Debug.Log(item);
		}
		UnityEngine.Debug.Log("-- No controllers set --");
		foreach (string item2 in hashSet2)
		{
			UnityEngine.Debug.Log(item2);
		}
	}

	private void OnConsoleCommand_animculloff()
	{
		int num = 0;
		Animator animator = null;
		Animator[] array = UnityEngine.Object.FindObjectsOfType<Animator>();
		foreach (Animator animator2 in array)
		{
			if (animator2.cullingMode == AnimatorCullingMode.AlwaysAnimate)
			{
				num++;
				if (animator == null)
				{
					animator = animator2;
				}
			}
			else
			{
				animator2.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			}
		}
		UnityEngine.Debug.Log("NOTE: " + num + " animators were already ALWAYS ANIMATING. first one = " + ((animator == null) ? "NONE" : animator.gameObject.name));
	}

	private void OnConsoleCommand_perf()
	{
		UnityEngine.Debug.Log("-------------- BEGIN PERF STATS ------------");
		UnityEngine.Debug.Log("REMEMBER: Always check for console spam, and that your Scene view is hidden.");
		if (ReportNumberOf<Camera>() > 5)
		{
			UnityEngine.Debug.LogError("There are more than 5 cameras in your scene. Necessary?");
		}
		if (ReportNumberOf<Animator>() > 50)
		{
			UnityEngine.Debug.LogError("There are more than 50 animators in your scene. Could be slow.");
		}
		int num = 0;
		Light[] array = UnityEngine.Object.FindObjectsOfType(typeof(Light)) as Light[];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].renderMode != LightRenderMode.ForceVertex)
			{
				num++;
			}
		}
		UnityEngine.Debug.Log("Num auto or important (ie. pixel) lights: " + num);
		if (num > 3)
		{
			UnityEngine.Debug.LogError("There are more than 3 pixel lights in your scene. This can really blow out your draw call count.");
		}
	}

	private void OnConsoleCommand_noshadows()
	{
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(Light));
		for (int i = 0; i < array.Length; i++)
		{
			((Light)array[i]).shadows = LightShadows.None;
		}
	}

	private void OnConsoleCommand_nobloom()
	{
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(Bloom));
		for (int i = 0; i < array.Length; i++)
		{
			((Bloom)array[i]).enabled = false;
		}
	}

	private void OnConsoleCommand_osgui()
	{
		LargeWorldStreamerGUI component = Landscape.main.GetComponent<LargeWorldStreamerGUI>();
		component.enabled = !component.enabled;
	}

	private void OnConsoleCommand_sizeref()
	{
		Transform transform = MainCamera.camera.transform;
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(transform.position + transform.forward, transform.forward, out hitInfo))
		{
			Utils.SpawnPrefabAt(sizeRefPrefab, null, hitInfo.point).transform.up = hitInfo.normal;
			UnityEngine.Debug.Log("Hit " + hitInfo.collider.gameObject.name);
		}
		else
		{
			ErrorMessage.AddDebug("Did not raycast anything");
		}
	}

	private void OnConsoleCommand_seaglide()
	{
		Transform transform = MainCamera.camera.transform;
		RaycastHit hitInfo = default(RaycastHit);
		Vector3 pos = transform.position + 10f * transform.forward;
		if (Physics.Raycast(transform.position + transform.forward, transform.forward, out hitInfo))
		{
			pos = hitInfo.point;
		}
		Utils.SpawnPrefabAt(seaGlidePrefab, null, pos);
	}

	private void OnConsoleCommand_vsync(NotificationCenter.Notification n)
	{
		int num = 0;
		num = ((n == null || n.data == null || n.data.Count <= 0) ? ((QualitySettings.vSyncCount <= 0) ? 1 : 0) : int.Parse((string)n.data[0]));
		ErrorMessage.AddDebug("vsync now " + num);
		QualitySettings.vSyncCount = num;
	}

	private void OnConsoleCommand_targetframerate(NotificationCenter.Notification n)
	{
		int frameRate = GraphicsUtil.GetFrameRate();
		if (n != null && n.data != null && n.data.Count > 0)
		{
			frameRate = int.Parse((string)n.data[0]);
		}
		GraphicsUtil.SetFrameRate(frameRate);
		ErrorMessage.AddDebug("targetFrameRate now " + GraphicsUtil.GetFrameRate());
	}

	private void OnConsoleCommand_gcc()
	{
		GC.Collect();
	}

	private void OnConsoleCommand_limitshadows(NotificationCenter.Notification n)
	{
		int num = int.Parse((string)n.data[0]);
		Vector3 position = MainCamera.camera.transform.position;
		UnityEngine.Debug.Log("limiting shadow casters to " + num + " m from camera");
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(Renderer));
		for (int i = 0; i < array.Length; i++)
		{
			Renderer obj = (Renderer)array[i];
			obj.castShadows = Vector3.Distance(obj.transform.position, position) < (float)num;
		}
	}

	private void OnConsoleCommand_dcstats()
	{
		Dictionary<string, Stats> dictionary = CollectStats();
		int num = 0;
		int num2 = 0;
		using (StreamWriter streamWriter = FileUtils.CreateTextFile("dcstats.csv"))
		{
			streamWriter.WriteLine(Stats.GetCSVHeader());
			foreach (KeyValuePair<string, Stats> item in dictionary)
			{
				streamWriter.WriteLine(item.Value.ToCSVLine(item.Key));
				num += item.Value.numEntInsts;
				num2 += item.Value.numDrawCalls;
			}
		}
		UnityEngine.Debug.Log("total ents = " + num + ", total DCs = " + num2);
	}

	public static Dictionary<string, Stats> CollectStats(HashSet<LargeWorldEntity> visEntsOut = null, bool groupTerrainByChunk = false)
	{
		Dictionary<string, Stats> dictionary = new Dictionary<string, Stats>();
		HashSet<LargeWorldEntity> hashSet = null;
		hashSet = ((visEntsOut == null) ? new HashSet<LargeWorldEntity>() : visEntsOut);
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(Renderer));
		for (int i = 0; i < array.Length; i++)
		{
			Renderer renderer = (Renderer)array[i];
			if (!renderer.enabled || !renderer.isVisible)
			{
				continue;
			}
			string text = renderer.gameObject.name;
			LargeWorldEntity largeWorldEntity = renderer.gameObject.FindAncestor<LargeWorldEntity>();
			if (largeWorldEntity != null)
			{
				text = largeWorldEntity.gameObject.name;
				if (!hashSet.Contains(largeWorldEntity))
				{
					hashSet.Add(largeWorldEntity);
				}
			}
			Voxeland voxeland = renderer.gameObject.FindAncestor<Voxeland>();
			if (voxeland != null)
			{
				text = ((!groupTerrainByChunk) ? voxeland.gameObject.GetFullHierarchyPath() : renderer.transform.parent.gameObject.name);
			}
			if (!dictionary.ContainsKey(text))
			{
				dictionary[text] = new Stats(text);
			}
			dictionary[text].numDrawCalls += renderer.sharedMaterials.Length;
		}
		foreach (LargeWorldEntity item in hashSet)
		{
			string text2 = item.gameObject.name;
			if (!dictionary.ContainsKey(text2))
			{
				dictionary[text2] = new Stats(text2);
			}
			dictionary[text2].numEntInsts++;
		}
		return dictionary;
	}

	private void OnConsoleCommand_killent(NotificationCenter.Notification n)
	{
		string value = (string)n.data[0];
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(LargeWorldEntity));
		for (int i = 0; i < array.Length; i++)
		{
			LargeWorldEntity largeWorldEntity = (LargeWorldEntity)array[i];
			if (largeWorldEntity.gameObject.name.Contains(value))
			{
				UnityEngine.Object.Destroy(largeWorldEntity.gameObject);
			}
		}
	}

	private void OnConsoleCommand_shaderkw(NotificationCenter.Notification n)
	{
		string text = (string)n.data[0];
		if (text[0] == '-')
		{
			Shader.DisableKeyword(text.Substring(1).ToUpper());
		}
		else
		{
			Shader.EnableKeyword(text.ToUpper());
		}
	}

	private IEnumerator OnConsoleCommand_spawnperf(NotificationCenter.Notification n)
	{
		string filter = (string)n.data[0];
		int count = int.Parse((string)n.data[1]);
		Utils.GetLocalPlayerComp().SetPosition(new Vector3(0f, -5f, 0f));
		List<KeyValuePair<string, string>> list = PrefabDatabase.prefabFiles.ToList();
		foreach (KeyValuePair<string, string> item in list)
		{
			string key = item.Key;
			string fname = item.Value;
			if (fname.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
			{
				continue;
			}
			IPrefabRequest request = PrefabDatabase.GetPrefabAsync(key);
			yield return request;
			if (request.TryGetPrefab(out var prefab))
			{
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Restart();
				for (int i = 0; i < count; i++)
				{
					Vector3 position = new Vector3(0f, -5f, 10f) + UnityEngine.Random.insideUnitSphere * 5f;
					UnityEngine.Object.Instantiate(prefab, position, prefab.transform.rotation);
				}
				UnityEngine.Debug.Log(fname + " took " + (float)stopwatch.ElapsedMilliseconds * 1f / (float)count + " ms/spawn");
				yield return null;
			}
		}
	}

	private void OnConsoleCommand_fog()
	{
		WaterscapeVolume[] array = UnityEngine.Object.FindObjectsOfType<WaterscapeVolume>();
		foreach (WaterscapeVolume obj in array)
		{
			obj.enabled = !obj.enabled;
		}
	}

	private void LogTable(Dictionary<string, int> table)
	{
		int num = 0;
		foreach (KeyValuePair<string, int> item in table)
		{
			num += item.Value;
			UnityEngine.Debug.Log(item.Key + " -> " + item.Value);
		}
		UnityEngine.Debug.Log("     Total: " + num);
	}

	private void OnConsoleCommand_ecostats()
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
		Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
		LargeWorldEntity[] array = UnityEngine.Object.FindObjectsOfType<LargeWorldEntity>();
		foreach (LargeWorldEntity largeWorldEntity in array)
		{
			Rigidbody component = largeWorldEntity.GetComponent<Rigidbody>();
			Dictionary<string, int> dictionary4 = dictionary;
			dictionary4 = ((!(component != null)) ? dictionary3 : ((!component.isKinematic) ? dictionary : dictionary2));
			dictionary4[largeWorldEntity.gameObject.name] = dictionary4.GetOrDefault(largeWorldEntity.gameObject.name, 0) + 1;
		}
		UnityEngine.Debug.Log("-- Dynamic rigidbody ents --");
		LogTable(dictionary);
		UnityEngine.Debug.Log("-- Kinematic rigidbody ents --");
		LogTable(dictionary2);
		UnityEngine.Debug.Log("-- Non-rigidbody ents --");
		LogTable(dictionary3);
	}

	[SuppressMessage("Gendarme.Rules.Portability", "DoNotHardcodePathsRule")]
	private IEnumerator OnConsoleCommand_drones()
	{
		string filename = "WorldEntities/Creatures/TestDrone.prefab";
		for (int i = 0; i < 40; i++)
		{
			Vector3 position = MainCamera.camera.transform.position + UnityEngine.Random.insideUnitSphere * 20f;
			yield return AddressablesUtility.InstantiateAsync(filename, null, position, Quaternion.identity);
		}
	}

	private void OnConsoleCommand_spark(NotificationCenter.Notification n)
	{
		int.Parse((string)n.data[0]);
		Player localPlayerComp = Utils.GetLocalPlayerComp();
		_ = localPlayerComp.gameObject;
		_ = localPlayerComp.transform.position;
	}

	private void OnConsoleCommand_resstats(NotificationCenter.Notification n)
	{
		SNUtils.EnsureCustomLogPathExists();
		using (StreamWriter streamWriter = FileUtils.CreateTextFile(SNUtils.InsideCustomLogs("meshstats.csv")))
		{
			Mesh[] array = Resources.FindObjectsOfTypeAll<Mesh>();
			foreach (Mesh mesh in array)
			{
				float num = (float)Profiler.GetRuntimeMemorySize(mesh) / 1024f / 1024f;
				streamWriter.WriteLine(mesh.name + ",numVerts," + mesh.vertexCount + ",mb," + num);
			}
		}
		using (StreamWriter streamWriter2 = FileUtils.CreateTextFile(SNUtils.InsideCustomLogs("tex2dstats.csv")))
		{
			Texture2D[] array2 = Resources.FindObjectsOfTypeAll<Texture2D>();
			foreach (Texture2D texture2D in array2)
			{
				float num2 = (float)Profiler.GetRuntimeMemorySize(texture2D) / 1024f / 1024f;
				float num3 = num2 / (float)(texture2D.width * texture2D.height);
				streamWriter2.WriteLine(string.Concat(texture2D.name, ",mb,", num2, ",format,", texture2D.format, ",mipmapCount,", texture2D.mipmapCount, ",width,", texture2D.width, ",height,", texture2D.height, ",mbPerPix,", num3));
			}
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private static void StartPhototour(NotificationCenter.Notification n)
	{
		string text = ((n != null && n.data != null && n.data.Count > 0) ? ((string)n.data[0]) : string.Empty);
		if (string.IsNullOrEmpty(text) || runCount > 0)
		{
			return;
		}
		currentPhototourId = text;
		runCount = Mathf.Max(TryGetNotificationInt(n, 1), Mathf.Max(TryGetNotificationInt(n, 2), TryGetNotificationInt(n, 3)));
		string text2 = text;
		if (!text2.Contains(".tour"))
		{
			text2 = $"{text2}.tour";
		}
		string path = Path.Combine(PhotoTour.toursFolder, text2);
		if (!File.Exists(path))
		{
			currentPhototourId = string.Empty;
			runCount = 0;
			return;
		}
		if (TryFindNotificationFlag(n, "heap"))
		{
			HeapStats.main.Clear();
			HeapStats.main.IsRecording = true;
		}
		currentPhototourFilename = path;
		PhotoTour.main.onPlaybackDone += OnPhototourFinished;
		PhotoTour.main.bScreenShotsAllowed = false;
		PhotoTour.main.PlayFile(path, string.Empty, ".");
	}

	private static void PrintDiagnosticDebugString(string s)
	{
		ErrorMessage.AddDebug(s);
		UnityEngine.Debug.Log(s);
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	[SuppressMessage("Gendarme.Rules.Performance", "AvoidConcatenatingCharsRule")]
	[SuppressMessage("Subnautica.Rules", "AvoidStringConcatenation")]
	private void OnConsoleCommand_recordtour(NotificationCenter.Notification n)
	{
		if (recordingPhototour)
		{
			PhotoTour.main.StopRecording();
		}
		string text = TryGetNotificationString(n, 0);
		if (!string.IsNullOrEmpty(text))
		{
			int num = TryGetNotificationInt(n, 1, 0);
			PhotoTour.main.StartRecording(text, num);
			PhotoTour.main.onRecordingDone += PhototourRecordingDone;
			recordingPhototour = true;
			if (num > 0)
			{
				PrintDiagnosticDebugString("Recording phototour:  " + text + ".tour / for " + num + " seconds.");
			}
			else
			{
				PrintDiagnosticDebugString("Recording phototour:  " + text + ".tour");
			}
		}
	}

	private void OnConsoleCommand_stoprecording(NotificationCenter.Notification n)
	{
		PhotoTour.main.StopRecording();
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private static void PhototourRecordingDone(PhotoTour tour)
	{
		PrintDiagnosticDebugString("Finished recording phototour.");
		recordingPhototour = false;
		PhotoTour.main.onRecordingDone -= PhototourRecordingDone;
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void OnConsoleCommand_phototour(NotificationCenter.Notification n)
	{
		isProfiling = false;
		StartPhototour(n);
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void OnConsoleCommand_stopphototour(NotificationCenter.Notification n)
	{
		currentPhototourId = string.Empty;
		runCount = 0;
		PhotoTour.main.StopPlaying();
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private static bool TryFindNotificationFlag(NotificationCenter.Notification n, string toMatch)
	{
		if (n != null && n.data != null)
		{
			for (int i = 0; i < n.data.Count; i++)
			{
				if ((string)n.data[i] == toMatch)
				{
					return true;
				}
			}
		}
		return false;
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private static string TryGetNotificationString(NotificationCenter.Notification n, int arrayIndex)
	{
		if (n == null || n.data == null || n.data.Count <= arrayIndex)
		{
			return string.Empty;
		}
		return (string)n.data[arrayIndex];
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private static int TryGetNotificationInt(NotificationCenter.Notification n, int arrayIndex, int defaultValue = 1)
	{
		int result = defaultValue;
		if (n != null && n.data != null && n.data.Count > arrayIndex)
		{
			if (!int.TryParse((string)n.data[arrayIndex], out result))
			{
				return defaultValue;
			}
			return result;
		}
		return defaultValue;
	}

	private void OnConsoleCommand_photoprofile(NotificationCenter.Notification n)
	{
		if (runCount <= 0)
		{
			bool abTestingEnabled = TryFindNotificationFlag(n, "ab");
			bool minimalProfile = TryFindNotificationFlag(n, "minimal");
			PhotoProfile(n, abTestingEnabled, minimalProfile);
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private static void PhotoProfile(NotificationCenter.Notification n, bool abTestingEnabled, bool minimalProfile)
	{
		string text = TryGetNotificationString(n, 0);
		if (!string.IsNullOrEmpty(text))
		{
			isProfiling = true;
			currentPhototourId = text;
			StartPhototour(n);
			if (string.IsNullOrEmpty(currentPhototourId))
			{
				UnityEngine.Debug.Log("PHOTOPROFILE syntax error.");
			}
			else
			{
				UnityEngine.Debug.LogFormat("START PHOTOTOUR PROFILING: {0} / AB Testing: {1} / Minimal: {2} / Num Runs: {3}", text, abTestingEnabled, minimalProfile, runCount);
			}
			StopwatchProfiler.Instance.ABTestingEnabled = abTestingEnabled;
			if (minimalProfile)
			{
				StopwatchProfiler.Instance.SetCategoryMinimal();
			}
			else
			{
				StopwatchProfiler.Instance.SetCategoryDetailed();
			}
			StartStopwatchProfiler(text);
		}
	}

	private static void OnPhototourFinished(PhotoTour tour)
	{
		runCount--;
		if (runCount > 0)
		{
			if (isProfiling)
			{
				StopStopwatchProfiler();
			}
			PhotoTour.main.PlayFile(currentPhototourFilename, "", ".");
			if (isProfiling)
			{
				StartStopwatchProfiler(currentPhototourId);
			}
			return;
		}
		if (isProfiling)
		{
			StopwatchProfiler.Instance.StopRecordingAndCloseSession();
		}
		if (HeapStats.main.IsRecording)
		{
			HeapStats.main.IsRecording = false;
			HeapStats.main.WriteStatsToFile(currentPhototourId);
			HeapStats.main.Clear();
		}
		PhotoTour.main.onPlaybackDone -= OnPhototourFinished;
		isProfiling = false;
		currentPhototourId = string.Empty;
	}

	private static void StartStopwatchProfiler(string profileId)
	{
		SNUtils.EnsureCustomLogPathExists();
		StopStopwatchProfiler();
		StopwatchProfiler.Instance.SetSettingsReportString(AutomatedProfiler.GetCurrentQualityOptionsCSV());
		StopwatchProfiler.Instance.StartRecording(SNUtils.GetDevTempPath(), profileId, 0f, string.Empty);
		PrintDiagnosticDebugString("Stopwatch Profiler started.");
	}

	private static void StopStopwatchProfiler()
	{
		if (StopwatchProfiler.Instance.IsRecording)
		{
			StopwatchProfiler.Instance.StopRecording();
			PrintDiagnosticDebugString("Stopwatch Profiler stopped.");
		}
	}

	private void ToggleStopwatchProfiler(string profileId)
	{
		SNUtils.EnsureCustomLogPathExists();
		if (!StopwatchProfiler.Instance.IsRecording)
		{
			StopwatchProfiler.Instance.SetSettingsReportString(AutomatedProfiler.GetCurrentQualityOptionsCSV());
			StopwatchProfiler.Instance.StartRecording(SNUtils.GetDevTempPath(), profileId, 0f, string.Empty);
			PrintDiagnosticDebugString("Stopwatch Profiler started.");
		}
		else
		{
			StopwatchProfiler.Instance.StopRecording();
			PrintDiagnosticDebugString("Stopwatch Profiler stopped.");
		}
	}

	private void OnConsoleCommand_abflip(NotificationCenter.Notification n)
	{
		StopwatchProfiler.Instance.FlipTestVariant();
		PrintDiagnosticDebugString($"Stopwatch Profiler AB Test Variant: {StopwatchProfiler.Instance.currentTestVariant.ToString()}");
	}

	private void OnConsoleCommand_stopwatch(NotificationCenter.Notification n)
	{
		string text = ((n != null && n.data != null && n.data.Count > 0) ? ((string)n.data[0]) : "");
		if (string.IsNullOrEmpty(text))
		{
			text = "Stopwatch";
		}
		ToggleStopwatchProfiler(text);
	}

	private void OnConsoleCommand_dblit()
	{
		MainCamera.camera.gameObject.AddComponent<DummyBlitOpaqueImageEffect>();
		MainCamera.camera.gameObject.AddComponent<DummyBlitImageEffect>();
	}

	private void OnConsoleCommand_testexception()
	{
		throw new UnityException("This is a test exception");
	}

	private void OnConsoleCommand_pcannon()
	{
		Material mat = Resources.Load<Material>("Materials/FireOverlay");
		GameObject gameObject = new GameObject("PropulsionCannonHolder");
		gameObject.SetActive(value: false);
		bool flag = GameModeUtils.RequiresPower();
		if (flag)
		{
			GameModeUtils.ActivateCheat(GameModeOption.NoEnergy);
		}
		EnergyMixin energyMixin = gameObject.AddComponent<EnergyMixin>();
		EnergyInterface energyInterface = gameObject.AddComponent<EnergyInterface>();
		PropulsionCannon propulsionCannon = gameObject.AddComponent<PropulsionCannon>();
		energyInterface.sources = new EnergyMixin[1] { energyMixin };
		propulsionCannon.energyInterface = energyInterface;
		propulsionCannon.shootForce = 60f;
		propulsionCannon.attractionForce = 145f;
		propulsionCannon.massScalingFactor = 0.005f;
		propulsionCannon.pickupDistance = 25f;
		propulsionCannon.maxMass = 1800f;
		propulsionCannon.maxAABBVolume = 400f;
		Rigidbody[] array = UnityEngine.Object.FindObjectsOfType<Rigidbody>();
		foreach (Rigidbody rigidbody in array)
		{
			if (!rigidbody.GetComponentInParent<Player>())
			{
				GameObject gameObject2 = rigidbody.gameObject;
				if (propulsionCannon.ValidateNewObject(gameObject2, Vector3.zero, checkLineOfSight: false))
				{
					VFXOverlayMaterial vFXOverlayMaterial = gameObject2.AddComponent<VFXOverlayMaterial>();
					vFXOverlayMaterial.ApplyOverlay(mat, "VFXOverlay: Scanning", instantiateMaterial: false);
					UnityEngine.Object.Destroy(vFXOverlayMaterial, 60f);
				}
			}
		}
		if (flag)
		{
			GameModeUtils.DeactivateCheat(GameModeOption.NoEnergy);
		}
		UnityEngine.Object.Destroy(gameObject);
	}

	private void OnConsoleCommand_disablepoolpurge()
	{
		GameObjectPool.DumpEnabled = false;
	}

	private void OnConsoleCommand_enablepoolpurge()
	{
		GameObjectPool.DumpEnabled = true;
	}

	private void OnConsoleCommand_dumpgameobjectpool()
	{
		using (StreamWriter streamWriter = FileUtils.CreateTextFile($"{Application.dataPath}/../GameObjectPools.csv"))
		{
			Dictionary<int, GameObjectPool.QueueInfo>.Enumerator enumerator = GameObjectPool.PoolMap.GetEnumerator();
			while (enumerator.MoveNext())
			{
				streamWriter.Write(enumerator.Current.Value.name);
				streamWriter.Write(",");
				streamWriter.Write(enumerator.Current.Value.id);
				streamWriter.Write(",");
				streamWriter.Write(enumerator.Current.Value.prefabId);
				streamWriter.Write(",");
				streamWriter.Write(enumerator.Current.Value.instanceCount);
				streamWriter.Write(",");
				streamWriter.Write(enumerator.Current.Value.hitCount);
				streamWriter.Write("\n");
			}
			streamWriter.Write(GameObjectPool.InstanceCount);
			streamWriter.Write(",");
			streamWriter.Write(GameObjectPool.HitCount);
			streamWriter.Write("\n");
		}
	}
}
