using System;
using System.Collections;
using System.IO;

public class OutputStream : Stream, IAwaitable
{
	private readonly Stream stream;

	public override bool CanRead => false;

	public override bool CanSeek => false;

	public override bool CanWrite => stream.CanWrite;

	public override long Length => stream.Length;

	public override long Position
	{
		get
		{
			return stream.Position;
		}
		set
		{
			throw new NotSupportedException("Can not seek OutputStream");
		}
	}

	public OutputStream(Stream stream, int numBuffers, int bufferSize)
	{
		this.stream = stream;
	}

	public override void Flush()
	{
		throw new NotSupportedException("Can not flush OutputStream");
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException("Can not seek OutputStream");
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException("Can not set length of OutputStream");
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException("Can not read from an OutputStream");
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		stream.Write(buffer, offset, count);
	}

	public IEnumerator GetAwaiter()
	{
		return null;
	}

	public IEnumerator GetFlushAwaiter()
	{
		stream.Flush();
		return null;
	}
}
