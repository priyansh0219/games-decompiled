using FMOD.Studio;
using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
	public FMODAsset music;

	public FMODAsset background;

	public static MainMenuMusic main;

	private EventInstance evt;

	private EventInstance evt2;

	private void Start()
	{
		main = this;
		evt = FMODUWE.GetEvent(music);
		evt2 = FMODUWE.GetEvent(background);
		Object.DontDestroyOnLoad(base.gameObject);
		Play();
	}

	public static void Play()
	{
		if ((bool)main && main.evt.hasHandle())
		{
			main.evt.start();
		}
		if ((bool)main && main.evt2.hasHandle())
		{
			main.evt2.start();
		}
	}

	public static void Stop()
	{
		if ((bool)main && main.evt.hasHandle())
		{
			main.evt.stop(STOP_MODE.ALLOWFADEOUT);
		}
		StopBackground();
	}

	public static void StopBackground()
	{
		if ((bool)main && main.evt2.hasHandle())
		{
			main.evt2.stop(STOP_MODE.ALLOWFADEOUT);
		}
	}
}
