using System;
using System.Collections;
using TMPro;
using UWE;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuLoadButton : MonoBehaviour
{
	private enum target
	{
		left = 0,
		right = 1,
		centre = 2
	}

	public string saveGame;

	public string sessionId;

	public int changeSet;

	public GameMode gameMode;

	[AssertNotNull]
	public GameObject delete;

	[AssertNotNull]
	public GameObject load;

	[AssertNotNull]
	public GameObject deleteButton;

	[AssertNotNull]
	public Selectable cancelDeleteButton;

	private GameObject upgradeWarning;

	[AssertNotNull]
	public TextMeshProUGUI saveGameModeText;

	[AssertNotNull]
	public TextMeshProUGUI saveGameTimeText;

	[AssertNotNull]
	public TextMeshProUGUI saveGameLengthText;

	[AssertNotNull]
	public GameObject saveIcons;

	[AssertNotNull]
	public RawImage saveGameScreenshot;

	[AssertNotNull]
	public GameObject loadButton;

	private CanvasGroup loadCg;

	private CanvasGroup deleteCg;

	private GameObject contentArea;

	[AssertNotNull]
	public TextMeshProUGUI[] labelsForColorSwap;

	[NonSerialized]
	public bool deleting;

	public ILoadButtonDelegate buttonDelegate;

	private GridLayoutGroup gridLayoutGroup;

	private ScrollRect scrollRect;

	private MainMenuLoadMenu mainMenuLoadMenu;

	public int scrollIndex;

	public float animTime = 0.25f;

	public float alphaPower = 1.5f;

	public float posPower = 2f;

	public float slotAnimTime = 0.3f;

	public float slotPosPower = 1.5f;

	public float shiftDistanace = 25f;

	public float slotShiftDistance = 80f;

	private bool hasShifted;

	private Vector3 centrePos;

	private void Start()
	{
		loadCg = load.GetComponent<CanvasGroup>();
		deleteCg = delete.GetComponent<CanvasGroup>();
		contentArea = base.transform.parent.gameObject;
		Vector3 localPosition = base.gameObject.transform.localPosition;
		Vector3 localPosition2 = new Vector3(localPosition.x, localPosition.y, 0f);
		base.gameObject.transform.localPosition = localPosition2;
		gridLayoutGroup = contentArea.GetComponent<GridLayoutGroup>();
		scrollRect = contentArea.transform.parent.GetComponentInParent<ScrollRect>();
		mainMenuLoadMenu = GetComponentInParent<MainMenuLoadMenu>();
	}

	private void OnDestroy()
	{
		RestoreParentsSettings();
		if (deleting)
		{
			mainMenuLoadMenu.isDeletingInProgress = false;
		}
	}

	public bool NeedsUpgrade()
	{
		return BatchUpgrade.NeedsUpgrade(changeSet);
	}

	public bool IsEmpty()
	{
		return buttonDelegate.GetGameInfo(saveGame) == null;
	}

	public void Load()
	{
		if (!IsEmpty())
		{
			CoroutineHost.StartCoroutine(buttonDelegate.LoadGameAsync(this));
		}
	}

	public void RequestDelete()
	{
		scrollIndex = mainMenuLoadMenu.GetSelectedIndex();
		uGUI_MainMenu.main.OnRightSideOpened(deleteCg.gameObject);
		uGUI_LegendBar.ClearButtons();
		uGUI_LegendBar.ChangeButton(0, GameInput.FormatButton(GameInput.Button.UICancel), Language.main.GetFormat("Back"));
		uGUI_LegendBar.ChangeButton(1, GameInput.FormatButton(GameInput.Button.UISubmit), Language.main.GetFormat("ItemSelectorSelect"));
		StartCoroutine(ShiftAlpha(loadCg, 0f, animTime, alphaPower, toActive: false));
		StartCoroutine(ShiftAlpha(deleteCg, 1f, animTime, alphaPower, toActive: true, cancelDeleteButton));
		StartCoroutine(ShiftPos(loadCg, target.left, target.centre, animTime, posPower));
		StartCoroutine(ShiftPos(deleteCg, target.centre, target.right, animTime, posPower));
	}

	public void CancelDelete()
	{
		MainMenuRightSide.main.OpenGroup(buttonDelegate.rightSideGroup);
		if (GameInput.IsPrimaryDeviceGamepad())
		{
			mainMenuLoadMenu.SelectItemByIndex(scrollIndex);
		}
		StartCoroutine(ShiftAlpha(loadCg, 1f, animTime, alphaPower, toActive: true));
		StartCoroutine(ShiftAlpha(deleteCg, 0f, animTime, alphaPower, toActive: false));
		StartCoroutine(ShiftPos(loadCg, target.centre, target.left, animTime, posPower));
		StartCoroutine(ShiftPos(deleteCg, target.right, target.centre, animTime, posPower));
	}

	public void Delete()
	{
		deleting = true;
		MainMenuRightSide.main.OpenGroup(buttonDelegate.rightSideGroup);
		if (GameInput.IsPrimaryDeviceGamepad() && !mainMenuLoadMenu.SelectItemInDirection(scrollIndex, 0, 1))
		{
			mainMenuLoadMenu.SelectItemInDirection(scrollIndex, 0, -1);
		}
		mainMenuLoadMenu.isDeletingInProgress = true;
		StartCoroutine(ShiftPos(deleteCg, target.left, target.centre, animTime, posPower));
		StartCoroutine(ShiftAlpha(deleteCg, 0f, animTime, alphaPower, toActive: false));
		StartCoroutine(FreeSlot(contentArea, slotAnimTime, slotPosPower));
		Debug.Log("Save / Load: User requested deletion of save instance with path " + saveGame);
		CoroutineHost.StartCoroutine(buttonDelegate.ClearSlotAsync(this));
	}

	private IEnumerator ShiftAlpha(CanvasGroup cg, float targetAlpha, float animTime, float power, bool toActive, Selectable buttonToSelect = null)
	{
		float start = Time.time;
		while (Time.time - start < animTime)
		{
			float f = Mathf.Clamp01((Time.time - start) / animTime);
			cg.alpha = Mathf.Lerp(cg.alpha, targetAlpha, Mathf.Pow(f, power));
			yield return null;
		}
		cg.alpha = targetAlpha;
		if (toActive)
		{
			cg.interactable = true;
			cg.blocksRaycasts = true;
		}
		else
		{
			cg.interactable = false;
			cg.blocksRaycasts = false;
		}
		if (GameInput.IsPrimaryDeviceGamepad() && buttonToSelect != null)
		{
			GamepadInputModule.current.SelectItem(buttonToSelect);
		}
	}

	private IEnumerator ShiftPos(CanvasGroup cg, target target, target origin, float animTime, float power)
	{
		Vector3 targetPos = new Vector3(0f, 0f, 0f);
		Vector3 localPosition = new Vector3(0f, 0f, 0f);
		if (!hasShifted)
		{
			centrePos = cg.transform.localPosition;
			hasShifted = true;
		}
		switch (target)
		{
		case target.left:
			targetPos = centrePos + new Vector3(0f - shiftDistanace, 0f, 0f);
			break;
		case target.right:
			targetPos = centrePos + new Vector3(shiftDistanace, 0f, 0f);
			break;
		case target.centre:
			targetPos = centrePos;
			break;
		}
		switch (origin)
		{
		case target.left:
			localPosition = centrePos + new Vector3(0f - shiftDistanace, 0f, 0f);
			break;
		case target.right:
			localPosition = centrePos + new Vector3(shiftDistanace, 0f, 0f);
			break;
		case target.centre:
			localPosition = centrePos;
			break;
		}
		cg.transform.localPosition = localPosition;
		float start = Time.time;
		while (Time.time - start < animTime)
		{
			float f = Mathf.Clamp01((Time.time - start) / animTime);
			cg.transform.localPosition = Vector3.Lerp(cg.transform.localPosition, targetPos, Mathf.Pow(f, power));
			yield return null;
		}
		cg.transform.localPosition = targetPos;
	}

	private IEnumerator FreeSlot(GameObject ca, float animTime, float power)
	{
		int siblingIndex = base.gameObject.transform.GetSiblingIndex();
		scrollRect.enabled = false;
		gridLayoutGroup.enabled = false;
		foreach (Transform item in ca.transform)
		{
			if (item.GetSiblingIndex() > siblingIndex)
			{
				RectTransform component = item.GetComponent<RectTransform>();
				StartCoroutine(Bump(component, animTime, power));
			}
		}
		yield return new WaitForSeconds(animTime);
		RestoreParentsSettings();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void RestoreParentsSettings()
	{
		gridLayoutGroup.enabled = true;
		scrollRect.enabled = true;
	}

	private IEnumerator Bump(RectTransform rt, float animTime, float power)
	{
		float start = Time.time;
		Vector3 targetPos = rt.transform.localPosition + new Vector3(0f, slotShiftDistance, 0f);
		while (Time.time - start < animTime)
		{
			float f = Mathf.Clamp01((Time.time - start) / animTime);
			rt.transform.localPosition = Vector3.Lerp(rt.transform.localPosition, targetPos, Mathf.Pow(f, power));
			yield return null;
		}
		rt.transform.localPosition = targetPos;
	}

	public void onCursorEnter()
	{
		TextMeshProUGUI[] array = labelsForColorSwap;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = Color.black;
		}
	}

	public void onCursorLeave()
	{
		TextMeshProUGUI[] array = labelsForColorSwap;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = Color.white;
		}
	}
}
