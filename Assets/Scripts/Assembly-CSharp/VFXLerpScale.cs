using UWE;
using UnityEngine;

public class VFXLerpScale : MonoBehaviour, GameObjectPool.IPooledObject
{
	public AnimationCurve scaleX;

	public AnimationCurve scaleY;

	public AnimationCurve scaleZ;

	public bool playing;

	public bool looping;

	public float duration = 1f;

	private float animTime;

	private Vector3 initScale;

	private Vector3 currentScale;

	private bool isPlaying;

	public void Play()
	{
		isPlaying = true;
		animTime = 0f;
		currentScale = initScale;
	}

	private void Awake()
	{
		if (playing)
		{
			Play();
		}
	}

	private void Update()
	{
		if (!isPlaying)
		{
			return;
		}
		animTime += Time.deltaTime / duration;
		if (animTime > 0.99f)
		{
			if (!looping)
			{
				isPlaying = false;
				return;
			}
			Play();
		}
		if (initScale == Vector3.zero)
		{
			initScale = base.transform.localScale;
		}
		currentScale = new Vector3(initScale.x * scaleX.Evaluate(animTime), initScale.y * scaleY.Evaluate(animTime), initScale.z * scaleZ.Evaluate(animTime));
		if (currentScale != Vector3.zero)
		{
			base.transform.localScale = currentScale;
		}
	}

	public void Spawn(float time = 0f, bool active = true)
	{
		if (playing)
		{
			Play();
		}
	}

	public void Despawn(float time = 0f)
	{
	}
}
