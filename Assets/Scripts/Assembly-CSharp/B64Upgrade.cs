using System.Collections.Generic;

public class B64Upgrade : IBatchUpgrade
{
	private static readonly Int3[] batches = new Int3[11]
	{
		new Int3(15, 18, 15),
		new Int3(8, 18, 12),
		new Int3(18, 17, 16),
		new Int3(12, 17, 18),
		new Int3(17, 16, 20),
		new Int3(4, 16, 17),
		new Int3(5, 17, 8),
		new Int3(5, 16, 10),
		new Int3(8, 18, 17),
		new Int3(10, 17, 7),
		new Int3(13, 18, 15)
	};

	public int GetChangeset()
	{
		return 31051;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return batches;
	}
}
