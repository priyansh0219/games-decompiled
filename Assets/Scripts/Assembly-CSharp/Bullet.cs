using UnityEngine;

public class Bullet : MonoBehaviour
{
	public LayerMask hitLayers;

	public float visibilityDistance = 0.5f;

	private bool _active;

	private float _energy;

	private float _consumption;

	private bool _visible;

	private float _speed;

	private float _path;

	public float energy => _energy;

	public bool visible => _visible;

	public float currentSpeed
	{
		get
		{
			if (!speedDependsOnEnergy)
			{
				return _speed;
			}
			return _speed * _energy;
		}
	}

	public float path => _path;

	protected Transform tr { get; private set; }

	protected GameObject go { get; private set; }

	protected virtual bool speedDependsOnEnergy => true;

	protected virtual float shellRadius => 0.5f;

	protected virtual LayerMask layerMask => hitLayers;

	protected virtual void Awake()
	{
		tr = base.transform;
		go = base.gameObject;
		_active = true;
		Deactivate();
		go.SetActive(value: false);
	}

	protected virtual void Update()
	{
		if (!_active)
		{
			return;
		}
		_energy -= _consumption * Time.deltaTime;
		if (_energy <= 0f)
		{
			_energy = 0f;
		}
		float num = currentSpeed * Time.deltaTime;
		_path += num;
		if (!_visible && _path >= visibilityDistance)
		{
			_visible = true;
			OnMadeVisible();
		}
		if (Physics.SphereCast(tr.position, shellRadius, tr.forward, out var hitInfo, num, layerMask.value))
		{
			num = hitInfo.distance;
			if (!_visible)
			{
				_visible = true;
				OnMadeVisible();
			}
			OnHit(hitInfo);
			Deactivate();
		}
		tr.position += tr.forward * num;
		if (_energy <= 0f)
		{
			OnEnergyDepleted();
			Deactivate();
		}
	}

	public virtual void Deactivate()
	{
		if (_active)
		{
			_active = false;
			_energy = 0f;
			_visible = false;
			_speed = 0f;
			_path = 0f;
		}
	}

	public virtual void Shoot(Vector3 position, Quaternion rotation, float speed, float lifeTime = -1f)
	{
		tr.position = position;
		tr.rotation = rotation;
		_active = true;
		_energy = 1f;
		_consumption = ((lifeTime < 0f) ? 0f : (1f / lifeTime));
		_visible = false;
		_speed = speed;
		_path = 0f;
		go.SetActive(value: true);
	}

	protected virtual void OnMadeVisible()
	{
	}

	protected virtual void OnHit(RaycastHit hitInfo)
	{
		Object.Destroy(go);
	}

	protected virtual void OnEnergyDepleted()
	{
		Object.Destroy(go);
	}
}
