using System;
using UnityEngine;

public class NullLogHandler : InterceptingLogHandler
{
	public NullLogHandler(ILogger logger)
		: base(logger)
	{
	}

	public override void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
	{
	}

	public override void LogException(Exception exception, UnityEngine.Object context)
	{
	}
}
