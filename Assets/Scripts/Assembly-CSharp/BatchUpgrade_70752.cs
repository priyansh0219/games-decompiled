using System.Collections.Generic;

public class BatchUpgrade_70752 : IBatchUpgrade
{
	private static readonly Int3[] batches = new Int3[47]
	{
		new Int3(18, 19, 12),
		new Int3(8, 18, 16),
		new Int3(9, 18, 14),
		new Int3(10, 18, 9),
		new Int3(11, 18, 10),
		new Int3(12, 18, 15),
		new Int3(13, 18, 8),
		new Int3(14, 18, 11),
		new Int3(15, 18, 14),
		new Int3(15, 18, 15),
		new Int3(17, 18, 16),
		new Int3(4, 17, 8),
		new Int3(4, 17, 9),
		new Int3(6, 17, 17),
		new Int3(7, 17, 10),
		new Int3(10, 17, 12),
		new Int3(18, 17, 17),
		new Int3(19, 17, 15),
		new Int3(8, 15, 7),
		new Int3(18, 15, 19),
		new Int3(6, 14, 13),
		new Int3(7, 14, 10),
		new Int3(7, 14, 17),
		new Int3(8, 14, 17),
		new Int3(10, 14, 14),
		new Int3(11, 14, 14),
		new Int3(10, 13, 14),
		new Int3(10, 13, 15),
		new Int3(11, 13, 14),
		new Int3(13, 10, 10),
		new Int3(14, 10, 11),
		new Int3(13, 9, 9),
		new Int3(13, 9, 10),
		new Int3(13, 9, 11),
		new Int3(14, 9, 9),
		new Int3(14, 9, 10),
		new Int3(14, 9, 11),
		new Int3(15, 9, 9),
		new Int3(15, 9, 10),
		new Int3(15, 9, 11),
		new Int3(13, 8, 9),
		new Int3(13, 8, 10),
		new Int3(14, 8, 9),
		new Int3(14, 8, 10),
		new Int3(14, 8, 11),
		new Int3(15, 8, 9),
		new Int3(15, 8, 10)
	};

	public int GetChangeset()
	{
		return 70752;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return batches;
	}
}
