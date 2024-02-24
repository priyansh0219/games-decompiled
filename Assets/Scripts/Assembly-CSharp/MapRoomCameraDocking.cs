using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class MapRoomCameraDocking : MonoBehaviour, IProtoEventListener
{
	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public bool cameraDocked = true;

	public Transform dockingTransform;

	public Transform undockTransform;

	public GameObject cameraPrefab;

	private MapRoomCamera camera;

	private bool deserialized;

	private void Start()
	{
		if (!deserialized)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(cameraPrefab);
			CrafterLogic.NotifyCraftEnd(gameObject, TechType.MapRoomCamera);
			camera = gameObject.GetComponent<MapRoomCamera>();
			DockCamera(camera);
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		deserialized = true;
	}

	public void DockCamera(MapRoomCamera camera)
	{
		camera.transform.rotation = dockingTransform.rotation;
		camera.transform.position = dockingTransform.position;
		camera.SetDocked(this);
		this.camera = camera;
		cameraDocked = true;
	}

	public void UndockCamera()
	{
		camera.transform.position = undockTransform.position;
		camera.transform.rotation = undockTransform.rotation;
		camera.SetDocked(null);
		camera = null;
		cameraDocked = false;
	}

	public void OnTriggerEnter(Collider other)
	{
		if (camera == null)
		{
			Rigidbody component = other.GetComponent<Rigidbody>();
			MapRoomCamera component2 = other.gameObject.GetComponent<MapRoomCamera>();
			if (component != null && !component.isKinematic && component2 != null)
			{
				DockCamera(component2);
			}
		}
	}

	private void OnDestroy()
	{
		if (camera != null)
		{
			UnityEngine.Object.Destroy(camera.gameObject);
		}
	}
}
