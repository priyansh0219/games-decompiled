using UnityEngine;

public class OpenURL : MonoBehaviour
{
	public void Open(string URL)
	{
		PlatformUtils.OpenURL(URL);
	}
}
