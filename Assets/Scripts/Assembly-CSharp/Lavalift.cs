using UnityEngine;

public class Lavalift : MonoBehaviour
{
	public float moveHeight;

	public float moveTime;

	public float moveDelay;

	public ParticleSystem smokeEffect;

	private float lastSmokeY;

	private float startY;

	private float timeLastEffectPlay;

	private float timeLastVerticalChange;

	private void Awake()
	{
		smokeEffect.loop = false;
		smokeEffect.playOnAwake = false;
		lastSmokeY = base.transform.position.y;
		startY = base.transform.position.y;
		timeLastEffectPlay = Time.time;
		timeLastVerticalChange = Time.time;
	}

	private void Start()
	{
		iTween.MoveTo(base.gameObject, iTween.Hash("y", base.transform.position.y + moveHeight, "time", moveTime, "easetype", iTween.EaseType.easeInOutCubic, "looptype", iTween.LoopType.pingPong, "delay", moveDelay));
	}

	private void Update()
	{
		if (Mathf.Abs(base.transform.position.y - lastSmokeY) > 0.01f)
		{
			timeLastVerticalChange = Time.time;
			if (!smokeEffect.isPlaying)
			{
				smokeEffect.Play();
				timeLastEffectPlay = Time.time;
			}
			smokeEffect.transform.position = new Vector3(smokeEffect.transform.position.x, startY, smokeEffect.transform.position.z);
			lastSmokeY = base.transform.position.y;
		}
		if (smokeEffect.isPlaying && Time.time - timeLastVerticalChange > 1f)
		{
			smokeEffect.Stop();
		}
	}
}
