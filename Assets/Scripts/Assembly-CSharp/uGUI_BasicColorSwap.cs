using TMPro;
using UnityEngine;

public class uGUI_BasicColorSwap : MonoBehaviour
{
	[SerializeField]
	private bool resetToWhiteOnDisable;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnDisable()
	{
		if (resetToWhiteOnDisable)
		{
			makeTextWhite();
		}
	}

	public void makeTextBlack()
	{
		TextMeshProUGUI[] componentsInChildren = GetComponentsInChildren<TextMeshProUGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].color = Color.black;
		}
	}

	public void makeTextWhite()
	{
		TextMeshProUGUI[] componentsInChildren = GetComponentsInChildren<TextMeshProUGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].color = Color.white;
		}
	}
}
