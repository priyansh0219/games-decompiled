using System.Collections.Generic;

public class B63Upgrade : IBatchUpgrade
{
	private static readonly Int3[] batches = new Int3[21]
	{
		new Int3(3, 16, 10),
		new Int3(3, 16, 17),
		new Int3(3, 17, 17),
		new Int3(4, 16, 17),
		new Int3(4, 16, 17),
		new Int3(4, 17, 10),
		new Int3(4, 17, 17),
		new Int3(5, 15, 10),
		new Int3(5, 16, 10),
		new Int3(5, 17, 10),
		new Int3(5, 17, 10),
		new Int3(5, 17, 17),
		new Int3(7, 19, 5),
		new Int3(7, 19, 6),
		new Int3(8, 18, 12),
		new Int3(8, 19, 5),
		new Int3(8, 19, 6),
		new Int3(11, 17, 18),
		new Int3(12, 17, 18),
		new Int3(18, 17, 16),
		new Int3(18, 17, 17)
	};

	public int GetChangeset()
	{
		return 28983;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return batches;
	}
}
