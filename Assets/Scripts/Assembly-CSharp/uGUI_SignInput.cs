using System;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_SignInput : uGUI_InputGroup, uGUI_IButtonReceiver
{
	[Serializable]
	public class ElementData
	{
		public GameObject normal;

		public GameObject edit;

		public bool state { get; private set; }

		public void SetMode(bool mode)
		{
			if (edit != null)
			{
				edit.SetActive(mode);
			}
		}

		public void SetState(bool s)
		{
			state = s;
			if (normal != null)
			{
				normal.SetActive(state);
			}
		}

		public void ToggleState()
		{
			SetState(!state);
		}
	}

	[AssertLocalization]
	public string stringDefaultLabel = "DefaultLabel";

	[AssertNotNull]
	public RectTransform rt;

	[AssertNotNull]
	public uGUI_NavigableControlGrid grid;

	[AssertNotNull]
	public uGUI_InputField inputField;

	[AssertNotNull]
	public uGUI_GraphicRaycaster graphicRaycaster;

	public Toggle backgroundToggle;

	public float baseScale = 0.001f;

	public float scaleStep = 0.0002f;

	public int scaleMin = -3;

	public int scaleMax = 3;

	public Color[] colors = new Color[7]
	{
		new Color32(211, byte.MaxValue, 253, byte.MaxValue),
		Color.black,
		Color.red,
		Color.yellow,
		Color.green,
		Color.blue,
		Color.magenta
	};

	public Graphic background;

	public Graphic[] colorizedElements;

	public ElementData[] elements;

	public GameObject[] editOnly;

	private Player player;

	private float terminationSqrDistance = 4f;

	private int _scaleIndex;

	private int _colorIndex;

	private bool textWasSet;

	public string text
	{
		get
		{
			return inputField.text;
		}
		set
		{
			inputField.text = value;
			textWasSet = true;
		}
	}

	public int scaleIndex
	{
		get
		{
			return _scaleIndex;
		}
		set
		{
			value = Mathf.Clamp(value, scaleMin, scaleMax);
			if (_scaleIndex != value)
			{
				_scaleIndex = value;
				UpdateScale();
			}
		}
	}

	public int colorIndex
	{
		get
		{
			return _colorIndex;
		}
		set
		{
			value = Mathf.Clamp(value, 0, colors.Length - 1);
			if (_colorIndex != value)
			{
				_colorIndex = value;
				UpdateColor();
			}
		}
	}

	public bool[] elementsState
	{
		get
		{
			int num = elements.Length;
			bool[] array = new bool[num];
			for (int i = 0; i < num; i++)
			{
				ElementData elementData = elements[i];
				array[i] = elementData.state;
			}
			return array;
		}
		set
		{
			if (value != null)
			{
				int i = 0;
				for (int num = Mathf.Max(value.Length, elements.Length); i < num; i++)
				{
					elements[i].SetState(value[i]);
				}
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		terminationSqrDistance = Mathf.Pow(3f, 2f);
		graphicRaycaster.enabled = false;
		SetElementsMode(mode: false);
		if (!textWasSet && Language.main != null)
		{
			inputField.text = Language.main.Get(stringDefaultLabel);
		}
		UpdateScale();
		UpdateColor();
		if (backgroundToggle != null)
		{
			SetBackground(backgroundToggle.isOn);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (base.focused && player != null && (player.transform.position - rt.position).sqrMagnitude >= terminationSqrDistance)
		{
			Deselect();
		}
	}

	public void ToggleElementState(int index)
	{
		if (index >= 0 && index < elements.Length)
		{
			elements[index].ToggleState();
		}
	}

	public void SetScale(bool increase)
	{
		scaleIndex += (increase ? 1 : (-1));
	}

	public void ToggleColor()
	{
		int num = colorIndex + 1;
		colorIndex = ((num < colors.Length) ? num : 0);
	}

	public void SetBackground(bool state)
	{
		if (!(background == null))
		{
			background.enabled = state;
		}
	}

	public bool IsBackground()
	{
		if (background != null)
		{
			return background.enabled;
		}
		return false;
	}

	private void SetElementsMode(bool mode)
	{
		int i = 0;
		for (int num = elements.Length; i < num; i++)
		{
			elements[i].SetMode(mode);
		}
		int j = 0;
		for (int num2 = editOnly.Length; j < num2; j++)
		{
			GameObject gameObject = editOnly[j];
			if (!(gameObject == null))
			{
				gameObject.SetActive(mode);
			}
		}
	}

	private void UpdateScale()
	{
		if (rt != null)
		{
			float num = baseScale + scaleStep * (float)scaleIndex;
			rt.localScale = new Vector3(num, num, 1f);
		}
	}

	private void UpdateColor()
	{
		Color color = colors[colorIndex];
		int i = 0;
		for (int num = colorizedElements.Length; i < num; i++)
		{
			Graphic graphic = colorizedElements[i];
			if (!(graphic == null))
			{
				color.a = graphic.color.a;
				graphic.color = color;
			}
		}
	}

	public override void OnSelect(bool lockMovement)
	{
		base.OnSelect(lockMovement);
		GamepadInputModule.current.SetCurrentGrid(grid);
		player = Player.main;
		GamepadInputModule current = GamepadInputModule.current;
		if (current != null && !current.UsingController)
		{
			inputField.ActivateInputField();
			inputField.SelectAllText();
		}
		graphicRaycaster.enabled = true;
		SetElementsMode(mode: true);
	}

	public override void OnReselect(bool lockMovement)
	{
		base.OnReselect(lockMovement: true);
	}

	public override void OnDeselect()
	{
		base.OnDeselect();
		player = null;
		graphicRaycaster.enabled = false;
		SetElementsMode(mode: false);
	}

	public bool OnButtonDown(GameInput.Button button)
	{
		if (button == GameInput.Button.UICancel)
		{
			Deselect();
			return true;
		}
		return false;
	}
}
