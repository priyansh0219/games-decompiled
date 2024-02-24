using UnityEngine;

public class EcoTargetDistanceTracker : MonoBehaviour
{
	public EcoTargetType targetType = EcoTargetType.Shark;

	public float maxDistance = 25f;

	public bool requireLineOfSight;

	public float updateInterval = 0.1f;

	private float timeLastUpdate;

	private bool _targetNearby;

	private float _distanceToTarget = float.PositiveInfinity;

	private Vector3 _targetPosition;

	private EcoRegion.TargetFilter isTargetValidFilter;

	public float distanceToTarget => _distanceToTarget;

	public bool targetNearby => _targetNearby;

	public Vector3 targetPosition => _targetPosition;

	private void Start()
	{
		isTargetValidFilter = IsTargetValid;
	}

	private void UpdateTarget()
	{
		if (EcoRegionManager.main != null)
		{
			_targetNearby = false;
			_distanceToTarget = float.PositiveInfinity;
			IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(targetType, base.transform.position, isTargetValidFilter);
			if (ecoTarget != null)
			{
				_targetPosition = ecoTarget.GetPosition();
				float magnitude = (_targetPosition - base.transform.position).magnitude;
				_targetNearby = magnitude < maxDistance;
				_distanceToTarget = (_targetNearby ? magnitude : float.PositiveInfinity);
			}
		}
	}

	private bool IsTargetValid(IEcoTarget target)
	{
		Vector3 direction = target.GetPosition() - base.transform.position;
		float magnitude = direction.magnitude;
		if (magnitude > maxDistance)
		{
			return false;
		}
		Player component = target.GetGameObject().GetComponent<Player>();
		if (component != null && !component.CanBeAttacked())
		{
			return false;
		}
		if (requireLineOfSight && magnitude > 0.5f && Physics.Raycast(base.transform.position, direction, magnitude - 0.5f, Voxeland.GetTerrainLayerMask()))
		{
			return false;
		}
		return true;
	}

	private void Update()
	{
		if (Mathf.Approximately(updateInterval, 0f) || Time.time >= timeLastUpdate + updateInterval)
		{
			UpdateTarget();
			timeLastUpdate = Time.time;
		}
	}
}
