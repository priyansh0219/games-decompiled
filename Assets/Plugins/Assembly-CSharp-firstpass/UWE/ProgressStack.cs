using System.Collections.Generic;

namespace UWE
{
	public class ProgressStack
	{
		public class Entry
		{
			public string title;

			public int total;

			public int stride;

			public int done;

			public Entry(string title, int total, int stride = 1)
			{
				this.title = title;
				this.total = total;
				this.stride = stride;
				done = 0;
			}
		}

		private Stack<Entry> stack = new Stack<Entry>();

		private bool cancel;

		public void Begin(Entry e)
		{
			if (stack.Count == 0)
			{
				cancel = false;
			}
			stack.Push(e);
			DisplayProgress();
		}

		private void DisplayProgress()
		{
			if (stack.Count != 0)
			{
				stack.Peek();
			}
		}

		public bool Tic(string msg = "")
		{
			if (cancel)
			{
				return true;
			}
			Entry entry = stack.Peek();
			entry.done++;
			if (entry.done % entry.stride == 0)
			{
				DisplayProgress();
			}
			return cancel;
		}

		public void End()
		{
			stack.Pop();
			DisplayProgress();
		}
	}
}
