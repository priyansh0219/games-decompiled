using System;

namespace Oculus.Platform
{
	public static class Net
	{
		public static Packet ReadPacket()
		{
			if (!Core.IsInitialized())
			{
				return null;
			}
			IntPtr intPtr = CAPI.ovr_Net_ReadPacket();
			if (intPtr == IntPtr.Zero)
			{
				return null;
			}
			return new Packet(intPtr);
		}

		public static bool SendPacket(ulong userID, byte[] bytes, SendPolicy policy)
		{
			if (Core.IsInitialized())
			{
				return CAPI.ovr_Net_SendPacket(userID, (uint)bytes.Length, bytes, policy);
			}
			return false;
		}

		public static void Connect(ulong userID)
		{
			if (Core.IsInitialized())
			{
				CAPI.ovr_Net_Connect(userID);
			}
		}

		public static void Accept(ulong userID)
		{
			if (Core.IsInitialized())
			{
				CAPI.ovr_Net_Accept(userID);
			}
		}

		public static void Close(ulong userID)
		{
			if (Core.IsInitialized())
			{
				CAPI.ovr_Net_Close(userID);
			}
		}

		public static void SetPeerConnectRequestCallback(Message<Oculus.Platform.Models.NetworkingPeer>.Callback callback)
		{
			Callback.SetNotificationCallback(Message.MessageType.Notification_Networking_PeerConnectRequest, callback);
		}

		public static void SetConnectionStateChangedCallback(Message<Oculus.Platform.Models.NetworkingPeer>.Callback callback)
		{
			Callback.SetNotificationCallback(Message.MessageType.Notification_Networking_ConnectionStateChange, callback);
		}
	}
}
