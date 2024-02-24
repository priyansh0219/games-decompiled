using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuLoadPanel : MonoBehaviour, ILoadButtonDelegate
{
	public GameObject saveInstance;

	public GameObject savedGameArea;

	public GameObject scrollBar;

	[AssertLocalization]
	private const string slotEmptyMessage = "SlotEmpty";

	[AssertLocalization]
	private const string saveDamagedMessage = "DamagedSavedGame";

	[AssertLocalization(1)]
	private const string saveChangesetMessage = "IncompatibleChangesetSavedGame";

	string ILoadButtonDelegate.rightSideGroup => "SavedGames";

	private void OnEnable()
	{
		Language.OnLanguageChanged += OnLanguageChanged;
		GameInput.OnPrimaryDeviceChanged += OnPrimaryDeviceChanged;
		PopulateSaveList();
	}

	private void PopulateSaveList()
	{
		string[] possibleSlotNames = SaveLoadManager.main.GetPossibleSlotNames();
		GameMode[] possibleSlotGameModes = SaveLoadManager.main.GetPossibleSlotGameModes();
		bool active = !GameInput.IsPrimaryDeviceGamepad();
		for (int i = 0; i < possibleSlotNames.Length; i++)
		{
			string saveGame = possibleSlotNames[i];
			GameObject obj = Object.Instantiate(saveInstance);
			obj.transform.SetParent(savedGameArea.transform);
			obj.transform.localPosition = new Vector3(0f, 0f, 0f);
			obj.transform.localScale = new Vector3(1f, 1f, 1f);
			obj.transform.localRotation = Quaternion.identity;
			MainMenuLoadButton component = obj.GetComponent<MainMenuLoadButton>();
			component.saveGame = saveGame;
			component.gameMode = possibleSlotGameModes[i];
			component.deleteButton.SetActive(active);
			component.buttonDelegate = this;
			UpdateLoadButtonState(component);
		}
		scrollBar.SetActive(possibleSlotNames.Length >= 5);
		SortSavesByDate();
	}

	private void OnDisable()
	{
		Language.OnLanguageChanged -= OnLanguageChanged;
		GameInput.OnPrimaryDeviceChanged -= OnPrimaryDeviceChanged;
		MainMenuLoadButton[] allComponentsInChildren = savedGameArea.GetAllComponentsInChildren<MainMenuLoadButton>();
		for (int i = 0; i < allComponentsInChildren.Length; i++)
		{
			Object.Destroy(allComponentsInChildren[i].gameObject);
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
		SaveLoadManager.GameInfo gameInfo = SaveLoadManager.main.GetGameInfo(lb.saveGame);
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
		Texture2D screenshot = gameInfo.GetScreenshot();
		lb.sessionId = gameInfo.session;
		lb.changeSet = gameInfo.changeSet;
		lb.gameMode = gameInfo.gameMode;
		if (screenshot != null)
		{
			lb.saveGameScreenshot.texture = screenshot;
		}
		lb.saveGameTimeText.text = text3;
		lb.saveGameLengthText.text = text2;
		lb.saveGameModeText.text = text;
		lb.saveIcons.FindChild("SavedCyclops").gameObject.SetActive(gameInfo.cyclopsPresent);
		lb.saveIcons.FindChild("SavedSeamoth").gameObject.SetActive(gameInfo.seamothPresent);
		lb.saveIcons.FindChild("SavedExo").gameObject.SetActive(gameInfo.exosuitPresent);
		lb.saveIcons.FindChild("SavedRocket").gameObject.SetActive(gameInfo.rocketPresent);
		if (gameInfo.isFallback)
		{
			lb.saveGameLengthText.text = "<color=#000000FF>" + Language.main.Get("DamagedSavedGame") + "</color>";
		}
		if (!gameInfo.IsValid())
		{
			lb.load.GetComponent<Image>().color = Color.red;
			lb.loadButton.SetActive(value: false);
			lb.saveGameLengthText.text = "<color=#ff0000ff>" + Language.main.GetFormat("IncompatibleChangesetSavedGame", gameInfo.changeSet) + "</color>";
		}
		if (gameInfo.corrupted)
		{
			lb.load.GetComponent<Image>().color = Color.red;
			lb.loadButton.SetActive(value: false);
			lb.saveGameLengthText.text = "<color=#ff0000ff>" + Language.main.Get("DamagedSavedGame") + "</color>";
		}
	}

	public void RefreshSaveList()
	{
		for (int num = savedGameArea.transform.childCount - 1; num >= 0; num--)
		{
			Transform child = savedGameArea.transform.GetChild(num);
			if (child.name != "NewGame")
			{
				Object.Destroy(child.gameObject);
			}
		}
		PopulateSaveList();
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

	SaveLoadManager.GameInfo ILoadButtonDelegate.GetGameInfo(string slotName)
	{
		return SaveLoadManager.main.GetGameInfo(slotName);
	}

	IEnumerator ILoadButtonDelegate.LoadGameAsync(MainMenuLoadButton button)
	{
		return uGUI_MainMenu.main.LoadGameAsync(button.saveGame, button.sessionId, button.changeSet, button.gameMode);
	}

	IEnumerator ILoadButtonDelegate.ClearSlotAsync(MainMenuLoadButton button)
	{
		return SaveLoadManager.main.ClearSlotAsync(button.saveGame);
	}
}
