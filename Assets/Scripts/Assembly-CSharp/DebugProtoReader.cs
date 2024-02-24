using System.IO;
using ProtoBuf;

public static class DebugProtoReader
{
	private static TextWriter _stream;

	private static int indent;

	private static TextWriter stream
	{
		get
		{
			if (_stream == null)
			{
				_stream = FileUtils.CreateTextFile("protoreader.log");
			}
			return _stream;
		}
	}

	public static void Log(string message)
	{
		stream.WriteLine(message);
	}

	public static void LogFormat(string format, params object[] args)
	{
		stream.WriteLine(format, args);
	}

	public static SubItemToken StartSubItem(ProtoReader reader)
	{
		for (int i = 0; i < indent; i++)
		{
			stream.Write("  ");
		}
		indent++;
		return ProtoReader.StartSubItem(reader);
	}

	public static void EndSubItem(SubItemToken token, ProtoReader reader)
	{
		stream.Flush();
		indent--;
		ProtoReader.EndSubItem(token, reader);
	}

	public static void NoteObject(object value, ProtoReader reader)
	{
		ProtoReader.NoteObject(value, reader);
	}

	public static byte[] AppendBytes(byte[] value, ProtoReader reader)
	{
		return ProtoReader.AppendBytes(value, reader);
	}
}
