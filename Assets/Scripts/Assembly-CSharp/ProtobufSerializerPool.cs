using UWE;

public static class ProtobufSerializerPool
{
	private static readonly ObjectPool<ProtobufSerializer> serializerPool = ObjectPoolHelper.CreatePool<ProtobufSerializer>();

	public static PooledObject<ProtobufSerializer> GetProxy()
	{
		return serializerPool.GetProxy();
	}
}
