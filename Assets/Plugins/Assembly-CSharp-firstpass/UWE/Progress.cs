using System;
using System.Runtime.InteropServices;

namespace UWE
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Progress : IDisposable
	{
		public Progress(string title, int total, int stride = 1, bool disableGUI = false)
		{
		}

		public bool Tic(string msg = "")
		{
			return false;
		}

		public void Dispose()
		{
		}
	}
}
