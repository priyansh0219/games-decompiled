using UWE;
using UnityEngine;

[ExecuteInEditMode]
public class VFXPassYboundsToMat : MonoBehaviour
{
	public float minYposScalar;

	public float maxYposScalar = 1f;

	public float minYpos;

	public float maxYpos;

	public bool scaleWaving;

	private Renderer[] renderers;

	private MaterialPropertyBlock block;

	private Bounds GetAABB(GameObject target)
	{
		FixedBounds component = target.GetComponent<FixedBounds>();
		if (component != null)
		{
			return component.bounds;
		}
		return UWE.Utils.GetEncapsulatedAABB(target);
	}

	private void GetMinAndMaxYpos()
	{
		minYpos = float.PositiveInfinity;
		maxYpos = 0f - minYpos;
		renderers = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i].bounds.max.y > maxYpos)
			{
				maxYpos = renderers[i].bounds.max.y;
			}
			if (renderers[i].bounds.min.y < minYpos)
			{
				minYpos = renderers[i].bounds.min.y;
			}
		}
		float value = minYpos + minYposScalar * (maxYpos - minYpos);
		float value2 = maxYpos + (maxYposScalar - 1f) * (maxYpos - minYpos);
		block = new MaterialPropertyBlock();
		for (int j = 0; j < renderers.Length; j++)
		{
			renderers[j].GetPropertyBlock(block);
			block.SetFloat(ShaderPropertyID._minYpos, value);
			block.SetFloat(ShaderPropertyID._maxYpos, value2);
			if (scaleWaving && base.transform.localScale != Vector3.one)
			{
				block.SetVector(ShaderPropertyID._ScaleModifier, base.transform.localScale - Vector3.one);
			}
			renderers[j].SetPropertyBlock(block);
		}
	}

	public void UpdateWavingScale(Vector3 scale)
	{
		if (block != null && scale != Vector3.one)
		{
			for (int i = 0; i < renderers.Length; i++)
			{
				block.Clear();
				renderers[i].GetPropertyBlock(block);
				block.SetVector(ShaderPropertyID._ScaleModifier, scale - Vector3.one);
				renderers[i].SetPropertyBlock(block);
			}
		}
	}

	private void Start()
	{
		Invoke("GetMinAndMaxYpos", 0.15f);
	}
}
