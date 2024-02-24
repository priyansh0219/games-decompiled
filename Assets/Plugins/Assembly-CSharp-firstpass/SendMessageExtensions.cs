using UnityEngine;

public static class SendMessageExtensions
{
	public static void SendMessageWithProfiling(this GameObject go, string profileName, string message, SendMessageOptions options = SendMessageOptions.RequireReceiver)
	{
		go.SendMessage(message, options);
	}

	public static void SendMessageWithProfiling(this GameObject go, string profileName, string message, object value, SendMessageOptions options)
	{
		go.SendMessage(message, value, options);
	}

	public static void SendMessageUpwardsWithProfiling(this GameObject go, string profileName, string message, SendMessageOptions options = SendMessageOptions.RequireReceiver)
	{
		go.SendMessageUpwards(message, options);
	}

	public static void SendMessageUpwardsWithProfiling(this GameObject go, string profileName, string message, object value, SendMessageOptions options)
	{
		go.SendMessageUpwards(message, value, options);
	}

	public static void BroadcastMessageWithProfiling(this GameObject go, string profileName, string message, SendMessageOptions options = SendMessageOptions.RequireReceiver)
	{
		go.BroadcastMessage(message, options);
	}

	public static void BroadcastMessageWithProfiling(this GameObject go, string profileName, string message, object value, SendMessageOptions options)
	{
		go.BroadcastMessage(message, value, options);
	}

	public static void SendMessageWithProfiling(this Component comp, string profileName, string message, SendMessageOptions options = SendMessageOptions.RequireReceiver)
	{
		comp.SendMessage(message, options);
	}

	public static void SendMessageWithProfiling(this Component comp, string profileName, string message, object value, SendMessageOptions options)
	{
		comp.SendMessage(message, value, options);
	}

	public static void SendMessageUpwardsWithProfiling(this Component comp, string profileName, string message, SendMessageOptions options = SendMessageOptions.RequireReceiver)
	{
		comp.SendMessageUpwards(message, options);
	}

	public static void SendMessageUpwardsWithProfiling(this Component comp, string profileName, string message, object value, SendMessageOptions options)
	{
		comp.SendMessageUpwards(message, value, options);
	}

	public static void BroadcastMessageWithProfiling(this Component comp, string profileName, string message, SendMessageOptions options = SendMessageOptions.RequireReceiver)
	{
		comp.BroadcastMessage(message, options);
	}

	public static void BroadcastMessageWithProfiling(this Component comp, string profileName, string message, object value, SendMessageOptions options)
	{
		comp.BroadcastMessage(message, value, options);
	}
}
