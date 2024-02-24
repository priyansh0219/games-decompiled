using UnityEngine;

public class FadeLightController : MonoBehaviour
{
	public float CurrentFade;

	public virtual void Initialize(float fade)
	{
		CurrentFade = fade;
	}

	public virtual void Update()
	{
	}

	public void FadeTo(float fade, float rate)
	{
	}

	public void FadeDestroy(float rate)
	{
	}
}
