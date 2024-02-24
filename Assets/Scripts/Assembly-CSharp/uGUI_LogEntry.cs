using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class uGUI_LogEntry : MonoBehaviour, INotificationTarget, ISelectable
{
	[AssertNotNull]
	public Image icon;

	[AssertNotNull]
	public TextMeshProUGUI text;

	[AssertNotNull]
	public Image background;

	[AssertNotNull]
	public GameObject buttonGameObject;

	[AssertNotNull]
	public Button button;

	[AssertNotNull]
	public Image buttonNotification;

	[AssertNotNull]
	public uGUI_LogEntryData commonData;

	public Color buttonColorDefault;

	public Color buttonColorNotification;

	public Color backgroundColorDefault;

	public Color backgroundColorNotification;

	public Color iconColorDefault;

	public Color iconColorNotification;

	private RectTransform _rectTransform;

	private string entryKey = string.Empty;

	private FMODAsset sound;

	public RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public void Initialize(PDALog.Entry entry)
	{
		base.gameObject.SetActive(value: true);
		Sprite sprite = null;
		entryKey = string.Empty;
		PDALog.EntryType entryType = PDALog.EntryType.Invalid;
		Color color = new Color(1f, 1f, 1f, 1f);
		FMODAsset fMODAsset = null;
		PDALog.EntryData data = entry.data;
		if (data != null)
		{
			sprite = data.icon;
			entryKey = data.key;
			entryType = data.type;
			fMODAsset = data.sound;
		}
		if (entryType == PDALog.EntryType.Invalid)
		{
			entryKey = "Entry '" + entryKey + "' is invalid. Missing in PDAData.asset?";
			color = Color.red;
		}
		icon.sprite = PDALog.GetIcon(sprite);
		icon.color = iconColorDefault;
		text.color = color;
		UpdateText();
		background.color = backgroundColorDefault;
		buttonNotification.color = buttonColorDefault;
		if (fMODAsset != null)
		{
			sound = fMODAsset;
			buttonGameObject.SetActive(value: true);
			SetPlaying(playing: false);
		}
		else
		{
			buttonGameObject.SetActive(value: false);
		}
	}

	public void Uninitialize()
	{
		entryKey = string.Empty;
		sound = null;
		base.gameObject.SetActive(value: false);
	}

	public void SetPlaying(bool playing)
	{
		SpriteState spriteState = (playing ? commonData.spriteStop : commonData.spritePlay);
		button.spriteState = spriteState;
		(button.targetGraphic as Image).sprite = spriteState.pressedSprite;
	}

	public void UpdateText()
	{
		text.SetText(Language.main.Get(entryKey));
	}

	public void ToggleSound()
	{
		if (sound == null)
		{
			return;
		}
		SoundQueue queue = PDASounds.queue;
		if (queue != null)
		{
			if (queue.current == sound.id)
			{
				queue.Stop();
			}
			else
			{
				queue.PlayImmediately(sound);
			}
		}
	}

	public void OnHoverPlayButton()
	{
	}

	bool INotificationTarget.IsVisible()
	{
		if (this != null && base.isActiveAndEnabled && background != null && background.isActiveAndEnabled)
		{
			CanvasRenderer canvasRenderer = background.canvasRenderer;
			if (canvasRenderer != null && !canvasRenderer.cull)
			{
				return canvasRenderer.GetInheritedAlpha() > 0f;
			}
		}
		return false;
	}

	bool INotificationTarget.IsDestroyed()
	{
		return this == null;
	}

	void INotificationTarget.Progress(float progress)
	{
		float t = Mathf.Sin(progress * ((float)Math.PI / 2f));
		icon.color = Color.Lerp(iconColorDefault, iconColorNotification, t);
		background.color = Color.Lerp(backgroundColorDefault, backgroundColorNotification, t);
		buttonNotification.color = Color.Lerp(buttonColorDefault, buttonColorNotification, t);
	}

	bool ISelectable.IsValid()
	{
		if (this != null)
		{
			return base.isActiveAndEnabled;
		}
		return false;
	}

	RectTransform ISelectable.GetRect()
	{
		return rectTransform;
	}

	bool ISelectable.OnButtonDown(GameInput.Button button)
	{
		if (button == GameInput.Button.UISubmit)
		{
			ToggleSound();
			return true;
		}
		return false;
	}
}
