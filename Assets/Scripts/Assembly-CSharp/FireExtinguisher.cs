using FMOD.Studio;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class FireExtinguisher : PlayerTool
{
	public FMODASRPlayer useSound;

	public FMOD_CustomLoopingEmitter soundEmitter;

	public VFXController fxControl;

	[SerializeField]
	private float expendFuelPerSecond = 3.5f;

	[SerializeField]
	private float fireDousePerSecond = 20f;

	[ProtoMember(1)]
	public float fuel = 100f;

	public float maxFuel = 100f;

	public LayerMask impactLayerFX;

	private bool usedThisFrame;

	private Fire fireTarget;

	private bool fxIsPlaying;

	private bool impactFXisPlaying;

	private PARAMETER_ID fmodIndexInWater = FMODUWE.invalidParameterId;

	private int lastUnderwaterValue = -1;

	[AssertLocalization(1)]
	private const string fuelPercentFormatKey = "FuelPercent";

	private int lastFuelStringValue = -1;

	private string cachedFuelString = "";

	public override void OnDraw(Player p)
	{
		base.OnDraw(p);
	}

	private void UseExtinguisher(float douseAmount, float expendAmount)
	{
		if ((bool)fireTarget)
		{
			fireTarget.Douse(douseAmount);
		}
		if (fxControl != null && !fxIsPlaying)
		{
			fxControl.Play(0);
			fxIsPlaying = true;
		}
		if (!IntroLifepodDirector.IsActive)
		{
			fuel = Mathf.Max(fuel - expendAmount, 0f);
		}
	}

	private void UpdateTarget()
	{
		fireTarget = null;
		if (!(usingPlayer != null))
		{
			return;
		}
		Vector3 position = default(Vector3);
		GameObject closestObj = null;
		UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, 8f, ref closestObj, ref position, out var _);
		if ((bool)closestObj)
		{
			Fire componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Fire>(closestObj);
			if ((bool)componentInHierarchy)
			{
				fireTarget = componentInHierarchy;
			}
		}
	}

	private void UpdateImpactFX()
	{
		if (!(fxControl != null) || fxControl.emitters[1] == null)
		{
			return;
		}
		if (fxIsPlaying && fireTarget != null && (float)fireTarget.GetExtinguishPercent() > 0f)
		{
			GameObject instanceGO = fxControl.emitters[1].instanceGO;
			Transform transform = fxControl.gameObject.transform;
			bool flag = false;
			if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, 3f, impactLayerFX, QueryTriggerInteraction.Ignore))
			{
				instanceGO.transform.position = hitInfo.point;
				instanceGO.transform.eulerAngles = hitInfo.normal * 360f;
				flag = true;
			}
			if (flag && !impactFXisPlaying)
			{
				instanceGO.transform.parent = null;
				fxControl.Play(1);
				impactFXisPlaying = true;
			}
			else if (!flag && impactFXisPlaying)
			{
				fxControl.Stop(1);
				impactFXisPlaying = false;
			}
		}
		else if (impactFXisPlaying)
		{
			fxControl.Stop(1);
			impactFXisPlaying = false;
		}
	}

	private void Update()
	{
		if (AvatarInputHandler.main.IsEnabled() && Player.main.IsAlive() && GameInput.GetButtonHeld(GameInput.Button.RightHand) && base.isDrawn)
		{
			usedThisFrame = true;
		}
		else
		{
			usedThisFrame = false;
		}
		int num = (Player.main.isUnderwater.value ? 1 : 0);
		if (num != lastUnderwaterValue)
		{
			lastUnderwaterValue = num;
			if (FMODUWE.IsInvalidParameterId(fmodIndexInWater))
			{
				fmodIndexInWater = soundEmitter.GetParameterIndex("in_water");
			}
			soundEmitter.SetParameterValue(fmodIndexInWater, num);
		}
		UpdateTarget();
		if (usedThisFrame && fuel > 0f)
		{
			if (Player.main.IsUnderwater())
			{
				Player.main.GetComponent<UnderwaterMotor>().SetVelocity(-MainCamera.camera.transform.forward * 5f);
			}
			float douseAmount = fireDousePerSecond * Time.deltaTime;
			float expendAmount = expendFuelPerSecond * Time.deltaTime;
			UseExtinguisher(douseAmount, expendAmount);
			soundEmitter.Play();
		}
		else
		{
			soundEmitter.Stop();
			if (fxControl != null)
			{
				fxControl.Stop(0);
				fxIsPlaying = false;
			}
		}
		UpdateImpactFX();
		UpdateControllerLightbarToToolBarValue();
	}

	private void StopExtinguisherFX()
	{
		fxControl.Stop(0);
		fxIsPlaying = false;
	}

	private void FireExSpray()
	{
		if (fxControl != null)
		{
			fxControl.Play(0);
			fxIsPlaying = true;
			Invoke("StopExtinguisherFX", 1.5f);
		}
	}

	public string GetFuelValueText()
	{
		int num = Mathf.FloorToInt(fuel);
		if (lastFuelStringValue != num)
		{
			float arg = fuel / maxFuel;
			cachedFuelString = Language.main.GetFormat("FuelPercent", arg);
			lastFuelStringValue = num;
		}
		return cachedFuelString;
	}

	public override string GetCustomUseText()
	{
		if (IntroLifepodDirector.IsActive)
		{
			return string.Empty;
		}
		return GetFuelValueText();
	}
}
