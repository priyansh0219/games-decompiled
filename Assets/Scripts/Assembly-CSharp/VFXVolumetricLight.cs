using UnityEngine;

[RequireComponent(typeof(Light))]
[ExecuteInEditMode]
public class VFXVolumetricLight : MonoBehaviour
{
	public bool syncMeshWithLight = true;

	[Range(5f, 175f)]
	public int angle;

	public float range;

	[Range(0.01f, 2f)]
	public float intensity = 0.5f;

	[Range(0f, 1f)]
	public float startOffset;

	[Range(0f, 1f)]
	public float startFallof;

	[Range(0.1f, 20f)]
	public float nearClip = 1f;

	[Range(0.01f, 3f)]
	public float softEdges = 2f;

	public int segments = 24;

	public Light lightSource;

	public LightType lightType;

	public Color color;

	public float lightIntensity;

	public GameObject volumGO;

	public MeshRenderer volumRenderer;

	public Material coneMat;

	public Material sphereMat;

	public MeshFilter volumMeshFilter;

	public Mesh volumMesh;

	private MaterialPropertyBlock block;

	private bool disableVolumetricVFX;

	public void Init()
	{
		if (lightSource == null)
		{
			lightSource = GetComponent<Light>();
			if (lightSource == null)
			{
				Debug.LogError(string.Concat("VolumetricLight - ", base.gameObject, "doesn't have a light component, aborting"));
				return;
			}
		}
		if (volumGO != null)
		{
			volumMeshFilter = volumGO.GetComponent<MeshFilter>();
			volumRenderer = volumGO.GetComponent<MeshRenderer>();
		}
	}

	public void Awake()
	{
		Init();
		InitMaterialBlock();
		UpdateMaterial(forceUpdate: true);
	}

	public void LateUpdate()
	{
		if (lightSource != null && volumMeshFilter != null && volumRenderer != null)
		{
			if (disableVolumetricVFX)
			{
				volumRenderer.enabled = false;
			}
			else
			{
				volumRenderer.enabled = lightSource.enabled;
			}
			UpdateMaterial();
		}
	}

	public void InitMaterialBlock()
	{
		if (!(volumRenderer == null) && !(lightSource == null) && !(coneMat == null) && !(sphereMat == null))
		{
			if (block == null)
			{
				block = new MaterialPropertyBlock();
			}
			block.SetFloat(ShaderPropertyID._Intensity, intensity);
			block.SetFloat(ShaderPropertyID._Offset, startOffset);
			block.SetFloat(ShaderPropertyID._Fallof, startFallof);
			block.SetFloat(ShaderPropertyID._InvFade, softEdges);
			block.SetFloat(ShaderPropertyID._ClipFade, nearClip);
			volumRenderer.SetPropertyBlock(block);
		}
	}

	public void DisableVolume()
	{
		disableVolumetricVFX = true;
	}

	public void RestoreVolume()
	{
		disableVolumetricVFX = false;
	}

	public void UpdateMaterial(bool forceUpdate)
	{
		if (!(volumRenderer == null) && !(lightSource == null) && !(coneMat == null) && !(sphereMat == null) && (lightSource.color != color || lightSource.intensity != lightIntensity || forceUpdate) && block != null)
		{
			color = lightSource.color;
			lightIntensity = lightSource.intensity;
			Color value = color;
			value.a *= lightIntensity / 8f;
			block.SetColor(ShaderPropertyID._Color, value);
			volumRenderer.SetPropertyBlock(block);
		}
	}

	public void UpdateMaterial()
	{
		UpdateMaterial(forceUpdate: false);
	}

	public void UpdateScale()
	{
		if (syncMeshWithLight)
		{
			range = lightSource.range;
		}
		if (lightType == LightType.Point)
		{
			volumGO.transform.localScale = new Vector3(range, range, range) * 0.75f;
		}
		else if (lightType == LightType.Spot)
		{
			volumGO.transform.localScale = new Vector3(range, range, range);
		}
	}

	private void OnEnable()
	{
		Init();
		InitMaterialBlock();
		UpdateMaterial(forceUpdate: true);
	}
}
