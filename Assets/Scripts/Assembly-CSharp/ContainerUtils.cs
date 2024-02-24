using System;

public static class ContainerUtils
{
	private static string PreserveOrReplace(string s, string oldValue, string newValue)
	{
		if (s.IndexOf(oldValue, StringComparison.Ordinal) != -1)
		{
			return s.Replace(oldValue, newValue);
		}
		return s;
	}

	public static string EncodeFileName(string fileName)
	{
		fileName = PreserveOrReplace(fileName, "_", "_U");
		fileName = PreserveOrReplace(fileName, "\\", "_S");
		fileName = PreserveOrReplace(fileName, "/", "_S");
		return fileName;
	}

	public static string DecodeFileName(string fileName)
	{
		fileName = PreserveOrReplace(fileName, "_S", "/");
		fileName = PreserveOrReplace(fileName, "_U", "_");
		return fileName;
	}
}
