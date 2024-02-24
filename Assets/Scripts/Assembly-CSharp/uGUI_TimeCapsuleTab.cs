using System;
using Gendarme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_TimeCapsuleTab : uGUI_PDATab
{
	public Color colorValid = Color.green;

	public Color colorInvalid = Color.red;

	[AssertNotNull]
	public TextMeshProUGUI timeCapsuleLabel;

	[AssertNotNull]
	public TextMeshProUGUI statusLabel;

	[AssertNotNull]
	public TextMeshProUGUI statusText;

	[AssertNotNull]
	public TextMeshProUGUI storageLabel;

	[AssertNotNull]
	public uGUI_ItemsContainerView storage;

	[AssertNotNull]
	public Toggle submitToggle;

	[AssertNotNull]
	public TextMeshProUGUI submitText;

	[AssertNotNull]
	public TextMeshProUGUI imageLabel;

	[AssertNotNull]
	public RawImage image;

	[AssertNotNull]
	public uGUI_InputField inputFieldText;

	[AssertNotNull]
	public TextMeshProUGUI inputFieldTextPlaceholderText;

	[AssertNotNull]
	public Texture2D defaultTexture;

	[AssertNotNull]
	public uGUI_NavigableControlGrid navigableControlGrid;

	[AssertNotNull]
	public GameObject inputFieldIconValid;

	[AssertNotNull]
	public GameObject inputFieldIconInvalid;

	[AssertNotNull]
	public GameObject itemsIconValid;

	[AssertNotNull]
	public GameObject itemsIconInvalid;

	[AssertNotNull]
	public GameObject screenShotValid;

	[AssertNotNull]
	public GameObject screenShotInvalid;

	[AssertNotNull]
	public GameObject capsuleIconValid;

	[AssertNotNull]
	public GameObject capsuleIconInvalid;

	private int cachedErrorLevel = -1;

	private int cachedHasItems = -1;

	private bool expectingImageSelection;

	private Material imageMaterial;

	private float imageAspect = 1f;

	public override int notificationsCount => 0;

	protected override void Awake()
	{
		imageMaterial = new Material(image.material);
		image.material = imageMaterial;
		Rect rect = image.rectTransform.rect;
		imageAspect = rect.width / rect.height;
		inputFieldText.characterLimit = 1000;
		inputFieldText.onValueChanged.AddListener(OnTextChanged);
		submitToggle.onValueChanged.AddListener(OnSubmitChanged);
		SetImageTexture(null);
		OnLanguageChanged();
	}

	private void UpdateCheckBoxes()
	{
		PlayerTimeCapsule main = PlayerTimeCapsule.main;
		if (!(main == null))
		{
			inputFieldIconValid.SetActive(main.hasText);
			inputFieldIconInvalid.SetActive(!main.hasText);
			itemsIconValid.SetActive(main.hasItems);
			itemsIconInvalid.SetActive(!main.hasItems);
			screenShotValid.SetActive(main.hasImage);
			screenShotInvalid.SetActive(!main.hasImage);
			capsuleIconValid.SetActive(main.IsValid());
			capsuleIconInvalid.SetActive(!main.IsValid());
		}
	}

	public override void OnClosePDA()
	{
		expectingImageSelection = false;
		storage.Uninit();
	}

	public override void Open()
	{
		base.gameObject.SetActive(value: true);
		PlayerTimeCapsule main = PlayerTimeCapsule.main;
		if (expectingImageSelection)
		{
			main.SetImage(null);
		}
		inputFieldText.text = main.text;
		submitToggle.isOn = main.submit;
		storage.Init(main.container);
		SetImageTexture(main.imageTexture);
		main.onTextureChanged = (PlayerTimeCapsule.OnTextureChanged)Delegate.Combine(main.onTextureChanged, new PlayerTimeCapsule.OnTextureChanged(SetImageTexture));
		UpdateCheckBoxes();
	}

	public override void Close()
	{
		base.gameObject.SetActive(value: false);
		PlayerTimeCapsule main = PlayerTimeCapsule.main;
		if (main != null)
		{
			main.onTextureChanged = (PlayerTimeCapsule.OnTextureChanged)Delegate.Remove(main.onTextureChanged, new PlayerTimeCapsule.OnTextureChanged(SetImageTexture));
		}
	}

	public override void OnUpdate(bool isOpen)
	{
		if (isOpen)
		{
			storage.DoUpdate();
			UpdateStatus();
		}
	}

	public override uGUI_INavigableIconGrid GetInitialGrid()
	{
		return navigableControlGrid;
	}

	[SuppressMessage("Gendarme.Rules.Naming", "AvoidRedundancyInMethodNameRule")]
	public override bool OnButtonDown(GameInput.Button button)
	{
		PDA pDA = Player.main.GetPDA();
		switch (button)
		{
		case GameInput.Button.UINextTab:
			return true;
		case GameInput.Button.UIPrevTab:
			return true;
		case GameInput.Button.UICancel:
			pDA.Close();
			return true;
		default:
			return false;
		}
	}

	private void OnTextChanged(string text)
	{
		PlayerTimeCapsule.main.text = text;
	}

	private void OnSubmitChanged(bool submit)
	{
		PlayerTimeCapsule.main.submit = submit;
	}

	private void UpdateStatus()
	{
		PlayerTimeCapsule main = PlayerTimeCapsule.main;
		if (main == null)
		{
			return;
		}
		int num = 0;
		num += (main.hasItems ? 1 : 0);
		num += (main.hasText ? 1 : 0);
		num += (main.hasImage ? 1 : 0);
		if (cachedErrorLevel != num)
		{
			cachedErrorLevel = num;
			UpdateCheckBoxes();
			switch (num)
			{
			case 0:
				statusText.text = Language.main.Get("TimeCapsuleNotValid");
				statusText.color = colorInvalid;
				break;
			case 1:
				statusText.text = Language.main.Get("TimeCapsuleErrorOne");
				statusText.color = colorInvalid;
				break;
			case 2:
				statusText.text = Language.main.Get("TimeCapsuleErrorTwo");
				statusText.color = colorInvalid;
				break;
			case 3:
				statusText.text = Language.main.Get("TimeCapsuleValid");
				statusText.color = colorValid;
				break;
			}
		}
		bool hasItems = main.hasItems;
		int num2 = (hasItems ? 1 : 0);
		if (num2 != cachedHasItems)
		{
			cachedHasItems = num2;
			storageLabel.enabled = !hasItems;
		}
	}

	public override void OnLanguageChanged()
	{
		Language main = Language.main;
		statusLabel.text = main.Get("TimeCapsuleStatusLabel");
		timeCapsuleLabel.text = main.Get("TimeCapsuleLabel");
		storageLabel.text = main.Get("TimeCapsuleStorageEmpty");
		submitText.text = main.Get("TimeCapsuleSubmit");
		imageLabel.text = main.Get("TimeCapsuleNoImage");
		inputFieldTextPlaceholderText.text = main.Get("TimeCapsuleTextPlaceholder");
	}

	public void SelectImage(string fileName)
	{
		expectingImageSelection = false;
		PlayerTimeCapsule.main.SetImage(fileName);
		pda.OpenTab(PDATab.TimeCapsule);
	}

	public void SetImageTexture(Texture2D texture)
	{
		if (texture != null)
		{
			imageLabel.enabled = false;
		}
		else
		{
			texture = defaultTexture;
			imageLabel.enabled = true;
		}
		image.texture = texture;
		MathExtensions.RectFit(image.texture.width, image.texture.height, imageAspect, RectScaleMode.Envelope, out var scale, out var offset);
		image.uvRect = new Rect(offset.x, offset.y, scale.x, scale.y);
		UpdateCheckBoxes();
	}

	public void OnImageClick()
	{
		expectingImageSelection = true;
		pda.OpenTab(PDATab.Gallery);
		pda.backButtonText.text = Language.main.Get("TimeCapsuleBackButton");
		pda.backButton.onClick.AddListener(OnBack);
		pda.backButton.gameObject.SetActive(value: true);
	}

	public void OnContainerClick()
	{
		pda.OpenTab(PDATab.Inventory);
		pda.backButtonText.text = Language.main.Get("TimeCapsuleBackButton");
		pda.backButton.onClick.AddListener(OnBack);
		pda.backButton.gameObject.SetActive(value: true);
	}

	public void OnBack()
	{
		pda.OpenTab(PDATab.TimeCapsule);
	}
}
