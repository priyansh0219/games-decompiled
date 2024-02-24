using Unity.Collections;

namespace UWE
{
	public interface ISplitNativeArrayPool<T> : ISplitArrayPoolBase where T : struct
	{
		INativeArrayPool<T> poolSmall { get; }

		INativeArrayPool<T> poolBig { get; }

		NativeArray<T> Get(int minLength);

		void Return(NativeArray<T> arr);
	}
}
