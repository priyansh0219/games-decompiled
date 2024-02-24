using System.Collections.Generic;

public class BatchUpgrade_71172 : IBatchUpgrade
{
	private static readonly Int3[] batches = new Int3[14]
	{
		new Int3(15, 17, 21),
		new Int3(18, 17, 20),
		new Int3(18, 17, 21),
		new Int3(19, 17, 20),
		new Int3(19, 17, 21),
		new Int3(20, 17, 15),
		new Int3(9, 16, 1),
		new Int3(18, 16, 20),
		new Int3(18, 16, 21),
		new Int3(19, 16, 20),
		new Int3(19, 16, 21),
		new Int3(8, 15, 7),
		new Int3(8, 14, 17),
		new Int3(13, 12, 17)
	};

	public int GetChangeset()
	{
		return 71172;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return batches;
	}
}
