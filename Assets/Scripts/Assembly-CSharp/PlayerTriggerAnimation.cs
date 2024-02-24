using UWE;
using UnityEngine;

public class PlayerTriggerAnimation : MonoBehaviour, ICompileTimeCheckable
{
	[AssertNotNull]
	public BoxCollider boxCollider;

	[AssertNotNull]
	public Animator targetAnimator;

	[AssertNotNull]
	public string parameterName = "";

	public FMODAsset optionalSoundOnSet;

	public FMODAsset optionalSoundOnUnset;

	public bool sendOnce;

	public bool sentAlready;

	private bool IsPlayerControlled(GameObject go)
	{
		GameObject gameObject = UWE.Utils.GetEntityRoot(go);
		if (!gameObject)
		{
			gameObject = go;
		}
		return gameObject.GetComponentInChildren<Player>() != null;
	}

	private void OnTriggerEnter(Collider enterCollider)
	{
		if (IsPlayerControlled(enterCollider.gameObject) && (!sendOnce || !sentAlready))
		{
			SafeAnimator.SetBool(targetAnimator, parameterName, value: true);
			if (optionalSoundOnSet != null)
			{
				Utils.PlayFMODAsset(optionalSoundOnSet, targetAnimator.transform, 30f);
			}
			sentAlready = true;
		}
	}

	private void OnTriggerExit(Collider exitCollider)
	{
		if (IsPlayerControlled(exitCollider.gameObject))
		{
			SafeAnimator.SetBool(targetAnimator, parameterName, value: false);
			if (optionalSoundOnUnset != null)
			{
				Utils.PlayFMODAsset(optionalSoundOnUnset, targetAnimator.transform, 30f);
			}
		}
	}

	public string CompileTimeCheck()
	{
		if (!boxCollider.isTrigger)
		{
			return "BoxCollider must be a trigger";
		}
		return null;
	}
}
