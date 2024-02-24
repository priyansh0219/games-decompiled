using System.Collections;
using UnityEngine;

public class MainMenuShifter : MonoBehaviour
{
	private CanvasGroup cg;

	private Vector3 startPos;

	private Vector3 shiftPos;

	private bool hasOpened;

	private float shiftPx;

	public float shiftFactor = 0.025f;

	public float animationTime = 0.3f;

	private void OnEnable()
	{
		if (!hasOpened)
		{
			shiftPx = (float)Screen.width * shiftFactor;
			cg = base.gameObject.GetComponent<CanvasGroup>();
			startPos = cg.transform.localPosition;
			shiftPos = startPos + new Vector3(shiftPx, 0f, 0f);
			hasOpened = true;
		}
		cg.alpha = 0f;
		cg.transform.localPosition = shiftPos;
		StartCoroutine("ShiftAlpha");
		StartCoroutine("ShiftPos");
	}

	private IEnumerator ShiftAlpha()
	{
		float start = Time.time;
		while (cg.alpha < 1f)
		{
			if (cg.alpha >= 0.995f)
			{
				cg.alpha = 1f;
				break;
			}
			float f = Mathf.Clamp01((Time.time - start) / animationTime);
			cg.alpha = Mathf.Lerp(cg.alpha, 1f, Mathf.Pow(f, 1.5f));
			yield return null;
		}
	}

	private IEnumerator ShiftPos()
	{
		float start = Time.time;
		while (cg.transform.localPosition.x > startPos.x && !(cg.transform.localPosition.x - startPos.x <= 1f))
		{
			float f = Mathf.Clamp01((Time.time - start) / animationTime);
			cg.transform.localPosition = Vector3.Lerp(shiftPos, startPos, Mathf.Pow(f, 0.25f));
			yield return null;
		}
	}
}
