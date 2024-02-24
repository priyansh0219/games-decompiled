using UnityEngine;
using UnityEngine.UI;

public class Damage : MonoBehaviour
{
	[AssertNotNull]
	public RawImage guiTexture;

	public void Init(float inHoldTime, float fadeTime, Texture texture, Vector3 inDamageSource)
	{
		guiTexture.texture = texture;
		RectTransform component = guiTexture.gameObject.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(guiTexture.texture.width, guiTexture.texture.height);
		Vector2 vector = MainCamera.camera.WorldToScreenPoint(inDamageSource);
		float num = Mathf.Clamp(vector.x / (float)Screen.width, 0.2f, 0.8f);
		float num2 = Mathf.Clamp(vector.y / (float)Screen.height, 0.2f, 0.8f);
		component.anchoredPosition = new Vector2((float)Screen.width * (num - 0.5f), (float)Screen.height * (num2 - 0.5f));
		iTween.ValueTo(base.gameObject, iTween.Hash("alpha", 0, "from", 1f, "to", 0f, "onupdate", "OnAlphaUpdate", "delay", inHoldTime, "time", fadeTime, "oncomplete", "FadeOut", "oncompletetarget", base.gameObject));
	}

	private void OnAlphaUpdate(float value)
	{
		Color color = guiTexture.color;
		color.a = value;
		guiTexture.color = color;
	}

	public void FadeOut()
	{
		Object.Destroy(base.gameObject);
	}
}
