using Unity.Collections;

namespace UWE
{
	public interface INativeArrayPool<T> : IArrayPoolBase<T> where T : struct
	{
		NativeArray<T> Get(int minLength);

		void Return(NativeArray<T> arr);
	}
}
