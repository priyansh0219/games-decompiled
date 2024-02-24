using UnityEngine;

public class LocalizeTextMesh : MonoBehaviour
{
	public TextMesh textMesh;

	private void Start()
	{
		textMesh.text = Language.main.Get(textMesh.text);
	}
}
