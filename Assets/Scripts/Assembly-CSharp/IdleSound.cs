using UnityEngine;

public class IdleSound : MonoBehaviour
{
	public float minTime = 5f;

	public float maxTime = 15f;

	public FMOD_StudioEventEmitter idleSound;

	public bool looping;

	private void OnEnable()
	{
		if (!looping)
		{
			Invoke("PlayIdle", Random.Range(minTime, maxTime));
		}
		else
		{
			Invoke("UpdateLoopingSound", 1f);
		}
	}

	private void PlayIdle()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Utils.PlayEnvSound(idleSound, base.transform.position);
			Invoke("PlayIdle", Random.Range(minTime, maxTime));
		}
	}

	private void UpdateLoopingSound()
	{
		bool flag = true;
		LiveMixin component = GetComponent<LiveMixin>();
		if (component != null && component.health <= 0f)
		{
			flag = false;
		}
		if (idleSound.GetIsPlaying() != flag)
		{
			if (flag)
			{
				Utils.PlayEnvSound(idleSound);
			}
			else
			{
				idleSound.Stop();
			}
		}
	}
}
