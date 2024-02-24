using UnityEngine;
using UnityEngine.Rendering;

public class XboxOneForceCameraResolution : MonoBehaviour
{
	public int resolutionX = -1;

	public int resolutionY = -1;

	public Shader copyShader;

	private Material material;

	private CommandBuffer commandBuffer;

	private void Start()
	{
		if (!(GetComponent<Camera>() != null))
		{
			Debug.LogWarning("XboxOneForceCameraResolution: No camera component present", base.gameObject);
		}
		material = new Material(copyShader);
		material.hideFlags = HideFlags.HideAndDontSave;
		commandBuffer = new CommandBuffer();
	}
}
