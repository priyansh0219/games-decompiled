using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
[RequireComponent(typeof(OnGroundTracker))]
public class SandShark : Creature
{
	[AssertNotNull]
	public GameObject digInEffect;

	[AssertNotNull]
	public GameObject digOutEffect;

	[AssertNotNull]
	public GameObject model;

	[AssertNotNull]
	public OnGroundTracker onGroundTracker;

	[AssertNotNull]
	public VFXController groundMoveTrail;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter idleSound;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter moveSandSound;

	[AssertNotNull]
	public FMOD_StudioEventEmitter burrowSound;

	[AssertNotNull]
	public FMOD_StudioEventEmitter alertSound;

	[AssertNotNull]
	public EcoTarget ecoTarget;

	public float minVelocityForFX = 1f;

	private Vector3 modelOffset = Vector3.up * -3.1f;

	private Vector3 modelStartPosition;

	private float timeLastAlertSound;

	private float onGroundFraction;

	private bool frozen;

	private bool wasOnGround;

	private bool groundFXPlaying;

	public override void Start()
	{
		base.Start();
		if (model != null)
		{
			modelStartPosition = model.transform.localPosition;
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		InvokeRepeating("UpdateFindLeashPosition", Random.value, 0.2f);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		CancelInvoke("UpdateFindLeashPosition");
	}

	private bool FindLeashPosition()
	{
		bool result = false;
		Vector3 origin = ((!(Vector3.Distance(base.transform.position, leashPosition) < 10f)) ? (leashPosition + Vector3.up) : (base.transform.position + Random.onUnitSphere + Vector3.up));
		if (UWE.Utils.TraceForTerrain(new Ray(origin, Vector3.down), 3000f, out var hitInfo))
		{
			leashPosition = hitInfo.point;
			result = true;
		}
		return result;
	}

	private void UpdateFindLeashPosition()
	{
		if (FindLeashPosition())
		{
			CancelInvoke("UpdateFindLeashPosition");
		}
	}

	public void OnMeleeAttack(GameObject target)
	{
		float forTime = Random.value * 2f + 3f;
		BurryInSand component = GetComponent<BurryInSand>();
		if (component != null)
		{
			component.Disable(forTime);
		}
	}

	public void Update()
	{
		Animator animator = GetAnimator();
		if (animator != null && animator.gameObject.activeInHierarchy)
		{
			SafeAnimator.SetBool(animator, "in_ground", onGroundTracker.onSurface);
		}
		float num = 0f;
		num = ((!onGroundTracker.onSurface) ? (-1f) : 1f);
		onGroundFraction = Mathf.Clamp01(onGroundFraction + Time.deltaTime * num * 0.75f);
		if (onGroundFraction < 1f && onGroundFraction > 0f)
		{
			model.transform.localPosition = modelStartPosition + modelOffset * onGroundFraction;
		}
		bool onSurface = onGroundTracker.onSurface;
		if (wasOnGround != onSurface)
		{
			GameObject gameObject = null;
			if (onSurface)
			{
				idleSound.Stop();
				gameObject = digInEffect;
				burrowSound.StartEvent();
				GetComponent<Rigidbody>().freezeRotation = true;
			}
			else
			{
				idleSound.Play();
				gameObject = digOutEffect;
				groundMoveTrail.Play(1);
				groundMoveTrail.Play(2);
				GetComponent<Rigidbody>().freezeRotation = false;
			}
			GameObject obj = UWE.Utils.InstantiateWrap(gameObject);
			obj.transform.position = onGroundTracker.lastSurfacePoint;
			obj.transform.forward = onGroundTracker.lastSurfaceNormal;
			wasOnGround = onSurface;
			ecoTarget.enabled = !onSurface;
		}
		float magnitude = GetComponent<Rigidbody>().velocity.magnitude;
		bool flag = onSurface && magnitude > minVelocityForFX;
		if (flag != groundFXPlaying)
		{
			if (flag)
			{
				moveSandSound.Play();
				groundMoveTrail.Play(0);
				groundMoveTrail.StopAndDestroy(1, 6f);
				groundMoveTrail.StopAndDestroy(2, 3f);
			}
			else
			{
				groundMoveTrail.StopAndDestroy(0, 6f);
				moveSandSound.Stop();
			}
			groundFXPlaying = flag;
		}
		if (Aggression.Value > 0.3f && timeLastAlertSound + 8f < Time.time)
		{
			alertSound.StartEvent();
			timeLastAlertSound = Time.time;
		}
	}

	public void OnFreezeByStasisSphere()
	{
		frozen = true;
	}

	public void OnUnfreezeByStasisSphere()
	{
		frozen = false;
	}
}
