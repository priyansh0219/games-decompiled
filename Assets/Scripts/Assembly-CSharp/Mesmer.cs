using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Mesmer : Creature
{
	private float hypnotizeAmount;

	private Renderer[] renderers;

	public override void Start()
	{
		base.Start();
		renderers = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material.SetFloat(ShaderPropertyID._Hypnotize, hypnotizeAmount);
		}
	}

	public void Update()
	{
		float num = -1f;
		CreatureAction bestAction = GetBestAction();
		if (bestAction != null && bestAction.GetType() == typeof(PullCreatures))
		{
			num = 1f;
		}
		hypnotizeAmount = Mathf.Clamp01(hypnotizeAmount + Time.deltaTime * num);
		if (hypnotizeAmount < 1f && hypnotizeAmount > 0f)
		{
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].material.SetFloat(ShaderPropertyID._Hypnotize, hypnotizeAmount);
			}
		}
		Animator animator = GetAnimator();
		if (animator != null && animator.gameObject.activeInHierarchy)
		{
			SafeAnimator.SetBool(animator, "hypnotize", num == 1f);
		}
	}

	public override void OnKill()
	{
		base.OnKill();
		Animator animator = GetAnimator();
		if (animator != null && animator.gameObject.activeInHierarchy)
		{
			SafeAnimator.SetBool(animator, "hypnotize", value: false);
		}
	}
}
