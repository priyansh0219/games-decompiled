using System;
using Gendarme;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[SuppressMessage("Subnautica.Rules", "AvoidDoubleInitializationRule")]
internal class ToggleButton : Toggle, ISelectable
{
	private Sprite normalSprite;

	public bool setOnSelect;

	public SelectButtonEvent onButtonPressed = new SelectButtonEvent();

	protected override void Start()
	{
		base.Start();
		normalSprite = ((Image)base.targetGraphic).sprite;
		onValueChanged.AddListener(delegate
		{
			UpdateVisuals();
		});
		UpdateVisuals();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (setOnSelect)
		{
			base.isOn = true;
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		if (setOnSelect && (base.group == null || base.group.allowSwitchOff))
		{
			base.isOn = false;
		}
	}

	private void UpdateVisuals()
	{
		switch (base.transition)
		{
		case Transition.ColorTint:
			base.image.color = (base.isOn ? base.colors.pressedColor : base.colors.normalColor);
			break;
		case Transition.SpriteSwap:
			base.image.sprite = (base.isOn ? base.spriteState.pressedSprite : normalSprite);
			break;
		default:
			throw new NotImplementedException();
		case Transition.Animation:
			break;
		}
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
		return GetComponent<RectTransform>();
	}

	bool ISelectable.OnButtonDown(GameInput.Button button)
	{
		if (IsInteractable() && button == GameInput.Button.UISubmit)
		{
			if (onButtonPressed != null)
			{
				onButtonPressed.Invoke();
			}
			return true;
		}
		return false;
	}
}
