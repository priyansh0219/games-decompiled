using UnityEngine;

public class ActionProgress : MonoBehaviour
{
	public static ActionProgress main;

	public float fraction;

	public string label = "";

	private void Awake()
	{
		main = this;
		Hide();
	}

	private void DrawDestroyProgressBar()
	{
		float num = 0.25f;
		float num2 = 0.075f;
		Rect position = new Rect((float)(Screen.width / 2) - (float)Screen.width * num / 2f, (float)Screen.height * 0.3f - (float)Screen.height * num2 / 2f, num * (float)Screen.width, num2 * (float)Screen.height);
		Rect position2 = new Rect(position.x + 5f, position.y + 5f, position.width * fraction - 10f, position.height - 10f);
		GUI.Box(position, "");
		GUI.Box(position2, "");
		GUIStyle gUIStyle = new GUIStyle();
		gUIStyle.normal.textColor = Color.white;
		gUIStyle.alignment = TextAnchor.MiddleCenter;
		string text = label + " " + Mathf.RoundToInt(fraction * 100f) + "%";
		GUI.Label(position, text, gUIStyle);
	}

	public void Show(string label = "Destroy")
	{
		base.gameObject.SetActive(value: true);
		this.label = label;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OnGUI()
	{
		DrawDestroyProgressBar();
	}
}
