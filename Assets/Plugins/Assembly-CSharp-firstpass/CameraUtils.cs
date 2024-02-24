using System.Collections.Generic;
using UnityEngine;

public class CameraUtils : MonoBehaviour
{
	private class CameraData
	{
		private int frameUpdated = int.MinValue;

		private Plane[] planes = new Plane[6];

		public Camera camera { get; private set; }

		public CameraData(Camera camera)
		{
			this.camera = camera;
		}

		private void Update()
		{
			int frameCount = Time.frameCount;
			if (frameUpdated != frameCount)
			{
				frameUpdated = frameCount;
				GeometryUtility.CalculateFrustumPlanes(camera, planes);
			}
		}

		public Plane[] GetSharedFrustumPlanes()
		{
			Update();
			return planes;
		}
	}

	private static CameraUtils _main;

	private static List<int> toRemove = new List<int>();

	private readonly Dictionary<int, CameraData> cameraData = new Dictionary<int, CameraData>();

	private static CameraUtils main
	{
		get
		{
			if (_main == null)
			{
				_main = new GameObject("CameraUtils").AddComponent<CameraUtils>();
			}
			return _main;
		}
	}

	private void Update()
	{
		foreach (KeyValuePair<int, CameraData> cameraDatum in cameraData)
		{
			if (cameraDatum.Value.camera == null)
			{
				toRemove.Add(cameraDatum.Key);
			}
		}
		for (int i = 0; i < toRemove.Count; i++)
		{
			cameraData.Remove(toRemove[i]);
		}
		toRemove.Clear();
	}

	public static Plane[] GetSharedFrustumPlanes(Camera camera)
	{
		if (camera == null)
		{
			return null;
		}
		return EnsureData(camera).GetSharedFrustumPlanes();
	}

	private static CameraData EnsureData(Camera camera)
	{
		int instanceID = camera.GetInstanceID();
		if (!main.cameraData.TryGetValue(instanceID, out var value))
		{
			value = new CameraData(camera);
			main.cameraData.Add(instanceID, value);
		}
		return value;
	}
}
