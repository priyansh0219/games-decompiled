using System;
using System.Runtime.InteropServices;

namespace Discord
{
	public class ActivityManager
	{
		internal struct FFIEvents
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ActivityJoinHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string secret);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ActivitySpectateHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string secret);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ActivityJoinRequestHandler(IntPtr ptr, ref User user);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ActivityInviteHandler(IntPtr ptr, ActivityActionType type, ref User user, ref Activity activity);

			internal ActivityJoinHandler OnActivityJoin;

			internal ActivitySpectateHandler OnActivitySpectate;

			internal ActivityJoinRequestHandler OnActivityJoinRequest;

			internal ActivityInviteHandler OnActivityInvite;
		}

		internal struct FFIMethods
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result RegisterCommandMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string command);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result RegisterSteamMethod(IntPtr methodsPtr, uint steamId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void UpdateActivityCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void UpdateActivityMethod(IntPtr methodsPtr, ref Activity activity, IntPtr callbackData, UpdateActivityCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ClearActivityCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ClearActivityMethod(IntPtr methodsPtr, IntPtr callbackData, ClearActivityCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void SendRequestReplyCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void SendRequestReplyMethod(IntPtr methodsPtr, long userId, ActivityJoinRequestReply reply, IntPtr callbackData, SendRequestReplyCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void SendInviteCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void SendInviteMethod(IntPtr methodsPtr, long userId, ActivityActionType type, [MarshalAs(UnmanagedType.LPStr)] string content, IntPtr callbackData, SendInviteCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void AcceptInviteCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void AcceptInviteMethod(IntPtr methodsPtr, long userId, IntPtr callbackData, AcceptInviteCallback callback);

			internal RegisterCommandMethod RegisterCommand;

			internal RegisterSteamMethod RegisterSteam;

			internal UpdateActivityMethod UpdateActivity;

			internal ClearActivityMethod ClearActivity;

			internal SendRequestReplyMethod SendRequestReply;

			internal SendInviteMethod SendInvite;

			internal AcceptInviteMethod AcceptInvite;
		}

		public delegate void UpdateActivityHandler(Result result);

		public delegate void ClearActivityHandler(Result result);

		public delegate void SendRequestReplyHandler(Result result);

		public delegate void SendInviteHandler(Result result);

		public delegate void AcceptInviteHandler(Result result);

		public delegate void ActivityJoinHandler(string secret);

		public delegate void ActivitySpectateHandler(string secret);

		public delegate void ActivityJoinRequestHandler(ref User user);

		public delegate void ActivityInviteHandler(ActivityActionType type, ref User user, ref Activity activity);

		private IntPtr MethodsPtr;

		private FFIMethods Methods => (FFIMethods)Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));

		public event ActivityJoinHandler OnActivityJoin;

		public event ActivitySpectateHandler OnActivitySpectate;

		public event ActivityJoinRequestHandler OnActivityJoinRequest;

		public event ActivityInviteHandler OnActivityInvite;

		public void RegisterCommand()
		{
			RegisterCommand(null);
		}

		internal ActivityManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
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
			events.OnActivityJoin = delegate(IntPtr ptr, string secret)
			{
				if (this.OnActivityJoin != null)
				{
					this.OnActivityJoin(secret);
				}
			};
			events.OnActivitySpectate = delegate(IntPtr ptr, string secret)
			{
				if (this.OnActivitySpectate != null)
				{
					this.OnActivitySpectate(secret);
				}
			};
			events.OnActivityJoinRequest = delegate(IntPtr ptr, ref User user)
			{
				if (this.OnActivityJoinRequest != null)
				{
					this.OnActivityJoinRequest(ref user);
				}
			};
			events.OnActivityInvite = delegate(IntPtr ptr, ActivityActionType type, ref User user, ref Activity activity)
			{
				if (this.OnActivityInvite != null)
				{
					this.OnActivityInvite(type, ref user, ref activity);
				}
			};
			Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
		}

		public void RegisterCommand(string command)
		{
			Result result = Methods.RegisterCommand(MethodsPtr, command);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void RegisterSteam(uint steamId)
		{
			Result result = Methods.RegisterSteam(MethodsPtr, steamId);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void UpdateActivity(Activity activity, UpdateActivityHandler callback)
		{
			FFIMethods.UpdateActivityCallback updateActivityCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.UpdateActivity(MethodsPtr, ref activity, Utility.Retain(updateActivityCallback), updateActivityCallback);
		}

		public void ClearActivity(ClearActivityHandler callback)
		{
			FFIMethods.ClearActivityCallback clearActivityCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.ClearActivity(MethodsPtr, Utility.Retain(clearActivityCallback), clearActivityCallback);
		}

		public void SendRequestReply(long userId, ActivityJoinRequestReply reply, SendRequestReplyHandler callback)
		{
			FFIMethods.SendRequestReplyCallback sendRequestReplyCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.SendRequestReply(MethodsPtr, userId, reply, Utility.Retain(sendRequestReplyCallback), sendRequestReplyCallback);
		}

		public void SendInvite(long userId, ActivityActionType type, string content, SendInviteHandler callback)
		{
			FFIMethods.SendInviteCallback sendInviteCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.SendInvite(MethodsPtr, userId, type, content, Utility.Retain(sendInviteCallback), sendInviteCallback);
		}

		public void AcceptInvite(long userId, AcceptInviteHandler callback)
		{
			FFIMethods.AcceptInviteCallback acceptInviteCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.AcceptInvite(MethodsPtr, userId, Utility.Retain(acceptInviteCallback), acceptInviteCallback);
		}
	}
}
