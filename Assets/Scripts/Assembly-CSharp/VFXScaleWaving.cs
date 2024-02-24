using UnityEngine;

public class VFXScaleWaving : MonoBehaviour
{
	private MaterialPropertyBlock block;

	private Renderer[] renderers;

	private void Start()
	{
		block = new MaterialPropertyBlock();
		renderers = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
	}

	public void UpdateWavingScale(Vector3 scale)
	{
		if (block != null && scale != Vector3.one && renderers != null)
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
}
