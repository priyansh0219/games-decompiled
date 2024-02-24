using System.Collections;
using UWE;

public class PooledStateMachine<T> : IEnumerator where T : IStateMachine, new()
{
	public readonly T stateMachine = new T();

	private ObjectPool<PooledStateMachine<T>> pool;

	public object Current => stateMachine.Current;

	public void Initialize(ObjectPool<PooledStateMachine<T>> pool)
	{
		this.pool = pool;
	}

	public bool MoveNext()
	{
		bool num = stateMachine.MoveNext();
		if (!num)
		{
			stateMachine.Clear();
			pool.Return(this);
		}
		return num;
	}

	public void Reset()
	{
		stateMachine.Reset();
	}
}
