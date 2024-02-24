using System;
using System.Runtime.InteropServices;

namespace Oculus.Platform
{
	public sealed class Packet : IDisposable
	{
		private readonly ulong size;

		private readonly IntPtr packetHandle;

		public ulong SenderID => CAPI.ovr_Packet_GetSenderID(packetHandle);

		public ulong Size => size;

		public SendPolicy Policy => (SendPolicy)CAPI.ovr_Packet_GetSendPolicy(packetHandle);

		public Packet(IntPtr packetHandle)
		{
			this.packetHandle = packetHandle;
			size = CAPI.ovr_Packet_GetSize(packetHandle);
		}

		public ulong ReadBytes(byte[] destination)
		{
			if ((ulong)destination.LongLength < size)
			{
				throw new ArgumentException($"Destination array was not big enough to hold {size} bytes");
			}
			Marshal.Copy(CAPI.ovr_Packet_GetBytes(packetHandle), destination, 0, (int)size);
			return size;
		}

		~Packet()
		{
			ReleaseHandle();
		}

		public void Dispose()
		{
			ReleaseHandle();
			GC.SuppressFinalize(this);
		}

		private void ReleaseHandle()
		{
			if (packetHandle != IntPtr.Zero)
			{
				CAPI.ovr_Packet_Free(packetHandle);
			}
		}
	}
}
