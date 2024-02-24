using UnityEngine;

namespace UWE
{
	public class PureTransform
	{
		public readonly Vector3 pos;

		public readonly Quaternion rot;

		public readonly Vector3 scale;

		public PureTransform(Vector3 pos, Quaternion rot, Vector3 scale)
		{
			this.pos = pos;
			this.rot = rot;
			this.scale = scale;
		}
	}
}
