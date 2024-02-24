using System.Collections.Generic;

public interface IBatchUpgrade
{
	int GetChangeset();

	IEnumerable<Int3> GetBatches();
}
