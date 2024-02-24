using System;
using System.IO;
using UWE;

public class SerialData
{
	private static IAlloc<byte> emptyData;

	public IAlloc<byte> Data { get; private set; }

	public int Length
	{
		get
		{
			if (Data == null)
			{
				return 0;
			}
			return Data.Length;
		}
	}

	public SerialData()
	{
		if (emptyData == null)
		{
			emptyData = CommonByteArrayAllocator.Allocate(0);
		}
		Data = emptyData;
	}

	public void CopyFrom(SerialData serialData)
	{
		if (serialData != null)
		{
			Clear();
			Data = CommonByteArrayAllocator.Allocate(serialData.Length);
			Buffer.BlockCopy(serialData.Data.Array, serialData.Data.Offset, Data.Array, Data.Offset, Data.Length);
		}
	}

	public void CopyFrom(ScratchMemoryStream stream)
	{
		if (stream != null)
		{
			Clear();
			int length = stream.GetLength();
			Data = CommonByteArrayAllocator.Allocate(length);
			stream.CopyTo(Data.Array, Data.Offset);
		}
	}

	public void ReadFromStream(Stream stream, int dataLength)
	{
		if (stream == null || dataLength == 0)
		{
			return;
		}
		Clear();
		Data = CommonByteArrayAllocator.Allocate(dataLength);
		int num;
		for (int i = 0; i < dataLength; i += num)
		{
			num = stream.Read(Data.Array, Data.Offset + i, dataLength - i);
			if (num == 0)
			{
				break;
			}
		}
	}

	public void Concatenate(ScratchMemoryStream stream)
	{
		if (stream != null)
		{
			int length = stream.GetLength();
			IAlloc<byte> alloc = CommonByteArrayAllocator.Allocate(length + Length);
			stream.CopyTo(alloc.Array, alloc.Offset);
			Buffer.BlockCopy(Data.Array, Data.Offset, alloc.Array, alloc.Offset + length, Data.Length);
			Clear();
			Data = alloc;
		}
	}

	public void Clear()
	{
		if (Data != null)
		{
			if (Data != emptyData)
			{
				CommonByteArrayAllocator.Free(Data);
			}
			Data = emptyData;
		}
	}

	public bool IsEmpty()
	{
		return Length == 0;
	}
}
