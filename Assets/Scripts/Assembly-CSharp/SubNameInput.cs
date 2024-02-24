using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SubNameInput : uGUI_InputGroup, IColorChangeHandler, IEventSystemHandler, uGUI_IButtonReceiver, IPointerHoverHandler
{
	[Serializable]
	public class ColorData
	{
		public Graphic image;

		public GameObject selectionIndicator;
	}

	[AssertNotNull]
	public RectTransform rt;

	[AssertNotNull]
	public uGUI_NavigableControlGrid panel;

	public GameObject uiActive;

	public GameObject uiInactive;

	public TextMeshProUGUI uiInactiveText;

	public TextMeshProUGUI colorDisplayText;

	[AssertNotNull]
	public TMP_InputField inputField;

	[AssertNotNull]
	public uGUI_ColorPicker colorPicker;

	public ColorData[] colorData;

	[AssertLocalization]
	public string hoverTextKey;

	private Player player;

	private float terminationSqrDistance = 4f;

	private int selectedColorIndex;

	private SubName target;

	[AssertLocalization]
	private const string notDockedLabel = "SubmersibleNotDocked";

	public int SelectedColorIndex => selectedColorIndex;

	protected override void Awake()
	{
		base.Awake();
		target = GetComponent<SubName>();
		terminationSqrDistance = Mathf.Pow(3f, 2f);
		SetSelected(selectedColorIndex);
	}

	private void Start()
	{
		if (uiInactiveText != null)
		{
			uiInactiveText.text = Language.main.Get("SubmersibleNotDocked");
		}
	}

	private void OnEnable()
	{
		colorPicker.onColorChange.AddListener(OnColorChange);
		Subscribe();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		colorPicker.onColorChange.RemoveListener(OnColorChange);
		Unsubscribe();
	}

	protected override void Update()
	{
		base.Update();
		if (base.focused && player != null && (player.transform.position - rt.position).sqrMagnitude >= terminationSqrDistance)
		{
			Deselect();
		}
	}

	public override void OnSelect(bool lockMovement)
	{
		base.OnSelect(lockMovement);
		player = Player.main;
		GamepadInputModule.current.SetCurrentGrid(panel);
	}

	public override void OnDeselect()
	{
		base.OnDeselect();
		player = null;
	}

	public void OnPointerHover(PointerEventData eventData)
	{
		if (base.enabled && !base.selected && target != null)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, hoverTextKey, translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Interact);
		}
	}

	public void SetTarget(SubName value)
	{
		if (base.isActiveAndEnabled)
		{
			Unsubscribe();
		}
		target = value;
		bool flag = target != null;
		if (flag && base.isActiveAndEnabled)
		{
			OnColorsDeserialize(target.GetColors());
			SetName(target.GetName());
			Subscribe();
		}
		if (uiActive != null)
		{
			uiActive.SetActive(flag);
		}
		if (uiInactive != null)
		{
			uiInactive.SetActive(!flag);
		}
	}

	public void SetSelected(int index)
	{
		if (target == null)
		{
			return;
		}
		int num = this.colorData.Length;
		if (index >= num)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			ColorData colorData = this.colorData[i];
			if (colorData.selectionIndicator != null)
			{
				colorData.selectionIndicator.SetActive(i == index);
			}
		}
		selectedColorIndex = index;
		Vector3 color = target.GetColor(selectedColorIndex);
		colorPicker.SetHSB(color);
		UpdateDisplayText(this.colorData[index].image.color);
	}

	public void OnNameChange(string text)
	{
		SetName(text);
		if (target != null)
		{
			target.SetName(text);
		}
	}

	public void OnColorChange(ColorChangeEventData eventData)
	{
		SetColor(selectedColorIndex, eventData.color);
		if (target != null)
		{
			target.SetColor(selectedColorIndex, eventData.hsb, eventData.color);
			UpdateDisplayText(eventData.color);
		}
	}

	private void Subscribe()
	{
		if (!(target == null))
		{
			SubName subName = target;
			subName.onNameDeserialize = (SubName.OnNameDeserialize)Delegate.Combine(subName.onNameDeserialize, new SubName.OnNameDeserialize(SetName));
			SubName subName2 = target;
			subName2.onColorsDeserialize = (SubName.OnColorsDeserialize)Delegate.Combine(subName2.onColorsDeserialize, new SubName.OnColorsDeserialize(OnColorsDeserialize));
		}
	}

	private void Unsubscribe()
	{
		if (!(target == null))
		{
			SubName subName = target;
			subName.onNameDeserialize = (SubName.OnNameDeserialize)Delegate.Remove(subName.onNameDeserialize, new SubName.OnNameDeserialize(SetName));
			SubName subName2 = target;
			subName2.onColorsDeserialize = (SubName.OnColorsDeserialize)Delegate.Remove(subName2.onColorsDeserialize, new SubName.OnColorsDeserialize(OnColorsDeserialize));
		}
	}

	private void SetName(string text)
	{
		inputField.text = text;
	}

	private void SetColor(int index, Color color)
	{
		if (index < this.colorData.Length)
		{
			ColorData colorData = this.colorData[index];
			if (colorData.image != null)
			{
				colorData.image.color = color;
				UpdateDisplayText(color);
			}
		}
	}

	private void UpdateDisplayText(Color color)
	{
		if (!(colorDisplayText == null))
		{
			string text = Utils.ColorToHexString(color);
			int num = (int)(color.r * 255f);
			int num2 = (int)(color.g * 255f);
			int num3 = (int)(color.b * 255f);
			string text2 = $">:\n#{text}\n{num} / {num2} /  {num3}";
			colorDisplayText.text = text2;
		}
	}

	private void OnColorsDeserialize(Vector3[] serializedHSB)
	{
		int i = 0;
		for (int num = Mathf.Min(serializedHSB.Length, colorData.Length); i < num; i++)
		{
			Color color = uGUI_ColorPicker.HSBToColor(serializedHSB[i]);
			SetColor(i, color);
		}
		SetSelected(selectedColorIndex);
	}

	public bool OnButtonDown(GameInput.Button button)
	{
		if (button == GameInput.button1)
		{
			Deselect();
			return true;
		}
		return false;
	}
}
