using UnityEngine;

public class AnimationSound : MonoBehaviour
{
	public bool debug;

	public FMODAsset[] sounds;

	private void OnAnimationSound(int soundNum)
	{
		if (soundNum >= sounds.Length)
		{
			return;
		}
		FMODAsset fMODAsset = sounds[soundNum];
		if (fMODAsset != null)
		{
			if (debug)
			{
				Debug.Log("OnAnimationSound " + soundNum + ": " + fMODAsset.path);
			}
			Utils.PlayFMODAsset(fMODAsset, base.transform);
		}
	}
}
