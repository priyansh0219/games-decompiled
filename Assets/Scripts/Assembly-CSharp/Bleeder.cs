using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Bleeder : Creature
{
	[AssertNotNull]
	public AttachAndSuck attachAndSuck;

	[AssertNotNull]
	public Renderer modelRenderer;

	[AssertNotNull]
	public FMODAsset punchSound;

	[AssertNotNull]
	public AnimationCurve initialFillLevel;

	[AssertNotNull]
	public CreatureTrait fillLevel;

	public float fillAmountPerSuck = 0.1f;

	public float fightDamage = 4f;

	private float smoothFillLevel;

	public override void Start()
	{
		base.Start();
		if (initialFillLevel != null && initialFillLevel.length > 0)
		{
			fillLevel.Value = initialFillLevel.Evaluate(Random.value);
		}
	}

	public void Update()
	{
		fillLevel.UpdateTrait(Time.deltaTime);
		smoothFillLevel = Mathf.Lerp(smoothFillLevel, fillLevel.Value, Time.deltaTime);
		modelRenderer.material.SetFloat(ShaderPropertyID._FillSack, smoothFillLevel);
		Animator animator = GetAnimator();
		if (animator != null && animator.isActiveAndEnabled)
		{
			SafeAnimator.SetBool(animator, "attached", attachAndSuck.attached);
			SafeAnimator.SetFloat(animator, "bloat", smoothFillLevel);
		}
	}

	public void OnHit(PlayerTool tool)
	{
		float originalDamage = ((tool == null) ? fightDamage : tool.bleederDamage);
		liveMixin.TakeDamage(originalDamage, base.transform.position);
		GetAnimator().SetTrigger("hit");
		FMODAsset fMODAsset = ((tool == null) ? punchSound : tool.GetBleederHitSound(punchSound));
		if (fMODAsset != null)
		{
			Utils.PlayFMODAsset(fMODAsset, base.transform);
		}
	}

	private void OnSuck()
	{
		fillLevel.Add(fillAmountPerSuck);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Object.Destroy(modelRenderer.material);
	}
}
