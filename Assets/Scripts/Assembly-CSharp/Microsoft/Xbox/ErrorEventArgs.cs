using System;

namespace Microsoft.Xbox
{
	public class ErrorEventArgs : EventArgs
	{
		public string ErrorCode { get; private set; }

		public string ErrorMessage { get; private set; }

		public ErrorEventArgs(string errorCode, string errorMessage)
		{
			ErrorCode = errorCode;
			ErrorMessage = errorMessage;
		}
	}
}
