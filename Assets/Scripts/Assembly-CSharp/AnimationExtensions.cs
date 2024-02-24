using System.Collections;
using UnityEngine;

public static class AnimationExtensions
{
	public static AnimationState GetState(this Animation animation, int index)
	{
		IEnumerator enumerator = animation.GetEnumerator();
		if (enumerator.MoveNext())
		{
			return enumerator.Current as AnimationState;
		}
		return null;
	}
}
