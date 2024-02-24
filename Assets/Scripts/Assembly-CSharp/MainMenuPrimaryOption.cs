using UnityEngine;

public class MainMenuPrimaryOption : MonoBehaviour
{
	public GameObject optionPanel;

	public GameObject optionDot;

	public float animationMovement = 0.1f;

	public float animationSpeed = 12f;

	public float animationStopThreshold = 0.005f;

	public float sphereAnimationMovement;

	public float dotAnimationSpeed;

	public bool isOpening;

	public bool isClosing;

	private Vector3 startPos;

	private Vector3 targetPos;

	private Vector3 startDot;

	private Vector3 targetDot;

	private Color dotStartColour;

	private Color dotTargetColour;

	public float dotColourAnimationSpeed;

	public GameObject optionText;

	public GameObject optionCircle;

	public float hideDistance = 7f;

	public float hideSpeed = 13f;

	private Vector3 hidingPos;

	private Vector3 normalPos;

	private void Start()
	{
		isOpening = false;
		isClosing = false;
		startPos = optionPanel.transform.localPosition;
		Vector3 vector = new Vector3(animationMovement, 0f, 0f);
		targetPos = startPos + vector;
		startDot = optionDot.transform.localScale;
		Vector3 vector2 = new Vector3(sphereAnimationMovement, sphereAnimationMovement, sphereAnimationMovement);
		targetDot = startDot + vector2;
		dotStartColour = optionDot.GetComponent<Renderer>().material.color;
		dotStartColour.a = 0f;
		optionDot.GetComponent<Renderer>().material.color = dotStartColour;
		Color color = new Color(1f, 1f, 1f, 1f);
		dotTargetColour = color;
		hidingPos = base.transform.localPosition;
		Vector3 vector3 = new Vector3(hideDistance, 0f, 0f);
		hidingPos += vector3;
		normalPos = base.transform.localPosition;
	}

	private void Update()
	{
		if (isOpening)
		{
			if (targetPos.x - optionPanel.transform.localPosition.x < animationStopThreshold)
			{
				isOpening = false;
				optionPanel.transform.localPosition = targetPos;
				optionDot.GetComponent<Renderer>().material.color = dotTargetColour;
			}
			else
			{
				AnimatePanel(targetPos, animationSpeed);
				AnimateDot(targetDot, dotAnimationSpeed);
				AnimateDotColour(dotTargetColour, dotColourAnimationSpeed);
			}
		}
		if (isClosing)
		{
			if (optionPanel.transform.localPosition.x - startPos.x < animationStopThreshold)
			{
				isClosing = false;
				optionPanel.transform.localPosition = startPos;
				optionDot.transform.localScale = startDot;
				optionDot.GetComponent<Renderer>().material.color = dotStartColour;
			}
			else
			{
				AnimatePanel(startPos, animationSpeed);
				AnimateDot(startDot, dotAnimationSpeed);
				AnimateDotColour(dotStartColour, dotColourAnimationSpeed);
			}
		}
	}

	private void AnimatePanel(Vector3 target, float speed)
	{
		optionPanel.transform.localPosition = Vector3.Lerp(optionPanel.transform.localPosition, target, speed * Time.deltaTime);
	}

	private void AnimateDot(Vector3 target, float speed)
	{
		optionDot.transform.localScale = Vector3.Lerp(optionDot.transform.localScale, target, speed * Time.deltaTime);
	}

	private void AnimateDotColour(Color target, float speed)
	{
		optionDot.GetComponent<Renderer>().material.color = Color.Lerp(optionDot.GetComponent<Renderer>().material.color, target, speed * Time.deltaTime);
	}

	private void OnDisable()
	{
		isClosing = false;
		optionPanel.transform.localPosition = startPos;
		optionDot.transform.localScale = startDot;
		optionDot.GetComponent<Renderer>().material.color = dotStartColour;
	}

	public void ClickAction()
	{
	}
}
