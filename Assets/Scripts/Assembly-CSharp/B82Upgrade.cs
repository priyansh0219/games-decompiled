using System.Collections.Generic;

public class B82Upgrade : IBatchUpgrade
{
	private static readonly Int3[] batches = new Int3[1]
	{
		new Int3(12, 19, 12)
	};

	public int GetChangeset()
	{
		return 61960;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return batches;
	}
}
