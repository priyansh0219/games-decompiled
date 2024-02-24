using System.Collections.Generic;

public class B89Upgrade : IBatchUpgrade
{
	private static readonly Int3[] batches = new Int3[40]
	{
		new Int3(7, 18, 11),
		new Int3(7, 18, 12),
		new Int3(8, 18, 11),
		new Int3(8, 18, 12),
		new Int3(9, 18, 12),
		new Int3(9, 18, 16),
		new Int3(10, 18, 16),
		new Int3(11, 18, 16),
		new Int3(12, 18, 12),
		new Int3(12, 18, 16),
		new Int3(13, 18, 8),
		new Int3(13, 18, 15),
		new Int3(13, 18, 16),
		new Int3(13, 18, 17),
		new Int3(14, 18, 13),
		new Int3(14, 18, 14),
		new Int3(15, 18, 13),
		new Int3(15, 18, 14),
		new Int3(16, 18, 13),
		new Int3(16, 18, 14),
		new Int3(8, 17, 7),
		new Int3(11, 17, 11),
		new Int3(11, 17, 12),
		new Int3(11, 17, 17),
		new Int3(12, 17, 10),
		new Int3(12, 17, 11),
		new Int3(12, 17, 12),
		new Int3(13, 17, 10),
		new Int3(13, 17, 11),
		new Int3(13, 17, 19),
		new Int3(19, 17, 16),
		new Int3(20, 17, 16),
		new Int3(7, 16, 20),
		new Int3(7, 15, 18),
		new Int3(18, 15, 18),
		new Int3(19, 15, 18),
		new Int3(8, 14, 17),
		new Int3(8, 14, 18),
		new Int3(9, 13, 12),
		new Int3(9, 13, 13)
	};

	public int GetChangeset()
	{
		return 63403;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return batches;
	}
}
