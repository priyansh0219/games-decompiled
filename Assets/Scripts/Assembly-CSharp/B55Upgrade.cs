using System.Collections.Generic;

public class B55Upgrade : IBatchUpgrade
{
	private static readonly Int3[] batches = new Int3[21]
	{
		new Int3(14, 18, 15),
		new Int3(14, 18, 14),
		new Int3(14, 18, 13),
		new Int3(15, 18, 13),
		new Int3(15, 18, 14),
		new Int3(9, 18, 12),
		new Int3(9, 18, 11),
		new Int3(10, 18, 10),
		new Int3(9, 18, 11),
		new Int3(8, 18, 11),
		new Int3(7, 18, 12),
		new Int3(7, 18, 11),
		new Int3(10, 18, 16),
		new Int3(10, 18, 15),
		new Int3(9, 18, 15),
		new Int3(9, 18, 16),
		new Int3(9, 18, 17),
		new Int3(13, 18, 9),
		new Int3(13, 18, 8),
		new Int3(12, 18, 8),
		new Int3(18, 19, 12)
	};

	public int GetChangeset()
	{
		return 18740;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return batches;
	}
}
