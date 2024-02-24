using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class ToggleLights : MonoBehaviour, IProtoEventListener
{
	public delegate void OnLightsToggled(bool active);

	[AssertNotNull]
	public GameObject lightsParent;

	public FMOD_StudioEventEmitter lightsOnSound;

	public FMOD_StudioEventEmitter lightsOffSound;

	public FMODAsset onSound;

	public FMODAsset offSound;

	[AssertNotNull]
	public EnergyMixin energyMixin;

	public float energyPerSecond = 1f;

	public int lightState;

	public int maxLightStates = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public bool lightsActive = true;

	private float timeLastLightToggle;

	private float timeLastPlayerModeChange;

	public event OnLightsToggled lightsCallback;

	private void OnEnable()
	{
		if ((bool)energyMixin)
		{
			energyMixin.onPoweredChanged += OnPoweredChanged;
		}
	}

	private void OnDisable()
	{
		if ((bool)energyMixin)
		{
			energyMixin.onPoweredChanged -= OnPoweredChanged;
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (lightsParent != null)
		{
			lightsParent.SetActive(lightsActive);
		}
	}

	private void OnPoweredChanged(bool powered)
	{
		SetLightsActive(powered);
	}

	public void SetLightsActive(bool isActive)
	{
		if (isActive == lightsActive)
		{
			return;
		}
		if (!isActive || !energyMixin.IsDepleted())
		{
			lightsActive = isActive;
			lightsParent.SetActive(lightsActive);
			if (lightsActive)
			{
				if ((bool)lightsOnSound)
				{
					Utils.PlayEnvSound(lightsOnSound, lightsOnSound.gameObject.transform.position);
				}
				if ((bool)onSound)
				{
					Utils.PlayFMODAsset(onSound, base.transform);
				}
			}
			else
			{
				if ((bool)lightsOffSound)
				{
					Utils.PlayEnvSound(lightsOffSound, lightsOffSound.gameObject.transform.position);
				}
				if ((bool)offSound)
				{
					Utils.PlayFMODAsset(offSound, base.transform);
				}
			}
		}
		if (this.lightsCallback != null)
		{
			this.lightsCallback(lightsActive);
		}
	}

	public void UpdateLightEnergy()
	{
		if ((bool)energyMixin)
		{
			if (energyMixin.IsDepleted())
			{
				SetLightsActive(isActive: false);
			}
			else if (lightsActive && energyPerSecond > 0f)
			{
				energyMixin.ConsumeEnergy(DayNightCycle.main.deltaTime * energyPerSecond);
			}
		}
	}

	public void CheckLightToggle()
	{
		if (GameInput.GetButtonDown(GameInput.Button.RightHand) && !Player.main.GetPDA().isInUse && AvatarInputHandler.main.IsEnabled() && Time.time > timeLastPlayerModeChange + 1f && timeLastLightToggle + 0.25f < Time.time)
		{
			SetLightsActive(!lightsActive);
			timeLastLightToggle = Time.time;
			lightState++;
			if (lightState == maxLightStates)
			{
				lightState = 0;
			}
		}
	}

	public bool GetLightsActive()
	{
		return lightsActive;
	}

	private void Update()
	{
		if (base.gameObject.activeInHierarchy)
		{
			UpdateLightEnergy();
		}
	}

	public Color FindLightColor()
	{
		if (lightsParent != null)
		{
			Light componentInChildren = lightsParent.GetComponentInChildren<Light>();
			if (componentInChildren != null)
			{
				return componentInChildren.color;
			}
		}
		return Color.red;
	}
}
