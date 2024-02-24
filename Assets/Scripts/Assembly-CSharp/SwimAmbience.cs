using UnityEngine;

[RequireComponent(typeof(FMOD_StudioEventEmitter))]
public class SwimAmbience : MonoBehaviour
{
	public FMOD_StudioEventEmitter surfaceSwim;

	public FMOD_StudioEventEmitter underwaterSwim;

	private void Start()
	{
	}

	private void Update()
	{
		float depthOf = Ocean.GetDepthOf(Utils.GetLocalPlayer());
		float num = 5f;
		if (depthOf >= 0f && depthOf < num && !surfaceSwim.GetIsPlaying())
		{
			surfaceSwim.PlayUI();
			underwaterSwim.Stop();
		}
		else if (depthOf >= num && !underwaterSwim.GetIsPlaying())
		{
			underwaterSwim.PlayUI();
			surfaceSwim.Stop();
		}
	}
}
