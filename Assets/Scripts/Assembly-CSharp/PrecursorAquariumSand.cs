using System;
using System.Collections;
using Story;
using UnityEngine;

public class PrecursorAquariumSand : MonoBehaviour, IStoryGoalListener
{
	[AssertNotNull]
	public string listenForGoal = "PrecursorPrisonAquariumFinalTeleporterUncover";

	[AssertNotNull]
	public Transform model;

	[AssertNotNull]
	public VFXController fxControl;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter shrinkSound;

	[AssertNotNull]
	public Collider collider;

	public float shrinkDuration = 5f;

	public float delayDuration = 0.7f;

	public Vector3 minScale = Vector3.zero;

	private Renderer modelRenderer;

	public void NotifyGoalComplete(string key)
	{
		if (string.Equals(key, listenForGoal, StringComparison.OrdinalIgnoreCase))
		{
			StartCoroutine(ShrinkAsync());
		}
	}

	private void Start()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			if (main.IsGoalComplete(listenForGoal))
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				main.AddListener(this);
			}
		}
		modelRenderer = model.GetComponent<Renderer>();
	}

	private void OnDestroy()
	{
		StoryGoalManager main = StoryGoalManager.main;
		if ((bool)main)
		{
			main.RemoveListener(this);
		}
		if (modelRenderer != null)
		{
			UnityEngine.Object.Destroy(modelRenderer.material);
		}
	}

	private IEnumerator ShrinkAsync()
	{
		Vector3 initialScale = model.localScale;
		fxControl.Play();
		shrinkSound.Play();
		collider.enabled = false;
		yield return new WaitForSeconds(delayDuration);
		float lerpFactor = 0f;
		while (lerpFactor < 1f)
		{
			lerpFactor += Time.deltaTime / shrinkDuration;
			modelRenderer.material.SetFloat(ShaderPropertyID._Cutoff, lerpFactor);
			model.localScale = Vector3.Lerp(initialScale, minScale, lerpFactor);
			yield return null;
		}
		shrinkSound.Stop();
		fxControl.Stop();
		yield return new WaitForSeconds(5f);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
