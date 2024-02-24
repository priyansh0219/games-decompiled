using FMOD.Studio;
using UnityEngine;

public class UpdateSFXDepth : MonoBehaviour
{
	[AssertNotNull]
	public FMOD_CustomEmitter sfx;

	private PARAMETER_ID fmodIndexDepth = FMODUWE.invalidParameterId;

	private void Start()
	{
		sfx.Play();
		InvokeRepeating("UpdateDepth", 0f, 1f);
		InvokeRepeating("SelfDestroy", 5f, 5f);
	}

	private void UpdateDepth()
	{
		if (FMODUWE.IsInvalidParameterId(fmodIndexDepth))
		{
			fmodIndexDepth = sfx.GetParameterIndex("depth");
		}
		sfx.SetParameterValue(fmodIndexDepth, Player.main.transform.position.y);
	}

	private void SelfDestroy()
	{
		if (!sfx.playing)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
