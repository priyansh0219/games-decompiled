using UnityEngine;
using mset;

public class EscapePodCinematicControl : MonoBehaviour
{
	public EscapePod escapePod;

	public VFXController introFX;

	public LightingController lightingControl;

	[AssertNotNull]
	public Sky interiorSky;

	public AnimationCurve skyIntensityCurve;

	public Animator lightsAnimator;

	public GameObject hatchLight;

	public float skyAnimTime;

	public bool animateSky;

	private void Update()
	{
		if (animateSky && MiscSettings.flashes)
		{
			skyAnimTime += Time.deltaTime;
			interiorSky.MasterIntensity = skyIntensityCurve.Evaluate(skyAnimTime);
			interiorSky.Dirty = true;
		}
	}

	private void OnIntroStart()
	{
		introFX.Play(9);
		introFX.Play(11);
		lightingControl.SnapToState(0);
	}

	private void OnShipSmoking()
	{
		introFX.Play(0);
	}

	private void OnShipExplode()
	{
		introFX.Play(1);
		introFX.Play(10);
		SafeAnimator.SetBool(lightsAnimator, "aurora_exploding", value: true);
		animateSky = true;
		skyAnimTime = 0f;
	}

	private void OnPanelBreak()
	{
		introFX.Play(2);
		introFX.Play(3);
		introFX.Play(4);
	}

	private void OnPanelHitCeiling()
	{
		introFX.Play(5);
	}

	private void OnExtinguisherDetach()
	{
		introFX.Play(6);
	}

	private void OnExtinguisherHitFloor()
	{
		introFX.Play(7);
	}

	private void OnExtinguisherStartSlide()
	{
		introFX.Play(8);
	}

	private void OnPanelHitLadder()
	{
		introFX.Play(5);
	}

	private void OnPanelHitFloor()
	{
		introFX.Play(5);
	}

	private void OnPanelHitLadderAgain()
	{
		introFX.Play(5);
	}

	private void OnPanelHitSit()
	{
		introFX.Play(5);
	}

	private void OnPanelHitFloorAgain()
	{
		introFX.Play(5);
	}

	private void OnPanelHitSitAgain()
	{
		introFX.Play(5);
	}

	private void OnDamagedPod()
	{
		SafeAnimator.SetBool(lightsAnimator, "aurora_exploding", value: false);
		lightsAnimator.enabled = false;
		introFX.StopAndDestroy(3, 0f);
		introFX.StopAndDestroy(4, 0f);
		introFX.StopAndDestroy(9, 0f);
		introFX.StopAndDestroy(10, 0f);
		introFX.StopAndDestroy(11, 0f);
		escapePod.ShowDamagedEffects();
		animateSky = false;
		lightingControl.SnapToState(1);
		IntroLifepodDirector.main.EnableIntroSequence();
	}

	public void StopAll()
	{
		introFX.StopAndDestroy(0f);
		SafeAnimator.SetBool(lightsAnimator, "aurora_exploding", value: false);
		lightsAnimator.enabled = false;
		animateSky = false;
		lightingControl.SnapToState(1);
		hatchLight.SetActive(value: false);
	}
}
