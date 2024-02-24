using UnityEngine;

public class IncubatorComputerTerminal : MonoBehaviour
{
	[AssertNotNull]
	public GameObject fx;

	[AssertNotNull]
	public VFXLerpScale scaleControl;

	[AssertNotNull]
	public VFXController fxControl;

	[AssertNotNull]
	public GameObject enabledStateGO;

	[AssertNotNull]
	public GameObject hoverStateGO;

	[AssertNotNull]
	public GameObject defaultStateGO;

	[AssertNotNull]
	public GameObject clickStateGO;

	private int _state = -1;

	private bool active;

	public void SetState(int state)
	{
		switch (state)
		{
		case -1:
			enabledStateGO.SetActive(value: false);
			hoverStateGO.SetActive(value: false);
			defaultStateGO.SetActive(value: false);
			clickStateGO.SetActive(value: false);
			break;
		case 0:
			enabledStateGO.SetActive(value: true);
			hoverStateGO.SetActive(value: false);
			defaultStateGO.SetActive(value: true);
			clickStateGO.SetActive(value: false);
			break;
		case 1:
			enabledStateGO.SetActive(value: true);
			hoverStateGO.SetActive(value: true);
			defaultStateGO.SetActive(value: false);
			clickStateGO.SetActive(value: false);
			break;
		case 2:
			enabledStateGO.SetActive(value: true);
			hoverStateGO.SetActive(value: false);
			defaultStateGO.SetActive(value: false);
			clickStateGO.SetActive(value: true);
			break;
		}
		_state = state;
	}

	public void OnUse()
	{
		if (active)
		{
			SetState(2);
			fxControl.Play();
			scaleControl.Play();
			Invoke("DisableFX", scaleControl.duration);
		}
		active = false;
	}

	public void OnHover()
	{
		if (active && _state != 2)
		{
			SetState(1);
		}
		active = false;
	}

	public void SetActive(bool state)
	{
		active = state;
		SetFXActive(state);
	}

	private void DisableFX()
	{
		SetFXActive(state: false);
	}

	private void SetFXActive(bool state)
	{
		fx.SetActive(state);
		SetState((!state) ? (-1) : 0);
	}
}
