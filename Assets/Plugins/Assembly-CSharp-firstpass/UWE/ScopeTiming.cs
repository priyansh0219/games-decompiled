using System;
using System.Runtime.InteropServices;

namespace UWE
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ScopeTiming : IDisposable
	{
		public ScopeTiming(string label)
		{
			Timer.Begin(label);
		}

		public void Dispose()
		{
			Timer.End();
		}
	}
}
