using System.Collections.Generic;
using UWE;
using UnityEngine;

public class AcidicBrineDamageTrigger : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour
{
	private List<LiveMixin> targets = new List<LiveMixin>();

	private int currentIndex;

	[AssertNotNull]
	public BoxCollider box;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "AcidicBrineDamageTrigger";
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private bool IsValidTarget(LiveMixin liveMixin)
	{
		if (liveMixin == null)
		{
			return false;
		}
		if (!liveMixin.IsAlive())
		{
			return false;
		}
		if (DamageSystem.IsAcidImmune(liveMixin.gameObject))
		{
			return false;
		}
		Player component = liveMixin.GetComponent<Player>();
		if (component != null && (component.currentSub != null || component.inSeamoth || component.inExosuit))
		{
			return false;
		}
		if (liveMixin.GetComponentInParent<SubRoot>() != null)
		{
			return false;
		}
		return true;
	}

	private bool Contains(Vector3 point)
	{
		Vector3 vector = box.transform.InverseTransformPoint(point);
		Vector3 v = box.center - vector;
		Vector3 vector2 = box.size * 0.5f + Vector3.one;
		return v.InBox(-vector2, vector2);
	}

	private void RemoveTarget(LiveMixin target)
	{
		if (targets.Contains(target))
		{
			AcidicBrineDamage component = target.GetComponent<AcidicBrineDamage>();
			if (component != null)
			{
				component.Decrement();
			}
			targets.Remove(target);
			RequestUpdateIfNecessary();
		}
	}

	private void AddTarget(LiveMixin target)
	{
		if (!targets.Contains(target))
		{
			AcidicBrineDamage acidicBrineDamage = target.GetComponent<AcidicBrineDamage>();
			if (acidicBrineDamage == null)
			{
				acidicBrineDamage = target.gameObject.AddComponent<AcidicBrineDamage>();
			}
			acidicBrineDamage.Increment();
			targets.Add(target);
			RequestUpdateIfNecessary();
		}
	}

	private LiveMixin GetLiveMixin(GameObject go)
	{
		GameObject gameObject = UWE.Utils.GetEntityRoot(go);
		if (gameObject == null)
		{
			gameObject = go;
		}
		if (go.GetComponentInChildren<IgnoreTrigger>() != null)
		{
			return null;
		}
		if ((bool)gameObject.GetComponent<SubRoot>())
		{
			return null;
		}
		return gameObject.GetComponent<LiveMixin>();
	}

	private void OnTriggerEnter(Collider other)
	{
		LiveMixin liveMixin = GetLiveMixin(other.gameObject);
		if (IsValidTarget(liveMixin))
		{
			AddTarget(liveMixin);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		LiveMixin liveMixin = GetLiveMixin(other.gameObject);
		if ((bool)liveMixin)
		{
			RemoveTarget(liveMixin);
		}
	}

	public void ManagedUpdate()
	{
		if (targets.Count <= 0)
		{
			return;
		}
		if (currentIndex >= targets.Count)
		{
			currentIndex = 0;
		}
		if (targets[currentIndex] == null)
		{
			targets.RemoveAt(currentIndex);
			return;
		}
		Vector3 position = targets[currentIndex].transform.position;
		if (!Contains(position) || !IsValidTarget(targets[currentIndex]))
		{
			RemoveTarget(targets[currentIndex]);
		}
		else
		{
			currentIndex++;
		}
	}

	private void RequestUpdateIfNecessary()
	{
		if (targets.Count != 0)
		{
			BehaviourUpdateUtils.Register(this);
		}
		else
		{
			BehaviourUpdateUtils.Deregister(this);
		}
	}
}
