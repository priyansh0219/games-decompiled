using UWE;
using UnityEngine;

public class CollisionSound : MonoBehaviour
{
	public FMODAsset hitSoundSmall;

	public FMODAsset hitSoundFast;

	public FMODAsset hitSoundMedium;

	public FMODAsset hitSoundSlow;

	public void OnCollisionEnter(Collision col)
	{
		if (col.contacts.Length != 0)
		{
			Rigidbody rootRigidbody = UWE.Utils.GetRootRigidbody(col.gameObject);
			float magnitude = col.relativeVelocity.magnitude;
			FMODAsset asset = (((bool)rootRigidbody && rootRigidbody.mass < 10f) ? hitSoundSmall : ((magnitude > 8f) ? hitSoundFast : ((!(magnitude > 4f)) ? hitSoundSlow : hitSoundMedium)));
			float soundRadiusObsolete = Mathf.Clamp01(magnitude / 8f);
			Utils.PlayFMODAsset(asset, col.contacts[0].point, soundRadiusObsolete);
		}
	}
}
