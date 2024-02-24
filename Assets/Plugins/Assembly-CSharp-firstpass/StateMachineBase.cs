using System.Collections;

public abstract class StateMachineBase<T> : IStateMachine<T>, IStateMachine, IEnumerator
{
	protected T host;

	protected object current;

	protected int state;

	public object Current => current;

	public void Initialize(T host)
	{
		this.host = host;
		current = null;
		state = 0;
		Reset();
	}

	public void Clear()
	{
		host = default(T);
		current = null;
		state = -1;
		Reset();
	}

	public abstract bool MoveNext();

	public abstract void Reset();
}
