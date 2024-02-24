using System.IO;
using Gendarme;
using Platform.IO;

public static class FileUtils
{
	public static bool FileExists(string path, bool skipManifest = false)
	{
		return Platform.IO.File.Exists(path);
	}

	[SuppressMessage("Subnautica.Rules", "EnsureLocalDisposalRule")]
	public static Stream CreateFile(string path)
	{
		return Slow(Platform.IO.File.Create(path));
	}

	[SuppressMessage("Subnautica.Rules", "EnsureLocalDisposalRule")]
	public static Stream ReadFile(string path)
	{
		return Slow(Platform.IO.File.OpenRead(path));
	}

	public static StreamWriter CreateTextFile(string path)
	{
		return new StreamWriter(CreateFile(path));
	}

	public static StreamReader ReadTextFile(string path)
	{
		return new StreamReader(ReadFile(path));
	}

	public static string[] GetFiles(string path)
	{
		return Platform.IO.Directory.GetFiles(path);
	}

	private static Stream Slow(Stream stream)
	{
		return stream;
	}
}
