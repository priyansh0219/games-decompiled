using UnityEngine;

public class VFXPooledFX : MonoBehaviour
{
	public float StopAfterSeconds = 1f;

	public float RecycleDelay = 2f;

	private float timer;

	private bool isPlaying;

	public VFXPool.FX fx;

	private void OnEnable()
	{
		timer = 0f;
		isPlaying = true;
	}

	private void Update()
	{
		if (fx != null)
		{
			timer += Time.deltaTime;
			if (timer > StopAfterSeconds && StopAfterSeconds != 0f && isPlaying)
			{
				base.gameObject.GetComponent<ParticleSystem>().Stop();
				isPlaying = false;
			}
			if (timer > StopAfterSeconds + RecycleDelay && RecycleDelay != 0f)
			{
				VFXPool.main.Recycle(base.gameObject, fx);
			}
		}
	}
}
