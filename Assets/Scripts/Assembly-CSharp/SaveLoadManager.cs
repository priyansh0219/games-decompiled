using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Platform.IO;
using ProtoBuf;
using UWE;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
	[ProtoContract]
	public class OptionsCache
	{
		[NonSerialized]
		[ProtoMember(1)]
		public readonly Dictionary<string, float> _floats = new Dictionary<string, float>();

		[NonSerialized]
		[ProtoMember(2)]
		public readonly Dictionary<string, string> _strings = new Dictionary<string, string>();

		[NonSerialized]
		[ProtoMember(3)]
		public readonly Dictionary<string, bool> _bools = new Dictionary<string, bool>();

		[NonSerialized]
		[ProtoMember(4)]
		public readonly Dictionary<string, int> _ints = new Dictionary<string, int>();

		public void Clear()
		{
			_floats.Clear();
			_strings.Clear();
			_bools.Clear();
			_ints.Clear();
		}

		public void SetInt(string name, int value)
		{
			_ints[name] = value;
		}

		public int GetInt(string name, int defaultValue)
		{
			return _ints.GetOrDefault(name, defaultValue);
		}

		public void SetFloat(string name, float value)
		{
			_floats[name] = value;
		}

		public float GetFloat(string name, float defaultValue)
		{
			return _floats.GetOrDefault(name, defaultValue);
		}

		public void SetBool(string name, bool value)
		{
			_bools[name] = value;
		}

		public bool GetBool(string name, bool defaultValue)
		{
			return _bools.GetOrDefault(name, defaultValue);
		}

		public void SetString(string name, string value)
		{
			_strings[name] = value;
		}

		public string GetString(string name, string defaultValue)
		{
			return _strings.GetOrDefault(name, defaultValue);
		}
	}

	public enum Error
	{
		None = 0,
		InvalidCall = 1,
		UnknownError = 2,
		OutOfSpace = 3,
		NoAccess = 4,
		NotFound = 5,
		InvalidFormat = 6,
		OutOfSlots = 7
	}

	public abstract class AsyncResult
	{
		public readonly bool success;

		public AsyncResult(bool success)
		{
			this.success = success;
		}
	}

	public class LoadResult : AsyncResult
	{
		public readonly Error error;

		public readonly string errorMessage;

		public LoadResult(bool success, Error error, string errorMessage)
			: base(success)
		{
			this.error = error;
			this.errorMessage = errorMessage;
		}
	}

	public class SaveResult : AsyncResult
	{
		public readonly Error error;

		public readonly string errorMessage;

		public readonly int size;

		public SaveResult(bool success, Error error, string errorMessage, int size)
			: base(success)
		{
			this.error = error;
			this.errorMessage = errorMessage;
			this.size = size;
		}
	}

	public class CreateResult : AsyncResult
	{
		public readonly Error error;

		public readonly string slotName;

		public CreateResult(bool success, Error error, string slotName)
			: base(success)
		{
			this.error = error;
			this.slotName = slotName;
		}
	}

	public class GameInfo
	{
		private const int screenshotWidth = 200;

		private const int screenshotQuality = 75;

		public int version;

		public int gameTime;

		public long dateTicks;

		public long startTicks;

		public int changeSet;

		public ulong protoBufVersion;

		public string session;

		public GameMode gameMode = GameMode.None;

		public bool isFallback;

		public bool cyclopsPresent;

		public bool seamothPresent;

		public bool exosuitPresent;

		public bool rocketPresent;

		public bool basePresent;

		public bool corrupted;

		private Texture2D screenshot;

		private static void SaveFile(string fileName, byte[] bytes)
		{
			Platform.IO.File.WriteAllBytes(CombineTemporarySavePathFilename(fileName), bytes);
		}

		private static string CombineTemporarySavePathFilename(string fileName)
		{
			return Platform.IO.Path.Combine(GetTemporarySavePath(), fileName);
		}

		public void Initialize(float timePlayed, long firstStart, string sessionId, Texture2D tex)
		{
			version = 2;
			gameTime = Mathf.FloorToInt(timePlayed);
			dateTicks = DateTime.Now.Ticks;
			startTicks = firstStart;
			changeSet = SNUtils.GetPlasticChangeSetOfBuild(0);
			protoBufVersion = 13uL;
			session = sessionId;
			gameMode = Utils.GetLegacyGameMode();
			screenshot = tex;
		}

		public override string ToString()
		{
			return JsonUtility.ToJson(this);
		}

		public static void SaveIntoCurrentSlot(GameInfo info)
		{
			string text = JsonUtility.ToJson(info);
			using (MemoryStream memoryStream = new MemoryStream(text.Length * 2))
			{
				using (StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
				{
					streamWriter.WriteLine(text);
				}
				SaveFile("gameinfo.json", memoryStream.ToArray());
			}
			if (info.screenshot != null)
			{
				byte[] array = info.screenshot.EncodeToJPG(75);
				if (array.Length != 0)
				{
					SaveFile("screenshot.jpg", array);
				}
			}
		}

		public static GameInfo LoadFromBytes(byte[] jsonData, byte[] screenshotData)
		{
			try
			{
				if (jsonData == null)
				{
					throw new ArgumentNullException("jsonData", "No gameinfo data");
				}
				GameInfo gameInfo = null;
				using (StreamReader streamReader = new StreamReader(new MemoryStream(jsonData)))
				{
					gameInfo = JsonUtility.FromJson<GameInfo>(streamReader.ReadToEnd());
				}
				if (screenshotData != null && screenshotData.Length != 0)
				{
					Texture2D texture2D = MathExtensions.LoadTexture(screenshotData);
					gameInfo.screenshot = texture2D;
				}
				return gameInfo;
			}
			catch (Exception ex)
			{
				Debug.LogWarningFormat("Exception while parsing: {0}. Using fallback gameinfo instead.", ex);
				DateTime dateTime = new DateTime(2015, 1, 1);
				return new GameInfo
				{
					version = 2,
					gameTime = 0,
					dateTicks = dateTime.Ticks,
					startTicks = dateTime.Ticks,
					changeSet = SNUtils.GetPlasticChangeSetOfBuild(0),
					protoBufVersion = 13uL,
					session = Guid.NewGuid().ToString(),
					gameMode = GameMode.Survival,
					screenshot = null,
					isFallback = true
				};
			}
		}

		public bool IsValid()
		{
			ulong num = 13uL;
			int plasticChangeSetOfBuild = SNUtils.GetPlasticChangeSetOfBuild(0);
			if (protoBufVersion != 0)
			{
				if (protoBufVersion > num)
				{
					Debug.LogWarning("Save version from the future! " + protoBufVersion + " vs. " + num);
					return false;
				}
			}
			else if (plasticChangeSetOfBuild > 0 && changeSet > plasticChangeSetOfBuild)
			{
				Debug.LogWarning("Savegame from the future! " + changeSet + " vs. " + plasticChangeSetOfBuild);
				return false;
			}
			return true;
		}

		public Texture2D GetScreenshot()
		{
			return screenshot;
		}
	}

	private const FreezeTime.Id freezerId = FreezeTime.Id.Save;

	private const int currentVersion = 2;

	private long firstStart;

	private string sessionId;

	private const string gameInfoFileName = "gameinfo.json";

	private const string screenshotFileName = "screenshot.jpg";

	private string currentSlot = "test";

	private bool allowWritingFiles = true;

	private bool accumulateTimePlayed;

	private float timePlayedThisSession;

	private float loadedGameTime;

	private DateTime lastSaveTime;

	private FreezeTimeInputHandler inputHandler = new FreezeTimeInputHandler(FreezeTime.Id.Save);

	private readonly Dictionary<string, GameInfo> gameInfoCache = new Dictionary<string, GameInfo>();

	private static SaveLoadManager _main;

	private static string temporarySavePath;

	public bool isSaving { get; private set; }

	public bool isLoading { get; private set; }

	public float timePlayedTotal => timePlayedThisSession + loadedGameTime;

	private int MaxSlotsAllowed
	{
		get
		{
			if (PlatformUtils.isWindowsStore)
			{
				return 10;
			}
			return 10000;
		}
	}

	public static SaveLoadManager main => _main;

	public static event Action<bool> notificationSaveInProgress;

	public void NotifySaveInProgress(bool isInProgress)
	{
		SaveLoadManager.notificationSaveInProgress?.Invoke(isInProgress);
	}

	public void CancelSave()
	{
		Debug.LogWarning("CancelSave should not be called for any other platform.");
	}

	public void SetCurrentSlot(string _currentSlot)
	{
		currentSlot = _currentSlot;
	}

	public string GetCurrentSlot()
	{
		return currentSlot;
	}

	public bool GetAllowWritingFiles()
	{
		return allowWritingFiles;
	}

	private void Awake()
	{
		if (_main != null)
		{
			Debug.LogError("Multiple SaveLoadManager instances found in scene!", this);
			Debug.Break();
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
		else
		{
			_main = this;
		}
	}

	private void Update()
	{
		if (accumulateTimePlayed)
		{
			timePlayedThisSession += Time.deltaTime;
		}
	}

	private void OnDestroy()
	{
		ClearTemporarySave();
	}

	public static string GetTemporarySavePath()
	{
		return temporarySavePath;
	}

	public static string GetTemporarySavePathRoot()
	{
		return Platform.IO.Path.Combine(PlatformUtils.temporaryCachePath, "TempSave");
	}

	public string StartNewSession()
	{
		sessionId = Guid.NewGuid().ToString();
		return sessionId;
	}

	public void InitializeNewGame()
	{
		accumulateTimePlayed = true;
		timePlayedThisSession = 0f;
		loadedGameTime = 0f;
		firstStart = DateTime.Now.Ticks;
		GameInfo gameInfo = GetGameInfo(currentSlot);
		if (gameInfo != null)
		{
			loadedGameTime = gameInfo.gameTime;
			firstStart = gameInfo.startTicks;
			sessionId = gameInfo.session;
		}
		ScreenshotManager.Initialize(GetTemporarySavePath());
	}

	public void Deinitialize()
	{
		gameInfoCache.Clear();
	}

	private bool SanityCheck(out string error)
	{
		if (isSaving)
		{
			Debug.LogError("Attempting use while save operation in progress");
			error = "Currently saving";
			return false;
		}
		if (isLoading)
		{
			Debug.LogError("Attempting use while load operation in progress");
			error = "Currently loading";
			return false;
		}
		error = null;
		return true;
	}

	public CoroutineTask<SaveResult> SaveToDeepStorageAsync()
	{
		TaskResult<SaveResult> result = new TaskResult<SaveResult>();
		return new CoroutineTask<SaveResult>(SaveToDeepStorageAsync(result), result);
	}

	public CoroutineTask<SaveResult> SaveToTemporaryStorageAsync(Texture2D screenshot)
	{
		TaskResult<SaveResult> result = new TaskResult<SaveResult>();
		return new CoroutineTask<SaveResult>(SaveToTemporaryStorageAsync(result, screenshot), result);
	}

	private static IEnumerator SaveWorldStateToTemporaryStorageAsync(IOut<SaveResult> result)
	{
		try
		{
			LargeWorldStreamer.main.SaveSceneObjectsIntoCurrentSlot();
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			result.Set(new SaveResult(success: false, GetResultForException(ex), ex.Message, 0));
			yield break;
		}
		yield return null;
		try
		{
			LargeWorldStreamer.main.SaveGlobalRootIntoCurrentSlot();
		}
		catch (Exception ex2)
		{
			Debug.LogException(ex2);
			result.Set(new SaveResult(success: false, GetResultForException(ex2), ex2.Message, 0));
			yield break;
		}
		yield return null;
		try
		{
			LargeWorldStreamer.main.cellManager.SaveAllBatchCells();
		}
		catch (Exception ex3)
		{
			Debug.LogException(ex3);
			result.Set(new SaveResult(success: false, GetResultForException(ex3), ex3.Message, 0));
			yield break;
		}
		yield return null;
	}

	private IEnumerator SaveToTemporaryStorageAsync(IOut<SaveResult> result, Texture2D screenshot)
	{
		if (!SanityCheck(out var error))
		{
			result.Set(new SaveResult(success: false, Error.InvalidCall, error, 0));
			yield break;
		}
		FreezeTime.Begin(FreezeTime.Id.Save);
		InputHandlerStack.main.Push(inputHandler);
		isSaving = true;
		GameInfo gameInfo = new GameInfo();
		gameInfo.Initialize(timePlayedTotal, firstStart, sessionId, screenshot);
		gameInfo.seamothPresent = GameInfoIcon.Has(TechType.Seamoth);
		gameInfo.exosuitPresent = GameInfoIcon.Has(TechType.Exosuit);
		gameInfo.rocketPresent = GameInfoIcon.Has(TechType.RocketBase);
		gameInfo.cyclopsPresent = GameInfoIcon.Has(TechType.Cyclops);
		gameInfo.basePresent = GameInfoIcon.Has(TechType.BaseCorridor);
		try
		{
			GameInfo.SaveIntoCurrentSlot(gameInfo);
		}
		catch (Exception ex)
		{
			result.Set(new SaveResult(success: false, GetResultForException(ex), ex.Message, 0));
			FreezeTime.End(FreezeTime.Id.Save);
			isSaving = false;
			yield break;
		}
		gameInfoCache[currentSlot] = gameInfo;
		LargeWorldStreamer streamer = LargeWorldStreamer.main;
		while (!streamer.IsWorldSettled())
		{
			yield return CoroutineUtils.waitForNextFrame;
		}
		streamer.frozen = true;
		CellManager cellManager = streamer.cellManager;
		yield return cellManager.IncreaseFreezeCount();
		Player player = Player.main;
		PilotingChair playerChair = player.GetPilotingChair();
		if ((bool)playerChair)
		{
			player.ExitPilotingMode(keepCinematicState: true);
		}
		Transform playerParent = player.transform.parent;
		player.transform.parent = null;
		player.transform.localScale = Vector3.one;
		TaskResult<SaveResult> worldTaskResult = new TaskResult<SaveResult>();
		yield return SaveWorldStateToTemporaryStorageAsync(worldTaskResult);
		player.transform.parent = playerParent;
		if ((bool)playerChair)
		{
			player.EnterPilotingMode(playerChair, keepCinematicState: true);
		}
		cellManager.DecreaseFreezeCount();
		streamer.frozen = false;
		FreezeTime.End(FreezeTime.Id.Save);
		isSaving = false;
		SaveResult saveResult = worldTaskResult.Get();
		if (saveResult == null)
		{
			saveResult = new SaveResult(success: true, Error.None, null, 0);
		}
		result.Set(saveResult);
	}

	public static string GetDeleteMetaFileName(string fileName)
	{
		return $"{fileName}.deleted";
	}

	public static string GetFileNameForDeleteMetaFileName(string fileName)
	{
		return fileName.Substring(0, fileName.LastIndexOf(".deleted"));
	}

	public static bool IsDeleteMetaFileName(string fileName)
	{
		return fileName.EndsWith(".deleted");
	}

	private IEnumerator SaveToDeepStorageAsync(IOut<SaveResult> result)
	{
		UserStorage userStorage = PlatformUtils.main.GetUserStorage();
		if (!SanityCheck(out var error))
		{
			result.Set(new SaveResult(success: false, Error.InvalidCall, error, 0));
			yield break;
		}
		isSaving = true;
		allowWritingFiles = false;
		string text = GetTemporarySavePath();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		try
		{
			string[] files = Platform.IO.Directory.GetFiles(text, "*", SearchOption.AllDirectories);
			foreach (string text2 in files)
			{
				DateTime lastWriteTime = Platform.IO.File.GetLastWriteTime(text2);
				string text3 = text2.Substring(text.Length + 1);
				if (lastWriteTime > lastSaveTime)
				{
					if (IsDeleteMetaFileName(text3))
					{
						if (!Platform.IO.File.Exists(GetFileNameForDeleteMetaFileName(text2)))
						{
							list2.Add(GetFileNameForDeleteMetaFileName(text3));
						}
					}
					else
					{
						list.Add(text3);
					}
				}
				else if (!IsDeleteMetaFileName(text3))
				{
					list3.Add(text3);
				}
			}
		}
		catch (Exception ex)
		{
			result.Set(new SaveResult(success: false, Error.UnknownError, ex.ToString(), 0));
			allowWritingFiles = true;
			isSaving = false;
			yield break;
		}
		UserStorageUtils.UpdateOperation updateOperation = new UserStorageUtils.UpdateOperation();
		try
		{
			updateOperation.saveOperation = userStorage.CopyFilesToContainerAsync(currentSlot, text, list, list2, list3);
		}
		catch (Exception ex2)
		{
			result.Set(new SaveResult(success: false, Error.UnknownError, ex2.ToString(), 0));
			allowWritingFiles = true;
			isSaving = false;
			yield break;
		}
		yield return updateOperation;
		allowWritingFiles = true;
		isSaving = false;
		bool flag = updateOperation.result == UserStorageUtils.Result.Success;
		if (flag)
		{
			lastSaveTime = DateTime.Now;
		}
		Error error2 = ConvertResult(updateOperation.result);
		result.Set(new SaveResult(flag, error2, updateOperation.errorMessage, updateOperation.saveOperation.saveDataSize));
	}

	public CoroutineTask<LoadResult> LoadSlotsAsync()
	{
		TaskResult<LoadResult> result = new TaskResult<LoadResult>();
		return new CoroutineTask<LoadResult>(LoadSlotsAsync(PlatformUtils.main.GetUserStorage(), gameInfoCache, result), result);
	}

	public CoroutineTask<LoadResult> LoadSlotsAsync(UserStorage userStorage, Dictionary<string, GameInfo> infoCache)
	{
		TaskResult<LoadResult> result = new TaskResult<LoadResult>();
		return new CoroutineTask<LoadResult>(LoadSlotsAsync(userStorage, infoCache, result), result);
	}

	private void RegisterSaveGame(string slotName, UserStorageUtils.LoadOperation loadOperation, Dictionary<string, GameInfo> infoCache)
	{
		if (loadOperation.GetSuccessful())
		{
			Debug.Log("RegisterSaveGame: Loading GameInfo for " + slotName);
			byte[] value = null;
			byte[] value2 = null;
			loadOperation.files.TryGetValue("gameinfo.json", out value);
			loadOperation.files.TryGetValue("screenshot.jpg", out value2);
			GameInfo gameInfo = GameInfo.LoadFromBytes(value, value2);
			if (gameInfo == null)
			{
				Debug.LogFormat("Skipping save directory because GameInfo failed to load: {0}", slotName);
			}
			else
			{
				infoCache[slotName] = gameInfo;
			}
		}
		else
		{
			Debug.Log("RegisterSaveGame: load operation for " + slotName + " was not successful.");
		}
	}

	private IEnumerator LoadSlotsAsync(UserStorage userStorage, Dictionary<string, GameInfo> infoCache, IOut<LoadResult> result)
	{
		if (!SanityCheck(out var error))
		{
			result.Set(new LoadResult(success: false, Error.InvalidCall, error));
			yield break;
		}
		isLoading = true;
		infoCache.Clear();
		UserStorageUtils.QueryOperation queryOperation = userStorage.GetContainerNamesAsync();
		yield return queryOperation;
		List<string> list = new List<string>();
		list.Add("gameinfo.json");
		list.Add("screenshot.jpg");
		UserStorageUtils.SlotsOperation slotsOperation = userStorage.LoadSlotsAsync(queryOperation.results, list);
		yield return slotsOperation;
		if (slotsOperation.GetSuccessful())
		{
			foreach (KeyValuePair<string, UserStorageUtils.LoadOperation> slot in slotsOperation.slots)
			{
				string slotName = slot.Key;
				UserStorageUtils.LoadOperation loadOperation = slot.Value;
				Debug.Log("Waiting for " + slotName + " to load...");
				yield return loadOperation;
				RegisterSaveGame(slotName, loadOperation, infoCache);
			}
		}
		isLoading = false;
		result.Set(new LoadResult(slotsOperation.GetSuccessful(), ConvertResult(slotsOperation.result), slotsOperation.errorMessage));
	}

	public CoroutineTask<LoadResult> LoadAsync()
	{
		TaskResult<LoadResult> result = new TaskResult<LoadResult>();
		return new CoroutineTask<LoadResult>(LoadAsync(result), result);
	}

	private IEnumerator LoadAsync(IOut<LoadResult> result)
	{
		UserStorage userStorage = PlatformUtils.main.GetUserStorage();
		if (!SanityCheck(out var error))
		{
			result.Set(new LoadResult(success: false, Error.InvalidCall, error));
			yield break;
		}
		isLoading = true;
		ClearTemporarySave();
		CreateTemporarySave();
		string destPath = GetTemporarySavePath();
		UserStorageUtils.AsyncOperation copyOperation = userStorage.CopyFilesFromContainerAsync(currentSlot, destPath);
		yield return copyOperation;
		lastSaveTime = DateTime.Now;
		isLoading = false;
		result.Set(new LoadResult(copyOperation.GetSuccessful(), ConvertResult(copyOperation.result), copyOperation.errorMessage));
	}

	public GameInfo GetGameInfo(string slotName)
	{
		if (gameInfoCache.TryGetValue(slotName, out var value))
		{
			return value;
		}
		return null;
	}

	private void ClearTemporarySave()
	{
		string temporarySavePathRoot = GetTemporarySavePathRoot();
		temporarySavePath = null;
		try
		{
			if (Platform.IO.Directory.Exists(temporarySavePathRoot))
			{
				Platform.IO.Directory.Delete(temporarySavePathRoot, recursive: true);
			}
		}
		catch (IOException exception)
		{
			Debug.LogException(exception);
		}
		catch (UnauthorizedAccessException exception2)
		{
			Debug.LogException(exception2);
		}
	}

	private static void CreateTemporarySave()
	{
		try
		{
			string temporarySavePathRoot = GetTemporarySavePathRoot();
			Platform.IO.Directory.CreateDirectory(temporarySavePathRoot);
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			string[] directories = Platform.IO.Directory.GetDirectories(temporarySavePathRoot, "tmp*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < directories.Length; i++)
			{
				System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(directories[i]);
				hashSet.Add(directoryInfo.Name);
			}
			new System.Random();
			for (int j = 0; j < 10000; j++)
			{
				int num = UnityEngine.Random.Range(0, 1000000);
				string text = $"tmp{num:000000}";
				if (!hashSet.Contains(text))
				{
					temporarySavePath = Platform.IO.Path.Combine(temporarySavePathRoot, text);
					Platform.IO.Directory.CreateDirectory(temporarySavePath);
					return;
				}
			}
			Debug.LogErrorFormat("Failed to find unused tmp slot in '{0}' ({1} existing tmp slots)", temporarySavePathRoot, hashSet.Count);
			temporarySavePath = Platform.IO.Path.Combine(temporarySavePathRoot, "tmp_error");
			Platform.IO.Directory.CreateDirectory(temporarySavePath);
		}
		catch (IOException exception)
		{
			Debug.LogException(exception);
		}
		catch (UnauthorizedAccessException exception2)
		{
			Debug.LogException(exception2);
		}
	}

	public CoroutineTask<CreateResult> CreateSlotAsync()
	{
		TaskResult<CreateResult> result = new TaskResult<CreateResult>();
		return new CoroutineTask<CreateResult>(CreateSlotAsync(result), result);
	}

	private IEnumerator CreateSlotAsync(IOut<CreateResult> result)
	{
		UserStorage userStorage = PlatformUtils.main.GetUserStorage();
		UserStorageUtils.QueryOperation queryOperation = userStorage.GetContainerNamesAsync();
		yield return queryOperation;
		if (queryOperation.results.Count >= MaxSlotsAllowed)
		{
			result.Set(new CreateResult(success: false, Error.OutOfSlots, null));
			yield break;
		}
		Error error = Error.UnknownError;
		string slotName = null;
		for (int i = 0; i < MaxSlotsAllowed; i++)
		{
			string testSlotName = $"slot{i:0000}";
			if (!queryOperation.results.Contains(testSlotName))
			{
				UserStorageUtils.AsyncOperation createOperation = userStorage.CreateContainerAsync(testSlotName);
				yield return createOperation;
				if (createOperation.GetSuccessful())
				{
					error = Error.None;
					slotName = testSlotName;
					break;
				}
				if (createOperation.result == UserStorageUtils.Result.OutOfSpace)
				{
					Debug.LogError("Could not create a new slot. Out of space.");
					error = Error.OutOfSpace;
					break;
				}
			}
		}
		ClearTemporarySave();
		CreateTemporarySave();
		lastSaveTime = DateTime.Now;
		result.Set(new CreateResult(error == Error.None, error, slotName));
	}

	public string[] GetActiveSlotNames()
	{
		return gameInfoCache.Keys.OrderBy((string p) => p).ToArray();
	}

	public string[] GetPossibleSlotNames()
	{
		return GetActiveSlotNames();
	}

	public GameMode[] GetPossibleSlotGameModes()
	{
		string[] activeSlotNames = GetActiveSlotNames();
		GameMode[] array = new GameMode[activeSlotNames.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = gameInfoCache[activeSlotNames[i]].gameMode;
		}
		return array;
	}

	public UserStorageUtils.AsyncOperation ClearSlotAsync(string slotName)
	{
		gameInfoCache.Remove(slotName);
		return PlatformUtils.main.GetUserStorage().DeleteContainerAsync(slotName);
	}

	public UserStorageUtils.AsyncOperation DeleteFilesInSlot(string slotName, List<string> filePaths)
	{
		return PlatformUtils.main.GetUserStorage().DeleteFilesAsync(slotName, filePaths);
	}

	public void DeleteFilesInTemporaryStorage(List<string> filePaths)
	{
		new UserStorageUtils.AsyncOperation();
		if (string.IsNullOrEmpty(temporarySavePath) || !Platform.IO.Directory.Exists(temporarySavePath))
		{
			return;
		}
		try
		{
			foreach (string filePath in filePaths)
			{
				string fullFilePath = Platform.IO.Path.Combine(temporarySavePath, filePath);
				DeleteFileInTemporaryStorage(fullFilePath);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void DeleteFilesInTemporaryStorage(string searchPattern)
	{
		if (string.IsNullOrEmpty(temporarySavePath) || !Platform.IO.Directory.Exists(temporarySavePath))
		{
			return;
		}
		string[] files = Platform.IO.Directory.GetFiles(temporarySavePath, searchPattern, SearchOption.AllDirectories);
		try
		{
			string[] array = files;
			foreach (string fullFilePath in array)
			{
				DeleteFileInTemporaryStorage(fullFilePath);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void DeleteFileInTemporaryStorage(string fullFilePath)
	{
		if (Platform.IO.File.Exists(fullFilePath))
		{
			Platform.IO.File.Delete(fullFilePath);
			Platform.IO.File.Create(GetDeleteMetaFileName(fullFilePath)).Dispose();
		}
	}

	public void RenameFilesInTemporaryStorage(string subDir, string searchPattern, Func<Platform.IO.DirectoryInfo, string> getNewName)
	{
		if (string.IsNullOrEmpty(temporarySavePath) || !Platform.IO.Directory.Exists(temporarySavePath))
		{
			return;
		}
		string text = Platform.IO.Path.Combine(temporarySavePath, subDir);
		if (!Platform.IO.Directory.Exists(text))
		{
			return;
		}
		string[] files = Platform.IO.Directory.GetFiles(text, searchPattern, SearchOption.AllDirectories);
		try
		{
			Platform.IO.DirectoryInfo directoryInfo = new Platform.IO.DirectoryInfo(text);
			string[] array = files;
			foreach (string text2 in array)
			{
				string destFileName = Platform.IO.Path.Combine(text, getNewName(directoryInfo));
				Platform.IO.File.Copy(text2, destFileName, overwrite: true);
				DeleteFileInTemporaryStorage(text2);
				directoryInfo.Refresh();
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static Error ConvertResult(UserStorageUtils.Result result)
	{
		switch (result)
		{
		case UserStorageUtils.Result.Success:
			return Error.None;
		case UserStorageUtils.Result.UnknownError:
			return Error.UnknownError;
		case UserStorageUtils.Result.OutOfSpace:
			return Error.OutOfSpace;
		case UserStorageUtils.Result.NoAccess:
			return Error.NoAccess;
		case UserStorageUtils.Result.NotFound:
			return Error.NotFound;
		case UserStorageUtils.Result.InvalidFormat:
			return Error.InvalidFormat;
		default:
			return Error.UnknownError;
		}
	}

	protected static Error GetResultForException(Exception exception)
	{
		if (UWE.Utils.GetDiskFull(exception))
		{
			return Error.OutOfSpace;
		}
		if (exception is UnauthorizedAccessException)
		{
			return Error.NoAccess;
		}
		if (exception is DirectoryNotFoundException)
		{
			return Error.NotFound;
		}
		return Error.UnknownError;
	}
}
