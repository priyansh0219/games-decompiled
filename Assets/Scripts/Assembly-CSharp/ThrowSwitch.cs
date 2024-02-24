using System.Collections;
using TMPro;
using UnityEngine;

public class ThrowSwitch : MonoBehaviour
{
	public bool completed;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public GameObject lamp;

	[AssertNotNull]
	public Material completeMat;

	[AssertNotNull]
	public Material incompleteMat;

	[AssertNotNull]
	public CinematicModeTrigger cinematicTrigger;

	[AssertNotNull]
	public RocketPreflightCheckManager preflightCheckManager;

	[AssertNotNull]
	public PreflightCheckSwitch preflightCheckSwitch;

	[AssertNotNull]
	public BoxCollider triggerCollider;

	public PreflightCheck preflightCheck;

	public TextMeshProUGUI signText;

	private float offPosition = 180f;

	private void Start()
	{
		lamp.GetComponent<SkinnedMeshRenderer>().material = incompleteMat;
		if ((bool)signText)
		{
			string text = preflightCheckManager.ReturnLocalizedPreflightCheckName(preflightCheck);
			if (!string.IsNullOrEmpty(text))
			{
				signText.text = text;
			}
		}
		completed = preflightCheckManager.GetPreflightComplete(preflightCheck);
		if (completed)
		{
			triggerCollider.enabled = false;
			lamp.GetComponent<SkinnedMeshRenderer>().material = completeMat;
		}
	}

	public void HandDown()
	{
		if (!completed)
		{
			animator.SetTrigger("Throw");
			StartCoroutine(CompleteThrow());
			completed = true;
			cinematicTrigger.showIconOnHandHover = false;
			triggerCollider.enabled = false;
		}
	}

	private IEnumerator CompleteThrow()
	{
		preflightCheckSwitch.CompletePreflightCheck();
		yield return new WaitForSeconds(3f);
		lamp.GetComponent<SkinnedMeshRenderer>().material = completeMat;
	}
}
