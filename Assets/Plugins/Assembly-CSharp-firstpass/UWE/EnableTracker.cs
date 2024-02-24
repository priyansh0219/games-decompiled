using UnityEngine;

namespace UWE
{
	public class EnableTracker : MonoBehaviour
	{
		private void OnEnable()
		{
			Debug.Log("Enabled: " + base.gameObject.name);
		}

		private void OnDisable()
		{
			Debug.Log("Disabled: " + base.gameObject.name);
		}
	}
}
