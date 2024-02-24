using System;
using System.IO;
using UWE;
using UnityEngine;

public class UshortHeightmap
{
	private static int BinaryVersion;

	private ushort[,] data;

	private Int3 numBlocks;

	public Int2 size => data.Dims();

	public void LoadWorldMachineU16Threaded(string filename, Int3 numBlocks)
	{
		data = UWE.Utils.LoadRawU16SquareGrid(filename);
		this.numBlocks = numBlocks;
	}

	public void Write(BinaryWriter w)
	{
		w.Write(BinaryVersion);
		byte[] array = new byte[data.Length * 2];
		Buffer.BlockCopy(data, 0, array, 0, array.Length);
		w.Write(data.GetLength(0));
		w.Write(data.GetLength(1));
		w.Write(array);
	}

	public void Read(BinaryReader r)
	{
		r.ReadInt32();
		int num = r.ReadInt32();
		int num2 = r.ReadInt32();
		byte[] src = r.ReadBytes(num * num2);
		data = new ushort[num, num2];
		Buffer.BlockCopy(src, 0, data, 0, data.Length);
	}

	public virtual float GetHeight(int x, int z)
	{
		return (float)(int)GetHeightRaw(x, z) / 65535f * (float)numBlocks.y;
	}

	public virtual bool GetMask(int x, int y, int z)
	{
		return (float)y + 0.5f < GetHeight(x, z);
	}

	public void SetHeightRaw(Int2 p, ushort val)
	{
		data.SetFlipYX(p, val);
	}

	public ushort GetHeightRaw(int x, int z)
	{
		Int2 p = new Int2(data.GetLength(0) - z - 1, x);
		if (data.CheckBounds(p))
		{
			return data.Get(p);
		}
		return 0;
	}

	public ushort GetHeightRaw(Int2 xz)
	{
		return GetHeightRaw(xz.x, xz.y);
	}

	public virtual byte GetType(int x, int y, int z)
	{
		return 1;
	}

	public bool IsBelowBlock(Int3 p)
	{
		return p.y > VoxelandUtils.TopBlockForHeight(GetHeight(p.x, p.z));
	}

	public void WritePNG(string path)
	{
		data.SavePNG(path, (ushort v) => Color.white * ((float)(int)v * 1f / 65535f));
	}
}
