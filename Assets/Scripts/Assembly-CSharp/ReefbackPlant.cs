using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class ReefbackPlant : MonoBehaviour, GameObjectPool.IPooledObject
{
	private bool hasRigidBody;

	private bool rigidBodyKinematic;

	private bool rigidBodyDetectCollisions;

	private float rbMass;

	private float rbDrag;

	private float rbAngularDrag;

	private RigidbodyInterpolation interp;

	private CollisionDetectionMode collDet;

	private RigidbodyConstraints constraints;

	private bool rbFreezeRot;

	private bool worldForcesActive;

	private WorldForces m_worldForces;

	private GameObject m_interactionTrigger;

	private Collider[] m_colliders;

	private bool[] m_collidersEnabled;

	private void Awake()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			hasRigidBody = true;
			rigidBodyKinematic = component.isKinematic;
			rigidBodyDetectCollisions = component.detectCollisions;
			rbMass = component.mass;
			rbDrag = component.drag;
			rbAngularDrag = component.angularDrag;
			interp = component.interpolation;
			collDet = component.collisionDetectionMode;
			constraints = component.constraints;
			rbFreezeRot = component.freezeRotation;
			Object.Destroy(component);
		}
		m_worldForces = GetComponent<WorldForces>();
		if (m_worldForces != null)
		{
			worldForcesActive = m_worldForces.enabled;
			m_worldForces.enabled = false;
		}
		m_colliders = GetComponentsInChildren<Collider>();
		if (m_colliders != null)
		{
			m_collidersEnabled = new bool[m_colliders.Length];
			for (int i = 0; i < m_colliders.Length; i++)
			{
				m_collidersEnabled[i] = m_colliders[i].enabled;
				m_colliders[i].enabled = false;
			}
		}
	}

	private void Start()
	{
		if ((bool)LargeWorldStreamer.main)
		{
			LargeWorldStreamer.main.cellManager.UnregisterEntity(base.gameObject);
		}
		m_interactionTrigger = new GameObject("interaction trigger");
		m_interactionTrigger.transform.localPosition = Vector3.zero;
		m_interactionTrigger.transform.SetParent(base.transform, worldPositionStays: false);
		Vector3 localScale = base.transform.localScale;
		m_interactionTrigger.transform.localScale = new Vector3(1f / localScale.x, 1f / localScale.y, 1f / localScale.z);
		SphereCollider sphereCollider = m_interactionTrigger.AddComponent<SphereCollider>();
		sphereCollider.radius = 2f;
		sphereCollider.center = new Vector3(0f, 1f, 0f);
		sphereCollider.isTrigger = true;
		m_interactionTrigger.layer = LayerMask.NameToLayer("Useable");
	}

	public void Despawn(float time = 0f)
	{
		if (hasRigidBody)
		{
			Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
			if (rigidbody != null)
			{
				UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, rigidBodyKinematic);
				rigidbody.detectCollisions = rigidBodyDetectCollisions;
				rigidbody.mass = rbMass;
				rigidbody.drag = rbDrag;
				rigidbody.angularDrag = rbAngularDrag;
				rigidbody.collisionDetectionMode = collDet;
				rigidbody.constraints = constraints;
				rigidbody.freezeRotation = rbFreezeRot;
			}
		}
		if (m_worldForces != null)
		{
			m_worldForces.enabled = worldForcesActive;
		}
		if (m_colliders != null)
		{
			for (int i = 0; i < m_colliders.Length; i++)
			{
				m_colliders[i].enabled = m_collidersEnabled[i];
			}
		}
		Object.Destroy(m_interactionTrigger);
		Object.Destroy(this);
	}

	public void Spawn(float time = 0f, bool active = true)
	{
	}
}
