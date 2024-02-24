namespace PriorityQueueInternal
{
	public struct Item
	{
		public readonly object item;

		public readonly int priority;

		public Item(object item, int priority)
		{
			this.item = item;
			this.priority = priority;
		}
	}
}
