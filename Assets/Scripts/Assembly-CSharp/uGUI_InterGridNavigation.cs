using System;
using UnityEngine;

[Serializable]
public struct uGUI_InterGridNavigation
{
	public MonoBehaviour upGrid;

	public MonoBehaviour downGrid;

	public MonoBehaviour leftGrid;

	public MonoBehaviour rightGrid;

	public uGUI_INavigableIconGrid GetNavigableGridInDirection(int dirX, int dirY)
	{
		if (dirX < 0)
		{
			return leftGrid as uGUI_INavigableIconGrid;
		}
		if (dirX > 0)
		{
			return rightGrid as uGUI_INavigableIconGrid;
		}
		if (dirY < 0)
		{
			return upGrid as uGUI_INavigableIconGrid;
		}
		if (dirY > 0)
		{
			return downGrid as uGUI_INavigableIconGrid;
		}
		return null;
	}
}
