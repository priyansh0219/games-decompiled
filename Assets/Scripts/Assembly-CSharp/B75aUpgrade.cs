using System.Collections.Generic;

public class B75aUpgrade : IBatchUpgrade
{
	private static readonly Int3[] batches = new Int3[8]
	{
		new Int3(14, 19, 18),
		new Int3(17, 19, 13),
		new Int3(19, 17, 17),
		new Int3(5, 14, 8),
		new Int3(5, 16, 19),
		new Int3(5, 16, 3),
		new Int3(8, 15, 22),
		new Int3(3, 16, 17)
	};

	public int GetChangeset()
	{
		return 47572;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return batches;
	}
}
