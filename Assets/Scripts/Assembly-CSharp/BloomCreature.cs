using ProtoBuf;
using UnityEngine;

[ProtoContract]
[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(Light))]
public class BloomCreature : Creature
{
	public static int maxParticles = 100;

	public ParticleSystem[] particleSystems;

	private Color currentColor;

	public Light light;

	public float colorChangeRate;

	public float energyRechargeRate;

	public float energyDischargeRate;

	private float energy;

	public Color attractColor;

	public float minParticleSize = 1f;

	public float maxParticleSize = 20f;

	public float maxLightRange = 20f;

	public float lightSearchRange = 10f;

	public float lightSearchInterval = 2f;

	private RegistredLightSource lightSource;

	private float timeNextSearch;

	private void Awake()
	{
		particleSystems = base.gameObject.GetComponentsInChildren<ParticleSystem>();
	}

	public void Update()
	{
		if (Time.time > timeNextSearch)
		{
			timeNextSearch = Time.time + lightSearchInterval;
			lightSource = RegistredLightSource.GetNearestLight(base.transform.position, lightSearchRange);
		}
		Light light = (lightSource ? lightSource.GetHostLight() : null);
		if ((bool)light)
		{
			leashPosition = light.transform.position;
		}
		if ((bool)light)
		{
			attractColor = light.color;
		}
		currentColor = Color.Lerp(currentColor, attractColor, Time.deltaTime * colorChangeRate);
		float num = (lightSource ? (lightSource.GetIntensity() * energyRechargeRate) : (0f - energyDischargeRate));
		energy = Mathf.Clamp01(energy + num * Time.deltaTime);
		currentColor.a = Mathf.Clamp01(energy);
		for (int i = 0; i < particleSystems.Length; i++)
		{
			particleSystems[i].startColor = currentColor;
			particleSystems[i].startSize = minParticleSize + (maxParticleSize - minParticleSize) * energy;
		}
		this.light.intensity = energy;
		this.light.color = currentColor;
		this.light.range = energy * maxLightRange;
	}
}
