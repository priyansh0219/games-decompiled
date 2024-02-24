using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TreaderPath.asset", menuName = "Subnautica/Create TreaderPath Asset")]
public class TreaderPath : ScriptableObject
{
	[Serializable]
	public class PathPoint
	{
		public Vector3 position;

		public float grazingRange;

		public float grazingTime;
	}

	public List<PathPoint> pathPoints = new List<PathPoint>();
}
