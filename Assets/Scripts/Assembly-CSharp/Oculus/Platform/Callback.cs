using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Platform
{
	public static class Callback
	{
		private class RequestCallback
		{
			private Message.Callback messageCallback;

			public RequestCallback()
			{
			}

			public RequestCallback(Message.Callback callback)
			{
				messageCallback = callback;
			}

			public virtual void HandleMessage(Message msg)
			{
				if (messageCallback != null)
				{
					messageCallback(msg);
				}
			}
		}

		private sealed class RequestCallback<T> : RequestCallback
		{
			private Message<T>.Callback callback;

			public RequestCallback(Message<T>.Callback callback)
			{
				this.callback = callback;
			}

			public override void HandleMessage(Message msg)
			{
				if (callback != null)
				{
					if (msg is Message<T>)
					{
						callback((Message<T>)msg);
					}
					else
					{
						Debug.LogError("Unable to handle message: " + msg.GetType());
					}
				}
			}
		}

		private static Dictionary<ulong, RequestCallback> requestIDsToCallbacks = new Dictionary<ulong, RequestCallback>();

		private static Dictionary<Message.MessageType, RequestCallback> notificationCallbacks = new Dictionary<Message.MessageType, RequestCallback>();

		internal static void SetNotificationCallback<T>(Message.MessageType type, Message<T>.Callback callback)
		{
			notificationCallbacks[type] = ((callback != null) ? new RequestCallback<T>(callback) : null);
		}

		internal static void SetNotificationCallback(Message.MessageType type, Message.Callback callback)
		{
			notificationCallbacks[type] = ((callback != null) ? new RequestCallback(callback) : null);
		}

		internal static void OnComplete<T>(Request<T> request, Message<T>.Callback callback)
		{
			requestIDsToCallbacks[request.RequestID] = new RequestCallback<T>(callback);
		}

		internal static void OnComplete(Request request, Message.Callback callback)
		{
			requestIDsToCallbacks[request.RequestID] = new RequestCallback(callback);
		}

		internal static void RunCallbacks()
		{
			while (true)
			{
				Message message = Message.PopMessage();
				if (message != null)
				{
					HandleMessage(message);
					continue;
				}
				break;
			}
		}

		internal static void RunLimitedCallbacks(uint limit)
		{
			for (int i = 0; i < limit; i++)
			{
				Message message = Message.PopMessage();
				if (message != null)
				{
					HandleMessage(message);
					continue;
				}
				break;
			}
		}

		private static void HandleMessage(Message msg)
		{
			if (requestIDsToCallbacks.TryGetValue(msg.RequestID, out var value))
			{
				try
				{
					value.HandleMessage(msg);
					return;
				}
				finally
				{
					requestIDsToCallbacks.Remove(msg.RequestID);
				}
			}
			if (notificationCallbacks.TryGetValue(msg.Type, out value))
			{
				value.HandleMessage(msg);
			}
		}
	}
}
