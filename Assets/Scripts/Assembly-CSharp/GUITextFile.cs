using UnityEngine;

[RequireComponent(typeof(GUIText))]
public class GUITextFile : MonoBehaviour
{
	public TextAsset src;

	private void Start()
	{
		GetComponent<GUIText>().text = src.text;
	}

	private void Update()
	{
	}
}
