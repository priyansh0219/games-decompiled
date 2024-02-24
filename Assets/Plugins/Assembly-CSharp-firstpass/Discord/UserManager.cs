using System;
using System.Runtime.InteropServices;

namespace Discord
{
	public class UserManager
	{
		internal struct FFIEvents
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void CurrentUserUpdateHandler(IntPtr ptr);

			internal CurrentUserUpdateHandler OnCurrentUserUpdate;
		}

		internal struct FFIMethods
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetCurrentUserMethod(IntPtr methodsPtr, ref User currentUser);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void GetUserCallback(IntPtr ptr, Result result, ref User user);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void GetUserMethod(IntPtr methodsPtr, long userId, IntPtr callbackData, GetUserCallback callback);

			internal GetCurrentUserMethod GetCurrentUser;

			internal GetUserMethod GetUser;
		}

		public delegate void GetUserHandler(Result result, ref User user);

		public delegate void CurrentUserUpdateHandler();

		private IntPtr MethodsPtr;

		private FFIMethods Methods => (FFIMethods)Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));

		public event CurrentUserUpdateHandler OnCurrentUserUpdate;

		internal UserManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
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
			events.OnCurrentUserUpdate = delegate
			{
				if (this.OnCurrentUserUpdate != null)
				{
					this.OnCurrentUserUpdate();
				}
			};
			Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
		}

		public User GetCurrentUser()
		{
			User currentUser = default(User);
			Result result = Methods.GetCurrentUser(MethodsPtr, ref currentUser);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return currentUser;
		}

		public void GetUser(long userId, GetUserHandler callback)
		{
			FFIMethods.GetUserCallback getUserCallback = delegate(IntPtr ptr, Result result, ref User user)
			{
				Utility.Release(ptr);
				callback(result, ref user);
			};
			Methods.GetUser(MethodsPtr, userId, Utility.Retain(getUserCallback), getUserCallback);
		}
	}
}
