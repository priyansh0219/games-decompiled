using System;
using System.Collections;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class CyclopsMotorMode : MonoBehaviour, ICompileTimeCheckable
{
	public enum CyclopsMotorModes
	{
		Slow = 0,
		Standard = 1,
		Flank = 2
	}

	private const int _version = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public bool engineOn;

	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public SubControl subController;

	public float[] motorModeNoiseValues;

	public CyclopsMotorModes cyclopsMotorMode = CyclopsMotorModes.Standard;

	public float[] motorModeSpeeds;

	public float[] motorModePowerConsumption;

	private bool engineOnOldState = true;

	private void Start()
	{
		engineOnOldState = engineOn;
		StartCoroutine(ChangeEngineState(engineOn));
	}

	public void ChangeCyclopsMotorMode(CyclopsMotorModes newMode)
	{
		cyclopsMotorMode = newMode;
		float num = motorModeSpeeds[(int)cyclopsMotorMode];
		subController.BaseForwardAccel = num;
		subController.BaseVerticalAccel = num;
		subController.NewSpeed((int)cyclopsMotorMode);
		SendMessage("RecalculateNoiseValues", null, SendMessageOptions.RequireReceiver);
		subRoot.BroadcastMessage("NewAlarmState", null, SendMessageOptions.DontRequireReceiver);
	}

	public void SaveEngineStateAndPowerDown()
	{
		engineOnOldState = engineOn;
		StartCoroutine(ChangeEngineState(changeToState: false));
	}

	public void RestoreEngineState()
	{
		StartCoroutine(ChangeEngineState(engineOnOldState));
	}

	public void InvokeChangeEngineState(bool changeToState)
	{
		float delay = 0f;
		if (changeToState)
		{
			delay = 5f;
		}
		StartCoroutine(ChangeEngineState(changeToState, delay));
	}

	private IEnumerator ChangeEngineState(bool changeToState, float delay = 0f)
	{
		yield return new WaitForSeconds(delay);
		if (changeToState)
		{
			engineOn = true;
		}
		else if (!changeToState)
		{
			engineOn = false;
		}
		subController.NewEngineMode(changeToState);
		BroadcastMessage("RecalculateNoiseValues");
	}

	public float GetNoiseValue()
	{
		return motorModeNoiseValues[(int)cyclopsMotorMode];
	}

	public float GetPowerConsumption()
	{
		return motorModePowerConsumption[(int)cyclopsMotorMode];
	}

	public string CompileTimeCheck()
	{
		int length = Enum.GetValues(typeof(CyclopsMotorModes)).Length;
		if (motorModeNoiseValues.Length != length)
		{
			return $"Number of motor mode noise values ({motorModeNoiseValues.Length}) mismatch number of motor modes ({length})";
		}
		if (motorModeSpeeds.Length != length)
		{
			return $"Number of motor mode speeds ({motorModeSpeeds.Length}) mismatch number of motor modes ({length})";
		}
		if (motorModePowerConsumption.Length != length)
		{
			return $"Number of motor mode power consumption ({motorModePowerConsumption.Length}) mismatch number of motor modes ({length})";
		}
		return null;
	}
}
