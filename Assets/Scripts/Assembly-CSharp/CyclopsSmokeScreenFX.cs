using UnityEngine;

[ExecuteInEditMode]
public class CyclopsSmokeScreenFX : MonoBehaviour
{
	[AssertNotNull]
	public Shader shader;

	[Range(0f, 1f)]
	public float intensity;

	[Range(0f, 1f)]
	public float diffusion = 0.3f;

	[AssertNotNull]
	public Texture3D noiseVolume;

	public Color color;

	public Vector4 noiseScale;

	public Vector3 scrollSpeed;

	public Texture2D ditherTexture;

	public Transform parentTransform;

	private Material mat;

	private Vector3 camPos;

	private int imagePlaneSizeID;

	private int intensityID;

	private int cameraToWorldMatrixID;

	private int parentWorldToLocalMatrixID;

	private int cameraPosID;

	private void Start()
	{
		InitMaterial();
	}

	private void OnEnable()
	{
		InitMaterial();
	}

	private void InitMaterial()
	{
		mat = new Material(shader);
		mat.hideFlags = HideFlags.HideAndDontSave;
		mat.SetTexture(ShaderPropertyID._NoiseVolume, noiseVolume);
		mat.SetTexture(ShaderPropertyID._DitherTex, ditherTexture);
		mat.SetVector(ShaderPropertyID._NoiseScale, noiseScale);
		mat.SetVector(ShaderPropertyID._NoiseSpeed, scrollSpeed);
		mat.SetColor(ShaderPropertyID._Color, color);
		mat.SetFloat(ShaderPropertyID._Diffusion, diffusion);
		imagePlaneSizeID = Shader.PropertyToID("_ImagePlaneSize");
		intensityID = Shader.PropertyToID("_Intensity");
		cameraToWorldMatrixID = Shader.PropertyToID("_CameraToWorldMatrix");
		parentWorldToLocalMatrixID = Shader.PropertyToID("_ParentWorldToLocalMatrix");
		cameraPosID = Shader.PropertyToID("_CameraPos");
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!(mat == null) && !(parentTransform == null))
		{
			Matrix4x4 projectionMatrix = MainCamera.camera.projectionMatrix;
			Vector2 vector = default(Vector2);
			vector.x = 1f / projectionMatrix[0, 0];
			vector.y = 1f / projectionMatrix[1, 1];
			mat.SetVector(imagePlaneSizeID, vector);
			mat.SetFloat(intensityID, intensity);
			Matrix4x4 cameraToWorldMatrix = MainCamera.camera.cameraToWorldMatrix;
			mat.SetMatrix(cameraToWorldMatrixID, cameraToWorldMatrix);
			mat.SetMatrix(parentWorldToLocalMatrixID, parentTransform.worldToLocalMatrix);
			camPos = parentTransform.InverseTransformPoint(base.transform.position);
			mat.SetVector(cameraPosID, camPos);
			Graphics.Blit(source, destination, mat);
		}
	}
}
