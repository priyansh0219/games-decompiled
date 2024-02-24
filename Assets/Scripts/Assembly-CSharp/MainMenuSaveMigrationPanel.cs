using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UWE;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSaveMigrationPanel : MonoBehaviour, ILoadButtonDelegate
{
	private sealed class MigrateResult : SaveLoadManager.CreateResult
	{
		public readonly string errorMessage;

		public MigrateResult(bool success, SaveLoadManager.Error error, string slotName, string errorMessage)
			: base(success, error, slotName)
		{
			this.errorMessage = errorMessage;
		}
	}

	public GameObject saveInstance;

	public GameObject savedGameArea;

	public GameObject scrollBar;

	public MainMenuLoadPanel mainMenuLoadPanel;

	private readonly Dictionary<string, SaveLoadManager.GameInfo> gameInfoCache = new Dictionary<string, SaveLoadManager.GameInfo>();

	private UserStorage localStorage;

	string ILoadButtonDelegate.rightSideGroup => "SaveMigration";

	private IEnumerator Start()
	{
		if (PlatformUtils.isWindowsStore)
		{
			Language.OnLanguageChanged += OnLanguageChanged;
			GameInput.OnPrimaryDeviceChanged += OnPrimaryDeviceChanged;
			PlatformServicesWindows platformServicesWindows = PlatformUtils.main.GetServices() as PlatformServicesWindows;
			localStorage = platformServicesWindows.GetLocalStorage();
			Debug.Log("Attempting to load slots from local storage.");
			yield return SaveLoadManager.main.LoadSlotsAsync(localStorage, gameInfoCache);
			string[] array = gameInfoCache.Keys.OrderBy((string p) => p).ToArray();
			GameMode[] array2 = new GameMode[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = gameInfoCache[array[i]].gameMode;
			}
			bool active = !GameInput.IsPrimaryDeviceGamepad();
			Debug.Log($"Found {array.Length} slots.");
			for (int j = 0; j < array.Length; j++)
			{
				string text = array[j];
				Debug.Log("Creating button for save game " + text);
				GameObject obj = UnityEngine.Object.Instantiate(saveInstance);
				obj.transform.SetParent(savedGameArea.transform);
				obj.transform.localPosition = new Vector3(0f, 0f, 0f);
				obj.transform.localScale = new Vector3(1f, 1f, 1f);
				obj.transform.localRotation = Quaternion.identity;
				MainMenuLoadButton component = obj.GetComponent<MainMenuLoadButton>();
				component.saveGame = text;
				component.gameMode = array2[j];
				component.deleteButton.SetActive(active);
				component.buttonDelegate = this;
				UpdateLoadButtonState(component);
			}
			scrollBar.SetActive(array.Length >= 5);
			SortSavesByDate();
		}
	}

	private void OnDestroy()
	{
		if (PlatformUtils.isWindowsStore)
		{
			Language.OnLanguageChanged -= OnLanguageChanged;
			GameInput.OnPrimaryDeviceChanged -= OnPrimaryDeviceChanged;
		}
	}

	private void OnLanguageChanged()
	{
		MainMenuLoadButton[] allComponentsInChildren = savedGameArea.GetAllComponentsInChildren<MainMenuLoadButton>();
		foreach (MainMenuLoadButton lb in allComponentsInChildren)
		{
			UpdateLoadButtonState(lb);
		}
	}

	private void OnPrimaryDeviceChanged()
	{
		bool active = !GameInput.IsPrimaryDeviceGamepad();
		MainMenuLoadButton[] componentsInChildren = savedGameArea.GetComponentsInChildren<MainMenuLoadButton>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].deleteButton.SetActive(active);
		}
	}

	private void UpdateLoadButtonState(MainMenuLoadButton lb)
	{
		string text = Language.main.Get(lb.gameMode.ToString());
		lb.saveGameModeText.text = text;
		SaveLoadManager.GameInfo gameInfo = GetGameInfo(lb.saveGame);
		if (gameInfo == null)
		{
			lb.deleteButton.gameObject.SetActive(value: false);
			lb.saveGameTimeText.gameObject.SetActive(value: false);
			lb.saveIcons.SetActive(value: false);
			lb.saveGameScreenshot.gameObject.SetActive(value: false);
			lb.saveGameLengthText.text = Language.main.Get("SlotEmpty");
			Debug.Log("Save / Load: Came across a directory that doesn't appear to contain a save game, skipping it.");
			return;
		}
		string text2 = Utils.PrettifyTime(gameInfo.gameTime);
		string text3 = Utils.PrettifyDate(gameInfo.dateTicks);
		lb.gameObject.name = "Saved" + gameInfo.dateTicks;
		bool cyclopsPresent = gameInfo.cyclopsPresent;
		bool seamothPresent = gameInfo.seamothPresent;
		bool exosuitPresent = gameInfo.exosuitPresent;
		bool rocketPresent = gameInfo.rocketPresent;
		Texture2D screenshot = gameInfo.GetScreenshot();
		lb.changeSet = gameInfo.changeSet;
		lb.gameMode = gameInfo.gameMode;
		if (screenshot != null)
		{
			lb.saveGameScreenshot.texture = screenshot;
		}
		lb.saveGameTimeText.text = text3;
		lb.saveGameLengthText.text = text2;
		lb.saveGameModeText.text = text;
		lb.saveIcons.FindChild("SavedCyclops").gameObject.SetActive(cyclopsPresent);
		lb.saveIcons.FindChild("SavedSeamoth").gameObject.SetActive(seamothPresent);
		lb.saveIcons.FindChild("SavedExo").gameObject.SetActive(exosuitPresent);
		lb.saveIcons.FindChild("SavedRocket").gameObject.SetActive(rocketPresent);
		if (gameInfo.isFallback)
		{
			lb.saveGameLengthText.text = Language.main.Get("DamagedSavedGame");
		}
		if (!gameInfo.IsValid())
		{
			lb.load.GetComponent<Image>().color = Color.red;
			lb.loadButton.SetActive(value: false);
			lb.saveGameLengthText.text = $"<color=#ff0000ff>Changeset {gameInfo.changeSet} is newer than the current version!</color>";
		}
	}

	private void SortSavesByDate()
	{
		List<Transform> list = new List<Transform>();
		for (int num = savedGameArea.transform.childCount - 1; num >= 0; num--)
		{
			Transform child = savedGameArea.transform.GetChild(num);
			if (child.name != "NewGame")
			{
				list.Add(child);
				child.SetParent(null);
			}
		}
		list.Sort((Transform t1, Transform t2) => t1.name.CompareTo(t2.name));
		list.Reverse();
		foreach (Transform item in list)
		{
			item.SetParent(savedGameArea.transform);
		}
	}

	public SaveLoadManager.GameInfo GetGameInfo(string slotName)
	{
		if (gameInfoCache.TryGetValue(slotName, out var value))
		{
			return value;
		}
		return null;
	}

	public IEnumerator LoadGameAsync(MainMenuLoadButton button)
	{
		if (localStorage == null)
		{
			yield break;
		}
		button.loadButton.SetActive(value: false);
		button.deleteButton.SetActive(value: false);
		button.saveGameLengthText.text = Language.main.Get("SaveMigrationInProgress");
		TaskResult<MigrateResult> task = new TaskResult<MigrateResult>();
		yield return MigrateGame(button.saveGame, task);
		MigrateResult result = task.Get();
		if (result.success)
		{
			mainMenuLoadPanel.RefreshSaveList();
			button.Delete();
			string descriptionText = Language.main.Get("SaveMigrationSuccess");
			uGUI.main.confirmation.Show(descriptionText, delegate(bool load)
			{
				if (load)
				{
					SaveLoadManager.GameInfo gameInfo = SaveLoadManager.main.GetGameInfo(result.slotName);
					CoroutineHost.StartCoroutine(uGUI_MainMenu.main.LoadGameAsync(result.slotName, gameInfo.session, gameInfo.changeSet, gameInfo.gameMode));
				}
			});
			yield break;
		}
		SaveLoadManager.Error error = result.error;
		string message = result.errorMessage;
		switch (error)
		{
		case SaveLoadManager.Error.OutOfSpace:
			message = Language.main.Get("SaveMigrationFailedSpace");
			break;
		case SaveLoadManager.Error.OutOfSlots:
			message = Language.main.Get("SaveMigrationFailedSlot");
			break;
		case SaveLoadManager.Error.InvalidCall:
		case SaveLoadManager.Error.UnknownError:
		case SaveLoadManager.Error.NoAccess:
		case SaveLoadManager.Error.NotFound:
		case SaveLoadManager.Error.InvalidFormat:
			message = Language.main.GetFormat("SaveMigrationFailedFormat", error);
			break;
		}
		if (!string.IsNullOrEmpty(result.slotName))
		{
			yield return SaveLoadManager.main.ClearSlotAsync(result.slotName);
		}
		Debug.LogError($"{error}: {result.errorMessage}");
		uGUI.main.confirmation.Show(message, delegate(bool tryAgain)
		{
			if (tryAgain)
			{
				CoroutineHost.StartCoroutine(LoadGameAsync(button));
			}
			else
			{
				button.loadButton.SetActive(value: true);
				button.deleteButton.SetActive(value: true);
				button.saveGameLengthText.text = Utils.PrettifyTime(GetGameInfo(button.saveGame).gameTime);
			}
		});
	}

	public IEnumerator ClearSlotAsync(MainMenuLoadButton button)
	{
		if (localStorage != null)
		{
			gameInfoCache.Remove(button.saveGame);
			scrollBar.SetActive(gameInfoCache.Count >= 5);
			return localStorage.DeleteContainerAsync(button.saveGame);
		}
		return null;
	}

	private IEnumerator MigrateGame(string saveGame, IOut<MigrateResult> result)
	{
		CoroutineTask<SaveLoadManager.CreateResult> createSlotTask = SaveLoadManager.main.CreateSlotAsync();
		yield return createSlotTask;
		SaveLoadManager.CreateResult createSlotResult = createSlotTask.GetResult();
		if (!createSlotResult.success)
		{
			result.Set(new MigrateResult(success: false, createSlotResult.error, createSlotResult.slotName, "Could not create " + createSlotResult.slotName + "."));
			yield break;
		}
		Debug.Log("Successfully created slot " + createSlotResult.slotName);
		SaveLoadManager.main.SetCurrentSlot(createSlotResult.slotName);
		string temporarySavePath = SaveLoadManager.GetTemporarySavePath();
		Debug.Log("Copying save data from local save " + saveGame + " to " + temporarySavePath);
		UserStorageUtils.CopyOperation copyOperation = localStorage.CopyFilesFromContainerAsync(saveGame, temporarySavePath);
		yield return copyOperation;
		if (!copyOperation.GetSuccessful())
		{
			SaveLoadManager.Error error = SaveLoadManager.ConvertResult(copyOperation.result);
			result.Set(new MigrateResult(success: false, error, createSlotResult.slotName, copyOperation.errorMessage));
			yield break;
		}
		Debug.Log("Attempting to save to the cloud...");
		UserStorageUtils.SaveOperation saveOperation;
		try
		{
			UserStorage userStorage = PlatformUtils.main.GetUserStorage();
			List<string> list = new List<string>();
			List<string> deletedFiles = new List<string>();
			string[] files = Directory.GetFiles(temporarySavePath, "*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				string text = files[i].Substring(temporarySavePath.Length + 1);
				list.Add(text);
				Debug.Log("Added " + text + " to filesToSave.");
			}
			saveOperation = userStorage.CopyFilesToContainerAsync(createSlotResult.slotName, temporarySavePath, list, deletedFiles, null);
		}
		catch (Exception ex)
		{
			result.Set(new MigrateResult(success: false, SaveLoadManager.Error.UnknownError, createSlotResult.slotName, ex.Message));
			yield break;
		}
		yield return saveOperation;
		if (!saveOperation.GetSuccessful())
		{
			SaveLoadManager.Error error2 = SaveLoadManager.ConvertResult(saveOperation.result);
			result.Set(new MigrateResult(success: false, error2, createSlotResult.slotName, saveOperation.errorMessage));
			yield break;
		}
		CoroutineTask<SaveLoadManager.LoadResult> loadSlotsOperation = SaveLoadManager.main.LoadSlotsAsync();
		yield return loadSlotsOperation;
		SaveLoadManager.LoadResult result2 = loadSlotsOperation.GetResult();
		if (!result2.success)
		{
			result.Set(new MigrateResult(success: false, result2.error, createSlotResult.slotName, result2.errorMessage));
		}
		else
		{
			result.Set(new MigrateResult(success: true, SaveLoadManager.Error.None, createSlotResult.slotName, ""));
		}
	}
}
