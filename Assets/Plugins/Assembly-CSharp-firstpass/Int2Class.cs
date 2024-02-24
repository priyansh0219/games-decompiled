using System;

[Serializable]
public class Int2Class
{
	public int x;

	public int y;

	public Int2 val
	{
		get
		{
			return new Int2(x, y);
		}
		set
		{
			x = value.x;
			y = value.y;
		}
	}

	public override string ToString()
	{
		return val.ToString();
	}
}
