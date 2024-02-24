using System;

public static class CommandLine
{
	public static string GetArgument(string argumentName)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		int num = Array.IndexOf(commandLineArgs, argumentName);
		if (num >= 0 && num + 1 < commandLineArgs.Length)
		{
			return commandLineArgs[num + 1];
		}
		return null;
	}

	public static bool GetFlag(string argumentName)
	{
		if (Array.IndexOf(Environment.GetCommandLineArgs(), argumentName) >= 0)
		{
			return true;
		}
		return false;
	}
}
