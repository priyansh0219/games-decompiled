using System;
using System.IO;

public class InputBuffer
{
	private readonly byte[] buffer;

	private int position;

	private int length;

	public InputBuffer(int bufferSize)
	{
		buffer = new byte[bufferSize];
		Reset();
	}

	public void Reset()
	{
		position = 0;
		length = 0;
	}

	public bool IsEmpty()
	{
		return position >= length;
	}

	public int FillBuffer(Stream source)
	{
		int num = 0;
		while (length < buffer.Length)
		{
			int num2 = source.Read(buffer, length, buffer.Length - length);
			if (num2 == 0)
			{
				break;
			}
			num += num2;
			length += num2;
		}
		return num;
	}

	public int CopyTo(byte[] dest, int offset, int count)
	{
		int num = length - position;
		int num2 = ((count < num) ? count : num);
		Buffer.BlockCopy(buffer, position, dest, offset, num2);
		position += num2;
		return num2;
	}
}
