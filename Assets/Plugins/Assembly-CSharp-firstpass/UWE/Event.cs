using System.Collections.Generic;
using UnityEngine;

namespace UWE
{
	public class Event<P>
	{
		public delegate void HandleFunction(P parms);

		public class Handler
		{
			public HandleFunction function;

			public Object obj;

			public bool Trigger(P parms)
			{
				if (!obj || function == null)
				{
					return false;
				}
				function(parms);
				return true;
			}
		}

		private HashSet<Handler> handlers;

		private List<Handler> toRemove = new List<Handler>();

		private List<Handler> handlersToTrigger = new List<Handler>();

		private bool triggering;

		public virtual void Trigger(P parms)
		{
			try
			{
				if (handlers == null || triggering)
				{
					return;
				}
				triggering = true;
				handlersToTrigger.AddRange(handlers);
				foreach (Handler item in handlersToTrigger)
				{
					if (!item.Trigger(parms))
					{
						toRemove.Add(item);
					}
				}
				foreach (Handler item2 in toRemove)
				{
					handlers.Remove(item2);
				}
				toRemove.Clear();
				triggering = false;
			}
			finally
			{
				handlersToTrigger.Clear();
			}
		}

		public void AddHandler(Object obj, HandleFunction func)
		{
			try
			{
				if (handlers == null)
				{
					handlers = new HashSet<Handler>();
				}
				Handler handler = new Handler();
				handler.obj = obj;
				handler.function = func;
				handlers.Add(handler);
			}
			finally
			{
			}
		}

		public bool RemoveHandler(Object obj, HandleFunction func = null)
		{
			bool result = false;
			try
			{
				if (handlers == null)
				{
					return result;
				}
				List<Handler> list = new List<Handler>();
				foreach (Handler handler in handlers)
				{
					if (handler.obj == obj && (func == null || handler.function == func))
					{
						list.Add(handler);
					}
				}
				foreach (Handler item in list)
				{
					if (handlers.Remove(item))
					{
						result = true;
					}
				}
				return result;
			}
			finally
			{
			}
		}

		public bool RemoveHandlers(GameObject obj)
		{
			return RemoveHandler(obj);
		}

		public void Clear()
		{
			if (handlers != null)
			{
				handlers.Clear();
			}
		}
	}
}
