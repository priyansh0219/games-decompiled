using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gendarme;
using UWE;

public class PoolingBinaryReader : IDisposable
{
	private static readonly ArrayPool<char> charPool = new ArrayPool<char>(2, 128);

	private const float DefaultBufferSize = 4096f;

	private const int MaxCharBytesSize = 128;

	[SuppressMessage("Gendarme.Rules.Correctness", "DisposableFieldsShouldBeDisposedRule")]
	private Stream m_stream;

	private IAlloc<byte> m_buffer;

	private Decoder m_decoder;

	private IAlloc<byte> m_charBytes;

	private char[] m_singleChar;

	private char[] m_charBuffer;

	private int m_maxCharsSize;

	private Encoding m_encoding;

	private int m_minBufferSize = 16;

	private StringBuilder stringBuilder;

	private bool initialized;

	public virtual Stream BaseStream => m_stream;

	internal void Open(Stream input)
	{
		m_stream = input;
		if (!initialized)
		{
			m_encoding = new UTF8Encoding();
			if (m_decoder == null)
			{
				m_decoder = m_encoding.GetDecoder();
			}
			m_maxCharsSize = m_encoding.GetMaxCharCount(128);
			m_minBufferSize = m_encoding.GetMaxByteCount(1);
			initialized = true;
		}
		if (m_minBufferSize < 16)
		{
			m_minBufferSize = 16;
		}
		m_buffer = CommonByteArrayAllocator.Allocate(m_minBufferSize);
	}

