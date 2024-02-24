using System;
using ProtoBuf;

[Serializable]
[Obsolete("Use Int3 instead")]
[ProtoContract]
public class Int3Class
{
	[ProtoMember(1)]
	public int x;

	[ProtoMember(2)]
	public int y;

	[ProtoMember(3)]
	public int z;

	public Int3 val
	{
		get
		{
			return new Int3(x, y, z);
		}
		set
		{
			x = value.x;
			y = value.y;
			z = value.z;
		}
	}

	public Int3Class()
	{
	}

	public Int3Class(Int3 v)
	{
		val = v;
	}

	public override string ToString()
	{
		return val.ToString();
	}
}
