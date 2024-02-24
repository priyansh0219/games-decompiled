using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Oculus.Platform
{
	public class WindowsPlatform
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void UnityLogDelegate(IntPtr tag, IntPtr msg);

		private void CPPLogCallback(IntPtr tag, IntPtr message)
		{
			Debug.Log($"{Marshal.PtrToStringAnsi(tag)}: {Marshal.PtrToStringAnsi(message)}");
		}

		public bool Initialize(string appId)
		{
			CAPI.ovr_UnityInitWrapperWindows(appId, IntPtr.Zero);
			return true;
		}
	}
}
