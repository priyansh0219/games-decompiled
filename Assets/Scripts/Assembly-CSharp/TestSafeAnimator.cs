using UnityEngine;

public class TestSafeAnimator : MonoBehaviour
{
	public Animator target;

	private void Update()
	{
		if (Random.value < 0.02f)
		{
			SafeAnimator.SetFloat(target, "lala", 0.1f);
		}
		else if (Random.value < 0.02f)
		{
			SafeAnimator.SetBool(target, "ecoactionShiver", value: false);
		}
		else if (Random.value < 0.02f)
		{
			SafeAnimator.SetBool(target, "ecoactionShiver", value: true);
		}
		else if (Random.value < 0.02f)
		{
			SafeAnimator.SetBool(target, "ecoactionShrink", value: false);
		}
		else if (Random.value < 0.02f)
		{
			SafeAnimator.SetBool(target, "ecoactionShrink", value: true);
		}
	}
}
