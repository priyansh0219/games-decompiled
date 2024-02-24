using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct StopwatchProfilerBlock : IDisposable
{
	public StopwatchProfilerBlock(string timerTag)
	{
	}

	public void Dispose()
	{
	}
}
