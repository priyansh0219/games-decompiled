using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class uGUI_ColorChoice : Selectable, uGUI_INavigableControl
{
	[Serializable]
	public class ColorEvent : UnityEvent<Color>
	{
	}

	[AssertNotNull]
	public Image indicator;

	public FMODAsset soundRight;

	public FMODAsset soundLeft;

	public int steps = 10;

	public ColorEvent onValueChanged;

	private Color _value;

	public Color value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			RefreshShownValue();
			if (onValueChanged != null)
			{
				onValueChanged.Invoke(value);
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		RefreshShownValue();
	}

	public void Right()
	{
		Change(1f);
		if (soundRight != null)
		{
			RuntimeManager.PlayOneShot(soundRight.path);
		}
	}

	public void Left()
	{
		Change(-1f);
		if (soundRight != null)
		{
			RuntimeManager.PlayOneShot(soundLeft.path);
		}
	}

	private void Change(float dir)
	{
		Color.RGBToHSV(_value, out var H, out var S, out var V);
		H = Mathf.Round(H * (float)steps);
		H = (H + dir) % (float)(steps + 1);
		H /= (float)steps;
		S = 1f;
		V = 1f;
		value = Color.HSVToRGB(H, S, V, hdr: false);
	}

	private void RefreshShownValue()
	{
		indicator.color = _value;
	}

	void uGUI_INavigableControl.OnMove(int dirX, int dirY)
	{
		if (dirX > 0)
		{
			Right();
		}
		else if (dirX < 0)
		{
			Left();
		}
	}
}
