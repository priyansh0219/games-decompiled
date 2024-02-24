using UnityEngine;

public class VFXGrapplingRope : MonoBehaviour
{
	public Renderer meshRenderer;

	public Transform origin;

	public Transform attachPoint;

	public float radius;

	public float stiffness;

	public float gravity = 1f;

	public AnimationCurve stifnessLaunchCurve;

	public AnimationCurve stifnessReleaseCurve;

	public float timeToHook = 1f;

	private MaterialPropertyBlock block;

	private float animTime;

	private float dist;

	private float maxDistance = 35f;

	private float prevDist = -1f;

	private float prevGravity = -1f;

	private float prevStiffness = -1f;

	private int UVmulPropertyID;

	private int RopeGravPropertyID;

	public bool isLaunching;

	public bool isHooked;

	public bool isReleasing;

	private void Start()
	{
		block = new MaterialPropertyBlock();
		UVmulPropertyID = Shader.PropertyToID("_UVmultiplier");
		RopeGravPropertyID = Shader.PropertyToID("_RopeGravity");
		meshRenderer.enabled = false;
		if (origin != null)
		{
			base.transform.parent = null;
		}
	}

	public void LaunchHook(float maxDistance)
	{
		isLaunching = true;
		isHooked = false;
		isReleasing = false;
		meshRenderer.enabled = true;
	}

	public void SetIsHooked()
	{
		isLaunching = false;
		isHooked = true;
		isReleasing = false;
	}

	public void Release()
	{
		isLaunching = false;
		isHooked = false;
		isReleasing = true;
	}

	private void UpdateStiffness()
	{
		float num = Mathf.Clamp01(dist / maxDistance);
		if (isLaunching)
		{
			stiffness = stifnessLaunchCurve.Evaluate(num);
		}
		else if (isHooked && stiffness < 1f)
		{
			stiffness += Time.deltaTime / timeToHook;
			stiffness = Mathf.Clamp01(stiffness);
		}
		else if (isReleasing)
		{
			stiffness = stifnessReleaseCurve.Evaluate(num);
			if (num <= 0.01f)
			{
				isReleasing = false;
				meshRenderer.enabled = false;
			}
		}
	}

	private void LateUpdate()
	{
		if (origin != null)
		{
			base.transform.position = origin.position;
		}
		if (attachPoint != null)
		{
			base.transform.LookAt(attachPoint.position, base.transform.up);
			dist = Vector3.Distance(attachPoint.position, base.transform.position);
			base.transform.localScale = new Vector3(radius, radius, dist);
		}
		UpdateStiffness();
		if (block != null && meshRenderer != null && (prevStiffness != stiffness || prevDist != dist || prevGravity != gravity))
		{
			float num = Mathf.Clamp01(1f - stiffness);
			block.Clear();
			meshRenderer.GetPropertyBlock(block);
			block.SetFloat(ShaderPropertyID._Fallof, num);
			block.SetFloat(RopeGravPropertyID, gravity * num * dist);
			block.SetVector(UVmulPropertyID, new Vector4(1f, base.transform.localScale.z, 0f, 0f));
			meshRenderer.SetPropertyBlock(block);
			prevStiffness = stiffness;
			prevDist = dist;
		}
	}
}
