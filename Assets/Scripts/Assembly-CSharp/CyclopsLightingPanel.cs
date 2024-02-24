using System;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

[ProtoContract]
public class CyclopsLightingPanel : MonoBehaviour
{
	[AssertNotNull]
	public SubRoot cyclopsRoot;

	[AssertNotNull]
	public Animator uiPanel;

	[AssertNotNull]
	public GameObject floodlightsHolder;

	[AssertNotNull]
	public Sprite[] internalLights;

	[AssertNotNull]
	public Sprite[] externalLights;

	[AssertNotNull]
	public Image internalLightsImage;

	[AssertNotNull]
	public Image externalLightsImage;

	[AssertNotNull]
	public FMODAsset vn_lightsOn;

	[AssertNotNull]
	public FMODAsset vn_lightsOff;

	[AssertNotNull]
	public FMODAsset vn_floodlightsOn;

	[AssertNotNull]
	public FMODAsset vn_floodlightsOff;

	private Color lightColor = Color.red;

	[NonSerialized]
	[ProtoMember(1)]
	public bool lightingOn = true;

	[NonSerialized]
	[ProtoMember(2)]
	public bool floodlightsOn;

	private bool wasLightingOnBeforeSR;

	private bool wasFloodlightsOnBeforeSR;

	private bool prevPowerRelayState;

	private void Start()
	{
		foreach (Transform item in floodlightsHolder.transform)
		{
			Light component = item.GetComponent<Light>();
			if (component != null)
			{
				lightColor = component.color;
				break;
			}
		}
		Player.main.playerModeChanged.AddHandler(base.gameObject, OnPlayerModeChanged);
		SetExternalLighting(floodlightsOn);
		cyclopsRoot.ForceLightingState(lightingOn);
		UpdateLightingButtons();
		prevPowerRelayState = CheckIsPowered();
	}

	private bool CheckIsPowered()
	{
		if (cyclopsRoot != null && cyclopsRoot.powerRelay != null)
		{
			return cyclopsRoot.powerRelay.IsPowered();
		}
		return false;
	}

	private void Update()
	{
		bool flag = CheckIsPowered();
		if (prevPowerRelayState && !flag)
		{
			SetExternalLighting(state: false);
		}
		else if (!prevPowerRelayState && flag)
		{
			SetExternalLighting(floodlightsOn);
		}
		prevPowerRelayState = flag;
	}

	private void ButtonsOn()
	{
		foreach (Transform item in uiPanel.transform)
		{
			item.gameObject.SetActive(value: true);
		}
	}

	private void ButtonsOff()
	{
		foreach (Transform item in uiPanel.transform)
		{
			item.gameObject.SetActive(value: false);
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.Equals(Player.main.gameObject))
		{
			uiPanel.SetBool("PanelActive", value: true);
			ButtonsOn();
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if (col.gameObject.Equals(Player.main.gameObject))
		{
			uiPanel.SetBool("PanelActive", value: false);
			Invoke("ButtonsOff", 0.5f);
		}
	}

	public void ToggleInternalLighting()
	{
		if (cyclopsRoot.powerRelay.GetPowerStatus() != 0 && !cyclopsRoot.silentRunning)
		{
			lightingOn = !lightingOn;
			cyclopsRoot.ForceLightingState(lightingOn);
			FMODUWE.PlayOneShot(lightingOn ? vn_lightsOn : vn_lightsOff, base.transform.position);
			UpdateLightingButtons();
		}
	}

	public void ToggleFloodlights()
	{
		if (cyclopsRoot.powerRelay.GetPowerStatus() != 0 && !cyclopsRoot.silentRunning)
		{
			floodlightsOn = !floodlightsOn;
			SetExternalLighting(floodlightsOn);
			FMODUWE.PlayOneShot(floodlightsOn ? vn_floodlightsOn : vn_floodlightsOff, base.transform.position);
			UpdateLightingButtons();
		}
	}

	private void UpdateLightingButtons()
	{
		int num = (lightingOn ? 1 : 0);
		internalLightsImage.sprite = internalLights[num];
		num = (floodlightsOn ? 1 : 0);
		externalLightsImage.sprite = externalLights[num];
		SendMessageUpwards("RecalculateNoiseValues", null, SendMessageOptions.RequireReceiver);
	}

	private void SetExternalLighting(bool state)
	{
		foreach (Transform item in floodlightsHolder.transform)
		{
			item.gameObject.SetActive(state);
		}
	}

	public void RigForSilentRunning()
	{
		wasLightingOnBeforeSR = lightingOn;
		wasFloodlightsOnBeforeSR = floodlightsOn;
		lightingOn = false;
		cyclopsRoot.ForceLightingState(lightingOn);
		floodlightsOn = false;
		SetExternalLighting(floodlightsOn);
		UpdateLightingButtons();
	}

	public void SecureFromSilentRunning()
	{
		lightingOn = wasLightingOnBeforeSR;
		cyclopsRoot.ForceLightingState(lightingOn);
		floodlightsOn = wasFloodlightsOnBeforeSR;
		SetExternalLighting(floodlightsOn);
		UpdateLightingButtons();
	}

	public void TempTurnOffFloodlights()
	{
		SetExternalLighting(state: false);
	}

	public void RestoreFloodlightsFromTempState()
	{
		SetExternalLighting(floodlightsOn);
	}

	public void SubConstructionComplete()
	{
		floodlightsOn = true;
		SetExternalLighting(state: true);
		UpdateLightingButtons();
	}

	public void OnPlayerModeChanged(Player.Mode mode)
	{
		_ = Player.main.currentSub != cyclopsRoot;
	}

	private void ToggleLightbar(bool enable)
	{
		if (enable)
		{
			PlatformUtils.SetLightbarColor(lightColor);
		}
		else
		{
			PlatformUtils.ResetLightbarColor();
		}
	}
}
