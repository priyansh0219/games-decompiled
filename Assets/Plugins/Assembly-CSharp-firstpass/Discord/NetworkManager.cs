using System;
using System.Runtime.InteropServices;

namespace Discord
{
	public class NetworkManager
	{
		internal struct FFIEvents
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void MessageHandler(IntPtr ptr, ulong peerId, byte channelId, IntPtr dataPtr, int dataLen);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void RouteUpdateHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string routeData);

			internal MessageHandler OnMessage;

			internal RouteUpdateHandler OnRouteUpdate;
		}

		internal struct FFIMethods
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void GetPeerIdMethod(IntPtr methodsPtr, ref ulong peerId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result FlushMethod(IntPtr methodsPtr);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result OpenPeerMethod(IntPtr methodsPtr, ulong peerId, [MarshalAs(UnmanagedType.LPStr)] string routeData);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result UpdatePeerMethod(IntPtr methodsPtr, ulong peerId, [MarshalAs(UnmanagedType.LPStr)] string routeData);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result ClosePeerMethod(IntPtr methodsPtr, ulong peerId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result OpenChannelMethod(IntPtr methodsPtr, ulong peerId, byte channelId, bool reliable);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result CloseChannelMethod(IntPtr methodsPtr, ulong peerId, byte channelId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result SendMessageMethod(IntPtr methodsPtr, ulong peerId, byte channelId, byte[] data, int dataLen);

			internal GetPeerIdMethod GetPeerId;

			internal FlushMethod Flush;

			internal OpenPeerMethod OpenPeer;

			internal UpdatePeerMethod UpdatePeer;

			internal ClosePeerMethod ClosePeer;

			internal OpenChannelMethod OpenChannel;

			internal CloseChannelMethod CloseChannel;

			internal SendMessageMethod SendMessage;
		}

		public delegate void MessageHandler(ulong peerId, byte channelId, byte[] data);

		public delegate void RouteUpdateHandler(string routeData);

		private IntPtr MethodsPtr;

		private FFIMethods Methods => (FFIMethods)Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));

		public event MessageHandler OnMessage;

		public event RouteUpdateHandler OnRouteUpdate;

		internal NetworkManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
		{
			if (eventsPtr == IntPtr.Zero)
			{
				throw new ResultException(Result.InternalError);
			}
			InitEvents(eventsPtr, ref events);
			MethodsPtr = ptr;
			if (MethodsPtr == IntPtr.Zero)
			{
				throw new ResultException(Result.InternalError);
			}
		}

		private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
		{
			events.OnMessage = delegate(IntPtr ptr, ulong peerId, byte channelId, IntPtr dataPtr, int dataLen)
			{
				if (this.OnMessage != null)
				{
					byte[] array = new byte[dataLen];
					Marshal.Copy(dataPtr, array, 0, dataLen);
					this.OnMessage(peerId, channelId, array);
				}
			};
			events.OnRouteUpdate = delegate(IntPtr ptr, string routeData)
			{
				if (this.OnRouteUpdate != null)
				{
					this.OnRouteUpdate(routeData);
				}
			};
			Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
		}

		public ulong GetPeerId()
		{
			ulong peerId = 0uL;
			Methods.GetPeerId(MethodsPtr, ref peerId);
			return peerId;
		}

		public void Flush()
		{
			Result result = Methods.Flush(MethodsPtr);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void OpenPeer(ulong peerId, string routeData)
		{
			Result result = Methods.OpenPeer(MethodsPtr, peerId, routeData);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void UpdatePeer(ulong peerId, string routeData)
		{
			Result result = Methods.UpdatePeer(MethodsPtr, peerId, routeData);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void ClosePeer(ulong peerId)
		{
			Result result = Methods.ClosePeer(MethodsPtr, peerId);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void OpenChannel(ulong peerId, byte channelId, bool reliable)
		{
			Result result = Methods.OpenChannel(MethodsPtr, peerId, channelId, reliable);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void CloseChannel(ulong peerId, byte channelId)
		{
			Result result = Methods.CloseChannel(MethodsPtr, peerId, channelId);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void SendMessage(ulong peerId, byte channelId, byte[] data)
		{
			Result result = Methods.SendMessage(MethodsPtr, peerId, channelId, data, data.Length);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}
	}
}
