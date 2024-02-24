using UnityEngine;
using mset;

public class MarmoLifepodSky : MonoBehaviour
{
	public Sky anchorSky;

	private Sky globalSky;

	private SkyManager mng;

	private void Start()
	{
		mng = SkyManager.Get();
		globalSky = MarmoSkies.main.GetSky(Skies.SafeShallow);
		Utils.MonitoredValue<bool> escapePod = Utils.GetLocalPlayerComp().escapePod;
		escapePod.changedEvent.AddHandler(this, InEscapePodChanged);
		InEscapePodChanged(escapePod);
	}

	private void SetSkyAnchor()
	{
		if (mng != null)
		{
			mng.GlobalSky = anchorSky;
		}
	}

	private void SetGlobalSky()
	{
		if (mng != null && globalSky != null)
		{
			mng.GlobalSky = globalSky;
		}
	}

	public void InEscapePodChanged(Utils.MonitoredValue<bool> inEscapePod)
	{
		if (inEscapePod.value)
		{
			SetSkyAnchor();
		}
		else
		{
			SetGlobalSky();
		}
	}
}
