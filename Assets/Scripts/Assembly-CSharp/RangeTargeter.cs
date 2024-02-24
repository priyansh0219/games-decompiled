using UWE;
using UnityEngine;

public class RangeTargeter : MonoBehaviour
{
	public EcoTargetType targetType = EcoTargetType.Shark;

	public float range;

	public Transform eye;

	public float updateTargetInterval = 2f;

	private float timeNextSearch;

	private EcoRegion.TargetFilter isTargetValidFilter;

	public GameObject target { get; private set; }

	private void Start()
	{
		isTargetValidFilter = IsTargetValid;
	}

	private bool IsTargetValid(IEcoTarget ecoTarget)
	{
		bool result = false;
		GameObject gameObject = ecoTarget.GetGameObject();
		if ((bool)gameObject && (gameObject.transform.position - eye.position).magnitude < range)
		{
			Player component = gameObject.GetComponent<Player>();
			if (component != null && !component.CanBeAttacked())
			{
				return false;
			}
			Vector3 position = default(Vector3);
			GameObject closestObj = null;
			Vector3 direction = Vector3.Normalize(gameObject.transform.position - eye.position);
			UWE.Utils.TraceForTarget(eye.position, direction, base.gameObject, range, ref closestObj, ref position);
			if (closestObj == gameObject || closestObj == null)
			{
				result = true;
			}
		}
		return result;
	}

	private void Update()
	{
		if (Time.time > timeNextSearch)
		{
			target = EcoRegionManager.main.FindNearestTarget(targetType, base.transform.position, isTargetValidFilter)?.GetGameObject();
		}
	}
}
