using UnityEngine;

public class CreatureFear : MonoBehaviour
{
	private Vector3 _defaultScarePosition;

	public Vector3 lastScarePosition
	{
		get
		{
			if (!(target != null))
			{
				return _defaultScarePosition;
			}
			return target.transform.position;
		}
	}

	public GameObject target { get; private set; }

	public float range { get; private set; }

	public float lastScareTime { get; private set; }

	public void SetTarget(GameObject target, float range = 100f)
	{
		this.target = target;
		this.range = range;
		lastScareTime = ((target != null) ? Time.time : (-1f));
		_defaultScarePosition = ((target != null) ? target.transform.position : Vector3.zero);
	}

	public void SetScarePosition(Vector3 position, float range = 100f)
	{
		target = null;
		this.range = range;
		lastScareTime = Time.time;
		_defaultScarePosition = position;
	}
}
