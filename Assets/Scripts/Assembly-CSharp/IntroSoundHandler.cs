using FMOD.Studio;
using UnityEngine;

public class IntroSoundHandler : MonoBehaviour
{
	public FMOD_StudioEventEmitter emitter;

	public float loadSoundFadeTime = 3f;

	private float timeLoaded;

	private PARAMETER_ID fmodIndexLoad = FMODUWE.invalidParameterId;

	private void OnLoaded()
	{
		timeLoaded = Time.time;
		InvokeRepeating("FadeLoadParam", 0f, 0f);
	}

	private void FadeLoadParam()
	{
		if (FMODUWE.IsInvalidParameterId(fmodIndexLoad))
		{
			fmodIndexLoad = emitter.GetParameterIndex("load");
		}
		float value = Mathf.Clamp01((Time.time - timeLoaded) / loadSoundFadeTime);
		emitter.SetParameterValue(fmodIndexLoad, value);
	}
}
