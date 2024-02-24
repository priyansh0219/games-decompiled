using System;
using System.Runtime.InteropServices;

namespace Discord
{
	internal class Utility
	{
		internal static IntPtr Retain<T>(T value)
		{
			return GCHandle.ToIntPtr(GCHandle.Alloc(value, GCHandleType.Normal));
		}

		internal static void Release(IntPtr ptr)
		{
			GCHandle.FromIntPtr(ptr).Free();
		}
	}
}
