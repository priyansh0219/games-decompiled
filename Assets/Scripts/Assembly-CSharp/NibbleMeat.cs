using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NibbleMeat : MonoBehaviour
{
	[AssertNotNull]
	public Creature creature;

	[AssertNotNull]
	public LiveMixin liveMixin;

	[AssertNotNull]
	public GameObject mouth;

	public float nibbleAggressionDecrement = 0.05f;

	public float nibbleHungerDecrement = 0.05f;

	public float nibbleMassToRemove = 1f;

	public GameObject NibbleMeatFX;

	public FMOD_StudioEventEmitter nibbleSound;

	protected bool frozen;

	private float timeLastNibble;

	private void OnTriggerEnter(Collider collider)
	{
		Rigidbody attachedRigidbody = collider.attachedRigidbody;
		GameObject gameObject = ((attachedRigidbody == null) ? collider.gameObject : attachedRigidbody.gameObject);
		if (!liveMixin.IsAlive() || frozen)
		{
			return;
		}
		EcoTarget component = gameObject.GetComponent<EcoTarget>();
		bool flag = component != null && component.type == EcoTargetType.DeadMeat;
		LiveMixin component2 = gameObject.GetComponent<LiveMixin>();
		if (!((component2 != null && !component2.IsAlive()) || flag))
		{
			return;
		}
		timeLastNibble = Time.time;
		Rigidbody componentInParent = gameObject.GetComponentInParent<Rigidbody>();
		if (componentInParent != null)
		{
			componentInParent.mass = Mathf.Max(componentInParent.mass - nibbleMassToRemove, 1f);
			Vector3 position = collider.ClosestPointOnBounds(mouth.transform.position);
			if (nibbleSound != null)
			{
				Utils.PlayEnvSound(nibbleSound, position);
			}
			if (NibbleMeatFX != null)
			{
				Utils.PlayOneShotPS(NibbleMeatFX, position, Quaternion.LookRotation(gameObject.transform.forward, Vector3.up));
			}
			creature.Aggression.Add(0f - nibbleAggressionDecrement);
			creature.Hunger.Add(0f - nibbleHungerDecrement);
			if (Mathf.Approximately(componentInParent.mass, 1f))
			{
				Object.Destroy(gameObject);
			}
		}
	}

	public void OnFreezeByStasisSphere()
	{
		frozen = true;
	}

	public void OnUnfreezeByStasisSphere()
	{
		frozen = false;
	}
}
