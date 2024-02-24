using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleMainMenuNews : MonoBehaviour
{
	public RawImage image;

	public TextMeshProUGUI header;

	public TextMeshProUGUI text;

	public TextMeshProUGUI date;

	public TextMeshProUGUI buttonText;

	public string URL;

	[SerializeField]
	private TranslationLiveUpdate headerTextLiveUpdater;

	[SerializeField]
	private TranslationLiveUpdate contentTextLiveUpdater;

	public void Start()
	{
		buttonText.text = Language.main.Get(buttonText.text);
	}

	public void Open()
	{
		PlatformUtils.OpenURL(URL);
	}

	public void Initialize(string header, string text, string date, string url, string imageUrl)
	{
		this.header.text = header;
		this.text.text = text;
		this.date.text = date;
		URL = url;
		StartCoroutine(LoadImage(imageUrl));
		headerTextLiveUpdater.enabled = false;
		contentTextLiveUpdater.enabled = false;
	}

	private IEnumerator LoadImage(string url)
	{
		WWW www = new WWW(url);
		yield return www;
		image.texture = www.texture;
	}
}
