using System;
using System.IO;
using ProtoBuf;

public static class DebugProtoWriter
{
	private static TextWriter _stream;

	private static int indent;

	private static TextWriter stream
	{
		get
		{
			if (_stream == null)
			{
				_stream = FileUtils.CreateTextFile("protowriter.log");
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

	public static SubItemToken StartSubItem(object instance, ProtoWriter writer)
	{
		for (int i = 0; i < indent; i++)
		{
			stream.Write("  ");
		}
		stream.Write("(");
		indent++;
		return ProtoWriter.StartSubItem(instance, writer);
	}

	public static void EndSubItem(SubItemToken token, ProtoWriter writer)
	{
		stream.WriteLine("End)");
		stream.Flush();
		indent--;
		ProtoWriter.EndSubItem(token, writer);
	}

	public static void WriteBoolean(bool value, ProtoWriter writer)
	{
		stream.Write("bool {0}, ", value);
		ProtoWriter.WriteBoolean(value, writer);
	}

	public static void WriteByte(byte value, ProtoWriter writer)
	{
		stream.Write("byte {0}, ", value);
		ProtoWriter.WriteByte(value, writer);
	}

	public static void WriteBytes(byte[] data, int offset, int length, ProtoWriter writer)
	{
		stream.Write("bytes {0}, ", data.Length);
		ProtoWriter.WriteBytes(data, offset, length, writer);
	}

	public static void WriteBytes(byte[] data, ProtoWriter writer)
	{
		stream.Write("bytes {0}, ", data.Length);
		ProtoWriter.WriteBytes(data, writer);
	}

	public static void WriteDouble(double value, ProtoWriter writer)
	{
		stream.Write("double {0}, ", value);
		ProtoWriter.WriteDouble(value, writer);
	}

	public static void WriteFieldHeader(int fieldNumber, WireType wireType, ProtoWriter writer)
	{
		stream.Write("field {0}, ", fieldNumber);
		ProtoWriter.WriteFieldHeader(fieldNumber, wireType, writer);
	}

	public static void WriteInt16(short value, ProtoWriter writer)
	{
		stream.Write("int16 {0}, ", value);
		ProtoWriter.WriteInt16(value, writer);
	}

	public static void WriteInt32(int value, ProtoWriter writer)
	{
		stream.Write("int32 {0}, ", value);
		ProtoWriter.WriteInt32(value, writer);
	}

	public static void WriteInt64(long value, ProtoWriter writer)
	{
		stream.Write("int64 {0}, ", value);
		ProtoWriter.WriteInt64(value, writer);
	}

	public static void WriteObject(object value, int key, ProtoWriter writer)
	{
		stream.Write("object {0}, ", key);
		ProtoWriter.WriteObject(value, key, writer);
	}

	public static void WriteRecursionSafeObject(object value, int key, ProtoWriter writer)
	{
		stream.Write("rso {0}, ", key);
		ProtoWriter.WriteRecursionSafeObject(value, key, writer);
	}

	public static void WriteSByte(sbyte value, ProtoWriter writer)
	{
		stream.Write("sbyte {0}, ", value);
		ProtoWriter.WriteSByte(value, writer);
	}

	public static void WriteSingle(float value, ProtoWriter writer)
	{
		stream.Write("float {0}, ", value);
		ProtoWriter.WriteSingle(value, writer);
	}

	public static void WriteString(string value, ProtoWriter writer)
	{
		stream.Write("string '{0}', ", value);
		ProtoWriter.WriteString(value, writer);
	}

	public static void WriteType(Type value, ProtoWriter writer)
	{
		stream.Write("type {0}, ", value);
		ProtoWriter.WriteType(value, writer);
	}

	public static void WriteUInt16(ushort value, ProtoWriter writer)
	{
		stream.Write("uint16 {0}, ", value);
		ProtoWriter.WriteUInt16(value, writer);
	}

	public static void WriteUInt32(uint value, ProtoWriter writer)
	{
		stream.Write("uint32 {0}, ", value);
		ProtoWriter.WriteUInt32(value, writer);
	}

	public static void WriteUInt64(ulong value, ProtoWriter writer)
	{
		stream.Write("uint64 {0}, ", value);
		ProtoWriter.WriteUInt64(value, writer);
	}
}
