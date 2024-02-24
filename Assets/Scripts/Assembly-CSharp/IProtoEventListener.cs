public interface IProtoEventListener
{
	void OnProtoSerialize(ProtobufSerializer serializer);

	void OnProtoDeserialize(ProtobufSerializer serializer);
}
