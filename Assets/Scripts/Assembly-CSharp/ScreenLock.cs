using UWE;
using UnityEngine;

public class ScreenLock : MonoBehaviour
{
	private void Start()
	{
		UWE.Utils.lockCursor = true;
	}
}
