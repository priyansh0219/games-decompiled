using UWE;

public class StateMachinePool<T, H> where T : IStateMachine<H>, new()
{
	private readonly ObjectPool<PooledStateMachine<T>> pool = ObjectPoolHelper.CreatePool<PooledStateMachine<T>>("StateMachinePool::PooledStateMachine", 32);

	public PooledStateMachine<T> Get(H host)
	{
		PooledStateMachine<T> pooledStateMachine = pool.Get();
		pooledStateMachine.Initialize(pool);
		pooledStateMachine.stateMachine.Initialize(host);
		return pooledStateMachine;
	}
}
