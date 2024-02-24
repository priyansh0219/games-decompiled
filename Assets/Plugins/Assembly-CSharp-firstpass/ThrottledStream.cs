using System.IO;
using System.Threading;

public sealed class ThrottledStream : Stream
{
	public readonly float latency;

	public readonly float bandwidth;

	private readonly Stream stream;

	public override bool CanRead => stream.CanRead;

	public override bool CanSeek => stream.CanSeek;

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
			stream.Position = value;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			stream.Dispose();
		}
		base.Dispose(disposing);
	}

	public ThrottledStream(Stream stream, float latency = 0f, float bandwidth = 1024f)
	{
		this.stream = stream;
		this.latency = latency;
		this.bandwidth = bandwidth;
	}

	public override void Flush()
	{
		stream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return stream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		stream.SetLength(value);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = stream.Read(buffer, offset, count);
		Wait(num);
		return num;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		stream.Write(buffer, offset, count);
		Wait(count);
	}

	private void Wait(int numBytes)
	{
		Thread.Sleep((int)((latency + (float)numBytes / bandwidth) * 1000f));
	}
}
