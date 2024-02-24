using UnityEngine;

public class EnableOnlyOnXboxOne : MonoBehaviour
{
	private void Start()
	{
		base.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: false);
	}
}