	public virtual void Close()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_stream != null)
		{
			m_stream.Close();
			m_stream = null;
		}
		if (m_charBuffer != null)
		{
			charPool.Return(m_charBuffer);
			m_charBuffer = null;
		}
		if (m_charBytes != null)
		{
			CommonByteArrayAllocator.Free(m_charBytes);
			m_charBytes = null;
		}
		if (m_singleChar != null)
		{
			charPool.Return(m_singleChar);
			m_singleChar = null;
		}
		if (m_buffer != null)
		{
			CommonByteArrayAllocator.Free(m_buffer);
			m_buffer = null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public virtual int Read()
	{
		if (m_stream == null)
		{
			throw new ObjectDisposedException(null, "ObjectDisposed_FileClosed");
		}
		return InternalReadOneChar();
	}

	public virtual bool ReadBoolean()
	{
		FillBuffer(1);
		return m_buffer[0] != 0;
	}

	public virtual byte ReadByte()
	{
		if (m_stream == null)
		{
			throw new ObjectDisposedException(null, "ObjectDisposed_FileClosed");
		}
		int num = m_stream.ReadByte();
		if (num == -1)
		{
			throw new EndOfStreamException("IO.EOF_ReadBeyondEOF");
		}
		return (byte)num;
	}

	[CLSCompliant(false)]
	public virtual sbyte ReadSByte()
	{
		FillBuffer(1);
		return (sbyte)m_buffer[0];
	}

	public virtual char ReadChar()
	{
		int num = Read();
		if (num == -1)
		{
			throw new EndOfStreamException("IO.EOF_ReadBeyondEOF");
		}
		return (char)num;
	}

	public virtual short ReadInt16()
	{
		FillBuffer(2);
		return (short)(m_buffer[0] | (m_buffer[1] << 8));
	}

	[CLSCompliant(false)]
	public virtual ushort ReadUInt16()
	{
		FillBuffer(2);
		return (ushort)(m_buffer[0] | (m_buffer[1] << 8));
	}

	public virtual int ReadInt32()
	{
		FillBuffer(4);
		return m_buffer[0] | (m_buffer[1] << 8) | (m_buffer[2] << 16) | (m_buffer[3] << 24);
	}

	public Int3 ReadInt3()
	{
		return new Int3(ReadInt32(), ReadInt32(), ReadInt32());
	}

	[CLSCompliant(false)]
	public virtual uint ReadUInt32()
	{
		FillBuffer(4);
		return (uint)(m_buffer[0] | (m_buffer[1] << 8) | (m_buffer[2] << 16) | (m_buffer[3] << 24));
	}

	public virtual long ReadInt64()
	{
		FillBuffer(8);
		uint num = (uint)(m_buffer[0] | (m_buffer[1] << 8) | (m_buffer[2] << 16) | (m_buffer[3] << 24));
		return (long)(((ulong)(uint)(m_buffer[4] | (m_buffer[5] << 8) | (m_buffer[6] << 16) | (m_buffer[7] << 24)) << 32) | num);
	}

	[CLSCompliant(false)]
	public virtual ulong ReadUInt64()
	{
		FillBuffer(8);
		uint num = (uint)(m_buffer[0] | (m_buffer[1] << 8) | (m_buffer[2] << 16) | (m_buffer[3] << 24));
		return ((ulong)(uint)(m_buffer[4] | (m_buffer[5] << 8) | (m_buffer[6] << 16) | (m_buffer[7] << 24)) << 32) | num;
	}

	public unsafe virtual float ReadSingle()
	{
		FillBuffer(4);
		uint num = (uint)(m_buffer[0] | (m_buffer[1] << 8) | (m_buffer[2] << 16) | (m_buffer[3] << 24));
		return *(float*)(&num);
	}

	public unsafe virtual double ReadDouble()
	{
		FillBuffer(8);
		uint num = (uint)(m_buffer[0] | (m_buffer[1] << 8) | (m_buffer[2] << 16) | (m_buffer[3] << 24));
		ulong num2 = ((ulong)(uint)(m_buffer[4] | (m_buffer[5] << 8) | (m_buffer[6] << 16) | (m_buffer[7] << 24)) << 32) | num;
		return *(double*)(&num2);
	}

	public string ReadStringOrNull()
	{
		if (!ReadBoolean())
		{
			return ReadString();
		}
		return null;
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	[SuppressMessage("Gendarme.Rules.Performance", "AvoidConcatenatingCharsRule")]
	[SuppressMessage("Subnautica.Rules", "AvoidStringConcatenation")]
	public virtual string ReadString()
	{
		if (m_stream == null)
		{
			throw new ObjectDisposedException(null, "ObjectDisposed_FileClosed");
		}
		int num = 0;
		int num2 = Read7BitEncodedInt();
		if (num2 < 0)
		{
			throw new IOException("Invalid string length: " + num2);
		}
		if (num2 == 0)
		{
			return string.Empty;
		}
		if (m_charBytes == null)
		{
			m_charBytes = CommonByteArrayAllocator.Allocate(128);
		}
		if (m_charBuffer == null)
		{
			m_charBuffer = charPool.Get(m_maxCharsSize);
		}
		do
		{
			int count = ((num2 - num > 128) ? 128 : (num2 - num));
			int num3 = m_stream.Read(m_charBytes.Array, m_charBytes.Offset, count);
			if (num3 == 0)
			{
				throw new EndOfStreamException("IO.EOF_ReadBeyondEOF");
			}
			int chars = m_decoder.GetChars(m_charBytes.Array, m_charBytes.Offset, num3, m_charBuffer, 0);
			if (num == 0 && num3 == num2)
			{
				return new string(m_charBuffer, 0, chars);
			}
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			stringBuilder.Append(m_charBuffer, 0, chars);
			num += num3;
		}
		while (num < num2);
		return stringBuilder.ToString();
	}

	public virtual int Read(char[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "ArgumentNull_Buffer");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_NeedNonNegNum");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException("Argument_InvalidOffLen");
		}
		if (m_stream == null)
		{
			throw new ObjectDisposedException(null, "ObjectDisposed_FileClosed");
		}
		return InternalReadChars(buffer, index, count);
	}

	[SuppressMessage("Gendarme.Rules.Exceptions", "InstantiateArgumentExceptionCorrectlyRule")]
	private unsafe int InternalReadChars(char[] buffer, int index, int count)
	{
		int num = 0;
		int num2 = count;
		if (m_charBytes == null)
		{
			m_charBytes = CommonByteArrayAllocator.Allocate(128);
		}
		while (num2 > 0)
		{
			int num3 = 0;
			num = num2;
			if (num > 128)
			{
				num = 128;
			}
			int num4 = 0;
			IAlloc<byte> alloc = null;
			num = m_stream.Read(m_charBytes.Array, m_charBytes.Offset, num);
			alloc = m_charBytes;
			if (num == 0)
			{
				return count - num2;
			}
			checked
			{
				if (num4 < 0 || num < 0 || num4 + num > alloc.Length)
				{
					throw new ArgumentOutOfRangeException("byteCount");
				}
				if (index < 0 || num2 < 0 || index + num2 > buffer.Length)
				{
					throw new ArgumentOutOfRangeException("charsRemaining");
				}
			}
			fixed (byte* ptr = alloc.Array)
			{
				fixed (char* ptr2 = buffer)
				{
					num3 = m_decoder.GetChars((byte*)checked(unchecked((ulong)(UIntPtr)(void*)checked(unchecked((ulong)ptr) + unchecked((ulong)num4))) + unchecked((ulong)alloc.Offset)), num, (char*)checked(unchecked((ulong)ptr2) + unchecked((ulong)(UIntPtr)(void*)checked(unchecked((long)index) * 2L))), num2, flush: false);
				}
			}
			num2 -= num3;
			index += num3;
		}
		return count - num2;
	}

	private int InternalReadOneChar()
	{
		int num = 0;
		int num2 = 0;
		long num3 = 0L;
		if (m_stream.CanSeek)
		{
			num3 = m_stream.Position;
		}
		if (m_charBytes == null)
		{
			m_charBytes = CommonByteArrayAllocator.Allocate(128);
		}
		if (m_singleChar == null)
		{
			m_singleChar = charPool.Get(1);
		}
		while (num == 0)
		{
			num2 = 1;
			int num4 = m_stream.ReadByte();
			m_charBytes[0] = (byte)num4;
			if (num4 == -1)
			{
				num2 = 0;
			}
			if (num2 == 2)
			{
				num4 = m_stream.ReadByte();
				m_charBytes[1] = (byte)num4;
				if (num4 == -1)
				{
					num2 = 1;
				}
			}
			if (num2 == 0)
			{
				return -1;
			}
			try
			{
				num = m_decoder.GetChars(m_charBytes.Array, m_charBytes.Offset, num2, m_singleChar, 0);
			}
			catch
			{
				if (m_stream.CanSeek)
				{
					m_stream.Seek(num3 - m_stream.Position, SeekOrigin.Current);
				}
				throw;
			}
		}
		if (num == 0)
		{
			return -1;
		}
		return m_singleChar[0];
	}

	public virtual int Read(byte[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "ArgumentNull_Buffer");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_NeedNonNegNum");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
		}
		if (buffer.Length - index < count)
		{
			throw new ArgumentException("Argument_InvalidOffLen");
		}
		if (m_stream == null)
		{
			throw new ObjectDisposedException(null, "ObjectDisposed_FileClosed");
		}
		return m_stream.Read(buffer, index, count);
	}

	protected virtual void FillBuffer(int numBytes)
	{
		if (m_buffer != null && (numBytes < 0 || numBytes > m_buffer.Length))
		{
			throw new ArgumentOutOfRangeException("numBytes", "ArgumentOutOfRange_BinaryReaderFillBuffer");
		}
		int num = 0;
		int num2 = 0;
		if (m_stream == null)
		{
			throw new ObjectDisposedException(null, "ObjectDisposed_FileClosed");
		}
		if (numBytes == 1)
		{
			num2 = m_stream.ReadByte();
			if (num2 == -1)
			{
				throw new EndOfStreamException("IO.EOF_ReadBeyondEOF");
			}
			m_buffer[0] = (byte)num2;
			return;
		}
		do
		{
			num2 = m_stream.Read(m_buffer.Array, m_buffer.Offset + num, numBytes - num);
			if (num2 == 0)
			{
				throw new EndOfStreamException("IO.EOF_ReadBeyondEOF");
			}
			num += num2;
		}
		while (num < numBytes);
	}

	protected internal int Read7BitEncodedInt()
	{
		int num = 0;
		int num2 = 0;
		byte b;
		do
		{
			if (num2 == 35)
			{
				throw new FormatException("Bad7BitInt32");
			}
			b = ReadByte();
			num |= (b & 0x7F) << num2;
			num2 += 7;
		}
		while ((b & 0x80u) != 0);
		return num;
	}

	public List<T> ReadList<T>(Func<PoolingBinaryReader, T> readItem)
	{
		int num = ReadInt32();
		List<T> list = new List<T>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add(readItem(this));
		}
		return list;
	}
}
