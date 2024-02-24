using System.Collections.Generic;
using UWE;
using UnityEngine;

public class SeamothTorpedoWhirlpool : MonoBehaviour
{
	public class State
	{
		public bool state;

		public Transform transform;

		public Creature creatureBehaviour;

		public Locomotion locomotion;

		public LargeWorldEntity largeWorldEntity;

		public LiveMixin liveMixin;

		public CollisionDetectionMode collisionDetectionMode;
	}

	private static Dictionary<Rigidbody, SeamothTorpedoWhirlpool> allTargets = new Dictionary<Rigidbody, SeamothTorpedoWhirlpool>();

	public AnimationCurve explosion;

	public AnimationCurve rotation;

	public float radiusPush = 1f;

	public float radiusPull = 15f;

	public float massMin = 100f;

	public float massMax = 300f;

	public float pullMin = 0.2f;

	public float pullMax = 3f;

	public float spinMin;

	public float spinMax = 600f;

	public float punchForce = 30f;

	public float duration = 15f;

	public LayerMask layerMask = -1;

	private Transform tr;

	private List<Rigidbody> targets;

	private Dictionary<Rigidbody, State> states;

	private Sequence sequence;

	private float force;

	private float momentum;

	public float life => sequence.t;

	private void Awake()
	{
		tr = GetComponent<Transform>();
		targets = new List<Rigidbody>();
		states = new Dictionary<Rigidbody, State>();
		sequence = new Sequence();
		sequence.Set(duration, current: false, target: true, Die);
	}

	private void Update()
	{
		sequence.Update();
		if (sequence.active)
		{
			force = explosion.Evaluate(sequence.t);
			momentum = rotation.Evaluate(sequence.t);
			Rigidbody target = null;
			int num = UWE.Utils.OverlapSphereIntoSharedBuffer(tr.position, radiusPull, layerMask);
			for (int i = 0; i < num; i++)
			{
				Collider other = UWE.Utils.sharedColliderBuffer[i];
				Register(other, ref target);
			}
		}
		UpdatePhysics(Time.deltaTime);
	}

	private void UpdatePhysics(float deltaTime)
	{
		if (!sequence.active)
		{
			return;
		}
		Vector3 up = Vector3.up;
		for (int num = targets.Count - 1; num >= 0; num--)
		{
			Rigidbody rigidbody = targets[num];
			if (rigidbody == null || !rigidbody.gameObject.activeSelf)
			{
				Unregister(rigidbody);
			}
			else
			{
				Pickupable componentInParent = rigidbody.GetComponentInParent<Pickupable>();
				if (componentInParent != null && componentInParent.attached)
				{
					Unregister(rigidbody);
				}
				else
				{
					State state = states[rigidbody];
					Transform transform = state.transform;
					if (transform == null)
					{
						Unregister(rigidbody);
					}
					if (rigidbody.collisionDetectionMode != CollisionDetectionMode.Continuous)
					{
						state.collisionDetectionMode = rigidbody.collisionDetectionMode;
						rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
					}
					Vector3 position = transform.position;
					Vector3 vector = tr.position - position;
					float magnitude = vector.magnitude;
					Vector3 vector2 = ((magnitude == 0f) ? Vector3.zero : (vector / magnitude));
					Vector3 normalized = Vector3.Cross(vector2, up).normalized;
					float mass = rigidbody.mass;
					float num2 = 1f - Mathf.Clamp01((magnitude - radiusPush) / (radiusPull - radiusPush));
					float num3 = 1f - Mathf.Clamp01((mass - massMin) / (massMax - massMin));
					if (state.state)
					{
						if (magnitude >= radiusPull + 1f)
						{
							Unregister(rigidbody);
						}
					}
					else if (magnitude > radiusPush)
					{
						float num4 = Mathf.Lerp(pullMin, pullMax, num2 * num3) * force * deltaTime;
						float num5 = Mathf.Lerp(spinMin, spinMax, num2 * num3) * momentum * deltaTime;
						Vector3 vector3 = transform.position + num4 * vector2;
						Quaternion quaternion = Quaternion.AngleAxis(num5, up);
						vector3 = tr.position + quaternion * (vector3 - tr.position);
						rigidbody.MovePosition(vector3);
						rigidbody.angularVelocity = up * num5;
					}
					else
					{
						rigidbody.AddForce(normalized * punchForce * Mathf.Clamp01(Mathf.Abs(force)) * num3, ForceMode.VelocityChange);
						state.state = true;
					}
				}
			}
		}
	}

