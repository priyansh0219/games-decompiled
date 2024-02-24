public interface IDeserializationListener
{
	void OnGameObject(ProtobufSerializer.GameObjectData data);

	void OnComponent(ProtobufSerializer.ComponentHeader data);
}
