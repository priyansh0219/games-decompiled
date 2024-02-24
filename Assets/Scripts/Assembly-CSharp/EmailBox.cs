using UnityEngine;

public class EmailBox : MonoBehaviour
{
	public void Dismiss()
	{
		MiscSettings.hideEmailBox = true;
		Object.Destroy(base.gameObject);
	}
}
