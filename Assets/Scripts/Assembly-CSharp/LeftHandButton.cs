using System;
using UnityEngine;
using UnityEngine.Events;

public class LeftHandButton : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour
{
	[Serializable]
	public class OnClickEvent : UnityEvent
	{
	}

	public OnClickEvent onClick;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "LeftHandToButtonClick";
	}

	public void ManagedUpdate()
	{
		if (base.enabled && GameInput.GetButtonDown(GameInput.Button.LeftHand))
		{
			onClick.Invoke();
		}
	}

	public void OnMouseEnter()
	{
		BehaviourUpdateUtils.Register(this);
	}

	public void OnMouseExit()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDisable()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			BehaviourUpdateUtils.Deregister(this);
		}
	}
}
