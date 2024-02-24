using UnityEngine;

public class MainMenuSubscriptionFeedback : MonoBehaviour
{
	private void OnEnable()
	{
		Invoke("Dismiss", 3f);
	}

	private void Dismiss()
	{
		base.gameObject.SetActive(value: false);
	}
}
