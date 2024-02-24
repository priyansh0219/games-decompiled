using System.Collections.Generic;

public class B78Upgrade : IBatchUpgrade
{
	public int GetChangeset()
	{
		return 53475;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return new Int3.Bounds(Int3.zero, new Int3(25, 19, 25)).ToEnumerable();
	}
}
