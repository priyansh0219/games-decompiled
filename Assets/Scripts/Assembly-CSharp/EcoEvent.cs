using UnityEngine;

public class EcoEvent
{
	private EcoEventType type;

	private float timeCreated;

	private string debugName = string.Empty;

	private GameObject source;

	private float range = 20f;

	private float param;

	private float mass;

	private Object genericParam;

	private Vector3 position = Vector3.zero;

	private Vector3 velocity = Vector3.zero;

	public bool debug;

	public override string ToString()
	{
		return $"{type} pos={position}, range={range}, param={param}, name='{debugName}', src='{source}'";
	}

	public void Initialize(EcoEventType eventType, string debugName, GameObject source, float range, float param, Vector3 position, Object genericParam)
	{
		timeCreated = Time.time;
		type = eventType;
		this.source = source;
		this.range = range;
		this.param = param;
		this.position = position;
		this.debugName = debugName;
		this.genericParam = genericParam;
		if (!(source != null))
		{
			return;
		}
		Rigidbody componentInChildren = source.GetComponentInChildren<Rigidbody>();
		if (componentInChildren != null)
		{
			mass = componentInChildren.mass;
			Player component = source.GetComponent<Player>();
			if (component != null)
			{
				velocity = component.playerController.velocity;
			}
			else
			{
				velocity = componentInChildren.velocity;
			}
		}
	}

	public void Initialize(EcoEvent e)
	{
		type = e.type;
		timeCreated = e.timeCreated;
		debugName = e.debugName;
		source = e.source;
		range = e.range;
		param = e.param;
		mass = e.mass;
		genericParam = e.genericParam;
		position = e.position;
		velocity = e.velocity;
	}

	public void ReleaseReferences()
	{
		source = null;
	}

	public EcoEventType GetEventType()
	{
		return type;
	}

	public GameObject GetSource()
	{
		return source;
	}

	public float GetTimeCreated()
	{
		return timeCreated;
	}

	public Vector3 GetPosition()
	{
		return position;
	}

	public Vector3 GetVelocity()
	{
		return velocity;
	}

	public float GetRange()
	{
		return range;
	}

	public float GetMass()
	{
		return mass;
	}

	public float GetParam()
	{
		return param;
	}

	public Object GetGenericParam()
	{
		return genericParam;
	}
}
