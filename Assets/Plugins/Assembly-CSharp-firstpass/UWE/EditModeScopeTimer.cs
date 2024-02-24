using System;
using System.Runtime.InteropServices;

namespace UWE
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct EditModeScopeTimer : IDisposable
	{
		public EditModeScopeTimer(string label)
		{
		}

		public void Dispose()
		{
		}
	}
}
