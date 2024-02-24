using UnityEngine;

public class PlayerBreathBubbles : MonoBehaviour
{
	[AssertNotNull]
	public ParticleSystem bubblesPrefab;

	[AssertNotNull]
	public Transform anchor;

	public float delay = 20f;

	[AssertNotNull]
	public FMOD_CustomEmitter bubbleSound;

	private GameObject bubbles;

	private Utils.MonitoredValue<bool> isUnderWater = new Utils.MonitoredValue<bool>();

	private void Start()
	{
		base.gameObject.FindAncestor<Player>().isUnderwater.changedEvent.AddHandler(this, OnUnderWaterStateChange);
		DevConsole.RegisterConsoleCommand(this, "nobubbles");
	}

	private void OnConsoleCommand_nobubbles()
	{
		base.enabled = false;
	}

	private void HideForScreenshots()
	{
		base.enabled = false;
		Object.Destroy(bubbles);
	}

	private void UnhideForScreenshots()
	{
		base.enabled = true;
	}

	private void OnUnderWaterStateChange(Utils.MonitoredValue<bool> isUnderWater)
	{
		if (isUnderWater.value)
		{
			InvokeRepeating("MakeBubbles", delay, delay);
		}
		else
		{
			CancelInvoke("MakeBubbles");
		}
	}

	private void MakeBubbles()
	{
		if (base.enabled)
		{
			bubbles = Object.Instantiate(bubblesPrefab.gameObject);
			if ((bool)bubbles)
			{
				Transform obj = bubbles.transform;
				bubbleSound.Play();
				obj.SetParent(anchor, worldPositionStays: false);
				obj.localPosition = Vector3.zero;
				obj.localRotation = Quaternion.identity;
				obj.localScale = Vector3.one;
				ParticleSystem component = bubbles.GetComponent<ParticleSystem>();
				component.Play();
				Object.Destroy(bubbles, component.duration + component.startLifetime);
			}
		}
	}
}
