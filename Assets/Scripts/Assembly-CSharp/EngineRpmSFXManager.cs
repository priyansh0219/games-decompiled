using FMOD.Studio;
using UnityEngine;

public class EngineRpmSFXManager : MonoBehaviour
{
	[AssertNotNull]
	public FMOD_CustomLoopingEmitter engineRpmSFX;

	public FMOD_CustomEmitter engineRevUp;

	public float rampUpSpeed = 1f;

	public float rampDownSpeed = 0.2f;

	private float rpmSpeed;

	private float topClampSpeed = 1f;

	private bool accelerating;

	private bool wasAccelerating;

	private PARAMETER_ID sfxIndex;

	public void AccelerateInput(float topClamp = 1f)
	{
		accelerating = true;
		topClampSpeed = topClamp;
	}

	public void PassParameter(string param, float value)
	{
		engineRpmSFX.SetParameterValue(param, value);
	}

	private void Awake()
	{
		sfxIndex = engineRpmSFX.GetParameterIndex("rpm");
	}

	private void Update()
	{
		if (engineRevUp != null && accelerating && !wasAccelerating)
		{
			if (rpmSpeed == 0f)
			{
				engineRevUp.Play();
			}
			wasAccelerating = true;
		}
		else if (engineRevUp != null && !accelerating && wasAccelerating)
		{
			engineRevUp.Stop();
			wasAccelerating = false;
		}
		if (accelerating)
		{
			rpmSpeed = Mathf.MoveTowards(rpmSpeed, topClampSpeed, Time.deltaTime * rampUpSpeed);
		}
		else
		{
			rpmSpeed = Mathf.MoveTowards(rpmSpeed, 0f, Time.deltaTime * rampDownSpeed);
		}
		if (rpmSpeed > 0f)
		{
			engineRpmSFX.Play();
		}
		else
		{
			engineRpmSFX.Stop();
		}
		engineRpmSFX.SetParameterValue(sfxIndex, rpmSpeed);
		if (!accelerating)
		{
			wasAccelerating = false;
		}
		accelerating = false;
	}
}
