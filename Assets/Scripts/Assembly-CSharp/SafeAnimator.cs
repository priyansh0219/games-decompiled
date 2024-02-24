using UnityEngine;

public static class SafeAnimator
{
	public static void SetFloat(Animator animator, string param, float value)
	{
		animator.SetFloat(param, value);
	}

	public static void SetBool(Animator animator, string param, bool value)
	{
		animator.SetBool(param, value);
	}

	public static void SetInteger(Animator animator, string param, int value)
	{
		animator.SetInteger(param, value);
	}

	public static void SetTrigger(Animator animator, string param)
	{
		animator.SetTrigger(param);
	}
}
