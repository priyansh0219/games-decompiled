using System.Collections.Generic;
using UnityEngine;

public class InteractionVolumeUser : MonoBehaviour
{
	private class UseInfo
	{
		public float enterTime;
	}

	private Dictionary<InteractionVolume, UseInfo> activeVolumes = new Dictionary<InteractionVolume, UseInfo>();

	private InteractionVolume bestVol;

	public InteractionVolume GetMostRecent()
	{
		return bestVol;
	}

	private void UpdateMostRecentVolume()
	{
		float num = 0f;
		bestVol = null;
		foreach (KeyValuePair<InteractionVolume, UseInfo> activeVolume in activeVolumes)
		{
			if (bestVol == null || activeVolume.Value.enterTime > num)
			{
				num = activeVolume.Value.enterTime;
				bestVol = activeVolume.Key;
			}
		}
	}

	public void OnEnterVolume(InteractionVolume vol)
	{
		if (!activeVolumes.ContainsKey(vol))
		{
			activeVolumes[vol] = new UseInfo();
		}
		activeVolumes[vol].enterTime = Time.time;
		UpdateMostRecentVolume();
	}

	public void OnExitVolume(InteractionVolume vol)
	{
		if (activeVolumes.ContainsKey(vol))
		{
			activeVolumes.Remove(vol);
			UpdateMostRecentVolume();
		}
	}
}
