public struct Grid3Point
{
	public int x;

	public int y;

	public int z;

	public int index;

	public static Grid3Point Invalid = new Grid3Point(-1, 0, 0, -1);

	public bool Valid => index != -1;

	public Grid3Point(int x, int y, int z, int index)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.index = index;
	}

	public Int3 ToInt3()
	{
		return new Int3(x, y, z);
	}
}
