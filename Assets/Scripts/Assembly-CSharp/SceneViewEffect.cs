using UnityEngine;

[ExecuteInEditMode]
public class SceneViewEffect : MonoBehaviour
{
	public Material material;

	public Camera camera;

	private void Start()
	{
		camera.depthTextureMode = DepthTextureMode.Depth;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		Vector2 vector = default(Vector2);
		vector.x = 1f / projectionMatrix[0, 0];
		vector.y = 1f / projectionMatrix[1, 1];
		material.SetVector(ShaderPropertyID._ImagePlaneSize, vector);
		material.SetMatrix(ShaderPropertyID._CameraToWorldMatrix, camera.cameraToWorldMatrix);
		Graphics.Blit(source, destination, material);
	}
}
