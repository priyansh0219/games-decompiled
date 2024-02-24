using System.Collections;
using UWE;
using UnityEngine;

public abstract class PrecursorVentBase : MonoBehaviour
{
	[AssertNotNull]
	public Transform entryPivot;

	[AssertNotNull]
	public Transform exitPivot;

	[AssertNotNull]
	public Animation entryAnimation;

	[AssertNotNull]
	public Animation exitAnimation;

	[AssertNotNull]
	public FMODAsset entrySound;

	[AssertNotNull]
	public FMODAsset exitSound;

	public float emitVelocity = 10f;

	public float minSuckInterval = 3f;

	public float interpolationDuration = 0.5f;

	private float timeNextSuck = -1f;

	protected abstract void StorePeeper(Peeper peeper);

	protected abstract Peeper RetrievePeeper();

	public bool CanSuckIn()
	{
		return DayNightUtils.time > timeNextSuck;
	}

	public void SuckInPeeper(Peeper peeper)
	{
		StartCoroutine(SuckInPeeperAsync(peeper));
	}

	private IEnumerator SuckInPeeperAsync(Peeper peeper)
	{
		timeNextSuck = DayNightUtils.time + minSuckInterval;
		entryAnimation.Rewind();
		entryAnimation.Play();
		entryAnimation.Sample();
		entryAnimation.Stop();
		Transform transform = peeper.transform;
		peeper.useRigidbody.isKinematic = true;
		transform.SetParent(entryPivot, worldPositionStays: true);
		LargeWorldStreamer main = LargeWorldStreamer.main;
		if ((bool)main && main.cellManager != null)
		{
			main.cellManager.UnregisterEntity(peeper.largeWorldEntity);
		}
		peeper.StartCoroutine(UWE.Utils.LerpTransform(transform, Vector3.zero, Quaternion.identity, Vector3.one, interpolationDuration));
		Utils.PlayFMODAsset(entrySound, base.transform);
		entryAnimation.Play();
		while (entryAnimation.isPlaying)
		{
			yield return null;
		}
		if ((bool)peeper)
		{
			StorePeeper(peeper);
		}
	}

	public void EmitPeeper()
	{
		StartCoroutine(EmitPeeperAsync());
	}

	private IEnumerator EmitPeeperAsync()
	{
		Peeper peeper = RetrievePeeper();
		if (!peeper)
		{
			yield break;
		}
		exitAnimation.Rewind();
		exitAnimation.Play();
		exitAnimation.Sample();
		exitAnimation.Stop();
		Transform obj = peeper.transform;
		peeper.useRigidbody.isKinematic = true;
		obj.SetParent(exitPivot, worldPositionStays: false);
		obj.localPosition = Vector3.zero;
		obj.localRotation = Quaternion.identity;
		peeper.gameObject.SetActive(value: true);
		Utils.PlayFMODAsset(exitSound, base.transform);
		exitAnimation.Play();
		while (exitAnimation.isPlaying)
		{
			yield return null;
		}
		if ((bool)peeper)
		{
			LargeWorldStreamer main = LargeWorldStreamer.main;
			if ((bool)main && main.cellManager != null)
			{
				main.cellManager.RegisterEntity(peeper.largeWorldEntity);
			}
			peeper.useRigidbody.isKinematic = false;
			peeper.useRigidbody.AddForce(exitPivot.forward * emitVelocity, ForceMode.VelocityChange);
		}
	}
}
