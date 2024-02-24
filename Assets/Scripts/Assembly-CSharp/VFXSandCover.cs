using UnityEngine;

public class VFXSandCover : MonoBehaviour
{
	public Renderer sandCoverRenderer;

	public VFXLerpScale sandCoverFadeIn;

	public VFXLerpScale sandCoverFadeOut;

	public VFXLerpColor sandCoverColor;

	public GameObject digInFX;

	private ParticleSystem digInPS;

	public GameObject digOutFX;

	public bool isDiggingUp;

	public void Start()
	{
		if (digInFX != null)
		{
			digInPS = digInFX.GetComponent<ParticleSystem>();
		}
	}

	public void StartBury()
	{
		if (sandCoverFadeIn != null)
		{
			sandCoverFadeIn.Play();
		}
		if (digInPS != null)
		{
			digInFX.SetActive(value: true);
			digInPS.Play();
		}
		if (sandCoverColor != null)
		{
			sandCoverColor.Play();
		}
		if (sandCoverRenderer != null)
		{
			sandCoverRenderer.enabled = true;
		}
	}

	public void StopBury()
	{
		if (digInPS != null)
		{
			digInPS.Stop();
		}
	}

	public void DigUp()
	{
		isDiggingUp = true;
		if (digOutFX != null)
		{
			Utils.SpawnPrefabAt(digOutFX, base.transform, base.transform.position).transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
		}
		if (sandCoverFadeOut != null)
		{
			sandCoverFadeOut.Play();
			if (sandCoverColor != null)
			{
				sandCoverColor.reverse = true;
				sandCoverColor.Play();
			}
			Object.Destroy(base.gameObject, sandCoverFadeOut.duration);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
