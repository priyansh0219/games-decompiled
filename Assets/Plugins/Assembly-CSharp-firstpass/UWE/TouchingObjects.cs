using System.Collections.Generic;
using UnityEngine;

namespace UWE
{
	public class TouchingObjects : MonoBehaviour
	{
		private HashSet<GameObject> touchingObjects = new HashSet<GameObject>();

		private void Start()
		{
		}

		private void Update()
		{
		}

		private void OnTriggerEnter(Collider other)
		{
			touchingObjects.Add(other.gameObject);
		}

		private void OnTriggerExit(Collider other)
		{
			touchingObjects.Remove(other.gameObject);
		}

		public HashSet<GameObject> Get()
		{
			return touchingObjects;
		}
	}
}
