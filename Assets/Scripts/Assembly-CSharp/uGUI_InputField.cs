using TMPro;
using UnityEngine.EventSystems;

public class uGUI_InputField : TMP_InputField
{
	public bool uppercase;

	private uGUI_InputGroup group;

	private bool deselecting;

	protected override void Awake()
	{
		base.Awake();
		base.onValidateInput = Validate;
	}

	protected override void Start()
	{
		base.Start();
		base.onEndEdit.AddListener(delegate
		{
			EndEdit();
		});
		group = GetComponentInParent<uGUI_InputGroup>();
	}

	protected new char Validate(string text, int pos, char ch)
	{
		if (uppercase)
		{
			ch = char.ToUpper(ch);
		}
		return base.Validate(text, pos, ch);
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (group != null)
		{
			group.Select(lockMovement: true);
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		deselecting = true;
		base.OnDeselect(eventData);
		deselecting = false;
	}

	public void SelectAllText()
	{
		SelectAll();
	}

	private void EndEdit()
	{
		if (!deselecting)
		{
			EventSystem.current.SetSelectedGameObject(null);
			if (group != null)
			{
				group.Deselect();
			}
		}
	}
}
