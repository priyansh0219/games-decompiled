namespace UWE
{
	public struct Result
	{
		public readonly bool success;

		public readonly string error;

		public Result(bool success, string error)
		{
			this.success = success;
			this.error = error;
		}

		public static Result Success()
		{
			return new Result(success: true, null);
		}

		public static Result Failure(string error)
		{
			return new Result(success: false, error);
		}
	}
}
