using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;

public static class CompressionUtils
{
	private static readonly byte[] compressionHeader = new byte[4] { 67, 77, 80, 0 };

	public static byte[] Compress(byte[] input, byte[] compressBuffer)
	{
		Deflater deflater = new Deflater();
		deflater.SetInput(input);
		deflater.Finish();
		using (MemoryStream memoryStream = new MemoryStream())
		{
			memoryStream.Write(compressionHeader, 0, compressionHeader.Length);
			while (!deflater.IsFinished)
			{
				int count = deflater.Deflate(compressBuffer);
				memoryStream.Write(compressBuffer, 0, count);
			}
			return memoryStream.ToArray();
		}
	}

	private static bool GetIsCompressed(byte[] input)
	{
		if (input.Length < compressionHeader.Length)
		{
			return false;
		}
		for (int i = 0; i < compressionHeader.Length; i++)
		{
			if (input[i] != compressionHeader[i])
			{
				return false;
			}
		}
		return true;
	}

	public static byte[] Decompress(byte[] input, byte[] compressBuffer)
	{
		if (!GetIsCompressed(input))
		{
			return input;
		}
		Inflater inflater = new Inflater();
		inflater.SetInput(input, compressionHeader.Length, input.Length - compressionHeader.Length);
		using (MemoryStream memoryStream = new MemoryStream())
		{
			while (!inflater.IsFinished)
			{
				int count = inflater.Inflate(compressBuffer);
				memoryStream.Write(compressBuffer, 0, count);
			}
			return memoryStream.ToArray();
		}
	}
}
