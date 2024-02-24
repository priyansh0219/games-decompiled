using UnityEngine;

public class RemoveSoundsOnKill : MonoBehaviour
{
	private void OnKill()
	{
		FMOD_StudioEventEmitter[] componentsInChildren = GetComponentsInChildren<FMOD_StudioEventEmitter>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i]);
		}
		FMOD_CustomEmitter[] componentsInChildren2 = GetComponentsInChildren<FMOD_CustomEmitter>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			Object.Destroy(componentsInChildren2[j]);
		}
	}
}
