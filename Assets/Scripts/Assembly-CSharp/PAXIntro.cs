using System.Collections;
using UnityEngine;

public class PAXIntro : MonoBehaviour
{
	public GUITexture curtains;

	public GUIText text;

	public GUIText skipText;

	private bool showing;

	private float fadeTime;

	private int dots;

	private bool UpdateTextAlpha(float duration, bool fadeIn)
	{
		Color color = text.color;
		if (fadeTime > duration)
		{
			color.a = (fadeIn ? 1f : 0f);
			text.color = color;
			return false;
		}
		if (fadeIn)
		{
			color.a = fadeTime / duration;
		}
		else
		{
			color.a = 1f - fadeTime / duration;
		}
		text.color = color;
		fadeTime += Time.deltaTime;
		return true;
	}

	private IEnumerator Start()
	{
		Color c = curtains.color;
		c.a = 0f;
		curtains.color = c;
		curtains.gameObject.SetActive(value: true);
		text.gameObject.SetActive(value: false);
		skipText.gameObject.SetActive(value: false);
		GetComponent<AudioSource>().Stop();
		InputHandlerStack.main.Push(base.gameObject);
		showing = true;
		c.a = 0.5f;
		curtains.color = c;
		text.text = "Loading...";
		text.gameObject.SetActive(value: true);
		skipText.gameObject.SetActive(value: true);
		yield return new WaitForSeconds(2f);
		skipText.gameObject.SetActive(value: false);
		text.text = "UNKNOWN WORLDS ENTERTAINMENT\nPRESENTS";
		fadeTime = 0f;
		while (UpdateTextAlpha(1f, fadeIn: true))
		{
			yield return null;
		}
		yield return new WaitForSeconds(3f);
		fadeTime = 0f;
		while (UpdateTextAlpha(1f, fadeIn: false))
		{
			yield return null;
		}
		yield return new WaitForSeconds(6f);
		text.text = "SUBNAUTICA";
		fadeTime = 0f;
		while (UpdateTextAlpha(4f, fadeIn: true))
		{
			yield return null;
		}
		yield return new WaitForSeconds(6f);
		fadeTime = 0f;
		while (UpdateTextAlpha(1f, fadeIn: false))
		{
			yield return null;
		}
		text.text = "";
		yield return new WaitForSeconds(3f);
		yield return new WaitForSeconds(0.1f);
		yield return new WaitForSeconds(1f);
		text.gameObject.SetActive(value: false);
		float fadeDuration = 1f;
		float curtainsFadeTime = 0f;
		while (curtainsFadeTime < fadeDuration)
		{
			c.a = Mathf.Lerp(0.5f, 0f, curtainsFadeTime / fadeDuration);
			curtains.color = c;
			curtainsFadeTime += Time.deltaTime;
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		dots = (dots + 1) % 10;
		text.text = "Loading..";
	}

	private void OnDestroy()
	{
		if (showing)
		{
			InputHandlerStack.main.Pop(base.gameObject);
		}
	}
}
