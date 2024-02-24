using UnityEngine;

public class VFXStopAfterSeconds : MonoBehaviour
{
	public enum PostBehaviour
	{
		nothing = 0,
		desactivate = 1,
		destroy = 2
	}

	public PostBehaviour postBehaviour;

	public float lifeTime = 1f;

	public float randomOffset;

	public float postBehaviourDelay;

	public bool isDistanceBased;

	public bool destroyMaterials;

	private float endTime;

	private bool stopped;

	private void Start()
	{
		endTime = Time.time + lifeTime + Random.Range(0f - randomOffset, randomOffset);
	}

	private void OnEnable()
	{
		if (postBehaviour == PostBehaviour.desactivate)
		{
			endTime = Time.time + lifeTime + Random.Range(0f - randomOffset, randomOffset);
		}
	}

	private void Update()
	{
		if (!(Time.time >= endTime))
		{
			return;
		}
		if (!stopped)
		{
			ParticleSystem component = GetComponent<ParticleSystem>();
			if (component != null)
			{
				component.Stop();
			}
			Trail_v2 component2 = GetComponent<Trail_v2>();
			if (component2 != null)
			{
				component2.Stop();
			}
			if (isDistanceBased)
			{
				base.transform.parent = null;
			}
			stopped = true;
		}
		if (!(Time.time >= endTime + postBehaviourDelay))
		{
			return;
		}
		if (postBehaviour == PostBehaviour.destroy)
		{
			Object.Destroy(base.gameObject);
		}
		else if (postBehaviour == PostBehaviour.desactivate)
		{
			ParticleSystem component3 = GetComponent<ParticleSystem>();
			if (component3 != null)
			{
				component3.Clear();
			}
			base.gameObject.SetActive(value: false);
		}
	}
}
