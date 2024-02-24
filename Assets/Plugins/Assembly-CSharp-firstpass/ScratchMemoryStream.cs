using System;
using System.IO;
using System.Runtime.InteropServices;
using UWE;

[Serializable]
public class ScratchMemoryStream : Stream
{
	internal const int MaxByteArrayLength = 2147483591;

	private const int minBufferSize = 32768;

	private IAlloc<byte> _buffer;

	private int _capacity;

	private int _position;

	private int _length;

	private bool _writable;

	private bool _isOpen;

	private const int MemStreamMaxLength = int.MaxValue;

	public override bool CanRead => _isOpen;

	public override bool CanSeek => _isOpen;

	public override bool CanWrite => _writable;

	public virtual int Capacity
	{
		get
		{
			if (!_isOpen)
			{
				throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
			}
			return _capacity;
		}
		set
		{
			if (value < Length)
			{
				throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_SmallCapacity");
			}
			if (!_isOpen)
			{
				throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
			}
			if (value == _capacity)
			{
				return;
			}
			if (value > 0)
			{
				IAlloc<byte> alloc = CommonByteArrayAllocator.Allocate(value);
				if (_length > 0)
				{
					Buffer.BlockCopy(_buffer.Array, _buffer.Offset, alloc.Array, alloc.Offset, _length);
					CommonByteArrayAllocator.Free(_buffer);
				}
				_buffer = alloc;
			}
			else
			{
				_buffer = null;
			}
			_capacity = value;
		}
	}

	public override long Length
	{
		get
		{
			if (!_isOpen)
			{
				throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
			}
			return _length;
		}
	}

	public override long Position
	{
		get
		{
			if (!_isOpen)
			{
				throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
			}
			return _position;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_NeedNonNegNum");
			}
			if (!_isOpen)
			{
				throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
			}
			if (value > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_StreamLength");
			}
			_position = (int)value;
		}
	}

	public ScratchMemoryStream()
	{
		if (_buffer == null)
		{
			_buffer = CommonByteArrayAllocator.Allocate(32768);
			_capacity = 32768;
		}
		_writable = true;
		_isOpen = true;
	}

	private void EnsureWriteable()
	{
		if (!CanWrite)
		{
			throw new NotSupportedException("NotSupported_UnwritableStream");
		}
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				_isOpen = false;
				_writable = false;
				CommonByteArrayAllocator.Free(_buffer);
				_buffer = null;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	private bool EnsureCapacity(int value)
	{
		if (value < 0)
		{
			throw new IOException("IO.IO_StreamTooLong");
		}
		if (value > _capacity)
		{
			int num = value;
			if (num < 256)
			{
				num = 256;
			}
			if (num < _capacity * 2)
			{
				num = _capacity * 2;
			}
			if ((uint)(_capacity * 2) > 2147483591u)
			{
				num = ((value > 2147483591) ? value : 2147483591);
			}
			Capacity = num;
			return true;
		}
		return false;
	}

	public override void Flush()
	{
	}

	public override int Read([In][Out] byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "ArgumentNull_Buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_NeedNonNegNum");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException("Argument_InvalidOffLen");
		}
		if (!_isOpen)
		{
			throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
		}
		int num = _length - _position;
		if (num > count)
		{
			num = count;
		}
		if (num <= 0)
		{
			return 0;
		}
		if (num <= 8)
		{
			int num2 = num;
			while (--num2 >= 0)
			{
				buffer[offset + num2] = _buffer[_position + num2];
			}
		}
		else
		{
			Buffer.BlockCopy(_buffer.Array, _buffer.Offset + _position, buffer, offset, num);
		}
		_position += num;
		return num;
	}

	public override int ReadByte()
	{
		if (!_isOpen)
		{
			throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
		}
		if (_position >= _length)
		{
			return -1;
		}
		return _buffer[_position++];
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (!_isOpen)
		{
			throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
		}
		if (offset > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_StreamLength");
		}
		switch (origin)
		{
		case SeekOrigin.Begin:
		{
			int num3 = (int)offset;
			if (offset < 0 || num3 < 0)
			{
				throw new IOException("IO.IO_SeekBeforeBegin");
			}
			_position = num3;
			break;
		}
		case SeekOrigin.Current:
		{
			int num2 = _position + (int)offset;
			if (_position + offset < 0 || num2 < 0)
			{
				throw new IOException("IO.IO_SeekBeforeBegin");
			}
			_position = num2;
			break;
		}
		case SeekOrigin.End:
		{
			int num = _length + (int)offset;
			if (_length + offset < 0 || num < 0)
			{
				throw new IOException("IO.IO_SeekBeforeBegin");
			}
			_position = num;
			break;
		}
		default:
			throw new ArgumentException("Argument_InvalidSeekOrigin");
		}
		return _position;
	}

	public override void SetLength(long value)
	{
		if (value < 0 || value > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_StreamLength");
		}
		EnsureWriteable();
		if (value > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_StreamLength");
		}
		int num = (int)value;
		if (!EnsureCapacity(num) && num > _length)
		{
			Array.Clear(_buffer.Array, _buffer.Offset + _length, num - _length);
		}
		_length = num;
		if (_position > num)
		{
			_position = num;
		}
	}

	public int GetLength()
	{
		return _length;
	}

	public void CopyTo(byte[] byteArray, int offset)
	{
		Buffer.BlockCopy(_buffer.Array, _buffer.Offset, byteArray, offset, _length);
	}

	public virtual byte[] ToArray()
	{
		byte[] array = new byte[_length];
		Buffer.BlockCopy(_buffer.Array, _buffer.Offset, array, 0, _length);
		return array;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer", "ArgumentNull_Buffer");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_NeedNonNegNum");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
		}
		if (buffer.Length - offset < count)
		{
			throw new ArgumentException("Argument_InvalidOffLen");
		}
		if (!_isOpen)
		{
			throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
		}
		EnsureWriteable();
		int num = _position + count;
		if (num < 0)
		{
			throw new IOException("IO.IO_StreamTooLong");
		}
		if (num > _length)
		{
			bool flag = _position > _length;
			if (num > _capacity && EnsureCapacity(num))
			{
				flag = false;
			}
			if (flag)
			{
				Array.Clear(_buffer.Array, _buffer.Offset + _length, num - _length);
			}
			_length = num;
		}
		if (count <= 8)
		{
			int num2 = count;
			while (--num2 >= 0)
			{
				_buffer[_position + num2] = buffer[offset + num2];
			}
		}
		else
		{
			Buffer.BlockCopy(buffer, offset, _buffer.Array, _buffer.Offset + _position, count);
		}
		_position = num;
	}

	public override void WriteByte(byte value)
	{
		if (!_isOpen)
		{
			throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
		}
		EnsureWriteable();
		if (_position >= _length)
		{
			int num = _position + 1;
			bool flag = _position > _length;
			if (num >= _capacity && EnsureCapacity(num))
			{
				flag = false;
			}
			if (flag)
			{
				Array.Clear(_buffer.Array, _buffer.Offset + _length, _position - _length);
			}
			_length = num;
		}
		_buffer[_position++] = value;
	}

	public virtual void WriteTo(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream", "ArgumentNull_Stream");
		}
		if (!_isOpen)
		{
			throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
		}
		stream.Write(_buffer.Array, _buffer.Offset, _length);
	}
}
