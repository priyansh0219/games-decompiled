using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class DisableBeforeExplosion : MonoBehaviour, IProtoEventListener
{
	private void Start()
	{
		if (CrashedShipExploder.main != null)
		{
			base.gameObject.SetActive(CrashedShipExploder.main.IsExploded());
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		base.gameObject.SetActive(CrashedShipExploder.main != null && CrashedShipExploder.main.IsExploded());
	}
}
