using System;

[Serializable]
public class VoxelandChunkWorkspaceProxy
{
	private VoxelandChunkWorkspace workspace;

	public VoxelandChunkWorkspaceProxy()
	{
		workspace = null;
	}

	public VoxelandChunkWorkspace InitializeWorkspace(VoxelandChunk chunk)
	{
		if (workspace == null)
		{
			workspace = new VoxelandChunkWorkspace();
		}
		workspace.SetSize(chunk.meshRes);
		return workspace;
	}
}
