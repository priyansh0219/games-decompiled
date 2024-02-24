using UnityEngine;
using UnityEngine.UI;

public class SelectableWrapper : ISelectable, uGUI_IButtonReceiver
{
	public delegate bool ProcessButton(GameInput.Button button);

	private Selectable _selectable;

	private ProcessButton _buttonDelegate;

	public Selectable selectable => _selectable;

	public SelectableWrapper(Selectable selectable, ProcessButton buttonDelegate)
	{
		_selectable = selectable;
		_buttonDelegate = buttonDelegate;
	}

	public bool IsValid()
	{
		if (selectable != null)
		{
			return selectable.isActiveAndEnabled;
		}
		return false;
	}

	public RectTransform GetRect()
	{
		if (!(selectable != null))
		{
			return null;
		}
		return selectable.GetComponent<RectTransform>();
	}

	public bool OnButtonDown(GameInput.Button button)
	{
		if (_buttonDelegate != null)
		{
			return _buttonDelegate(button);
		}
		return false;
	}
}
