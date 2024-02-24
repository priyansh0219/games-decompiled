using UnityEngine;

public class PowerFX : MonoBehaviour
{
	public GameObject vfxPrefab;

	private GameObject vfxEffectObject;

	public Transform attachPoint;

	private GameObject target;

	private bool vfxVisible = true;

	public static Vector3 GetConnectionPoint(GameObject target, Vector3 sourcePoint)
	{
		PowerRelay component = target.GetComponent<PowerRelay>();
		PowerFX component2 = target.GetComponent<PowerFX>();
		if (component != null)
		{
			return component.GetConnectPoint(sourcePoint);
		}
		if (component2 != null)
		{
			if (!(component2.attachPoint != null))
			{
				return target.transform.position;
			}
			return component2.attachPoint.position;
		}
		return target.transform.position;
	}

	public void SetTarget(GameObject targetObj)
	{
		if (targetObj != null)
		{
			if (vfxEffectObject == null)
			{
				vfxEffectObject = Object.Instantiate(vfxPrefab, Vector3.zero, Quaternion.identity);
				vfxEffectObject.transform.parent = base.gameObject.transform;
				vfxEffectObject.SetActive(vfxVisible);
			}
			LineRenderer component = vfxEffectObject.GetComponent<LineRenderer>();
			vfxEffectObject.transform.localPosition = Vector3.zero;
			Vector3 vector = ((attachPoint != null) ? attachPoint.position : base.transform.position);
			component.SetPosition(0, vector);
			Vector3 connectionPoint = GetConnectionPoint(targetObj, vector);
			component.SetPosition(1, connectionPoint);
		}
		target = targetObj;
	}

	private void DestroyVFX()
	{
		if (vfxEffectObject != null)
		{
			Object.Destroy(vfxEffectObject);
			vfxEffectObject = null;
		}
	}

	private void UpdateTarget()
	{
		SetTarget(target);
	}

	private void Start()
	{
		InvokeRepeating("UpdateTarget", Random.value, 5f);
	}

	private void Update()
	{
		if (target == null)
		{
			DestroyVFX();
		}
	}

	private void OnDestroy()
	{
		DestroyVFX();
	}

	public void SetVFXVisible(bool visible)
	{
		vfxVisible = visible;
		if (vfxEffectObject != null)
		{
			vfxEffectObject.SetActive(visible);
		}
	}
}
