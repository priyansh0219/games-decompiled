using UnityEngine;

public class VFXRandomPlayStop : MonoBehaviour
{
	public ParticleSystem ps;

	public float minDuration = 0.15f;

	public float maxDuration = 1.5f;

	public float minDelay = 0.5f;

	public float maxDelay = 4.5f;

	private float timeLeft;

	private float currentDuration = 1f;

	private float currentDelay = 1f;

	public bool isPlaying = true;

	private void PlayOnce()
	{
		currentDuration = Random.Range(minDuration, maxDuration);
		currentDelay = Random.Range(minDelay, maxDelay);
		timeLeft = currentDuration + currentDelay;
		ps.Play();
		isPlaying = true;
	}

	private void Start()
	{
		PlayOnce();
	}

	private void Update()
	{
		timeLeft -= Time.deltaTime;
		if (timeLeft <= currentDelay && isPlaying)
		{
			ps.Stop();
			isPlaying = false;
		}
		else if (timeLeft <= 0f && !isPlaying)
		{
			PlayOnce();
		}
	}
}
