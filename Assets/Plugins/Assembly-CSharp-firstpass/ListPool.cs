using System.Collections.Generic;

public sealed class ListPool<T> : Pool<ListPool<T>>
{
	public readonly List<T> list = new List<T>();

	protected override void Deinitialize()
	{
		list.Clear();
	}
}
