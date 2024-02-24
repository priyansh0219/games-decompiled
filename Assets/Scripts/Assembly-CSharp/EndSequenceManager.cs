using System.Collections;
using Gendarme;
using UnityEngine;
using UnityEngine.SceneManagement;

[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
public class EndSequenceManager : MonoBehaviour
{
	[AssertNotNull]
	public InvertMotion inverseMotion;

	[AssertNotNull]
	public Animator primarySceneAnimator;

	public bool fullSequence;

	public static EndSequenceManager main;

	private void Awake()
	{
		main = this;
	}

	public void AssignRocketToCenter(Transform transform)
	{
		inverseMotion.center = transform;
	}

	public void InitializeEndingCinematic()
	{
		primarySceneAnimator.SetBool("ending_sequence", value: true);
		StartCoroutine(InitializeCreditsSequence());
	}

	private IEnumerator InitializeCreditsSequence()
	{
		yield return new WaitForSeconds(fullSequence ? 100f : 50f);
		uGUI_PlayerDeath.main.TriggerDeathVignette();
		yield return new WaitForSeconds(3f);
		EndCreditsManager.showEaster = true;
		yield return AddressablesUtility.LoadSceneAsync("EndCreditsSceneCleaner", LoadSceneMode.Single);
	}
}
