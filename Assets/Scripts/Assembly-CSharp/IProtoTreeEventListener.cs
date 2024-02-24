public interface IProtoTreeEventListener
{
	void OnProtoSerializeObjectTree(ProtobufSerializer serializer);

	void OnProtoDeserializeObjectTree(ProtobufSerializer serializer);
}
