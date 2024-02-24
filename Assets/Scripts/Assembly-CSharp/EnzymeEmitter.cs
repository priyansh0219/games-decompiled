using UnityEngine;

public class EnzymeEmitter : MonoBehaviour
{
	[AssertNotNull]
	public GameObject enzymeCloudPrefab;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter enzymeSpraySound;

	[AssertNotNull]
	public Transform emitPoint;

	[AssertNotNull]
	public TrailRenderer enzymeTrail;

	public float enzymeTrailDuration = 20f;

	public float emitCloudInterval = 10f;

	private float timeNextEmitCloud = -1f;

	private float enzymeAmount = -1f;

	public void SprayEnzymes(AnimationEvent animationEvent)
	{
		enzymeAmount = enzymeTrailDuration;
		timeNextEmitCloud = Time.time + Random.value * 2f;
	}

	public void Update()
	{
		if (enzymeAmount > 0f)
		{
			enzymeAmount -= Time.deltaTime;
			enzymeTrail.time = Mathf.Clamp(enzymeAmount, 0f, enzymeTrailDuration);
			enzymeTrail.enabled = true;
			enzymeSpraySound.Play();
			float time = Time.time;
			if (time > timeNextEmitCloud)
			{
				Object.Instantiate(enzymeCloudPrefab, emitPoint.position, emitPoint.rotation, null);
				timeNextEmitCloud = time + emitCloudInterval;
			}
		}
		else
		{
			enzymeTrail.enabled = false;
			enzymeSpraySound.Stop();
		}
	}
}
