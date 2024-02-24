using UWE;
using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class AttachAndSuck : CreatureAction
{
	[AssertNotNull]
	public Bleeder bleeder;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter attachedSound;

	[AssertNotNull]
	public ParticleSystem suckBloodEffect;

	public float transitionTime = 1f;

	public float leechDamage = 5f;

	private BleederAttachTarget currentTarget;

	private bool active;

	private Vector3 startPos;

	private Quaternion startRot;

	private float timeTargetSet;

	private LiveMixin targetLiveMixin;

	private float timeDetached;

	private bool _playEffects;

	private float timeLastSuck;

	private bool playEffects
	{
		set
		{
			if (value != _playEffects)
			{
				if (value)
				{
					attachedSound.Play();
					suckBloodEffect.Play();
				}
				else
				{
					attachedSound.Stop();
					suckBloodEffect.Stop();
				}
				_playEffects = value;
			}
		}
	}

	public bool attached { get; private set; }

	private void OnCollisionEnter(Collision collision)
	{
		float time = Time.time;
		BleederAttachTarget componentInHierarchy = UWE.Utils.GetComponentInHierarchy<BleederAttachTarget>(collision.gameObject);
		LiveMixin component = collision.gameObject.GetComponent<LiveMixin>();
		bool flag = timeDetached + 4f < time;
		if (componentInHierarchy != null && component != null && !componentInHierarchy.occupied && flag && GetComponent<LiveMixin>().IsAlive())
		{
			Player componentInHierarchy2 = UWE.Utils.GetComponentInHierarchy<Player>(collision.gameObject);
			if (!(componentInHierarchy2 != null) || (!componentInHierarchy2.IsInSub() && componentInHierarchy2.GetMode() == Player.Mode.Normal && componentInHierarchy2.CanBeAttacked()))
			{
				currentTarget = componentInHierarchy;
				targetLiveMixin = component;
				attached = false;
				startPos = base.transform.position;
				startRot = base.transform.rotation;
				timeTargetSet = time;
				componentInHierarchy.occupied = true;
				componentInHierarchy.attached = false;
				componentInHierarchy.bleeder = bleeder;
			}
		}
	}

	private void OnTargetKilled()
	{
		SetDetached();
	}

	private void OnKill()
	{
		SetDetached();
	}

	private void OnDisable()
	{
		SetDetached();
	}

	private void SuckBlood()
	{
		float time = Time.time;
		if (timeLastSuck + 1f < time)
		{
			timeLastSuck = time;
			GetComponent<LiveMixin>().AddHealth(1f);
			if (targetLiveMixin.TakeDamage(leechDamage, base.transform.position))
			{
				SetDetached();
			}
			base.gameObject.SendMessage("OnSuck", SendMessageOptions.DontRequireReceiver);
		}
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (GameModeUtils.IsInvisible())
		{
			return 0f;
		}
		bool flag = timeDetached + 4f < time;
		if (currentTarget != null && targetLiveMixin != null && targetLiveMixin.IsAlive() && GetComponent<LiveMixin>().IsAlive() && flag)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	public void SetDetached()
	{
		timeDetached = Time.time;
		if (currentTarget != null)
		{
			currentTarget.occupied = false;
			currentTarget.attached = false;
			currentTarget.bleeder = null;
		}
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Collider>().enabled = true;
		currentTarget = null;
		attached = false;
		targetLiveMixin = null;
		playEffects = false;
		SetLayer(LayerMask.NameToLayer("Default"));
	}

	public override void StartPerform(Creature creature, float time)
	{
		GetComponent<Rigidbody>().isKinematic = true;
		GetComponent<Collider>().enabled = false;
		active = true;
		creature.Aggression.Add(1f);
		SafeAnimator.SetBool(creature.GetAnimator(), "attacking", value: true);
	}

	public override void StopPerform(Creature creature, float time)
	{
		creature.Aggression.Add(-1f);
		SafeAnimator.SetBool(creature.GetAnimator(), "attacking", value: false);
		SetDetached();
		active = false;
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (currentTarget != null && attached)
		{
			SuckBlood();
		}
	}

	private void Update()
	{
		if (!active || !(currentTarget != null))
		{
			return;
		}
		if ((bool)currentTarget.player && (currentTarget.player.IsInsideWalkable() || currentTarget.player.precursorOutOfWater))
		{
			SetDetached();
			base.transform.position = creature.leashPosition;
			return;
		}
		float num = (Time.time - timeTargetSet) / transitionTime;
		if (num >= 1f)
		{
			attached = true;
			currentTarget.attached = true;
			base.transform.position = currentTarget.transform.position;
			base.transform.rotation = currentTarget.transform.rotation;
			SetLayer(LayerMask.NameToLayer("Viewmodel"));
			playEffects = true;
		}
		else
		{
			Vector3 position = currentTarget.transform.position;
			Vector3 b = Vector3.Lerp(position + currentTarget.transform.up, position, num);
			base.transform.position = Vector3.Lerp(startPos, b, num);
			base.transform.rotation = Quaternion.Lerp(startRot, currentTarget.transform.rotation, num);
		}
	}

	private void SetLayer(int layer)
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
	}
}
