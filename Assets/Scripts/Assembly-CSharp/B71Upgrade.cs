using System.Collections.Generic;

public class B71Upgrade : IBatchUpgrade
{
	private static readonly Int3[] batches = new Int3[12]
	{
		new Int3(14, 18, 18),
		new Int3(14, 18, 19),
		new Int3(14, 18, 20),
		new Int3(14, 19, 18),
		new Int3(14, 19, 19),
		new Int3(14, 19, 20),
		new Int3(15, 18, 18),
		new Int3(15, 18, 19),
		new Int3(15, 18, 20),
		new Int3(15, 19, 18),
		new Int3(15, 19, 19),
		new Int3(15, 19, 20)
	};

	public int GetChangeset()
	{
		return 41907;
	}

	public IEnumerable<Int3> GetBatches()
	{
		return batches;
	}
}
