using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class POIMemory : MonoBehaviour
{
	private HashSet<PointOfInterest> pois = new HashSet<PointOfInterest>();

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Add(PointOfInterest poi)
	{
		pois.Add(poi);
	}

	public HashSet<PointOfInterest> GetPOIs()
	{
		return pois;
	}
}