	private void OnDisable()
	{
		UnregisterAll();
	}

	private bool Register(Collider other, ref Rigidbody target)
	{
		target = other.GetComponentInParent<Rigidbody>();
		if (target == null)
		{
			return false;
		}
		if (target.isKinematic)
		{
			return false;
		}
		if (targets.Contains(target))
		{
			return false;
		}
		if (allTargets.TryGetValue(target, out var value))
		{
			if (!(life < value.life))
			{
				return false;
			}
			value.Unregister(target);
		}
		Transform transform = target.GetComponent<Transform>();
		do
		{
			if ((bool)transform.GetComponent<Player>() || (bool)transform.GetComponent<SubRoot>() || (bool)transform.GetComponent<Vehicle>())
			{
				return false;
			}
			transform = transform.parent;
		}
		while (transform != null);
		Transform component = target.GetComponent<Transform>();
		if ((component.position - tr.position).magnitude >= radiusPull)
		{
			return false;
		}
		Creature component2 = target.GetComponent<Creature>();
		Locomotion component3 = target.GetComponent<Locomotion>();
		LargeWorldEntity component4 = target.GetComponent<LargeWorldEntity>();
		LiveMixin component5 = target.GetComponent<LiveMixin>();
		State state = new State();
		state.transform = component;
		state.creatureBehaviour = ((component2 != null && component2.enabled) ? component2 : null);
		state.locomotion = ((component3 != null && component3.enabled) ? component3 : null);
		state.largeWorldEntity = ((component4 != null && component4.enabled) ? component4 : null);
		state.liveMixin = component5;
		state.collisionDetectionMode = target.collisionDetectionMode;
		allTargets.Add(target, this);
		targets.Add(target);
		states.Add(target, state);
		TriggerBehaviours(state, enable: false);
		return true;
	}

	private void TriggerBehaviours(State state, bool enable)
	{
		if (state == null)
		{
			return;
		}
		LiveMixin liveMixin = state.liveMixin;
		if (liveMixin != null && liveMixin.IsAlive())
		{
			Creature creatureBehaviour = state.creatureBehaviour;
			if (creatureBehaviour != null)
			{
				creatureBehaviour.enabled = enable;
				if (enable)
				{
					creatureBehaviour.leashPosition = creatureBehaviour.transform.position;
				}
			}
			Locomotion locomotion = state.locomotion;
			if (locomotion != null)
			{
				locomotion.enabled = enable;
			}
		}
		if (!(state.largeWorldEntity != null))
		{
			return;
		}
		Transform transform = state.transform;
		GameObject gameObject = transform.gameObject;
		CellManager cellManager = LargeWorldStreamer.main.cellManager;
		if (enable)
		{
			Pickupable component = gameObject.GetComponent<Pickupable>();
			if (component == null || !component.attached)
			{
				cellManager.RegisterEntity(gameObject);
			}
		}
		else
		{
			transform.parent = null;
			cellManager.UnregisterEntity(gameObject);
		}
	}

	private void Unregister(Rigidbody target)
	{
		if (states.TryGetValue(target, out var value))
		{
			if (target != null && value != null)
			{
				target.collisionDetectionMode = value.collisionDetectionMode;
			}
			TriggerBehaviours(value, enable: true);
			states.Remove(target);
		}
		allTargets.Remove(target);
		targets.Remove(target);
	}

	private void UnregisterAll()
	{
		for (int num = targets.Count - 1; num >= 0; num--)
		{
			Unregister(targets[num]);
		}
	}

	private void Die()
	{
		Object.Destroy(base.gameObject);
	}
}
