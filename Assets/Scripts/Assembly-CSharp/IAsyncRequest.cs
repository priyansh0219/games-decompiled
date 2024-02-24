using System.Collections;

public interface IAsyncRequest : IEnumerator
{
	bool isDone { get; }

	float progress { get; }
}
