using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class SwimWaterSplash : MonoBehaviour
{
	[AssertNotNull]
	public GameObject swimUnderWaterEffect;

	[AssertNotNull]
	public GameObject swimSurfaceEffect;

	[AssertNotNull]
	public Transform leftArmSplashTransform;

	[AssertNotNull]
	public Transform rightArmSplashTransform;

	[AssertNotNull]
	public FMOD_StudioEventEmitter splashUnderwaterSound;

	private PARAMETER_ID fmodIndexSpeed = FMODUWE.invalidParameterId;

	private PARAMETER_ID fmodIndexDepth = FMODUWE.invalidParameterId;

	private void SpawnEffect(Transform useTransform)
	{
		GameObject original = swimSurfaceEffect;
		FMOD_StudioEventEmitter fMOD_StudioEventEmitter = null;
		if (Player.main.IsUnderwater())
		{
			original = swimUnderWaterEffect;
			fMOD_StudioEventEmitter = splashUnderwaterSound;
		}
		if (Inventory.main.GetHeldTool() == null)
		{
			GameObject obj = Object.Instantiate(original);
			obj.transform.position = useTransform.position;
			obj.transform.parent = useTransform;
		}
		if (fMOD_StudioEventEmitter != null)
		{
			EventInstance @event = FMODUWE.GetEvent(fMOD_StudioEventEmitter.asset);
			if (FMODUWE.IsInvalidParameterId(fmodIndexSpeed))
			{
				fmodIndexSpeed = FMODUWE.GetEventInstanceParameterIndex(@event, "speed");
			}
			if (FMODUWE.IsInvalidParameterId(fmodIndexDepth))
			{
				fmodIndexDepth = FMODUWE.GetEventInstanceParameterIndex(@event, "depth");
			}
			ATTRIBUTES_3D attributes = useTransform.To3DAttributes();
			@event.set3DAttributes(attributes);
			@event.setVolume(1f);
			@event.setParameterValueByIndex(fmodIndexSpeed, Player.main.movementSpeed);
			@event.setParameterValueByIndex(fmodIndexDepth, Player.main.depthLevel);
			@event.start();
			@event.release();
		}
	}

	private void LeftArmSplash()
	{
		SpawnEffect(leftArmSplashTransform);
	}

	private void RightArmSplash()
	{
		SpawnEffect(rightArmSplashTransform);
	}
}
