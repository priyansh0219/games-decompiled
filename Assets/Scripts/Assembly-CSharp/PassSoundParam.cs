using FMOD.Studio;
using UnityEngine;

public abstract class PassSoundParam : MonoBehaviour
{
	public FMOD_StudioEventEmitter[] emitters;

	private PARAMETER_ID[] emitterParamIndex;

	public FMOD_CustomEmitter[] customEmitters;

	private PARAMETER_ID[] customEmitterParamIndex;

	public abstract float GetParamValue();

	public abstract string GetParamName();

	private void Start()
	{
		string paramName = GetParamName();
		if (customEmitters.Length != 0)
		{
			customEmitterParamIndex = new PARAMETER_ID[customEmitters.Length];
			for (int i = 0; i < customEmitters.Length; i++)
			{
				if (customEmitters[i] != null)
				{
					customEmitterParamIndex[i] = customEmitters[i].GetParameterIndex(paramName);
				}
				else
				{
					customEmitterParamIndex[i] = FMODUWE.invalidParameterId;
				}
			}
		}
		if (emitters.Length == 0)
		{
			return;
		}
		emitterParamIndex = new PARAMETER_ID[emitters.Length];
		for (int j = 0; j < emitters.Length; j++)
		{
			if (emitters[j] != null)
			{
				emitterParamIndex[j] = emitters[j].GetParameterIndex(paramName);
			}
			else
			{
				emitterParamIndex[j] = FMODUWE.invalidParameterId;
			}
		}
	}

	private void Update()
	{
		float paramValue = GetParamValue();
		for (int i = 0; i < emitters.Length; i++)
		{
			if (FMODUWE.IsValidParameterId(emitterParamIndex[i]))
			{
				emitters[i].SetParameterValue(emitterParamIndex[i], paramValue);
			}
		}
		for (int j = 0; j < customEmitters.Length; j++)
		{
			if (FMODUWE.IsValidParameterId(customEmitterParamIndex[j]))
			{
				customEmitters[j].SetParameterValue(customEmitterParamIndex[j], paramValue);
			}
		}
	}
}
