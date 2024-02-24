using System.Collections.Generic;

public class B65Upgrade : IBatchUpgrade
{
	private static readonly Int3[] batches = new Int3[43]
	{
		new Int3(9, 17, 21),
		new Int3(9, 17, 22),
		new Int3(9, 18, 10),
		new Int3(9, 18, 11),
		new Int3(9, 18, 18),
		new Int3(9, 18, 19),
		new Int3(10, 15, 21),
		new Int3(10, 15, 22),
		new Int3(10, 16, 19),
		new Int3(10, 16, 19),
		new Int3(10, 16, 20),
		new Int3(10, 16, 21),
		new Int3(10, 16, 22),
		new Int3(10, 16, 23),
		new Int3(10, 17, 18),
		new Int3(10, 17, 19),
		new Int3(10, 17, 19),
		new Int3(10, 17, 20),
		new Int3(10, 18, 10),
		new Int3(10, 18, 11),
		new Int3(11, 11, 12),
		new Int3(11, 11, 13),
		new Int3(11, 11, 14),
		new Int3(11, 12, 13),
		new Int3(11, 15, 20),
		new Int3(11, 15, 21),
		new Int3(11, 15, 22),
		new Int3(11, 16, 20),
		new Int3(11, 16, 21),
		new Int3(11, 16, 22),
		new Int3(11, 17, 20),
		new Int3(11, 17, 21),
		new Int3(11, 18, 21),
		new Int3(12, 11, 12),
		new Int3(12, 11, 13),
		new Int3(12, 11, 14),
		new Int3(12, 12, 12),
		new Int3(12, 12, 13),
		new Int3(12, 12, 14),
		new Int3(14, 11, 13),
		new Int3(14, 11, 14),
		new Int3(15, 11, 13),
		new Int3(15, 11, 14)
	};

	public int GetChangeset()
	{
		return 32370;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return batches;
	}
}
