using System.Collections.Generic;

public class B79Upgrade : IBatchUpgrade
{
	public int GetChangeset()
	{
		return 54761;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return new Int3.Bounds(Int3.zero, new Int3(25, 19, 25)).ToEnumerable();
	}
}
