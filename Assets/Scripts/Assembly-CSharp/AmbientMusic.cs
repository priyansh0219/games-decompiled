using System.Collections.Generic;
using UnityEngine;

public class AmbientMusic : MonoBehaviour
{
	public List<AudioClip> clipList = new List<AudioClip>();

	public List<int> depthList = new List<int>();

	public float minSilenceLength = 10f;

	public float maxSilenceLength = 120f;

	public float playbackVolume = 1f;

	public float currentSilenceLength;

	public float timeOfLastMusic;

	public float trackLength;

	private AudioSource musicSource;

	private float GetPlayerDepth()
	{
		return Ocean.GetDepthOf(Utils.GetLocalPlayer());
	}

	private void ComputeSilence()
	{
		currentSilenceLength = minSilenceLength + (maxSilenceLength - minSilenceLength) * Random.value;
	}

	private void Start()
	{
		musicSource = base.gameObject.AddComponent<AudioSource>();
		musicSource.dopplerLevel = 0f;
		if (depthList.Count == 0)
		{
			Debug.Log("AmbientMusic.Start() - At least one track must be specified.");
		}
		for (int i = 1; i < depthList.Count; i++)
		{
			if (depthList[i - 1] > depthList[i])
			{
				Debug.Log("AmbientMusic.Start() - All levels in depthList must be increasing (entry #" + i + " isn't greater than " + (i - 1) + ").");
			}
		}
		if (depthList.Count != clipList.Count)
		{
			Debug.Log("AmbientMusic.Start() - You must specify the same number of clips as depth levels.");
		}
		ComputeSilence();
		timeOfLastMusic = Time.time;
	}

	private void PlayAmbientMusic()
	{
		int index = 0;
		for (int i = 1; i < depthList.Count; i++)
		{
			if (depthList[i] < (int)GetPlayerDepth())
			{
				index = i;
			}
		}
		AudioClip audioClip = clipList[index];
		musicSource.clip = audioClip;
		musicSource.volume = playbackVolume;
		musicSource.Play();
		trackLength = audioClip.length;
		timeOfLastMusic = Time.time;
	}

	private void Update()
	{
		if (Time.time > timeOfLastMusic + currentSilenceLength + trackLength)
		{
			PlayAmbientMusic();
			ComputeSilence();
		}
	}
}
