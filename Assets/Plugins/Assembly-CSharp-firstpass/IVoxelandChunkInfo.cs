using System.Collections.Generic;

public interface IVoxelandChunkInfo
{
	List<VoxelandChunk.TypeUse> usedTypes { get; }

	IVoxeland land { get; }
}
