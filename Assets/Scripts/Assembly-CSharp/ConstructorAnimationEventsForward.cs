using UnityEngine;

public class ConstructorAnimationEventsForward : MonoBehaviour
{
	[AssertNotNull]
	public Constructor constructor;

	public void OnDeployAnimationEnd()
	{
		constructor.OnDeployAnimationEnd();
	}
}
