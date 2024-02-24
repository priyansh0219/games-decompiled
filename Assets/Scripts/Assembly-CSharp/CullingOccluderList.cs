using System.Collections.Generic;

public class CullingOccluderList
{
	private List<CullingOccluder> occluderList = new List<CullingOccluder>(20);

	public int Count => occluderList.Count;

	public CullingOccluder this[int i] => occluderList[i];

	public void Add(CullingOccluder occluder)
	{
		if (occluder.occluderListPosition == -1)
		{
			occluder.occluderListPosition = occluderList.Count;
			occluderList.Add(occluder);
		}
	}

	public void Remove(CullingOccluder occluder)
	{
		if (occluder.occluderListPosition != -1)
		{
			occluderList[occluder.occluderListPosition] = occluderList[occluderList.Count - 1];
			occluderList[occluder.occluderListPosition].occluderListPosition = occluder.occluderListPosition;
			occluder.occluderListPosition = -1;
			occluderList.RemoveAt(occluderList.Count - 1);
		}
	}
}
