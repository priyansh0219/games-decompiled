using UnityEngine;

public class MusicOnStart : MonoBehaviour
{
	public AudioClip clipOnStart;

	public float volume = 1f;

	private AudioSource musicSource;

	private void Start()
	{
		musicSource = base.gameObject.AddComponent<AudioSource>();
		musicSource.dopplerLevel = 0f;
		musicSource.clip = clipOnStart;
		musicSource.volume = volume;
		musicSource.Play();
	}
}
