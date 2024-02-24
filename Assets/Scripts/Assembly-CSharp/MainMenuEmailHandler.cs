using System.Collections;
using TMPro;
using UnityEngine;

public class MainMenuEmailHandler : MonoBehaviour
{
	private string email;

	public string emailUrl;

	public GameObject inputfield;

	public GameObject subscribing;

	public GameObject success;

	public GameObject error;

	public void Subscribe()
	{
		PlatformServices services = PlatformUtils.main.GetServices();
		if (services == null)
		{
			Debug.LogError("Failed to subscribe, platform services unavailable.");
			return;
		}
		subscribing.SetActive(value: true);
		Debug.Log("Main Menu: Beginning email subscription process");
		email = inputfield.GetComponent<TMP_InputField>().text;
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("email", email);
		wWWForm.AddField("platform", "pc");
		WWW w = new WWW(emailUrl, wWWForm);
		StartCoroutine(sendEmail(w, services));
	}

	private IEnumerator sendEmail(WWW w, PlatformServices platformServices)
	{
		yield return platformServices.TryEnsureServerAccessAsync(onUserInput: true);
		if (platformServices.CanAccessServers())
		{
			yield return w;
			if (!string.IsNullOrEmpty(w.error))
			{
				error.SetActive(value: true);
				Debug.LogFormat("Main Menu: Error in sending new email subscription from main menu! - {0}", w.error);
			}
			else
			{
				success.SetActive(value: true);
				Debug.LogFormat("Main Menu: Backend response to email subscription: {0}", w.text);
			}
		}
		subscribing.SetActive(value: false);
	}
}
