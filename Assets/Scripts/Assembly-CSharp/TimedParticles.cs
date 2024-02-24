using UnityEngine;

public class TimedParticles : MonoBehaviour
{
	public ParticleSystem particles;

	public float particleInterval = 5f;

	public float timeSinceParticles;

	public bool onlyUnderwater = true;

	private void Update()
	{
		timeSinceParticles += Time.deltaTime;
		if (timeSinceParticles > particleInterval)
		{
			if ((!onlyUnderwater || Player.main.IsUnderwater()) && (bool)particles)
			{
				particles.Play();
			}
			timeSinceParticles -= particleInterval;
		}
	}
}
