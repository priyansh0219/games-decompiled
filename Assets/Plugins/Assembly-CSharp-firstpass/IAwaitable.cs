using System.Collections;

public interface IAwaitable
{
	IEnumerator GetAwaiter();
}
