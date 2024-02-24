using System.Collections.Generic;
using UnityEngine;

namespace UWE
{
	public class TriggerStayTracker : MonoBehaviour, IManagedFixedUpdateBehaviour, IManagedBehaviour
	{
		public string componentFilter = "";

		public bool includeTriggers = true;

		public bool debug;

		private readonly HashSet<GameObject> touchingSet = new HashSet<GameObject>();

		public int managedFixedUpdateIndex { get; set; }

		public string GetProfileTag()
		{
			return "TriggerStayTracker";
		}

		private void OnEnable()
		{
			BehaviourUpdateUtils.Register(this);
		}

		private void OnDisable()
		{
			BehaviourUpdateUtils.Deregister(this);
		}

		private void OnDestroy()
		{
			BehaviourUpdateUtils.Deregister(this);
		}

		public void ManagedFixedUpdate()
		{
			touchingSet.Clear();
		}

		private void OnTriggerStay(Collider other)
		{
			if (debug)
			{
				Debug.Log("TriggerStayTracker.OnTriggerStay(includeTriggers = " + includeTriggers + "): " + other.gameObject.name);
			}
			if ((componentFilter == "" || (bool)other.gameObject.GetComponent(componentFilter)) && (includeTriggers || !other.isTrigger))
			{
				if (debug)
				{
					Debug.Log("     " + other.gameObject.name + " added to list");
				}
				touchingSet.Add(other.gameObject);
			}
		}

		public HashSet<GameObject> Get()
		{
			return touchingSet;
		}
	}
}
