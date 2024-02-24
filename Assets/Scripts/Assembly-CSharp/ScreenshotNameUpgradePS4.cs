using System;
using Platform.IO;

public static class ScreenshotNameUpgradePS4
{
	private const int upgradeChangeset = 70637;

	public static void Upgrade(int changeSet)
	{
	}

	private static string GetScreenshotName(DirectoryInfo dirInfo)
	{
		string prefix = DateTime.Now.ToString("yyyy-MM-dd_");
		return MathExtensions.GetUniqueFileName(dirInfo, prefix, "jpg", 5, startFromOne: true, dense: true);
	}
}
