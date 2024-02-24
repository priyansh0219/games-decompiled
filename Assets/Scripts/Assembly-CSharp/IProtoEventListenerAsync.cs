using System.Collections;

public interface IProtoEventListenerAsync
{
	IEnumerator OnProtoDeserializeAsync(ProtobufSerializer serializer);
}
