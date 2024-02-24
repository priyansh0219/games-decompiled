using Gendarme;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCamera : MonoBehaviour
{
	private static Camera _camera;

	[SuppressMessage("Subnautica.Rules", "AvoidCameraMain")]
	public static Camera camera
	{
		get
		{
			if ((bool)_camera)
			{
				return _camera;
			}
			return Camera.main;
		}
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	public void OnEnable()
	{
		_camera = GetComponent<Camera>();
	}

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	public void OnDisable()
	{
		_camera = null;
	}
}
