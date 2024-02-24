using System.Collections.Generic;
using UnityEngine;

public class CyclopsDecoyManager : MonoBehaviour
{
	public int decoyMaxWithUpgrade = 5;

	public static readonly HashSet<GameObject> decoyList = new HashSet<GameObject>();

	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public CyclopsDecoyLauncher decoyLauncher;

	[AssertNotNull]
	public CyclopsDecoyLaunchButton decoyLaunchButton;

	[AssertNotNull]
	public CyclopsDecoyLoadingTube decoyLoadingTube;

	[AssertNotNull]
	public CyclopsDecoyScreenHUDManager decoyHUD;

	[AssertNotNull]
	public FMOD_CustomEmitter loadDecoySFX;

	private int oldTotal;

	public int decoyCount { get; private set; }

	public int decoyMax { get; set; }

	private void Start()
	{
		UpdateMax();
		oldTotal = decoyCount;
	}

	private void LaunchWithDelay()
	{
		decoyLauncher.LaunchDecoy();
	}

	public static void AddDecoyToGlobalHashSet(GameObject decoy)
	{
		decoyList.Add(decoy);
	}

	public static void RemoveDecoyFromGlobalHashSet(GameObject decoy)
	{
		if ((bool)decoy)
		{
			decoyList.Remove(decoy);
		}
	}

	public bool TryLaunchDecoy()
	{
		if (decoyCount > 0)
		{
			decoyCount--;
			Invoke("LaunchWithDelay", 3f);
			decoyLoadingTube.TryRemoveDecoyFromTube();
			decoyLaunchButton.UpdateText();
			subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.decoyNotification, addToQueue: false, forcePlay: true);
			return true;
		}
		return false;
	}

	public void UpdateTotalDecoys(int totalDecoys)
	{
		UpdateMax();
		decoyCount = totalDecoys;
		decoyLaunchButton.UpdateText();
		decoyHUD.UpdateDecoyScreen();
		if (decoyCount > oldTotal)
		{
			loadDecoySFX.Play();
			decoyHUD.SlotNewDecoy();
		}
		oldTotal = decoyCount;
	}

	private void UpdateMax()
	{
		decoyMax = ((!subRoot.decoyTubeSizeIncreaseUpgrade) ? 1 : decoyMaxWithUpgrade);
	}
}
