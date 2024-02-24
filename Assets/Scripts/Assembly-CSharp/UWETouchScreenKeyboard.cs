using UnityEngine;

public class UWETouchScreenKeyboard : TouchScreenKeyboardBase, ITouchScreenKeyboard
{
	private bool _hideInput;

	private TouchScreenKeyboard.Status _status;

	private RangeInt _selection;

	private int _characterLimit;

	private string _text;

	private bool _active;

	public override bool isSupported => true;

	public override bool hideInput
	{
		get
		{
			return _hideInput;
		}
		set
		{
			_hideInput = value;
		}
	}

	public override bool isInPlaceEditingAllowed => false;

	public override bool visible => active;

	public bool active
	{
		get
		{
			return _active;
		}
		set
		{
			_active = value;
		}
	}

	public string text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
		}
	}

	public int characterLimit
	{
		get
		{
			return _characterLimit;
		}
		set
		{
			_characterLimit = value;
		}
	}

	public RangeInt selection
	{
		get
		{
			return _selection;
		}
		set
		{
			if (_text != null)
			{
				_selection.start = Mathf.Clamp(value.start, 0, _text.Length);
				_selection.length = Mathf.Clamp(value.length, 0, _text.Length - _selection.start);
			}
			else
			{
				_selection.start = 0;
				_selection.length = 0;
			}
		}
	}

	public TouchScreenKeyboard.Status status => _status;

	public override ITouchScreenKeyboard Open(string text, TouchScreenKeyboardType keyboardType, bool autocorrection, bool multiline, bool secure, bool alert, string textPlaceholder, int characterLimit)
	{
		GameInput.ClearInput();
		_status = TouchScreenKeyboard.Status.Canceled;
		_selection = new RangeInt(0, text.Length);
		_characterLimit = characterLimit;
		_text = text;
		_active = false;
		PlatformServices services = PlatformUtils.main.GetServices();
		if (services != null && services.ShowVirtualKeyboard(string.Empty, text, OnVirtualKeyboardFinished, characterLimit))
		{
			_status = TouchScreenKeyboard.Status.Visible;
			_active = true;
		}
		return this;
	}

	private void OnVirtualKeyboardFinished(bool success, bool cancelled, string text)
	{
		_status = ((success && !cancelled) ? TouchScreenKeyboard.Status.Done : TouchScreenKeyboard.Status.Canceled);
		_selection = new RangeInt(0, text.Length);
		_text = text;
		_active = false;
	}
}
