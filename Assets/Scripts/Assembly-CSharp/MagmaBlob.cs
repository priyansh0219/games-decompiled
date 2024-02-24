using UnityEngine;

public class MagmaBlob : MonoBehaviour
{
	private float scaleRatio;

	private Vector3 localStartPos;

	private Vector3 localEndPos;

	private SphereCollider sphereCollider;

	private float timeLastGrown;

	public void SetScaleRatio(float ratio)
	{
		scaleRatio = ratio;
	}

	public Vector3 GetLocalStartPos()
	{
		return localStartPos;
	}

	public Vector3 GetLocalEndPos()
	{
		return localEndPos;
	}

	public void SetLocalStartPos(Vector3 pos)
	{
		localStartPos = pos;
	}

	public void SetLocalEndPos(Vector3 pos)
	{
		localEndPos = pos;
	}

	private void OnEnable()
	{
		sphereCollider = base.gameObject.GetComponent<SphereCollider>();
		if (sphereCollider == null)
		{
			sphereCollider = (SphereCollider)base.gameObject.AddComponent(typeof(SphereCollider));
		}
	}

	public void UpdateGrowingBlob(float time, float growTime, float growCurveValue, float moveCurveValue, float freezeCurveValue)
	{
		_ = time / growTime;
		float num = growCurveValue * scaleRatio;
		base.transform.localPosition = Vector3.Lerp(localStartPos, localEndPos, moveCurveValue);
		base.transform.localScale = new Vector3(num, num, num);
		sphereCollider.radius = 0.5f;
		base.gameObject.GetComponent<Renderer>().material.SetFloat(ShaderPropertyID._EmissiveCut, freezeCurveValue);
		if (!Utils.NearlyEqual(time, 0f) && !Utils.NearlyEqual(time, 1f))
		{
			timeLastGrown = Time.time;
		}
		base.gameObject.SetActive(value: true);
	}

	public void UpdateDestroyingBlob(float time, float destroyTime, float destroyCurveValue)
	{
		float a = time / destroyTime;
		base.gameObject.GetComponent<Renderer>().material.SetFloat(ShaderPropertyID._Cutoff, destroyCurveValue);
		if (Utils.NearlyEqual(a, 1f))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		Player component = other.gameObject.GetComponent<Player>();
		if (component != null && component.GetCurrentSub() == null && Time.time < timeLastGrown + 0.1f)
		{
			component.gameObject.GetComponent<LiveMixin>().TakeDamage(20f, base.transform.position);
		}
	}
}
