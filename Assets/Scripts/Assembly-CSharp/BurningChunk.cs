using UnityEngine;

public class BurningChunk : MonoBehaviour
{
	private enum State
	{
		burning = 0,
		coolingDown = 1
	}

	public float burningTime = 4f;

	public float fireDamage = 5f;

	public float cooldownTime = 4f;

	public float scaleRandomFactor = 0.3f;

	public Transform model;

	[AssertNotNull]
	public Transform fxSpawnPoint;

	[AssertNotNull]
	public Rigidbody rb;

	[AssertNotNull]
	public VFXController fxControl;

	private State currentState;

	private float timeNextState;

	private SeaDragon seaDragon;

	private Vector3 initScale = Vector3.one;

	private void Start()
	{
		currentState = State.burning;
		timeNextState = Time.time + burningTime;
		fxControl.Play(0);
		if (model != null)
		{
			model.rotation = Random.rotation;
		}
		float num = Random.Range(1f - scaleRandomFactor, 1f + scaleRandomFactor);
		initScale = new Vector3(num, num, num);
		base.transform.localScale = initScale;
	}

	private void Update()
	{
		Vector3 forward = Vector3.Lerp(Vector3.up, Vector3.Normalize(rb.velocity), Mathf.Clamp01(rb.velocity.magnitude * 0.1f));
		fxSpawnPoint.forward = forward;
		if (Time.time > timeNextState)
		{
			if (currentState == State.burning)
			{
				currentState = State.coolingDown;
				timeNextState = Time.time + cooldownTime;
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}
		else if (currentState == State.coolingDown)
		{
			base.transform.localScale = initScale * Mathf.InverseLerp(timeNextState, timeNextState - cooldownTime, Time.time);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (currentState != 0 || (collider.isTrigger && collider.gameObject.layer != LayerMask.NameToLayer("Useable")))
		{
			return;
		}
		GameObject gameObject = ((collider.attachedRigidbody == null) ? collider.gameObject : collider.attachedRigidbody.gameObject);
		if (!(gameObject == null) && (!(seaDragon != null) || !(gameObject == seaDragon.gameObject)))
		{
			LiveMixin component = gameObject.GetComponent<LiveMixin>();
			if (!(component == null))
			{
				float originalDamage = ((seaDragon == null) ? fireDamage : seaDragon.GetBurningChunkDamage(fireDamage));
				component.TakeDamage(originalDamage, base.transform.position, DamageType.Heat);
				Object.Destroy(base.gameObject);
			}
		}
	}

	private void OnProjectileCasted(SeaDragon creature)
	{
		seaDragon = creature;
	}
}
