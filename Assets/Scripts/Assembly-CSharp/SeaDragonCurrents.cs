using UnityEngine;

public class SeaDragonCurrents : MonoBehaviour
{
	[AssertNotNull]
	public Rigidbody dragonRigidbody;

	[AssertNotNull]
	public Transform leftClaw;

	[AssertNotNull]
	public Transform rightClaw;

	public AnimationCurve currentSpeedMultiplier;

	public float radius;

	public float currentLifeTime = 3f;

	public float generateInterval = 0.2f;

	private bool flapping;

	private float timeNextGenerate;

	private void OnFlapStart()
	{
		flapping = true;
	}

	private void OnFlapEnd()
	{
		flapping = false;
	}

	private void Update()
	{
		if (flapping && Time.time > timeNextGenerate)
		{
			timeNextGenerate = Time.time + generateInterval;
			float magnitude = dragonRigidbody.velocity.magnitude;
			float num = currentSpeedMultiplier.Evaluate(magnitude);
			if (num > 0f)
			{
				WorldForces.AddCurrent(leftClaw.position, DayNightCycle.main.timePassed, radius, -base.transform.forward, num, currentLifeTime);
				WorldForces.AddCurrent(rightClaw.position, DayNightCycle.main.timePassed, radius, -base.transform.forward, num, currentLifeTime);
			}
		}
	}
}
