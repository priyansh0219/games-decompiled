using UnityEngine;

[RequireComponent(typeof(GUIText))]
public class UnTyper : MonoBehaviour
{
	public int typeSpeed = 40;

	public string destString;

	private float timeStarted;

	private GUIText guiText;

	private string origString;

	private void Start()
	{
		guiText = base.gameObject.GetComponent<GUIText>();
		timeStarted = Time.time;
		origString = guiText.text;
	}

	private void Update()
	{
		if (Time.deltaTime > 0f)
		{
			int length = origString.Length;
			int length2 = (int)Mathf.Clamp((float)length - (Time.time - timeStarted) * (float)typeSpeed, 0f, length);
			guiText.text = origString.Substring(0, length2);
			if (guiText.text.Length == 0)
			{
				guiText.text = destString;
				Object.DestroyImmediate(this);
			}
		}
	}
}
