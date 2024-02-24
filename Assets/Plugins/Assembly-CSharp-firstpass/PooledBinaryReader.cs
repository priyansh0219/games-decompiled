using System;
using System.IO;
using UWE;

public struct PooledBinaryReader : IDisposable
{
	private static readonly ObjectPool<PoolingBinaryReader> pool = ObjectPoolHelper.CreatePool<PoolingBinaryReader>("PooledBinaryReader::PoolingBinaryReader", 32);

	private PoolingBinaryReader reader;

	public PooledBinaryReader(Stream stream)
	{
		reader = pool.Get();
		reader.Open(stream);
	}

	public void Dispose()
	{
		reader.Close();
		pool.Return(reader);
		reader = null;
	}

	public bool ReadBoolean()
	{
		return reader.ReadBoolean();
	}

	public byte ReadByte()
	{
		return reader.ReadByte();
	}

	public sbyte ReadSByte()
	{
		return reader.ReadSByte();
	}

	public char ReadChar()
	{
		return reader.ReadChar();
	}

	public short ReadInt16()
	{
		return reader.ReadInt16();
	}

	public ushort ReadUInt16()
	{
		return reader.ReadUInt16();
	}

	public int ReadInt32()
	{
		return reader.ReadInt32();
	}

	public uint ReadUInt32()
	{
		return reader.ReadUInt32();
	}

	public long ReadInt64()
	{
		return reader.ReadInt64();
	}

	public ulong ReadUInt64()
	{
		return reader.ReadUInt64();
	}

	public float ReadSingle()
	{
		return reader.ReadSingle();
	}

	public double ReadDouble()
	{
		return reader.ReadDouble();
	}

	public string ReadString()
	{
		return reader.ReadString();
	}

	public int Read(byte[] buffer, int index, int count)
	{
		return reader.Read(buffer, index, count);
	}

	public int Read(char[] buffer, int index, int count)
	{
		return reader.Read(buffer, index, count);
	}

	public static implicit operator PoolingBinaryReader(PooledBinaryReader reader)
	{
		return reader.reader;
	}
}
