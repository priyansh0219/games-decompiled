using UnityEngine;
using UnityEngine.UI;

public class MainMenuLogoScaler : MonoBehaviour
{
	public Image logo;

	public float animationSpeed = 10f;

	public float scaleFactor = 1.03f;

	private bool isMouseOver;

	private Vector3 startScale;

	private Vector3 targetScale;

	private void Start()
	{
		startScale = logo.rectTransform.localScale;
		targetScale = new Vector3(startScale.x * scaleFactor, startScale.y * scaleFactor, startScale.z * scaleFactor);
	}

	private void Update()
	{
		if (isMouseOver)
		{
			Animate(targetScale);
		}
		if (!isMouseOver)
		{
			Animate(startScale);
		}
	}

	private void Animate(Vector3 target)
	{
		logo.rectTransform.localScale = Vector3.Lerp(logo.rectTransform.localScale, target, animationSpeed * Time.deltaTime);
	}

	public void MouseOver()
	{
		isMouseOver = true;
	}

	public void MouseExit()
	{
		isMouseOver = false;
	}
}
