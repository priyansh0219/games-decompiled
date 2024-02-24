using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_BuildBotScreen : MonoBehaviour
{
	[AssertNotNull]
	public Image buildBotIcon;

	[AssertNotNull]
	public TextMeshProUGUI constructingText;

	[AssertNotNull]
	public Material builderBarsMat;

	[AssertNotNull]
	public RectTransform screenTransform;

	[AssertLocalization]
	private const string constructingInfoKey = "ConstructingInfo";

	private void Start()
	{
		constructingText.text = Language.main.Get("ConstructingInfo");
	}

	private void Update()
	{
		Vector2 value = new Vector2(builderBarsMat.GetTextureOffset(ShaderPropertyID._MainTex).x + 0.5f * Time.deltaTime, 0f);
		builderBarsMat.SetTextureOffset(ShaderPropertyID._MainTex, value);
	}

	private float GetRandomDelay()
	{
		return Random.Range(2f, 5f);
	}
}
