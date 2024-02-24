using System;
using UnityEngine;

[Serializable]
public class VoxelandBlockTypePrefab : MonoBehaviour, ICompileTimeCheckable
{
	public VoxelandBlockType blockType;

	public byte globalId;

	public void InvalidateId()
	{
		globalId = 0;
	}

	public bool HasValidId()
	{
		if (globalId != 0)
		{
			return globalId != byte.MaxValue;
		}
		return false;
	}

	public string CompileTimeCheck()
	{
		return blockType.Check();
	}
}
