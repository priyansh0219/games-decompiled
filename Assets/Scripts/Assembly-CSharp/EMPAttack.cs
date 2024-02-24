using UnityEngine;

public class EMPAttack : CreatureAction
{
	public float swimVelocity = 10f;

	public float swimInterval = 0.8f;

	public float blastInterval = 15f;

	public float cyclopsBlastInterval = 30f;

	public float castDelay = 1.3f;

	public float maxAttackRange = 10f;

	public float searchRange = 35f;

	public float searchInterval = 5f;

	[AssertNotNull]
	public VFXController fxControl;

	public GameObject ammoPrefab;

	public FMOD_StudioEventEmitter empSound;

	private RegistredLightSource target;

	private float timeNextSearch;

	private float timeNextSwim;

	private float timeLastBlast;

	private float timeNextBlast;

	private float blastSpawnTime;

	private bool casting;

	public float minHeightDiff = 2f;

	public override float Evaluate(Creature creature, float time)
	{
		if (time > timeNextSearch)
		{
			timeNextSearch = time + searchInterval;
			UpdateTarget();
		}
		if ((bool)target && time > timeNextBlast)
		{
			return base.Evaluate(creature, time);
		}
		return 0f;
	}

	private void UpdateTarget()
	{
		target = RegistredLightSource.GetNearestLight(base.transform.position, searchRange);
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (casting || !(time > timeNextBlast) || !target || !(target.GetIntensity() > 0f) || !(time > timeNextSwim))
		{
			return;
		}
		timeNextSwim = time + swimInterval;
		Vector3 position = target.GetPosition();
		Vector3 position2 = base.transform.position;
		_ = base.transform.forward;
		Vector3 vector = Vector3.Normalize(position - position2);
		float num = Vector3.Distance(position2, position);
		if (Mathf.Abs(position.y - position2.y) < minHeightDiff && num < maxAttackRange)
		{
			casting = true;
			SafeAnimator.SetBool(creature.GetAnimator(), "blast", value: true);
			if (empSound != null)
			{
				Utils.PlayEnvSound(empSound, base.transform.position);
			}
			fxControl.Play(0);
			Invoke("CastEMPBlast", castDelay);
		}
		else
		{
			Vector3 targetPosition = position2 + 0.5f * num * vector;
			targetPosition.y = position.y;
			base.swimBehaviour.SwimTo(targetPosition, swimVelocity);
		}
	}

	private void CastEMPBlast()
	{
		casting = false;
		timeLastBlast = Time.time;
		timeNextBlast = timeLastBlast + blastInterval;
		SafeAnimator.SetBool(creature.GetAnimator(), "blast", value: false);
		EMPBlast component = Utils.SpawnPrefabAt(ammoPrefab, null, base.transform.position).GetComponent<EMPBlast>();
		if ((bool)component)
		{
			component.source = this;
		}
	}

	public void OnCyclopsHit()
	{
		timeNextBlast = timeLastBlast + cyclopsBlastInterval;
	}
}
