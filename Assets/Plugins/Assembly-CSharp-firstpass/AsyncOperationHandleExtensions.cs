using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AsyncOperationHandleExtensions
{
	public static void ThrowExceptionIfFailed<TObject>(this AsyncOperationHandle<TObject> handle)
	{
		if (handle.Status == AsyncOperationStatus.Failed)
		{
			throw handle.OperationException;
		}
	}

	public static void LogExceptionIfFailed<TObject>(this AsyncOperationHandle<TObject> handle, string description)
	{
		if (handle.Status == AsyncOperationStatus.Failed)
		{
			Debug.LogException(new Exception("AsyncOperation '" + description + "' has failed with exception:", handle.OperationException));
		}
	}
}
