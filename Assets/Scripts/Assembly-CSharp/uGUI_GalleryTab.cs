using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class uGUI_GalleryTab : uGUI_PDATab, uGUI_IIconGridManager, IScreenshotClient, uGUI_INavigableIconGrid
{
	public delegate void ImageSelectListener(string image);

	private class IconData
	{
		public string id;

		public string tooltip;

		public DateTime date;

		public IconData(string id, string tooltip, DateTime date)
		{
			this.id = id;
			this.tooltip = tooltip;
			this.date = date;
		}
	}

	private class IconDataComparer : IComparer<IconData>
	{
		public enum SortBy
		{
			DateDescending = 0,
			DateAscending = 1,
			Name = 2,
			NameDescending = 3
		}

		private SortBy sortBy;

		private int dir = 1;

		public void Initialize(SortBy sortBy)
		{
			this.sortBy = sortBy;
			switch (sortBy)
			{
			case SortBy.DateDescending:
				dir = -1;
				break;
			case SortBy.DateAscending:
				dir = 1;
				break;
			case SortBy.Name:
				dir = 1;
				break;
			case SortBy.NameDescending:
				dir = -1;
				break;
			}
		}

		public int Compare(IconData icon1, IconData icon2)
		{
			if (sortBy == SortBy.DateAscending || sortBy == SortBy.DateDescending)
			{
				if (icon1.date < icon2.date)
				{
					return -dir;
				}
				if (icon1.date > icon2.date)
				{
					return dir;
				}
				return 0;
			}
			if (sortBy == SortBy.Name || sortBy == SortBy.NameDescending)
			{
				return dir * icon1.id.CompareTo(icon2.id);
			}
			return 0;
		}
	}

	[AssertLocalization]
	private const string galleryLabelKey = "GalleryLabel";

	[AssertLocalization(1)]
	private const string galleryInstructionsKey = "PDAGalleryTabInstructions";

	[AssertLocalization]
	private const string deleteConfirmationKey = "PDAGalleryTabDeleteConfirmation";

	[AssertNotNull]
	public CanvasGroup content;

	[AssertNotNull]
	public TextMeshProUGUI galleryLabel;

	[AssertNotNull]
	public uGUI_IconGrid iconGrid;

	[AssertNotNull]
	public ScrollRect scrollRect;

	[AssertNotNull]
	public GameObject thumbnailsCanvasGO;

	[AssertNotNull]
	public GameObject fullScreenCanvasGO;

	[AssertNotNull]
	public RectTransform fullScreenCanvas;

	[AssertNotNull]
	public Button buttonFullscreen;

	[AssertNotNull]
	public Button buttonBack;

	[AssertNotNull]
	public Button buttonSelect;

	[AssertNotNull]
	public TextMeshProUGUI buttonSelectText;

	[AssertNotNull]
	public SimpleTooltip buttonSelectTooltip;

	[AssertNotNull]
	public Button buttonShare;

	[AssertNotNull]
	public Button buttonRemove;

	[AssertNotNull]
	public GameObject buttonSelectGO;

	[AssertNotNull]
	public GameObject buttonShareGO;

	[AssertNotNull]
	public GameObject buttonDeleteGO;

	[AssertNotNull]
	public TextMeshProUGUI instructions;

	[AssertNotNull]
	public GameObject instructionsGO;

	[AssertNotNull]
	public RawImage fullScreenImage;

	[AssertNotNull]
	public Sprite thumbnailBackground;

	[AssertLocalization]
	private const string screenshotTakenNotification = "PDAGalleryScreenshotTaken";

	[AssertLocalization]
	private const string screenshotDeniedNotification = "PDAGalleryScreenshotDenied";

	[AssertNotNull]
	public TextMeshProUGUI screenshotCountText;

	public Vector2 thumbnailSize = new Vector2(204f, 126f);

	public Vector2 thumbnailSpace = new Vector2(10f, 17f);

	public Vector2 thumbnailBorder = new Vector2(8f, 8f);

	public float thumbnailBackgroundRadius = 4f;

	public Color colorNormal = new Color(0.463f, 0.463f, 0.463f, 0.784f);

	public Color colorHover = new Color(0.443f, 0.443f, 0.443f, 1f);

	public Color colorPress = new Color(0.361f, 0.361f, 0.361f, 1f);

	private static IconDataComparer iconDataComparer = new IconDataComparer();

	private string fullScreenImageID;

	private Dictionary<string, IconData> icons = new Dictionary<string, IconData>();

	private IconDataComparer.SortBy sortBy = IconDataComparer.SortBy.NameDescending;

	private bool unsorted = true;

	private ImageSelectListener selectListener;

	private SelectableWrapper selectableFullscreenWrapper;

	private List<ISelectable> navigationFullscreen;

	private bool isConfirmationDialogShown;

	private readonly List<IconData> sortData = new List<IconData>();

	public override int notificationsCount
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<string, IconData> icon in icons)
			{
				if (NotificationManager.main.Contains(NotificationManager.Group.Gallery, icon.Key))
				{
					num++;
				}
			}
			return num;
		}
	}

	private bool isFullScreen => fullScreenImageID != null;

	bool uGUI_INavigableIconGrid.ShowSelector => true;

	bool uGUI_INavigableIconGrid.EmulateRaycast => true;

	protected override void Awake()
	{
		iconGrid.iconSize = thumbnailSize;
		iconGrid.iconBorder = thumbnailBorder;
		iconGrid.minSpaceX = thumbnailSpace.x;
		iconGrid.minSpaceY = thumbnailSpace.y;
		iconGrid.Initialize(this);
		SpriteManager.SetProceduralSlice9Grid(thumbnailBackground);
		iconGrid.SetIconBackgroundImage(thumbnailBackground);
		iconGrid.SetIconBackgroundRadius(thumbnailBackgroundRadius);
		iconGrid.SetIconBackgroundColors(colorNormal, colorHover, colorPress);
		fullScreenCanvasGO.SetActive(value: false);
		thumbnailsCanvasGO.SetActive(value: true);
		buttonSelect.onClick.AddListener(OnSelect);
		Dictionary<string, ScreenshotManager.Thumbnail>.Enumerator thumbnails = ScreenshotManager.GetThumbnails();
		while (thumbnails.MoveNext())
		{
			KeyValuePair<string, ScreenshotManager.Thumbnail> current = thumbnails.Current;
			string key = current.Key;
			ScreenshotManager.Thumbnail value = current.Value;
			OnThumbnailAdd(key, value);
		}
		((uGUI_IIconGridManager)this).OnSortRequested();
		ScreenshotManager.onThumbnailAdd += OnThumbnailAdd;
		ScreenshotManager.onThumbnailUpdate += OnThumbnailUpdate;
		ScreenshotManager.onThumbnailRemove += OnThumbnailRemove;
		ScreenshotManager.onScreenshotTaken += OnScreenshotTaken;
		ScreenshotManager.onScreenshotDenied += OnScreenshotDenied;
		UpdateButtonsState();
		Close();
		InitNavigation();
	}

	private void OnDestroy()
	{
		ScreenshotManager.onThumbnailAdd -= OnThumbnailAdd;
		ScreenshotManager.onThumbnailUpdate -= OnThumbnailUpdate;
		ScreenshotManager.onThumbnailRemove -= OnThumbnailRemove;
		ScreenshotManager.onScreenshotTaken -= OnScreenshotTaken;
		ScreenshotManager.onScreenshotDenied -= OnScreenshotDenied;
	}

	public void SetSelectListener(ImageSelectListener listener, string text, string tooltip)
	{
		selectListener = listener;
		buttonSelectText.text = Language.main.Get(text);
		buttonSelectTooltip.text = tooltip;
	}

	public override void OnClosePDA()
	{
		CloseDeleteConfirmationDialog();
		selectListener = null;
		ExitFullScreenMode();
	}

	public override void Open()
	{
		content.SetVisible(visible: true);
		UpdateScreenshotText();
	}

	public override void Close()
	{
		CloseDeleteConfirmationDialog();
		scrollRect.velocity = Vector2.zero;
		ExitFullScreenMode();
		content.SetVisible(visible: false);
	}

	public override void OnUpdate(bool isOpen)
	{
		if (isOpen)
		{
			instructionsGO.SetActive(iconGrid.GetCount() == 0);
		}
	}

	public override uGUI_INavigableIconGrid GetInitialGrid()
	{
		return iconGrid;
	}

	public override bool OnButtonDown(GameInput.Button button)
	{
		if (selectListener == new ImageSelectListener((pda.GetTab(PDATab.TimeCapsule) as uGUI_TimeCapsuleTab).SelectImage))
		{
			if (button == GameInput.Button.UICancel)
			{
				pda.OpenTab(PDATab.TimeCapsule);
				return true;
			}
			return false;
		}
		return base.OnButtonDown(button);
	}

	public override void OnLanguageChanged()
	{
		galleryLabel.text = Language.main.Get("GalleryLabel");
		UpdateInstructions();
	}

	public override void OnBindingsChanged()
	{
		UpdateInstructions();
	}

	private void OnThumbnailAdd(string id, ScreenshotManager.Thumbnail thumbnail)
	{
		if (!icons.ContainsKey(id))
		{
			if (iconGrid.AddItem(id, Sprite.Create(thumbnail.texture, new Rect(0f, 0f, thumbnail.texture.width, thumbnail.texture.height), new Vector2(0.5f, 0.5f))))
			{
				IconData value = new IconData(id, Path.GetFileName(id), thumbnail.lastWriteTimeUtc);
				icons.Add(id, value);
				unsorted = true;
				iconGrid.RegisterNotificationTarget(id, NotificationManager.Group.Gallery, id);
			}
			UpdateScreenshotText();
		}
	}

	private void OnThumbnailRemove(string fileName)
	{
		if (icons.Remove(fileName))
		{
			iconGrid.RemoveItem(fileName);
			unsorted = true;
		}
		UpdateScreenshotText();
	}

	private void OnThumbnailUpdate(string fileName, ScreenshotManager.Thumbnail thumbnail)
	{
		if (icons.TryGetValue(fileName, out var _))
		{
			iconGrid.SetSprite(fileName, Sprite.Create(thumbnail.texture, new Rect(0f, 0f, thumbnail.texture.width, thumbnail.texture.height), new Vector2(0.5f, 0.5f)));
			unsorted = true;
		}
	}

	private void OnScreenshotTaken(string fileName)
	{
		ErrorMessage.AddError(Language.main.Get("PDAGalleryScreenshotTaken"));
	}

	private void OnScreenshotDenied()
	{
		ErrorMessage.AddError(Language.main.Get("PDAGalleryScreenshotDenied"));
	}

	public void OnScroll(BaseEventData eventData)
	{
		if (eventData is PointerEventData pointerEventData)
		{
			float y = pointerEventData.scrollDelta.y;
			if (y > 0f)
			{
				OnPrevious();
			}
			else if (y < 0f)
			{
				OnNext();
			}
		}
	}

	public void OnPrevious()
	{
		int index = iconGrid.GetIndex(fullScreenImageID);
		if (index < 0)
		{
			ExitFullScreenMode();
		}
		else if (index > 0)
		{
			EnterFullScreenMode(iconGrid.GetIdentifier(index - 1));
		}
	}

	public void OnNext()
	{
		int index = iconGrid.GetIndex(fullScreenImageID);
		if (index < 0)
		{
			ExitFullScreenMode();
		}
		else
		{
			EnterFullScreenMode(iconGrid.GetIdentifier(index + 1));
		}
	}

	public void OnExitFullscreen()
	{
		ExitFullScreenMode();
	}

	public void OnSelect()
	{
		OnSelect(fullScreenImageID);
	}

	public void OnSelect(string image)
	{
		ExitFullScreenMode();
		if (selectListener != null)
		{
			selectListener(image);
		}
	}

	public void OnShare()
	{
		if (ScreenshotManager.ShareScreenshot(fullScreenImageID))
		{
			UpdateButtonsState();
		}
	}

	public void OnDelete(BaseEventData eventData)
	{
		if (eventData is PointerEventData)
		{
			ShowDeleteConfirmationDialog(fullScreenImageID);
		}
	}

	void uGUI_IIconGridManager.GetTooltip(string id, TooltipData data)
	{
		if (Application.platform != RuntimePlatform.PS4)
		{
			if (icons.TryGetValue(id, out var value))
			{
				TooltipFactory.Label(value.tooltip, data.prefix);
			}
			else
			{
				TooltipFactory.Label(id, data.prefix);
			}
		}
	}

	void uGUI_IIconGridManager.OnPointerEnter(string id)
	{
	}

	void uGUI_IIconGridManager.OnPointerExit(string id)
	{
	}

	void uGUI_IIconGridManager.OnPointerClick(string id, int button)
	{
		switch (button)
		{
		case 0:
			EnterFullScreenMode(id);
			return;
		case 1:
			if (GameInput.PrimaryDevice == GameInput.Device.Controller)
			{
				ClosePDA();
				return;
			}
			break;
		}
		if (button == 2 && GameInput.PrimaryDevice == GameInput.Device.Controller)
		{
			ShowDeleteConfirmationDialog(id);
		}
		else if (button == 3 && GameInput.PrimaryDevice == GameInput.Device.Controller && IsReadyForSelect())
		{
			OnSelect(id);
		}
	}

	void uGUI_IIconGridManager.OnSortRequested()
	{
		if (unsorted)
		{
			iconDataComparer.Initialize(sortBy);
			sortData.Clear();
			Dictionary<string, IconData>.Enumerator enumerator = icons.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, IconData> current = enumerator.Current;
				sortData.Add(current.Value);
			}
			sortData.Sort(iconDataComparer);
			int i = 0;
			for (int count = sortData.Count; i < count; i++)
			{
				IconData iconData = sortData[i];
				iconGrid.SetIndex(iconData.id, i);
			}
			sortData.Clear();
			unsorted = false;
		}
	}

	void IScreenshotClient.OnProgress(string fileName, float progress)
	{
	}

	void IScreenshotClient.OnDone(string fileName, Texture2D texture)
	{
		if (isFullScreen)
		{
			if (texture == null)
			{
				ExitFullScreenMode();
				return;
			}
			SetFullScreenImage(texture);
			UpdateButtonsState();
		}
	}

	void IScreenshotClient.OnRemoved(string fileName)
	{
		if (isFullScreen)
		{
			ExitFullScreenMode();
		}
	}

	private void UpdateButtonsState()
	{
		buttonSelectGO.SetActive(IsReadyForSelect());
		bool supportsSharingScreenshots = PlatformUtils.main.GetServices().GetSupportsSharingScreenshots();
		buttonShareGO.SetActive(supportsSharingScreenshots);
		buttonDeleteGO.SetActive(value: true);
	}

	private bool IsReadyForSelect()
	{
		return selectListener != null;
	}

	private void SetFullScreenImage(Texture2D texture)
	{
		fullScreenImage.texture = texture;
		if (texture == null)
		{
			fullScreenImage.enabled = false;
			return;
		}
		fullScreenImage.enabled = true;
		Rect rect = fullScreenCanvas.rect;
		MathExtensions.RectFit(texture.width, texture.height, rect.width, rect.height, RectScaleMode.Envelope, out var scale, out var offset);
		fullScreenImage.uvRect = new Rect(offset.x, offset.y, scale.x, scale.y);
	}

	private void EnterFullScreenMode(string id)
	{
		if (id != null && !string.Equals(id, fullScreenImageID))
		{
			Texture2D thumbnail = ScreenshotManager.GetThumbnail(id);
			SetFullScreenImage(thumbnail);
			thumbnailsCanvasGO.SetActive(value: false);
			fullScreenCanvasGO.SetActive(value: true);
			ScreenshotManager.RemoveRequest(fullScreenImageID, this);
			fullScreenImageID = id;
			ScreenshotManager.AddRequest(id, this, highPriority: true);
			UpdateButtonsState();
			if (GamepadInputModule.current.GetCurrentGrid() != this)
			{
				GamepadInputModule.current.SetCurrentGrid(this);
			}
		}
	}

	private void ExitFullScreenMode()
	{
		if (isFullScreen)
		{
			fullScreenImage.texture = null;
			ScreenshotManager.RemoveRequest(fullScreenImageID, this);
			fullScreenCanvasGO.SetActive(value: false);
			thumbnailsCanvasGO.SetActive(value: true);
			GamepadInputModule.current.SetCurrentGrid(iconGrid);
			if (GamepadInputModule.current.isControlling)
			{
				iconGrid.SelectItem(iconGrid.GetIcon(fullScreenImageID));
			}
			fullScreenImageID = null;
		}
	}

	private void OnRemove(string toRemove)
	{
		if (ScreenshotManager.IsScreenshotBeingRequested(toRemove))
		{
			return;
		}
		if (isFullScreen)
		{
			int count = iconGrid.GetCount();
			if (count > 0)
			{
				int index = iconGrid.GetIndex(fullScreenImageID);
				int num = Mathf.Clamp(index + 1, 0, count - 1);
				if (num < 0 || num == index)
				{
					num = Mathf.Clamp(index - 1, 0, count - 1);
				}
				if (num < 0 || num == index)
				{
					ExitFullScreenMode();
				}
				else
				{
					EnterFullScreenMode(iconGrid.GetIdentifier(num));
				}
			}
		}
		ScreenshotManager.Delete(toRemove);
		UpdateScreenshotText();
	}

	private void UpdateInstructions()
	{
		instructions.text = LanguageCache.GetButtonFormat("PDAGalleryTabInstructions", GameInput.Button.TakePicture);
	}

	public object GetSelectedItem()
	{
		return UISelection.selected;
	}

	public Graphic GetSelectedIcon()
	{
		if (UISelection.selected == null)
		{
			return null;
		}
		RectTransform rect = UISelection.selected.GetRect();
		if (rect == null)
		{
			return null;
		}
		return rect.GetComponent<Graphic>();
	}

	public void SelectItem(object item)
	{
		UISelection.selected = item as ISelectable;
	}

	public void DeselectItem()
	{
		UISelection.selected = null;
	}

	public bool SelectFirstItem()
	{
		UISelection.selected = selectableFullscreenWrapper;
		return true;
	}

	public bool SelectItemClosestToPosition(Vector3 worldPos)
	{
		return false;
	}

	public bool SelectItemInDirection(int dirX, int dirY)
	{
		if (UISelection.selected == null)
		{
			return SelectFirstItem();
		}
		if (dirX == 0 && dirY == 0)
		{
			return false;
		}
		if (dirY == 0 && UISelection.selected as SelectableWrapper == selectableFullscreenWrapper)
		{
			if (dirX > 0)
			{
				OnNext();
				return true;
			}
			OnPrevious();
			return true;
		}
		ISelectable selectable = UISelection.FindSelectable(fullScreenCanvas, new Vector2(dirX, -dirY), UISelection.selected, navigationFullscreen, fromEdge: false);
		if (selectable != null)
		{
			SelectItem(selectable);
			return true;
		}
		return false;
	}

	public uGUI_INavigableIconGrid GetNavigableGridInDirection(int dirX, int dirY)
	{
		return null;
	}

	private void InitNavigation()
	{
		selectableFullscreenWrapper = new SelectableWrapper(buttonFullscreen, delegate(GameInput.Button button)
		{
			switch (button)
			{
			case GameInput.Button.UISubmit:
			case GameInput.Button.UICancel:
				ExitFullScreenMode();
				return true;
			case GameInput.Button.UIClear:
				ShowDeleteConfirmationDialog(fullScreenImageID);
				return true;
			case GameInput.Button.UIAssign:
				if (IsReadyForSelect())
				{
					OnSelect();
					return true;
				}
				return false;
			default:
				return false;
			}
		});
		SelectableWrapper item = new SelectableWrapper(buttonBack, delegate(GameInput.Button button)
		{
			if ((uint)(button - 28) <= 1u)
			{
				ExitFullScreenMode();
				return true;
			}
			return false;
		});
		SelectableWrapper item2 = new SelectableWrapper(buttonSelect, delegate(GameInput.Button button)
		{
			switch (button)
			{
			case GameInput.Button.UISubmit:
				OnSelect();
				return true;
			case GameInput.Button.UICancel:
				ExitFullScreenMode();
				return true;
			default:
				return false;
			}
		});
		SelectableWrapper item3 = new SelectableWrapper(buttonShare, delegate(GameInput.Button button)
		{
			switch (button)
			{
			case GameInput.Button.UISubmit:
				OnShare();
				return true;
			case GameInput.Button.UICancel:
				ExitFullScreenMode();
				return true;
			default:
				return false;
			}
		});
		SelectableWrapper item4 = new SelectableWrapper(buttonRemove, delegate(GameInput.Button button)
		{
			switch (button)
			{
			case GameInput.Button.UISubmit:
				ShowDeleteConfirmationDialog(fullScreenImageID);
				return true;
			case GameInput.Button.UICancel:
				ExitFullScreenMode();
				return true;
			default:
				return false;
			}
		});
		navigationFullscreen = new List<ISelectable> { selectableFullscreenWrapper, item, item2, item3, item4 };
	}

	private void UpdateScreenshotText()
	{
		if (screenshotCountText != null)
		{
			if (ScreenshotManager.IsLimitingScreenhots())
			{
				screenshotCountText.text = $"{ScreenshotManager.GetNumScreenshots()} / {ScreenshotManager.GetMaxNumScreenshots()}";
			}
			else
			{
				screenshotCountText.gameObject.SetActive(value: false);
			}
		}
	}

	private void ShowDeleteConfirmationDialog(string toRemove)
	{
		if (isConfirmationDialogShown)
		{
			return;
		}
		if (uGUI_PDA.main.dialog.open)
		{
			Debug.LogWarning("Overriding already opened dialog");
		}
		uGUI_PDA.main.dialog.Show(Language.main.Get("PDAGalleryTabDeleteConfirmation"), delegate(int option)
		{
			if (option == 1)
			{
				OnRemove(toRemove);
			}
			isConfirmationDialogShown = false;
		}, Language.main.Get("No"), Language.main.Get("Yes"));
		isConfirmationDialogShown = true;
	}

	private void CloseDeleteConfirmationDialog()
	{
		if (isConfirmationDialogShown)
		{
			uGUI_PDA.main.dialog.Close();
			isConfirmationDialogShown = false;
		}
	}
}
