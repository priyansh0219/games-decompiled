using UWE;
using UnityEngine;

public class ModelPlug : MonoBehaviour
{
	public Transform plugOrigin;

	public static void PlugIntoSocket(ModelPlug modelPlug, Transform socket)
	{
		PlugIntoSocket(modelPlug.transform, modelPlug.plugOrigin, socket);
	}

	public static void PlugIntoSocket(Transform model, Transform plug, Transform socket)
	{
		Vector3 vector = Vector3.zero;
		Quaternion q = Quaternion.identity;
		if (plug != null)
		{
			vector = model.InverseTransformPoint(plug.position);
			q = plug.rotation * Quaternion.Inverse(model.rotation);
		}
		GameObject obj = new GameObject("temp adaptor");
		Transform transform = obj.transform;
		transform.parent = socket;
		model.parent = transform;
		Utils.ZeroTransform(transform);
		Utils.ZeroTransform(model);
		model.localPosition = -vector;
		transform.localRotation = q.GetInverse();
		model.parent = transform.parent;
		Object.Destroy(obj);
	}
}
