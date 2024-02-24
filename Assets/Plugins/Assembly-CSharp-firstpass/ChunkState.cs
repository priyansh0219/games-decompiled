using System;
using UnityEngine;

[Serializable]
public class ChunkState
{
	public bool dirty;

	public bool beingBuilt;

	public bool destroyQueued;

	public VoxelandChunk chunk;

	public void Reset()
	{
		dirty = false;
		beingBuilt = false;
		destroyQueued = false;
		chunk = null;
	}

	public bool IsOccupiedByChunk(int cx, int cy, int cz)
	{
		if (chunk != null)
		{
			return chunk.IsChunkNumber(cx, cy, cz);
		}
		return false;
	}

	public void SwitchToLOD(Voxeland host, bool high)
	{
		for (int i = 0; i < chunk.loFilters.Count; i++)
		{
			if (!high)
			{
				if (!chunk.skipHiRes && chunk.hiFilters[i].GetComponent<Renderer>().enabled)
				{
					chunk.hiFilters[i].GetComponent<Renderer>().enabled = false;
				}
				if (!chunk.loFilters[i].GetComponent<Renderer>().enabled)
				{
					chunk.loFilters[i].GetComponent<Renderer>().enabled = true;
				}
			}
			else if (!chunk.skipHiRes)
			{
				if (!chunk.hiFilters[i].GetComponent<Renderer>().enabled)
				{
					chunk.hiFilters[i].GetComponent<Renderer>().enabled = true;
				}
				if (chunk.loFilters[i].GetComponent<Renderer>().enabled)
				{
					chunk.loFilters[i].GetComponent<Renderer>().enabled = false;
				}
			}
			else if (!chunk.loFilters[i].GetComponent<Renderer>().enabled)
			{
				chunk.loFilters[i].GetComponent<Renderer>().enabled = true;
			}
		}
		if (host.generateCollider && !(chunk.collision != null) && chunk.loFilters != null && chunk.loFilters.Count > 0)
		{
			chunk.loFilters[0].gameObject.EnsureComponent<MeshCollider>();
		}
		if (host.eventHandler != null)
		{
			int cx = chunk.cx;
			int cy = chunk.cy;
			int cz = chunk.cz;
			if (high)
			{
				host.eventHandler.OnChunkHighLOD(host, cx, cy, cz);
			}
			else
			{
				host.eventHandler.OnChunkLowLOD(host, cx, cy, cz);
			}
		}
	}

	public void DestroyChunk(Voxeland host)
	{
		if (chunk == null)
		{
			return;
		}
		if (beingBuilt)
		{
			destroyQueued = true;
			return;
		}
		int cx = chunk.cx;
		int cy = chunk.cy;
		int cz = chunk.cz;
		chunk.DestroySelf();
		if (host.eventHandler != null)
		{
			host.eventHandler.OnChunkDestroyed(host, cx, cy, cz);
		}
		Reset();
	}
}
