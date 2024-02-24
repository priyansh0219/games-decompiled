using UnityEngine;

[ExecuteInEditMode]
public class SimpleFogBoxVolume : VolumetricObjectBase
{
	public Vector3 boxSize = Vector3.one * 5f;

	private Vector3 previousBoxSize = Vector3.one;

	protected override void OnEnable()
	{
		if (volumeShader == "")
		{
			PopulateShaderName();
		}
		base.OnEnable();
	}

	public override void PopulateShaderName()
	{
		volumeShader = "Advanced SS/Volumetric/SimpleBoxFogVolume";
	}

	public override bool HasChanged()
	{
		if (boxSize != previousBoxSize || base.HasChanged())
		{
			return true;
		}
		return false;
	}

	protected override void SetChangedValues()
	{
		previousBoxSize = boxSize;
		base.SetChangedValues();
	}

	public override void UpdateVolume()
	{
		Vector3 vector = boxSize * 0.5f;
		if ((bool)meshInstance)
		{
			ScaleMesh(meshInstance, boxSize);
			Bounds bounds = default(Bounds);
			bounds.SetMinMax(-vector, vector);
			meshInstance.bounds = bounds;
		}
		if ((bool)materialInstance)
		{
			materialInstance.SetVector(ShaderPropertyID._BoxMin, new Vector4(0f - vector.x, 0f - vector.y, 0f - vector.z, 0f));
			materialInstance.SetVector(ShaderPropertyID._BoxMax, new Vector4(vector.x, vector.y, vector.z, 0f));
			materialInstance.SetFloat(ShaderPropertyID._Visibility, visibility);
			materialInstance.SetColor(ShaderPropertyID._Color, volumeColor);
		}
	}
}
