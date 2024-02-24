using System.Collections.Generic;

public class B60Upgrade : IBatchUpgrade
{
	private static readonly Int3[] batches = new Int3[2]
	{
		new Int3(14, 18, 15),
		new Int3(15, 18, 15)
	};

	public int GetChangeset()
	{
		return 26110;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return batches;
	}
}
