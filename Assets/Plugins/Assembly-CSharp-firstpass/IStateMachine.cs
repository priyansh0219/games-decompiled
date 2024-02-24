using System.Collections;

public interface IStateMachine : IEnumerator
{
	void Clear();
}
public interface IStateMachine<T> : IStateMachine, IEnumerator
{
	void Initialize(T host);
}
