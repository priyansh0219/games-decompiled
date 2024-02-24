using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Peeper : Creature
{
	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public new int version = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public float enzymeAmount;

	[NonSerialized]
	[ProtoMember(3)]
	public float timeRechargeEnzyme = -1f;

	[AssertNotNull]
	public LargeWorldEntity largeWorldEntity;

	[AssertNotNull]
	public Rigidbody useRigidbody;

	[AssertNotNull]
	public TrailRenderer enzymeTrail;

	[AssertNotNull]
	public ParticleSystem enzymeParticles;

	[AssertNotNull]
	public GameObject healingTrigger;

	public float rechargeInterval = 120f;

	public float enzymeDuration = 60f;

	public float trailDuration = 10f;

	public float enzymeParticlesRate = 30f;

	private bool _inPrisonAquarium;

	public bool isHero => enzymeAmount > 0f;

	public bool isBornHero => timeRechargeEnzyme > 0f;

	public bool isInPrisonAquarium => _inPrisonAquarium;

	public override void Start()
	{
		_inPrisonAquarium = Creature.prisonAquriumBounds.Contains(base.transform.position);
		base.Start();
	}

	protected override void InitializeOnce()
	{
		base.InitializeOnce();
		LargeWorld main = LargeWorld.main;
		if ((bool)main)
		{
			if (!string.Equals("safeShallows", main.GetBiome(base.transform.position), StringComparison.OrdinalIgnoreCase) && UnityEngine.Random.value < 0.05f)
			{
				timeRechargeEnzyme = 1f;
				BecomeHero();
			}
			if (_inPrisonAquarium)
			{
				InitializeInPrison();
			}
		}
	}

	public void InitializeInPrison()
	{
		largeWorldEntity.cellLevel = LargeWorldEntity.CellLevel.Medium;
	}

	public void Update()
	{
		if (!_inPrisonAquarium && enzymeAmount > 0f)
		{
			enzymeAmount -= Time.deltaTime;
			UpdateEnzymeFX();
			if (enzymeAmount <= 0f)
			{
				enzymeTrail.enabled = false;
				enzymeParticles.Stop();
				healingTrigger.SetActive(value: false);
			}
		}
	}

	public void BecomeHero()
	{
		enzymeAmount = enzymeDuration;
		UpdateEnzymeFX();
		enzymeParticles.Play();
		enzymeTrail.enabled = true;
		healingTrigger.SetActive(value: true);
		InfectedMixin component = base.gameObject.GetComponent<InfectedMixin>();
		if ((bool)component)
		{
			component.SetInfectedAmount(0f);
		}
	}

	public override void OnProtoSerialize(ProtobufSerializer serializer)
	{
		base.OnProtoSerialize(serializer);
		if (isBornHero)
		{
			timeRechargeEnzyme = DayNightUtils.time + rechargeInterval;
		}
	}

	public override void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		base.OnProtoDeserialize(serializer);
		if (isBornHero && DayNightUtils.time > timeRechargeEnzyme)
		{
			BecomeHero();
		}
		else if (isHero)
		{
			enzymeParticles.Play();
			enzymeTrail.enabled = true;
			healingTrigger.SetActive(value: true);
			UpdateEnzymeFX();
		}
		if (version < 2 && isInitialized && Creature.prisonAquriumBounds.Contains(base.transform.position))
		{
			InitializeInPrison();
		}
		version = 2;
	}

	private void UpdateEnzymeFX()
	{
		enzymeTrail.time = Mathf.Clamp(enzymeAmount, 0f, trailDuration);
		_ = enzymeParticlesRate;
		_ = enzymeAmount / enzymeDuration;
		enzymeParticles.SetEmissionRate(enzymeParticlesRate);
	}
}
