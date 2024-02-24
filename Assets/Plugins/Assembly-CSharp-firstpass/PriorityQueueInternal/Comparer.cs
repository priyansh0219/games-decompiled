using System.Collections.Generic;

namespace PriorityQueueInternal
{
	public sealed class Comparer : IComparer<Item>
	{
		public static readonly Comparer comparer = new Comparer();

		public int Compare(Item a, Item b)
		{
			return b.priority - a.priority;
		}
	}
}
