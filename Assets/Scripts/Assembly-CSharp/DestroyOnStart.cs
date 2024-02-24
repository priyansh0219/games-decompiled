using UnityEngine;

public class DestroyOnStart : MonoBehaviour, IProtoEventListener
{
	private bool destroying;

	private void Awake()
	{
		DestroySafe();
	}

	private void DestroySafe()
	{
		if (!destroying)
		{
			destroying = true;
			if ((bool)GetComponent<Pickupable>())
			{
				Invoke("Drop", 0f);
			}
			Object.Destroy(base.gameObject, 1f);
		}
	}

	private void Drop()
	{
		Pickupable component = GetComponent<Pickupable>();
		if ((bool)component)
		{
			component.Drop();
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		DestroySafe();
	}
}
