using System;
using UnityEngine;

public class DemotingLogHandler : InterceptingLogHandler
{
	public DemotingLogHandler(ILogger logger)
		: base(logger)
	{
	}

	public override void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
	{
		base.originalLogHandler.LogFormat(LogType.Log, context, format, args);
	}

	public override void LogException(Exception exception, UnityEngine.Object context)
	{
		base.originalLogHandler.LogFormat(LogType.Log, context, "Exception: {0}", exception);
	}
}
