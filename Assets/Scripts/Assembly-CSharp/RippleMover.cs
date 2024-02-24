using UnityEngine;

public class RippleMover : MonoBehaviour
{
	public float kRippleSpeed = 1f;

	public float kRippleAmount = 1f;

	public Vector3 kRippleNormal = new Vector3(1f, 0f, 0f);

	private Vector3 originalLocalPosition;

	private void OnEnable()
	{
		Debug.Log("RippleMover - Setting original local position");
		originalLocalPosition = base.transform.localPosition;
	}

	public virtual float GetRippleAmount()
	{
		return kRippleAmount;
	}

	private void Update()
	{
		float f = Time.time * kRippleSpeed;
		base.transform.localPosition = originalLocalPosition + kRippleNormal * Mathf.Sin(f) * GetRippleAmount();
	}
}
