using UnityEngine;

public class HangingStinger : PlantBehaviour
{
	public enum Size
	{
		Short = 1,
		Middle = 2,
		Long = 3
	}

	private const float damage = 30f;

	private const float damageDuration = 2.5f;

	private float venomRechargeTime = 5f;

	private Renderer[] renderers;

	public Size size = Size.Short;

	private float lastGlowAmount = -1f;

	private float glowAmount;

	private float _venomAmount = 1f;

	public float venomAmount => _venomAmount;

	public override void Start()
	{
		base.Start();
		renderers = GetComponentsInChildren<Renderer>();
	}

	public virtual void Update()
	{
		_venomAmount = Mathf.Min(1f, _venomAmount + Time.deltaTime * (1f / venomRechargeTime));
		float num = ((_venomAmount >= 1f) ? 1f : (-2f));
		glowAmount = Mathf.Clamp01(glowAmount + Time.deltaTime * num);
		if (glowAmount == lastGlowAmount)
		{
			return;
		}
		lastGlowAmount = glowAmount;
		for (int i = 0; i < renderers.Length; i++)
		{
			Material[] materials = renderers[i].materials;
			for (int j = 0; j < materials.Length; j++)
			{
				materials[j].SetFloat(ShaderPropertyID._GlowStrength, 0.3f + glowAmount);
				materials[j].SetFloat(ShaderPropertyID._GlowStrengthNight, 0.3f + glowAmount);
			}
		}
	}

	private void OnCollisionEnter(Collision other)
	{
		if (_venomAmount >= 1f && other.gameObject.GetComponentInChildren<LiveMixin>() != null)
		{
			DamageOverTime damageOverTime = other.gameObject.AddComponent<DamageOverTime>();
			damageOverTime.doer = base.gameObject;
			damageOverTime.totalDamage = 30f;
			damageOverTime.duration = 2.5f * (float)size;
			damageOverTime.damageType = DamageType.Poison;
			damageOverTime.ActivateInterval(0.5f);
			_venomAmount = 0f;
			venomRechargeTime = Random.value * 5f + 5f;
		}
	}
}
