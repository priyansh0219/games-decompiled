using System.Collections;
using TMPro;
using UnityEngine;

public class MainMenuRedemptionHandler : MonoBehaviour
{
	private string key;

	public string apiUrl;

	public GameObject inputfield;

	public GameObject working;

	public GameObject success;

	public GameObject error;

	public TextMeshProUGUI errorText;

	public string defaultError;

	public CanvasGroup errorPanel;

	public void Redeem()
	{
		PlatformServices services = PlatformUtils.main.GetServices();
		if (services == null)
		{
			Debug.LogError("Failed to redeem key, platform services unavailable.");
			return;
		}
		string userId = services.GetUserId();
		working.SetActive(value: true);
		Debug.Log("Merchandise: Beginning key redemption process");
		key = inputfield.GetComponent<TMP_InputField>().text;
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("key", key);
		wWWForm.AddField("steam_id", userId);
		WWW w = new WWW(apiUrl, wWWForm);
		StartCoroutine(sendkey(w, services));
	}

	private IEnumerator sendkey(WWW w, PlatformServices platformServices)
	{
		yield return platformServices.TryEnsureServerAccessAsync(onUserInput: true);
		if (platformServices.CanAccessServers())
		{
			yield return w;
			if (!string.IsNullOrEmpty(w.error))
			{
				error.SetActive(value: true);
				string text = w.error.ToString();
				Debug.Log("Merchandise: Error when attempting to redeem key - " + text);
				switch (text.Substring(0, Mathf.Min(3, text.Length)))
				{
				case "422":
					ShowError("key invalid (error A)", 4f);
					break;
				case "Key aleady used.":
					ShowError("key already used (error B)", 4f);
					break;
				case "Please log in with Steam first.":
					ShowError("cannot connect to Steam (Error D)", 4f);
					break;
				}
			}
			else
			{
				success.SetActive(value: true);
				Debug.Log("Merchandise: Backend response to key redemption: " + w.text);
			}
		}
		working.SetActive(value: false);
	}

	private void ShowError(string text, float time)
	{
		errorPanel.alpha = 1f;
		errorText.text = text;
		Invoke("Hide", time);
	}

	private void Hide()
	{
		errorPanel.alpha = 0f;
		errorText.text = defaultError;
	}
}
