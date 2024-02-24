using System;
using UnityEngine;

public abstract class InterceptingLogHandler : ILogHandler, IDisposable
{
	private ILogger logger;

	private ILogHandler handler;

	protected ILogHandler originalLogHandler => handler;

	protected InterceptingLogHandler(ILogger logger)
	{
		this.logger = logger;
		if (logger != null)
		{
			handler = logger.logHandler;
			logger.logHandler = this;
		}
	}

	public void Dispose()
	{
		if (logger != null)
		{
			logger.logHandler = handler;
			logger = null;
			handler = null;
		}
	}

	public abstract void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args);

	public abstract void LogException(Exception exception, UnityEngine.Object context);
}
