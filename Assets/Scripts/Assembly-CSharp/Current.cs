using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Current : MonoBehaviour
{
	public float objectForce;

	public bool activeAtDay;

	public bool activeAtNight;

	public bool currentsActive = true;

	public List<GameObject> disableObjs = new List<GameObject>();

	public List<Rigidbody> rigidbodyList = new List<Rigidbody>();

	private CapsuleCollider capsuleCollider;

	private void Start()
	{
		capsuleCollider = base.gameObject.GetComponent<CapsuleCollider>();
		DayNightCycle.main.dayNightCycleChangedEvent.AddHandler(this, OnDayNightCycleChanged);
		OnDayNightCycleChanged(DayNightCycle.main.IsDay());
		SetActiveState(GetCurrentsActive(DayNightCycle.main.IsDay()));
	}

	private Rigidbody FilterCollider(Collider other)
	{
		Rigidbody component = other.gameObject.GetComponent<Rigidbody>();
		if (component != null && !component.isKinematic)
		{
			return component;
		}
		return null;
	}

	private void OnTriggerEnter(Collider other)
	{
		Rigidbody rigidbody = FilterCollider(other);
		if ((bool)rigidbody)
		{
			rigidbodyList.Add(rigidbody);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Rigidbody rigidbody = FilterCollider(other);
		if ((bool)rigidbody)
		{
			rigidbodyList.Remove(rigidbody);
		}
	}

	private void Update()
	{
		int num = 0;
		while (num < rigidbodyList.Count)
		{
			Rigidbody rigidbody = rigidbodyList[num];
			if ((bool)rigidbody)
			{
				rigidbody.AddForce(base.transform.forward * objectForce * Time.timeScale, ForceMode.Acceleration);
				num++;
			}
			else
			{
				rigidbodyList.RemoveAt(num);
			}
		}
	}

	private bool GetCurrentsActive(bool isDay)
	{
		if (!isDay || !activeAtDay)
		{
			if (!isDay)
			{
				return activeAtNight;
			}
			return false;
		}
		return true;
	}

	public void OnDayNightCycleChanged(bool isDay)
	{
		SetActiveState(GetCurrentsActive(isDay));
	}

	private void SetActiveState(bool newActiveState)
	{
		if (newActiveState == currentsActive)
		{
			return;
		}
		currentsActive = newActiveState;
		foreach (GameObject disableObj in disableObjs)
		{
			ParticleSystem component = base.gameObject.GetComponent<ParticleSystem>();
			if ((bool)component)
			{
				if (currentsActive)
				{
					component.Play();
				}
				else
				{
					component.Stop();
				}
			}
			else
			{
				disableObj.SetActive(currentsActive);
			}
		}
		capsuleCollider.enabled = currentsActive;
	}
}
