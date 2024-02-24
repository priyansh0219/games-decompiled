using UnityEngine;

public class LastTarget : MonoBehaviour
{
	public GameObject target { get; private set; }

	public float targetTime { get; private set; }

	public bool targetLocked { get; private set; }

	protected virtual void SetTargetInternal(GameObject newTarget)
	{
		target = newTarget;
		targetTime = Time.time;
	}

	public void SetTarget(GameObject target)
	{
		if (!targetLocked || !(target != this.target))
		{
			SetTargetInternal(target);
		}
	}

	public void SetLockedTarget(GameObject target)
	{
		SetTargetInternal(target);
		targetLocked = true;
	}

	public void UnlockTarget()
	{
		targetLocked = false;
	}
}
