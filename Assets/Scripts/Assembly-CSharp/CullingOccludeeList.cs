using System.Collections.Generic;
using UnityEngine;

public class CullingOccludeeList
{
	private int maxOccludees;

	private bool locked;

	private List<int> freeList;

	private List<int> usedListForDynamicOccludees;

	private HashSet<CullingOccludee> occludeesToAdd = new HashSet<CullingOccludee>();

	public CullingOccludee[] occludees { get; private set; }

	public List<int> usedList { get; private set; }

	public CullingGPUDataStructures.RendererBoundingBox[] boundingBoxes { get; private set; }

	public int maxBufferSizeRequired { get; private set; }

	public int Count => usedList.Count;

	public CullingOccludeeList(int maxOccludees)
	{
		this.maxOccludees = maxOccludees;
		occludees = new CullingOccludee[maxOccludees];
		boundingBoxes = new CullingGPUDataStructures.RendererBoundingBox[maxOccludees];
		freeList = new List<int>(maxOccludees);
		usedList = new List<int>(maxOccludees);
		usedListForDynamicOccludees = new List<int>();
		for (int num = maxOccludees - 1; num >= 0; num--)
		{
			freeList.Add(num);
		}
	}

	public void Add(CullingOccludee occludee)
	{
		if (occludee.computeBufferPosition != -1)
		{
			return;
		}
		if (locked)
		{
			occludeesToAdd.Add(occludee);
		}
		else if (freeList.Count != 0)
		{
			int num = freeList[freeList.Count - 1];
			freeList.RemoveAt(freeList.Count - 1);
			usedList.Add(num);
			occludees[num] = occludee;
			occludee.computeBufferPosition = num;
			occludee.usedListPosition = usedList.Count - 1;
			if (!occludee.isStatic)
			{
				usedListForDynamicOccludees.Add(num);
				occludee.dynamicUsedListPosition = usedListForDynamicOccludees.Count - 1;
			}
			occludee.GetWorldBounds(out boundingBoxes[num].min, out boundingBoxes[num].max);
			maxBufferSizeRequired = Mathf.Max(num + 1, maxBufferSizeRequired);
		}
	}

	public void Remove(CullingOccludee occludee)
	{
		if (occludee.computeBufferPosition != -1 && occludee.usedListPosition != -1)
		{
			occludeesToAdd.Remove(occludee);
			freeList.Add(occludee.computeBufferPosition);
			int num = usedList[usedList.Count - 1];
			occludees[num].usedListPosition = occludee.usedListPosition;
			usedList[occludee.usedListPosition] = usedList[usedList.Count - 1];
			usedList.RemoveAt(usedList.Count - 1);
			if (occludee.dynamicUsedListPosition != -1)
			{
				int num2 = usedListForDynamicOccludees[usedListForDynamicOccludees.Count - 1];
				occludees[num2].dynamicUsedListPosition = occludee.dynamicUsedListPosition;
				usedListForDynamicOccludees[occludee.dynamicUsedListPosition] = num2;
				usedListForDynamicOccludees.RemoveAt(usedListForDynamicOccludees.Count - 1);
			}
			occludees[occludee.computeBufferPosition] = null;
			occludee.computeBufferPosition = -1;
			occludee.usedListPosition = -1;
			occludee.dynamicUsedListPosition = -1;
			if (usedList.Count == 0)
			{
				maxBufferSizeRequired = 0;
			}
		}
	}

	public void Lock()
	{
		locked = true;
	}

	public void Unlock()
	{
		locked = false;
		foreach (CullingOccludee item in occludeesToAdd)
		{
			Add(item);
		}
		occludeesToAdd.Clear();
	}

	public void UpdateDynamicBounds()
	{
		foreach (int usedListForDynamicOccludee in usedListForDynamicOccludees)
		{
			occludees[usedListForDynamicOccludee].GetWorldBounds(out boundingBoxes[usedListForDynamicOccludee].min, out boundingBoxes[usedListForDynamicOccludee].max);
		}
	}
}
